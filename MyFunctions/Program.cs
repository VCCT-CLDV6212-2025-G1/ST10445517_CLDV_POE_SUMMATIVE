using ClassLibrary.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
var configuration = builder.Configuration;

// Register AzureTableStorageService with connection string from configuration
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("AzureStorage")
                          ?? config["AzureStorage"]
                          ?? Environment.GetEnvironmentVariable("AzureStorage");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "Azure Storage connection string not found. " +
            "Please set 'AzureStorage' in local.settings.json or application settings.");
    }

    return new AzureTableStorageService(connectionString);
});

builder.Build().Run();
