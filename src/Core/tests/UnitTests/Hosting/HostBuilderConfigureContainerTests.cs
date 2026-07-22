using System;
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