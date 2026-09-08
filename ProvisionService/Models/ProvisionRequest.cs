using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProvisionService.Models
{
    public class ProvisionRequest
    {
        [Required]
        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("bootstrap_token")]
        public string BootstrapToken { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("csr")]
        public string Csr { get; set; } = string.Empty;
    }
}
