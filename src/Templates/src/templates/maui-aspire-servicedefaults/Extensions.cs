using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.DisableDevCertSecurityCheck();

            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IMauiInitializeService, OpenTelemetryInitializer>(_ => new OpenTelemetryInitializer()));

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {                
                metrics.AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Disables the security check for development certificates.
    /// This should only be used in development environments.
    /// </summary>
    public static IHttpClientBuilder DisableDevCertSecurityCheck(this IHttpClientBuilder builder)
    {
        builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert is not null && cert.Issuer.Equals("CN=localhost", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return errors == System.Net.Security.SslPolicyErrors.None;
            }
        });

        return builder;
    }

    public static TBuilder WithDevTunnel<TBuilder>(this TBuilder builder, string devTunnelId) where TBuilder : IHostApplicationBuilder
    {
        // Check if key already exists in configuration
        var foo = builder.Configuration.Properties.Any();
        
        if (builder.Configuration["DOTNET_DEV_TUNNEL_ID"] is not null)
        {
            // Replace localhost in existing configuration values
            foreach (var key in builder.Configuration.AsEnumerable().Select(kvp => kvp.Key))
            {
                var value = builder.Configuration[key];

                if (value is not null)
                {
                    builder.Configuration[key] = ReplaceLocalhost(value, devTunnelId);
                }
            }
        }
        else
        {
            // Add the dev tunnel ID to the configuration
            builder.Configuration["DOTNET_DEV_TUNNEL_ID"] = devTunnelId;
        }

        return builder;
    }

    private class OpenTelemetryInitializer : IMauiInitializeService
    {
        public void Initialize(IServiceProvider services)
        {
            services.GetService<MeterProvider>();
            services.GetService<TracerProvider>();
            services.GetService<LoggerProvider>();
        }
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    private static string ReplaceLocalhost(string uri, string devTunnelId)
    {
        // source format is `http[s]://localhost:[port]`
        // tunnel format is `http[s]://exciting-tunnel-[port].devtunnels.ms`

        var replacement = Regex.Replace(
            uri,
            @"://localhost\:(\d+)(.*)",
            $"://{devTunnelId}-$1.devtunnels.ms$2",
            RegexOptions.Compiled);

        return replacement;
    }
}
