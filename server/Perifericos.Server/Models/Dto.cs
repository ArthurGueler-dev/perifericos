namespace Perifericos.Server.Models;

public record DeviceInfoDto(string VendorId, string ProductId, string? SerialNumber, string DeviceInstanceId, string FriendlyName);

public record EventDto
{
    public required string PcIdentifier { get; init; }
    public required string EventType { get; init; }
    public required DeviceInfoDto Device { get; init; }
    public DateTime TimestampUtc { get; init; }
}

public record AuthorizedDevicesResponse
{
    public required string PcIdentifier { get; init; }
    public required List<DeviceInfoDto> AuthorizedDevices { get; init; }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token);


