namespace ProvisionService.Services
{
    public interface IDeviceStore
    {
        bool ValidateBootstrapToken(string deviceId, string bootstrapToken);
    }
}
