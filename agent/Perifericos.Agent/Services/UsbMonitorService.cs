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
        // Ignorar dispositivos internos do sistema
        var ignorePatterns = new[]
        {
            "ROOT_HUB", "USB ROOT HUB", "Generic USB Hub",
            "Composite Device", "USB Composite Device",
            "Audio Device", "Realtek", "Intel", "AMD",
            "Bluetooth", "WiFi", "Ethernet", "Network",
            "Webcam", "Camera", "Microphone",
            "Card Reader", "SD Card", "MMC",
            "Host Controller", "xHCI", "EHCI", "OHCI", "UHCI"
        };

        // Incluir apenas dispositivos interessantes
        var includePatterns = new[]
        {
            "Mouse", "Keyboard", "Teclado",
            "Storage", "Mass Storage", "Disk", "Drive",
            "Printer", "Scanner", "Joystick", "Gamepad",
            "Razer", "Logitech", "Microsoft", "Corsair",
            "HID-compliant", "USB Input Device"
        };

        var nameUpper = name.ToUpperInvariant();
        var instanceUpper = instanceId.ToUpperInvariant();

        // Se contém padrões a ignorar, pular
        if (ignorePatterns.Any(pattern => 
            nameUpper.Contains(pattern.ToUpperInvariant()) || 
            instanceUpper.Contains(pattern.ToUpperInvariant())))
        {
            return false;
        }

        // Se contém padrões relevantes, incluir
        if (includePatterns.Any(pattern => 
            nameUpper.Contains(pattern.ToUpperInvariant()) || 
            instanceUpper.Contains(pattern.ToUpperInvariant())))
        {
            return true;
        }

        // Incluir dispositivos com VendorID de marcas conhecidas
        var knownVendors = new[]
        {
            "VID_1532", // Razer
            "VID_046D", // Logitech  
            "VID_045E", // Microsoft
            "VID_1B1C", // Corsair
            "VID_0781", // SanDisk
            "VID_058F", // Alcor Micro (pendrives)
            "VID_0951"  // Kingston
        };

        if (knownVendors.Any(vendor => instanceUpper.Contains(vendor)))
        {
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


