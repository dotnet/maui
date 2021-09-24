using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	/// <summary>
	/// A builder for .NET MAUI cross-platform applications and services.
	/// </summary>
	public sealed class MauiAppBuilder
	{
		private readonly HostBuilder _hostBuilder = new();
		private readonly BootstrapHostBuilder _bootstrapHostBuilder;
		private readonly MauiApplicationServiceCollection _services = new();

		private MauiApp? _builtApplication;

		internal MauiAppBuilder(bool useDefaults = true)
		{
			Services = _services;

			// Run methods to configure both generic and web host defaults early to populate config from appsettings.json
			// environment variables (both DOTNET_ and ASPNETCORE_ prefixed) and other possible default sources to prepopulate
			// the correct defaults.
			_bootstrapHostBuilder = new BootstrapHostBuilder(Services, _hostBuilder.Properties);

			MauiHostingDefaults.ConfigureDefaults(_bootstrapHostBuilder);

			Configuration = new();

			// This is the application configuration
			var hostContext = _bootstrapHostBuilder.RunDefaultCallbacks(Configuration, _hostBuilder);

			Logging = new LoggingBuilder(Services);
			Host = new ConfigureHostBuilder(hostContext, Configuration, Services);

			if (useDefaults)
			{
				// Register required services
				this.ConfigureMauiHandlers(configureDelegate: null);

				this.ConfigureFonts();
				this.ConfigureImageSources();
				this.ConfigureAnimations();
				this.ConfigureCrossPlatformLifecycleEvents();
			}
		}

		/// <summary>
		/// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
		/// </summary>
		public IServiceCollection Services { get; }

		/// <summary>
		/// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
		/// </summary>
		public ConfigurationManager Configuration { get; }

		/// <summary>
		/// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
		/// </summary>
		public ILoggingBuilder Logging { get; }

		/// <summary>
		/// An <see cref="IHostBuilder"/> for configuring host specific properties, but not building.
		/// To build after configuration, call <see cref="Build"/>.
		/// </summary>
		public ConfigureHostBuilder Host { get; }

		/// <summary>
		/// Builds the <see cref="MauiApp"/>.
		/// </summary>
		/// <returns>A configured <see cref="MauiApp"/>.</returns>
		public MauiApp Build()
		{
			// Copy the configuration sources into the final IConfigurationBuilder
			_hostBuilder.ConfigureHostConfiguration(builder =>
			{
				foreach (var source in ((IConfigurationBuilder)Configuration).Sources)
				{
					builder.Sources.Add(source);
				}

				foreach (var kvp in ((IConfigurationBuilder)Configuration).Properties)
				{
					builder.Properties[kvp.Key] = kvp.Value;
				}

				builder.AddInMemoryCollection(new Dictionary<string, string> { { HostDefaults.ApplicationKey, BootstrapHostBuilder.GetDefaultApplicationName() } });
			});

			// This needs to go here to avoid adding the IHostedService that boots the server twice (the GenericWebHostService).
			// Copy the services that were added via WebApplicationBuilder.Services into the final IServiceCollection
			_hostBuilder.ConfigureServices((context, services) =>
			{
				// We've only added services configured by the GenericWebHostBuilder and WebHost.ConfigureWebDefaults
				// at this point. HostBuilder news up a new ServiceCollection in HostBuilder.Build() we haven't seen
				// until now, so we cannot clear these services even though some are redundant because
				// we called ConfigureWebHostDefaults on both the _deferredHostBuilder and _hostBuilder.
				foreach (var s in _services)
				{
					services.Add(s);
				}

				// Drop the reference to the existing collection and set the inner collection
				// to the new one. This allows code that has references to the service collection to still function.
				_services.InnerCollection = services;
			});

			// Run the other callbacks on the final host builder
			Host.RunDeferredCallbacks(_hostBuilder);

			_builtApplication = new MauiApp(_hostBuilder.Build());

			// Make builder.Configuration match the final configuration. To do that
			// we clear the sources and add the built configuration as a source
			((IConfigurationBuilder)Configuration).Sources.Clear();
			Configuration.AddConfiguration(_builtApplication.Configuration);

			// Mark the service collection as read-only to prevent future modifications
			_services.IsReadOnly = true;


			var initServices = _builtApplication.Services.GetServices<IMauiInitializeService>();
			if (initServices != null)
			{
				foreach (var instance in initServices)
				{
					instance.Initialize(_builtApplication.Services);
				}
			}


			return _builtApplication;
		}

		private class LoggingBuilder : ILoggingBuilder
		{
			public LoggingBuilder(IServiceCollection services)
			{
				Services = services;
			}

			public IServiceCollection Services { get; }
		}

		internal static class MauiHostingDefaults
		{
			// NOTE: This is a modified copy of Microsoft.Extensions.Hosting.HostingHostBuilderExtensions.ConfigureDefaults() from
			// https://github.com/dotnet/runtime/blob/8bfb45a83f55e21f48e593c853d48f398379ba04/src/libraries/Microsoft.Extensions.Hosting/src/HostingHostBuilderExtensions.cs#L188
			// The modifications are to remove things related to:
			// - Command line arguments (not relevant in .NET MAUI)
			// - PhysicalFileProvider (brings in references to file system watchers, which are not relevant in .NET MAUI, and disallowed in some app stores)
			// - Browser restrictions for WebAssembly (not relevant in .NET MAUI)
			// - Reading from disk (need to decide best way to do this in .NET MAUI)
			// - Reading env vars (not sure if relevant in .NET MAUI)
			public static IHostBuilder ConfigureDefaults(IHostBuilder builder)
			{
				builder.UseContentRoot(Directory.GetCurrentDirectory());

				// TODO: Consider whether we want env vars usage here
				// See https://github.com/dotnet/maui/issues/2270 for discussion on suitable default configuration
				// Also see https://github.com/dotnet/runtime/issues/58156 for a bug in .NET on iOS

				//builder.ConfigureHostConfiguration(config =>
				//{
				//	config.AddEnvironmentVariables(prefix: "DOTNET_");
				//});


				// TODO: Consider whether we want to read files from "disk" and the best way to do that on each platform
				// See https://github.com/dotnet/maui/issues/2270 for discussion on suitable default configuration

				//builder.ConfigureAppConfiguration((hostingContext, config) =>
				//{
				//	IHostEnvironment env = hostingContext.HostingEnvironment;

				//	config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				//			.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);

				//	if (env.IsDevelopment() && env.ApplicationName is { Length: > 0 })
				//	{
				//		var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
				//		if (appAssembly is not null)
				//		{
				//			config.AddUserSecrets(appAssembly, optional: true, reloadOnChange: false);
				//		}
				//	}

				//	config.AddEnvironmentVariables();
				//});

				builder
					.ConfigureLogging((hostingContext, logging) =>
					{
						bool isWindows =
#if NET6_0_OR_GREATER
						OperatingSystem.IsWindows();
#else
						RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

						// IMPORTANT: This needs to be added *before* configuration is loaded, this lets
						// the defaults be overridden by the configuration.
						if (isWindows)
						{
							// Default the EventLogLoggerProvider to warning or above
							logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
						}

						logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
						logging.AddConsole();
						logging.AddDebug();
						logging.AddEventSourceLogger();

						if (isWindows)
						{
							// Add the EventLogLoggerProvider on windows machines
							logging.AddEventLog();
						}

						logging.Configure(options =>
						{
							options.ActivityTrackingOptions =
								ActivityTrackingOptions.SpanId |
								ActivityTrackingOptions.TraceId |
								ActivityTrackingOptions.ParentId;
						});
					})
					.UseDefaultServiceProvider((context, options) =>
					{
						bool isDevelopment = context.HostingEnvironment.IsDevelopment();
						options.ValidateScopes = isDevelopment;
						options.ValidateOnBuild = isDevelopment;
					});

				return builder;
			}
		}
	}
}
