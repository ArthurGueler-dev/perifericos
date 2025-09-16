using System.Management;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Perifericos.Agent.Models;

namespace Perifericos.Agent.Services;

public class UsbMonitorService : BackgroundService
{
    private readonly ILogger<UsbMonitorService> _logger;
    private readonly PopupNotifier _notifier;
    private readonly ApiClient _api;
    private readonly AuthorizedDevicesCache _authorized;
    private readonly AgentOptions _options;
    private ManagementEventWatcher? _arrivalWatcher;
    private ManagementEventWatcher? _removalWatcher;
    private readonly Dictionary<string, DateTime> _lastEventTime = new();
    private readonly TimeSpan _debounceInterval = TimeSpan.FromSeconds(3);

    public UsbMonitorService(ILogger<UsbMonitorService> logger, PopupNotifier notifier, ApiClient api, AuthorizedDevicesCache authorized, IOptions<AgentOptions> options)
    {
        _logger = logger;
        _notifier = notifier;
        _api = api;
        _authorized = authorized;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadAuthorizedDevices(stoppingToken);
        SetupWatchers();
        _logger.LogInformation("UsbMonitorService iniciado para PC: {pc}", _options.PcIdentifier);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task LoadAuthorizedDevices(CancellationToken ct)
    {
        try
        {
            var resp = await _api.GetAuthorizedDevicesAsync(ct);
            if (resp != null)
            {
                _authorized.Load(resp.AuthorizedDevices);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao carregar dispositivos autorizados");
        }
    }

    private void SetupWatchers()
    {
        var arrivalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        _arrivalWatcher = new ManagementEventWatcher(arrivalQuery);
        _arrivalWatcher.EventArrived += async (_, __) => await OnDeviceChange("Connected");
        _arrivalWatcher.Start();

        var removalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
        _removalWatcher = new ManagementEventWatcher(removalQuery);
        _removalWatcher.EventArrived += async (_, __) => await OnDeviceChange("Disconnected");
        _removalWatcher.Start();
    }

    private async Task OnDeviceChange(string eventType)
    {
        try
        {
            foreach (var device in EnumerateUsbDevices())
            {
                // Debounce: evitar eventos duplicados do mesmo dispositivo
                var deviceKey = $"{device.VendorId}:{device.ProductId}:{device.SerialNumber}:{eventType}";
                var now = DateTime.UtcNow;
                
                if (_lastEventTime.TryGetValue(deviceKey, out var lastTime))
                {
                    if (now - lastTime < _debounceInterval)
                    {
                        continue; // Pular evento duplicado
                    }
                }
                
                _lastEventTime[deviceKey] = now;
                
                // Limpar eventos antigos do cache (manter apenas últimos 10 minutos)
                var keysToRemove = _lastEventTime
                    .Where(kvp => now - kvp.Value > TimeSpan.FromMinutes(10))
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (var key in keysToRemove)
                {
                    _lastEventTime.Remove(key);
                }

                var dto = new EventDto
                {
                    PcIdentifier = _options.PcIdentifier,
                    EventType = eventType,
                    Device = device,
                    TimestampUtc = DateTime.UtcNow
                };

                await _api.SendEventAsync(dto, CancellationToken.None);

                if (eventType == "Connected" && !_authorized.IsAuthorized(device))
                {
                    _notifier.ShowPopup("Periférico não autorizado", $"Dispositivo {device.FriendlyName} conectado e não autorizado.");
                    var alertDto = dto with { EventType = "AlertUnauthorized" };
                    await _api.SendEventAsync(alertDto, CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mudança de dispositivo");
        }
    }

    private static IEnumerable<DeviceInfo> EnumerateUsbDevices()
    {
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'USB' OR PNPDeviceID LIKE 'USB%'");
        foreach (ManagementObject device in searcher.Get())
        {
            var instanceId = device["PNPDeviceID"]?.ToString() ?? string.Empty;
            var name = device["Name"]?.ToString() ?? "USB Device";

            // Filtrar apenas dispositivos relevantes
            if (!IsRelevantDevice(instanceId, name))
                continue;

            // Try parse VID, PID, SN from InstanceId like: USB\\VID_046D&PID_C534&MI_00\\7&2a5a...&0&0000
            string vendorId = Extract(instanceId, "VID_");
            string productId = Extract(instanceId, "PID_");
            string? serial = ExtractSerial(instanceId);

            yield return new DeviceInfo(vendorId, productId, serial, instanceId, name);
        }
    }

    private static bool IsRelevantDevice(string instanceId, string name)
    {
        var nameUpper = name.ToUpperInvariant();
        var instanceUpper = instanceId.ToUpperInvariant();

        // PRIMEIRO: Ignorar dispositivos internos/sistema (mais restritivo)
        var ignorePatterns = new[]
        {
            "ROOT_HUB", "USB ROOT HUB", "Generic USB Hub", "USB HUB",
            "Composite Device", "USB Composite Device", 
            "Audio Device", "Realtek", "Intel", "AMD", "VIA", "NVIDIA",
            "Bluetooth", "WiFi", "Ethernet", "Network", "Wireless",
            "Webcam", "Camera", "Microphone", "Audio",
            "Card Reader", "SD Card", "MMC", "Smart Card",
            "Host Controller", "xHCI", "EHCI", "OHCI", "UHCI",
            "HID Keyboard Device", "HID-compliant system", 
            "USB Receiver", "Unifying", "Wireless Receiver",
            "MI_", "Collection", "Consumer Control", "System Control"
        };

        // Se contém padrões a ignorar, pular
        if (ignorePatterns.Any(pattern => 
            nameUpper.Contains(pattern.ToUpperInvariant()) || 
            instanceUpper.Contains(pattern.ToUpperInvariant())))
        {
            return false;
        }

        // SEGUNDO: Incluir APENAS dispositivos muito específicos
        var specificDevices = new[]
        {
            // Mouses específicos
            "RAZER", "LOGITECH MOUSE", "MICROSOFT MOUSE", 
            // Teclados específicos  
            "RAZER KEYBOARD", "LOGITECH KEYBOARD", "MICROSOFT KEYBOARD",
            // Storage específico
            "MASS STORAGE", "USB DISK", "FLASH DRIVE", "SANDISK", "KINGSTON",
            // Impressoras
            "PRINTER", "HP ", "CANON", "EPSON"
        };

        // Incluir apenas se nome contém dispositivos específicos
        if (specificDevices.Any(device => nameUpper.Contains(device)))
        {
            return true;
        }

        // TERCEIRO: VendorIDs muito específicos de periféricos externos
        var peripheralVendors = new[]
        {
            "VID_1532", // Razer (apenas se não for receiver/wireless)
            "VID_046D", // Logitech (apenas se for mouse/teclado direto)
            "VID_0781", // SanDisk
            "VID_0951", // Kingston
            "VID_058F"  // Alcor Micro (pendrives)
        };

        if (peripheralVendors.Any(vendor => instanceUpper.Contains(vendor)))
        {
            // Para Razer e Logitech, ser ainda mais específico
            if (instanceUpper.Contains("VID_1532") || instanceUpper.Contains("VID_046D"))
            {
                // Apenas se o nome não contém "receiver", "wireless", "unifying"
                if (nameUpper.Contains("RECEIVER") || nameUpper.Contains("WIRELESS") || 
                    nameUpper.Contains("UNIFYING") || nameUpper.Contains("DONGLE"))
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }

    private static string Extract(string input, string key)
    {
        var idx = input.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return string.Empty;
        var start = idx + key.Length;
        var end = input.IndexOfAny(new[] { '\\', '&' }, start);
        if (end < 0) end = input.Length;
        return input.Substring(start, Math.Min(4, end - start));
    }

    private static string? ExtractSerial(string input)
    {
        var parts = input.Split('\\');
        if (parts.Length >= 3)
        {
            var last = parts[^1];
            if (!string.IsNullOrWhiteSpace(last)) return last;
        }
        return null;
    }
}


