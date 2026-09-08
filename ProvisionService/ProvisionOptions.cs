using System.Collections.Generic;

namespace ProvisionService
{
    public class ProvisionOptions
    {
        public Dictionary<string, string> AllowedDevices { get; set; } = new();
        public string? CaPrivateKeyPath { get; set; }
        public string? CaCertPath { get; set; }
    }
}
