using KalolCommunity.NotificationWorker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.ApplicationInsights;
using System;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddApplicationInsightsTelemetryWorkerService();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    var appInsightsConnectionString =
        builder.Configuration["ApplicationInsights:ConnectionString"]
        ?? builder.Configuration["ApplicationInsights:ConnStr"];

    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.ApplicationInsights(
            appInsightsConnectionString,
            TelemetryConverter.Traces));

    builder.Services.AddHostedService<NotificationWorker>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Notification worker terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
