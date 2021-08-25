using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

			_bootstrapHostBuilder.ConfigureHostConfiguration(config =>
			{
				// Disable reloading config on change so we don't use up system file watchers, which are a limited resource
				// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0#disable-app-configuration-reload-on-change
				config.AddInMemoryCollection(new Dictionary<string, string> { { "hostBuilder:reloadConfigOnChange", "false" } });
			});

			// Don't specify the args here since we want to apply them later so that args
			// can override the defaults specified by ConfigureWebHostDefaults
			_bootstrapHostBuilder.ConfigureDefaults(args: null);

			_bootstrapHostBuilder.ConfigureHostConfiguration(config =>
			{
				// TODO: This has no more code left. Delete?
			});

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

				// Add any services to the user visible service collection so that they are observable
				// just in case users capture the Services property. Orchard does this to get a "blueprint"
				// of the service collection

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
	}
}
