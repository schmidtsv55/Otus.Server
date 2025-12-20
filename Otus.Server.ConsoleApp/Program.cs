using Otus.Server.ConsoleApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logingBuilder => { logingBuilder.ClearProviders(); })
    .ConfigureHostConfiguration(hostConfig => { })
    .ConfigureServices((host, services) =>
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Cache.Server"))
            .WithTracing(tracing => tracing
                .AddSource("Otus.Server.ConsoleApp")
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddMeter("Otus.Server.ConsoleApp")
                .AddConsoleExporter())
            .WithLogging(logging => logging
                .AddConsoleExporter());

        services.AddSingleton<SimpleStore>();
        services.AddSingleton(serviceProvider =>
        {
            SimpleStore simpleStore = serviceProvider.GetRequiredService<SimpleStore>();
            return new TcpServer(simpleStore, 8080, 1_024, 4);
        });
        services.AddHostedService<Worker>();
    }).Build();
    
await host.RunAsync();