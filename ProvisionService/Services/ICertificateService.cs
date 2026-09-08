namespace ProvisionService.Services
{
    public interface ICertificateService
    {
        Task<string> SignCsrAsync(string csrPem, string deviceId);
    }
}
