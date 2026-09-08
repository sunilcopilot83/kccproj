using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace ProvisionService.Services
{
    public class InMemoryDeviceStore : IDeviceStore
    {
        private readonly ConcurrentDictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase);

        public InMemoryDeviceStore(IConfiguration config)
        {
            var section = config.GetSection("ProvisionOptions:AllowedDevices");
            foreach (var child in section.GetChildren())
            {
                _map[child.Key] = child.Value ?? string.Empty;
            }
        }

        public bool ValidateBootstrapToken(string deviceId, string bootstrapToken)
        {
            if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(bootstrapToken)) return false;
            if (!_map.TryGetValue(deviceId, out var expected)) return false;
            return string.Equals(expected, bootstrapToken, StringComparison.OrdinalIgnoreCase);
        }
    }
}
