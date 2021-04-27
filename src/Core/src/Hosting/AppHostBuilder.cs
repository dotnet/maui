#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public class AppHostBuilder : IAppHostBuilder
	{
		readonly Dictionary<Type, List<Action<HostBuilderContext, IMauiServiceBuilder>>> _configureServiceBuilderActions = new Dictionary<Type, List<Action<HostBuilderContext, IMauiServiceBuilder>>>();
		readonly List<IMauiServiceBuilder> _configureServiceBuilderInstances = new List<IMauiServiceBuilder>();
		readonly List<Action<IConfigurationBuilder>> _configureHostConfigActions = new List<Action<IConfigurationBuilder>>();
		readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigActions = new List<Action<HostBuilderContext, IConfigurationBuilder>>();
		readonly List<Action<HostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();
		readonly List<IConfigureContainerAdapter> _configureContainerActions = new List<IConfigureContainerAdapter>();
		readonly Func<IServiceCollection> _serviceCollectionFactory = new Func<IServiceCollection>(() => new MauiServiceCollection());

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
			// This is here just to make sure that the IMauiHandlersServiceProvider gets registered.
			this.ConfigureMauiHandlers(handlers => { });
		}

		public IDictionary<object, object> Properties => new Dictionary<object, object>();

		public IAppHost Build()
		{
			if (_hostBuilt)
				throw new InvalidOperationException("Build can only be called once.");

			_hostBuilt = true;

			// the order is important here
			BuildHostConfiguration();
			CreateHostingEnvironment();
			CreateHostBuilderContext();
			BuildAppConfiguration();

			_services = _serviceCollectionFactory();
			if (_services == null)
				throw new InvalidOperationException("The ServiceCollection cannot be null");

			BuildServiceCollections(_services);
			BuildServices(_services);

			_services.TryAddSingleton<ILoggerFactory, FallbackLoggerFactory>();

			_serviceProvider = ConfigureContainerAndGetProvider(_services);
			if (_serviceProvider == null)
				throw new InvalidOperationException($"The IServiceProviderFactory returned a null IServiceProvider.");

			ConfigureServiceCollectionBuilders(_serviceProvider);

			return new Internal.AppHost(_serviceProvider, null);
		}

		public IAppHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			_configureAppConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

		public IAppHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
		{
			_configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate))));
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

		public IAppHostBuilder ConfigureServices<TBuilder>(Action<HostBuilderContext, TBuilder> configureDelegate)
			where TBuilder : IMauiServiceBuilder, new()
		{
			_ = configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate));

			var key = typeof(TBuilder);
			if (!_configureServiceBuilderActions.TryGetValue(key, out var list))
			{
				list = new List<Action<HostBuilderContext, IMauiServiceBuilder>>();
				_configureServiceBuilderActions.Add(key, list);
			}

			list.Add((context, builder) => configureDelegate(context, (TBuilder)builder));

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

		void BuildServices(IServiceCollection services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (_hostBuilderContext == null)
				throw new InvalidOperationException($"The HostBuilderContext was not set.");

			if (_appConfiguration != null)
				services.AddSingleton(_appConfiguration);

			foreach (Action<HostBuilderContext, IServiceCollection> configureServicesAction in _configureServicesActions)
			{
				configureServicesAction(_hostBuilderContext, services);
			}
		}

		void BuildAppConfiguration()
		{
			if (_hostBuilderContext == null)
				throw new InvalidOperationException($"The HostBuilderContext was not set.");

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
			if (_hostBuilderContext == null)
				throw new InvalidOperationException($"The HostBuilderContext was not set.");

			object containerBuilder = _serviceProviderFactory.CreateBuilder(services);

			foreach (IConfigureContainerAdapter containerAction in _configureContainerActions)
			{
				containerAction.ConfigureContainer(_hostBuilderContext, containerBuilder);
			}

			return _serviceProviderFactory.CreateServiceProvider(containerBuilder);
		}

		void BuildServiceCollections(IServiceCollection? services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (_hostBuilderContext == null)
				throw new InvalidOperationException($"The HostBuilderContext was not set.");

			foreach (var pair in _configureServiceBuilderActions)
			{
				var instance = (IMauiServiceBuilder)Activator.CreateInstance(pair.Key)!;

				foreach (var action in pair.Value)
				{
					action(_hostBuilderContext, instance);
				}

				instance.ConfigureServices(_hostBuilderContext, services);

				_configureServiceBuilderInstances.Add(instance);
			}
		}

		void ConfigureServiceCollectionBuilders(IServiceProvider? serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (_hostBuilderContext == null)
				throw new InvalidOperationException($"The HostBuilderContext was not set.");

			foreach (var instance in _configureServiceBuilderInstances)
			{
				instance.Configure(_hostBuilderContext, serviceProvider);
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