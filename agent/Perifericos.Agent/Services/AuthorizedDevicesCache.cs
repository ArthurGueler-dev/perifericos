using Perifericos.Agent.Models;

namespace Perifericos.Agent.Services;

public class AuthorizedDevicesCache
{
    private readonly Dictionary<string, DeviceInfo> _authorizedByKey = new();

    public void Load(IEnumerable<DeviceInfo> devices)
    {
        _authorizedByKey.Clear();
        foreach (var d in devices)
        {
            var key = BuildKey(d);
            _authorizedByKey[key] = d;
        }
    }

    public bool IsAuthorized(DeviceInfo device)
    {
        return _authorizedByKey.ContainsKey(BuildKey(device));
    }

    private static string BuildKey(DeviceInfo d)
    {
        return $"{d.VendorId}:{d.ProductId}:{d.SerialNumber ?? ""}".ToUpperInvariant();
    }
}

