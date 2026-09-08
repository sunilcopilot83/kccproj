using Microsoft.AspNetCore.Mvc;
using ProvisionService.Models;

namespace ProvisionService.Controllers
{
    [ApiController]
    [Route("api/v1/provision")]
    public class ProvisionController : ControllerBase
    {
        private readonly Services.IDeviceStore _deviceStore;
        private readonly Services.ICertificateService _certService;
        private readonly ILogger<ProvisionController> _logger;

        public ProvisionController(Services.IDeviceStore deviceStore, Services.ICertificateService certService, ILogger<ProvisionController> logger)
        {
            _deviceStore = deviceStore;
            _certService = certService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProvisionRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "invalid request" });

            if (!_deviceStore.ValidateBootstrapToken(req.DeviceId, req.BootstrapToken))
            {
                _logger.LogWarning("Provision rejected for device {deviceId}", req.DeviceId);
                return BadRequest(new { error = "provisioning request rejected" });
            }

            try
            {
                var pem = await _certService.SignCsrAsync(req.Csr, req.DeviceId);
                return Ok(new { device_certificate = pem });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing CSR for device {deviceId}", req.DeviceId);
                return BadRequest(new { error = "provisioning request rejected" });
            }
        }
    }
}
