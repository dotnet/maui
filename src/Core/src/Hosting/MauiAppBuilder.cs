using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A builder for .NET MAUI cross-platform applications and services.
	/// </summary>
	public sealed class MauiAppBuilder : IHostApplicationBuilder
	{
		private readonly ServiceCollection _services = new();
		private Func<IServiceProvider>? _createServiceProvider;
		private readonly Lazy<ConfigurationManager> _configuration;
		private readonly Lazy<MauiHostEnvironment> _hostEnvironment;
		private readonly Lazy<MauiMetricsBuilder> _metricsBuilder;
		private ILoggingBuilder? _logging;
		private IDictionary<object, object> _properties;

		internal MauiAppBuilder(bool useDefaults)
		{
			// Lazy-load these classes, so they aren't created if they are never used.
			// Don't capture the 'this' variable in AddSingleton, so MauiAppBuilder can be GC'd.
			var configuration = new Lazy<ConfigurationManager>(() => new ConfigurationManager());
			var hostEnvironment = new Lazy<MauiHostEnvironment>(() => new MauiHostEnvironment());
			var metricsBuilder = new Lazy<MauiMetricsBuilder>(() => new MauiMetricsBuilder(Services));
			Services.AddSingleton<IConfiguration>(sp => configuration.Value);
			Services.AddSingleton<IHostEnvironment>(sp => hostEnvironment.Value);
			Services.AddSingleton<IMetricsBuilder>(sp => metricsBuilder.Value);

			_configuration = configuration;
			_hostEnvironment = hostEnvironment;
			_metricsBuilder = metricsBuilder;

			_properties = new Dictionary<object, object>();

			if (useDefaults)
			{
				// Register required services
				this.ConfigureMauiHandlers(configureDelegate: null);

				this.ConfigureFonts();
				this.ConfigureImageSources();
				this.ConfigureAnimations();
				this.ConfigureCrossPlatformLifecycleEvents();
				this.ConfigureWindowEvents();
				this.ConfigureDispatching();
				this.ConfigureEnvironmentVariables();
				this.ConfigureMauiDiagnostics();

				this.UseEssentials();

#if WINDOWS
				this.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiCoreInitializer>());
#endif
			}
		}

		class MauiCoreInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
#if WINDOWS
				// WORKAROUND: use the MAUI dispatcher instead of the OS dispatcher to
				// avoid crashing: https://github.com/microsoft/WindowsAppSDK/issues/2451
				var dispatcher = services.GetRequiredApplicationDispatcher();
				if (dispatcher.IsDispatchRequired)
					dispatcher.Dispatch(() => SetupResources());
				else
					SetupResources();

				static void SetupResources()
				{
					if (UI.Xaml.Application.Current?.Resources is not UI.Xaml.ResourceDictionary resources)
						return;

					// WinUI
					resources.AddLibraryResources<UI.Xaml.Controls.XamlControlsResources>();

					// Microsoft.Maui
					resources.AddLibraryResources("MicrosoftMauiCoreIncluded", "ms-appx:///Microsoft.Maui/Platform/Windows/Styles/Resources.xbf");
				}
