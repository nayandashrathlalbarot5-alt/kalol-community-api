using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddHttpClient();

var serviceBusConnection =
    builder.Configuration["ServiceBusConnection"]
    ?? builder.Configuration["ServiceBus:ConnectionString"]
    ?? builder.Configuration["ServiceBus:ConnStr"];

if (!string.IsNullOrWhiteSpace(serviceBusConnection))
{
    Environment.SetEnvironmentVariable("ServiceBusConnection", serviceBusConnection);
}

builder.Build().Run();
