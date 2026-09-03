using System;
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
	}
}