#endif
			}
		}

		/// <summary>
		/// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
		/// </summary>
		public IServiceCollection Services => _services;

		/// <summary>
		/// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
		/// </summary>
		public ConfigurationManager Configuration => _configuration.Value;

		IConfigurationManager IHostApplicationBuilder.Configuration => Configuration;

		/// <summary>
		/// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
		/// </summary>
		public ILoggingBuilder Logging
		{
			get
			{
				return _logging ??= InitializeLogging();

				ILoggingBuilder InitializeLogging()
				{
					// if someone accesses the Logging builder, ensure Logging has been initialized.
					Services.AddLogging();
					return new LoggingBuilder(Services);
				}
			}
		}

		/// <summary>
		/// Gets a central location for sharing state between components during the host building process.
		/// </summary>
		public IDictionary<object, object> Properties => _properties;

		IDictionary<object, object> IHostApplicationBuilder.Properties => Properties;

		/// <summary>
		/// Information about the environment an application is running in.
		/// </summary>
		public MauiHostEnvironment Environment => _hostEnvironment.Value;

		IHostEnvironment IHostApplicationBuilder.Environment => Environment;

		/// <summary>
		/// Gets a builder for configuring metrics services.
		/// </summary>
		internal MauiMetricsBuilder Metrics => _metricsBuilder.Value;

		IMetricsBuilder IHostApplicationBuilder.Metrics => Metrics;

		/// <summary>
		/// Registers a <see cref="IServiceProviderFactory{TBuilder}" /> instance to be used to create the <see cref="IServiceProvider" />.
		/// </summary>
		/// <param name="factory">The <see cref="IServiceProviderFactory{TBuilder}" />.</param>
		/// <param name="configure">
		/// A delegate used to configure the <typeparamref T="TBuilder" />. This can be used to configure services using
		/// APIS specific to the <see cref="IServiceProviderFactory{TBuilder}" /> implementation.
		/// </param>
		/// <typeparam name="TBuilder">The type of builder provided by the <see cref="IServiceProviderFactory{TBuilder}" />.</typeparam>
		/// <remarks>
		/// <para>
		/// <see cref="ConfigureContainer{TBuilder}(IServiceProviderFactory{TBuilder}, Action{TBuilder})"/> is called by <see cref="Build"/>
		/// and so the delegate provided by <paramref name="configure"/> will run after all other services have been registered.
		/// </para>
		/// <para>
		/// Multiple calls to <see cref="ConfigureContainer{TBuilder}(IServiceProviderFactory{TBuilder}, Action{TBuilder})"/> will replace
		/// the previously stored <paramref name="factory"/> and <paramref name="configure"/> delegate.
		/// </para>
		/// </remarks>
		public void ConfigureContainer<TBuilder>(IServiceProviderFactory<TBuilder> factory, Action<TBuilder>? configure = null) where TBuilder : notnull
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			_createServiceProvider = () =>
			{
				var container = factory.CreateBuilder(Services);
				configure?.Invoke(container);
				return factory.CreateServiceProvider(container);
			};
		}

		/// <summary>
		/// Builds the <see cref="MauiApp"/>.
		/// </summary>
		/// <returns>A configured <see cref="MauiApp"/>.</returns>
		public MauiApp Build()
		{
			ConfigureDefaultLogging();

			IServiceProvider serviceProvider = _createServiceProvider != null
				? _createServiceProvider()
				: _services.BuildServiceProvider();

			// Mark the service collection as read-only to prevent future modifications
			_services.MakeReadOnly();

			MauiApp builtApplication = new MauiApp(serviceProvider);

			// Initialize any singleton/app services, for example the OS hooks
			builtApplication.InitializeAppServices();

			return builtApplication;
		}

		private sealed class LoggingBuilder : ILoggingBuilder
		{
			public LoggingBuilder(IServiceCollection services)
			{
				Services = services;
			}

			public IServiceCollection Services { get; }
		}

		private void ConfigureDefaultLogging()
		{
			// By default, if no one else has configured logging, add a "no-op" LoggerFactory
			// and Logger services with no providers. This way when components try to get an
			// ILogger<> from the IServiceProvider, they don't get 'null'.
			Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, NullLoggerFactory>());
			Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(NullLogger<>)));
		}

		private sealed class NullLoggerFactory : ILoggerFactory
		{
			public void AddProvider(ILoggerProvider provider) { }

			public ILogger CreateLogger(string categoryName) => NullLogger.Instance;

			public void Dispose() { }
		}

		private sealed class NullLogger<T> : ILogger<T>, IDisposable
		{
			public IDisposable BeginScope<TState>(TState state) where TState : notnull => this;

			public void Dispose() { }

			public bool IsEnabled(LogLevel logLevel) => false;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			{
			}
		}
	}
}
