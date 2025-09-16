namespace Perifericos.Server.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class Computer
{
    public int Id { get; set; }
    public string Identifier { get; set; } = string.Empty; // PC-001
    public string? Hostname { get; set; }
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}

public class Peripheral
{
    public int Id { get; set; }
    public string VendorId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string FriendlyName { get; set; } = string.Empty;
}

public class PeripheralAssignment
{
    public int Id { get; set; }
    public int PeripheralId { get; set; }
    public Peripheral Peripheral { get; set; } = null!;
    public int ComputerId { get; set; }
    public Computer Computer { get; set; } = null!;
}

public class DeviceEvent
{
    public long Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string PcIdentifier { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // Connected | Disconnected | AlertUnauthorized
    public string VendorId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string DeviceInstanceId { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public bool IsIncident { get; set; }
    public bool IsAlert { get; set; }
}


