namespace Perifericos.Agent.Models;

public record DeviceInfo(string VendorId, string ProductId, string? SerialNumber, string DeviceInstanceId, string FriendlyName);

public record EventDto
{
    public required string PcIdentifier { get; init; }
    public required string EventType { get; init; } // Connected, Disconnected, AlertUnauthorized
    public required DeviceInfo Device { get; init; }
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
};

public record AuthorizedDevicesResponse
{
    public required string PcIdentifier { get; init; }
    public required List<DeviceInfo> AuthorizedDevices { get; init; }
}


