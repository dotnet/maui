using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A builder for .NET MAUI cross-platform applications and services.
	/// </summary>
	public sealed class MauiAppBuilder
	{
		private readonly MauiApplicationServiceCollection _services = new();
		private Func<IServiceProvider>? _createServiceProvider;
		private MauiApp? _builtApplication;

		internal MauiAppBuilder(bool useDefaults)
		{
			Configuration = new();
			Services.AddSingleton<IConfiguration>(Configuration);

			Logging = new LoggingBuilder(Services);
			// By default, add LoggerFactory and Logger services with no providers. This way
			// when components try to get an ILogger<> from the IServiceProvider, they don't get 'null'.
			Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
			Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

			if (useDefaults)
			{
				// Register required services
				this.ConfigureMauiHandlers(configureDelegate: null);

				this.ConfigureFonts();
				this.ConfigureImageSources();
				this.ConfigureAnimations();
				this.ConfigureCrossPlatformLifecycleEvents();
				this.ConfigureDispatching();

				this.UseEssentials();
			}
		}

		/// <summary>
		/// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
		/// </summary>
		public IServiceCollection Services => _services;

		/// <summary>
		/// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
		/// </summary>
		public ConfigurationManager Configuration { get; }

		/// <summary>
		/// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
		/// </summary>
		public ILoggingBuilder Logging { get; }

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
			IServiceProvider serviceProvider = _createServiceProvider != null
				? _createServiceProvider()
				: _services.BuildServiceProvider();

			_builtApplication = new MauiApp(serviceProvider);

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

		private sealed class LoggingBuilder : ILoggingBuilder
		{
			public LoggingBuilder(IServiceCollection services)
			{
				Services = services;
			}

			public IServiceCollection Services { get; }
		}
	}
}
