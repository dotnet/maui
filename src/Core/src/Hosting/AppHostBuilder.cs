using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public class AppHostBuilder
		: IAppHostBuilder
	{
		readonly List<Action<HostBuilderContext, IServiceCollection>> _configureHandlersActions = new List<Action<HostBuilderContext, IServiceCollection>>();
		readonly List<Action<IConfigurationBuilder>> _configureHostConfigActions = new List<Action<IConfigurationBuilder>>();
		readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigActions = new List<Action<HostBuilderContext, IConfigurationBuilder>>();
		readonly List<Action<HostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();
		readonly List<Action<HostBuilderContext, IFontCollection>> _configureFontsActions = new List<Action<HostBuilderContext, IFontCollection>>();
		readonly List<IConfigureContainerAdapter> _configureContainerActions = new List<IConfigureContainerAdapter>();
		readonly Func<IServiceCollection> _serviceColectionFactory = new Func<IServiceCollection>(() => new MauiServiceCollection());

		IServiceFactoryAdapter _serviceProviderFactory = new ServiceFactoryAdapter<IServiceCollection>(new MauiServiceProviderFactory(false));

		bool _hostBuilt;
		HostBuilderContext? _hostBuilderContext;
		IHostEnvironment? _hostEnvironment;
		IServiceProvider? _serviceProvider;
		IServiceCollection? _services;
		IConfiguration? _hostConfiguration;
		IConfiguration? _appConfiguration;

		public AppHostBuilder()
		{

		}
		public IDictionary<object, object> Properties => new Dictionary<object, object>();

		public static IAppHostBuilder CreateDefaultAppBuilder()
		{
			var builder = new AppHostBuilder();

			builder.UseMauiHandlers();
			builder.UseFonts();

			return builder;
		}

		public IAppHost Build()
		{
			_services = _serviceColectionFactory();

			if (_hostBuilt)
				throw new InvalidOperationException("Build can only be called once.");

			_hostBuilt = true;

			// the order is important here
			BuildHostConfiguration();
			CreateHostingEnvironment();
			CreateHostBuilderContext();
			BuildAppConfiguration();

			if (_services == null)
				throw new InvalidOperationException("The ServiceCollection cannot be null");

			ConfigureHandlers(_services);
			CreateServiceProvider(_services);

			if (_serviceProvider == null)
				throw new InvalidOperationException($"The ServiceProvider cannot be null");

			BuildFontRegistrar(_serviceProvider);

			return new AppHost(_serviceProvider, null);
		}

		public IAppHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			_configureAppConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

		public IAppHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
		{
			_configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate
			 ?? throw new ArgumentNullException(nameof(configureDelegate))));
			return this;
		}

		public IAppHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
		{
			_configureHostConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

		public IAppHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
		{
			_configureServicesActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

		public IAppHostBuilder ConfigureFonts(Action<HostBuilderContext, IFontCollection> configureDelegate)
		{
			_configureFontsActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

		public IAppHostBuilder ConfigureHandlers(Action<HostBuilderContext, IServiceCollection> configureDelegate)
		{
			_configureHandlersActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
		public IAppHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
		{
			_serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(factory ?? throw new ArgumentNullException(nameof(factory)));
			return this;
		}

		public IAppHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
		{
			_serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(() => _hostBuilderContext!, factory ?? throw new ArgumentNullException(nameof(factory)));

			return this;
		}
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint

		void BuildHostConfiguration()
		{
			var configBuilder = new ConfigurationBuilder();
			foreach (var buildAction in _configureHostConfigActions)
			{
				buildAction(configBuilder);
			}
			_hostConfiguration = configBuilder.Build();
		}

		void CreateHostingEnvironment()
		{
			_hostEnvironment = new AppHostEnvironment()
			{
				//ApplicationName = _hostConfiguration[HostDefaults.ApplicationKey],
				//EnvironmentName = _hostConfiguration[HostDefaults.EnvironmentKey] ?? Environments.Production,
				//ContentRootPath = ResolveContentRootPath(_hostConfiguration[HostDefaults.ContentRootKey], AppContext.BaseDirectory),
			};

			if (string.IsNullOrEmpty(_hostEnvironment.ApplicationName))
			{
				// Note GetEntryAssembly returns null for the net4x console test runner.
				_hostEnvironment.ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name;
			}
		}

		void CreateHostBuilderContext()
		{
			_hostBuilderContext = new HostBuilderContext(Properties)
			{
				HostingEnvironment = _hostEnvironment,
			};
		}

		void CreateServiceProvider(IServiceCollection services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			if (_appConfiguration != null)
				services.AddSingleton(_appConfiguration);

			foreach (Action<HostBuilderContext, IServiceCollection> configureServicesAction in _configureServicesActions)
			{
				if (_hostBuilderContext != null)
					configureServicesAction(_hostBuilderContext, services);
			}

			_serviceProvider = ConfigureContainerAndGetProvider(services);

			if (_serviceProvider == null)
			{
				throw new InvalidOperationException($"The IServiceProviderFactory returned a null IServiceProvider.");
			}
		}

		void BuildAppConfiguration()
		{
			if (_hostBuilderContext == null)
				return;

			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddConfiguration(_hostConfiguration);
			foreach (var buildAction in _configureAppConfigActions)
			{
				buildAction(_hostBuilderContext, configBuilder);
			}
			_appConfiguration = configBuilder.Build();

			_hostBuilderContext.Configuration = _appConfiguration;
		}

		IServiceProvider ConfigureContainerAndGetProvider(IServiceCollection services)
		{
			object containerBuilder = _serviceProviderFactory.CreateBuilder(services);

			foreach (IConfigureContainerAdapter containerAction in _configureContainerActions)
			{
				if (_hostBuilderContext != null)
					containerAction.ConfigureContainer(_hostBuilderContext, containerBuilder);
			}

			return _serviceProviderFactory.CreateServiceProvider(containerBuilder);
		}

		void ConfigureHandlers(IServiceCollection? services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			//we need to use our own ServiceCollction because the default ServiceCollection
			//enforces the instance to implement the servicetype
			var _handlersCollection = new MauiServiceCollection();
			foreach (var configureHandlersAction in _configureHandlersActions)
			{
				if (_hostBuilderContext != null)
					configureHandlersAction(_hostBuilderContext, _handlersCollection);
			}
			services.AddSingleton(_handlersCollection.BuildHandlersServiceProvider());
		}

		void BuildFontRegistrar(IServiceProvider? serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			var fontCollection = new FontCollection();
			foreach (var action in _configureFontsActions)
			{
				if (_hostBuilderContext != null)
					action(_hostBuilderContext, fontCollection);
			}

			var fontRegistrar = serviceProvider.GetRequiredService<IFontRegistrar>();
			foreach (var font in fontCollection)
			{
				fontRegistrar.Register(font.Filename, font.Alias);
			}
		}

		IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
		{
			return ConfigureHostConfiguration(configureDelegate);
		}

		IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			return ConfigureAppConfiguration(configureDelegate);
		}

		IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
		{
			return ConfigureServices(configureDelegate);
		}

		IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
		{
			return UseServiceProviderFactory(factory);
		}

		IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
		{
			return UseServiceProviderFactory<TContainerBuilder>(factory);
		}

		IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
		{
			return ConfigureContainer<TContainerBuilder>(configureDelegate);
		}

		IHost IHostBuilder.Build()
		{
			return Build();
		}
	}
}