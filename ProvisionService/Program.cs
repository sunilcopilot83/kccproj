using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// bind options and register services
builder.Services.Configure<ProvisionService.ProvisionOptions>(builder.Configuration.GetSection("ProvisionOptions"));
builder.Services.AddSingleton<ProvisionService.Services.IDeviceStore, ProvisionService.Services.InMemoryDeviceStore>();
builder.Services.AddSingleton<ProvisionService.Services.ICertificateService, ProvisionService.Services.CertificateService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();
app.Run();
