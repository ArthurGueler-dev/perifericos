using Microsoft.EntityFrameworkCore;
using Perifericos.Server.Data;
using Perifericos.Server.Models;

namespace Perifericos.Server;

public class DeviceService
{
    private readonly AppDbContext _db;

    public DeviceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthorizedDevicesResponse> GetAuthorizedDevicesAsync(string pcIdentifier, CancellationToken ct)
    {
        var computer = await _db.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.Identifier == pcIdentifier, ct);
        if (computer == null)
        {
            computer = new Computer { Identifier = pcIdentifier };
            _db.Computers.Add(computer);
            await _db.SaveChangesAsync(ct);
        }

        var query = from pa in _db.PeripheralAssignments.AsNoTracking()
                    join p in _db.Peripherals on pa.PeripheralId equals p.Id
                    where pa.ComputerId == computer.Id
                    select new DeviceInfoDto(p.VendorId, p.ProductId, p.SerialNumber, string.Empty, p.FriendlyName);

        var list = await query.ToListAsync(ct);
        return new AuthorizedDevicesResponse
        {
            PcIdentifier = pcIdentifier,
            AuthorizedDevices = list
        };
    }

    public async Task<(DeviceEvent saved, bool isAlert, bool isIncident)> StoreEventAsync(EventDto dto, CancellationToken ct)
    {
        bool isAuthorized = await IsAuthorizedAsync(dto.PcIdentifier, dto.Device, ct);
        bool isAlert = dto.EventType == "AlertUnauthorized" || (dto.EventType == "Connected" && !isAuthorized);
        bool isIncident = await IsIncidentAsync(dto.PcIdentifier, dto.Device, ct);

        var ev = new DeviceEvent
        {
            TimestampUtc = dto.TimestampUtc,
            PcIdentifier = dto.PcIdentifier,
            EventType = dto.EventType,
            VendorId = dto.Device.VendorId,
            ProductId = dto.Device.ProductId,
            SerialNumber = dto.Device.SerialNumber,
            DeviceInstanceId = dto.Device.DeviceInstanceId,
            FriendlyName = dto.Device.FriendlyName,
            IsAlert = isAlert,
            IsIncident = isIncident
        };
        _db.DeviceEvents.Add(ev);
        await _db.SaveChangesAsync(ct);
        return (ev, isAlert, isIncident);
    }

    private async Task<bool> IsAuthorizedAsync(string pcIdentifier, DeviceInfoDto device, CancellationToken ct)
    {
        var computer = await _db.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.Identifier == pcIdentifier, ct);
        if (computer == null) return false;
        var query = from pa in _db.PeripheralAssignments.AsNoTracking()
                    join p in _db.Peripherals.AsNoTracking() on pa.PeripheralId equals p.Id
                    where pa.ComputerId == computer.Id
                    select p;
        var list = await query.ToListAsync(ct);
        return list.Any(p => p.VendorId.Equals(device.VendorId, StringComparison.OrdinalIgnoreCase)
                          && p.ProductId.Equals(device.ProductId, StringComparison.OrdinalIgnoreCase)
                          && ((p.SerialNumber ?? string.Empty).Equals(device.SerialNumber ?? string.Empty, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<bool> IsIncidentAsync(string pcIdentifier, DeviceInfoDto device, CancellationToken ct)
    {
        // incidente se serial já vinculado a outro PC
        var peripheral = await _db.Peripherals.AsNoTracking().FirstOrDefaultAsync(p =>
            p.VendorId == device.VendorId && p.ProductId == device.ProductId && p.SerialNumber == device.SerialNumber, ct);
        if (peripheral == null) return false;

        var assignment = await _db.PeripheralAssignments.AsNoTracking().FirstOrDefaultAsync(a => a.PeripheralId == peripheral.Id, ct);
        if (assignment == null) return false;

        var comp = await _db.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == assignment.ComputerId, ct);
        if (comp == null) return false;

        return !string.Equals(comp.Identifier, pcIdentifier, StringComparison.OrdinalIgnoreCase);
    }
}


