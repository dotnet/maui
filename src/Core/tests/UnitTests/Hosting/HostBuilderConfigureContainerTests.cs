using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderConfigureContainerTests
	{
		[Fact]
		public void CreatesServiceProviderByDefault()
		{
			var builder = MauiApp.CreateBuilder();
			var mauiApp = builder.Build();

			Assert.IsType<ServiceProvider>(mauiApp.Services);
		}

		[Fact]
		public void ConfigureContainerThrowArgNull()
		{
			var builder = MauiApp.CreateBuilder();
			Assert.Throws<ArgumentNullException>(() => builder.ConfigureContainer<IServiceCollection>(null));
		}

		[Fact]
		public void ConfigureContainerCanReplaceIServiceProvider()
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);

			builder.ConfigureContainer(
				new MyServiceProviderFactory(),
				builder => builder.Configured = true);

			var mauiApp = builder.Build();
			Assert.IsType<MyServiceProvider>(mauiApp.Services);
			Assert.True(((MyServiceProvider)mauiApp.Services).Builder.Configured);
		}

		[Fact]
		public void AppCleanupRunsBeforeConfigurationIsDisposed()
		{
			var (app, configuration, cleanup) = BuildAppWithTrackedConfiguration();

			app.Dispose();

			Assert.True(cleanup.WasCalled);
			Assert.True(configuration.IsDisposed);
		}

		[Fact]
		public async Task AppCleanupRunsBeforeConfigurationIsDisposedAsync()
		{
			var (app, configuration, cleanup) = BuildAppWithTrackedConfiguration();

			await app.DisposeAsync();

			Assert.True(cleanup.WasCalled);
			Assert.True(configuration.IsDisposed);
		}

		[Fact]
		public void AppCleanupRunsAllServicesAndAggregatesFailures()
		{
			var executionOrder = new List<int>();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					executionOrder.Add(1);
					throw new InvalidOperationException("first cleanup failed");
				}));
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() => executionOrder.Add(2)));
			builder.Services.AddSingleton<IMauiAppCleanupService>(
				new CallbackCleanup(() =>
				{
					executionOrder.Add(3);
					throw new InvalidOperationException("third cleanup failed");
				}));

			var aggregate = Assert.Throws<AggregateException>(() => builder.Build().Dispose());

			Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
			Assert.Collection(
				aggregate.InnerExceptions,
				ex => Assert.Equal("first cleanup failed", ex.Message),
				ex => Assert.Equal("third cleanup failed", ex.Message));
		}

		[Fact]
		public void MauiAppDisposeDisposesAsyncOnlyServiceProvider()
		{
			var factory = new AsyncOnlyServiceProviderFactory();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(factory);

			var app = builder.Build();
			app.Dispose();

			Assert.True(factory.Provider?.IsDisposed == true);
		}

		[Fact]
		public void BuildFailureDisposesAsyncOnlyServiceProvider()
		{
			var factory = new AsyncOnlyServiceProviderFactory();
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(factory);
			builder.Services.AddSingleton<IMauiInitializeService, ThrowingInitializeService>();

			var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

			Assert.Equal("initialization failed", exception.Message);
			Assert.True(factory.Provider?.IsDisposed == true);
		}

		static (MauiApp App, DisposableConfigurationProvider Configuration, ConfigurationReadingCleanup Cleanup)
			BuildAppWithTrackedConfiguration()
		{
			var configuration = new DisposableConfigurationProvider();
			var cleanup = new ConfigurationReadingCleanup(configuration);
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			((IConfigurationBuilder)builder.Configuration).Add(new DisposableConfigurationSource(configuration));
			builder.Services.AddSingleton<IMauiAppCleanupService>(cleanup);

			return (builder.Build(), configuration, cleanup);
		}

		private class MyServiceProviderFactory : IServiceProviderFactory<MyServiceBuilder>
		{
			public MyServiceBuilder CreateBuilder(IServiceCollection services) => new MyServiceBuilder(services);

			public IServiceProvider CreateServiceProvider(MyServiceBuilder containerBuilder) => new MyServiceProvider(containerBuilder);
		}

		private class MyServiceBuilder
		{
			public bool Configured { get; set; }
			public IServiceCollection Services { get; }

			public MyServiceBuilder(IServiceCollection services)
			{
				Services = services;
			}
		}

		private class MyServiceProvider : IServiceProvider
		{
			private readonly ServiceProvider _serviceProvider;

			public MyServiceBuilder Builder { get; set; }

			public MyServiceProvider(MyServiceBuilder builder)
			{
				Builder = builder;
				_serviceProvider = builder.Services.BuildServiceProvider();
			}

			public object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);
		}

		sealed class AsyncOnlyServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
		{
			public AsyncOnlyServiceProvider? Provider { get; private set; }

			public IServiceCollection CreateBuilder(IServiceCollection services) => services;

			public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
			{
				Provider = new AsyncOnlyServiceProvider(containerBuilder.BuildServiceProvider());
				return Provider;
			}
		}

		sealed class AsyncOnlyServiceProvider : IServiceProvider, IAsyncDisposable
		{
			readonly ServiceProvider _innerProvider;

			public AsyncOnlyServiceProvider(ServiceProvider innerProvider)
			{
				_innerProvider = innerProvider;
			}

			public bool IsDisposed { get; private set; }

			public object? GetService(Type serviceType) =>
				_innerProvider.GetService(serviceType);

			public ValueTask DisposeAsync()
			{
				IsDisposed = true;
				return _innerProvider.DisposeAsync();
			}
		}

		sealed class ThrowingInitializeService : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
				throw new InvalidOperationException("initialization failed");
			}
		}

		sealed class ConfigurationReadingCleanup : IMauiAppCleanupService
		{
			readonly DisposableConfigurationProvider _configuration;

			public ConfigurationReadingCleanup(DisposableConfigurationProvider configuration)
			{
				_configuration = configuration;
			}

			public bool WasCalled { get; private set; }

			public void Cleanup()
			{
				if (_configuration.IsDisposed)
					throw new InvalidOperationException("Configuration was disposed before app cleanup.");

				WasCalled = true;
			}
		}

		sealed class CallbackCleanup : IMauiAppCleanupService
		{
			readonly Action _cleanup;

			public CallbackCleanup(Action cleanup)
			{
				_cleanup = cleanup;
			}

			public void Cleanup() => _cleanup();
		}

		sealed class DisposableConfigurationSource : IConfigurationSource
		{
			readonly DisposableConfigurationProvider _provider;

			public DisposableConfigurationSource(DisposableConfigurationProvider provider)
			{
				_provider = provider;
			}

			public IConfigurationProvider Build(IConfigurationBuilder builder) =>
				_provider;
		}

		sealed class DisposableConfigurationProvider : ConfigurationProvider, IDisposable
		{
			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				IsDisposed = true;
			}
		}
	}
}