using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Tests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderServicesTests
	{
		[Fact]
		public void CanGetServices()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.Build();

			Assert.NotNull(host);
			Assert.NotNull(host.Services);
		}

		[Fact]
		public void GetServiceThrowsWhenConstructorParamTypesWereNotRegistered()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) => services.AddTransient<IFooBarService, FooBarService>())
				.Build();

			Assert.Throws<InvalidOperationException>(() => host.Services.GetService<IFooBarService>());
		}

		[Fact]
		public void GetServiceThrowsOnMultipleConstructors()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) => services.AddTransient<IFooBarService, FooDualConstructor>())
				.Build();

			var ex = Assert.Throws<InvalidOperationException>(() => host.Services.GetService<IFooBarService>());

			Assert.Contains("IFooService", ex.Message);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveConstructorParams()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IBarService, BarService>();
					services.AddTransient<IFooBarService, FooBarService>();
				})
				.Build();

			var foobar = host.Services.GetService<IFooBarService>();

			Assert.NotNull(foobar);
			Assert.IsType<FooBarService>(foobar);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveUnregisteredConstructorParamsButHaveDefaultValues()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooBarService, FooDefaultValueConstructor>();
				})
				.Build();

			var foo = host.Services.GetService<IFooBarService>();

			Assert.NotNull(foo);

			var actual = Assert.IsType<FooDefaultValueConstructor>(foo);

			Assert.Null(actual.Bar);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveRegisteredConstructorParamsAndHaveDefaultValues()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IBarService, BarService>();
					services.AddTransient<IFooBarService, FooDefaultValueConstructor>();
				})
				.Build();

			var foo = host.Services.GetService<IFooBarService>();

			Assert.NotNull(foo);

			var actual = Assert.IsType<FooDefaultValueConstructor>(foo);

			Assert.NotNull(actual.Bar);
			Assert.IsType<BarService>(actual.Bar);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveSystemDefaultValues()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooBarService, FooDefaultSystemValueConstructor>();
				})
				.Build();

			var foo = host.Services.GetService<IFooBarService>();

			Assert.NotNull(foo);

			var actual = Assert.IsType<FooDefaultSystemValueConstructor>(foo);

			Assert.Equal("Default Value", actual.Text);
		}

		[Fact]
		public void WillRetrieveDifferentTransientServices()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.ConfigureServices((ctx, services) => services.AddTransient<IFooService, FooService>())
				.Build();

			AssertTransient<IFooService, FooService>(host.Services);
		}

		[Fact]
		public void WillRetrieveSameSingletonServices()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.ConfigureServices((ctx, services) => services.AddSingleton<IFooService, FooService>())
				.Build();

			AssertSingleton<IFooService, FooService>(host.Services);
		}

		[Fact]
		public void WillRetrieveMixedServices()
		{
			var host = AppHostBuilder
				.CreateDefaultAppBuilder()
				.ConfigureServices((ctx, services) =>
				{
					services.AddSingleton<IFooService, FooService>();
					services.AddTransient<IBarService, BarService>();
				})
				.Build();

			AssertSingleton<IFooService, FooService>(host.Services);
			AssertTransient<IBarService, BarService>(host.Services);
		}

		static void AssertTransient<TInterface, TConcrete>(IServiceProvider services)
		{
			var service1 = services.GetService<TInterface>();

			Assert.NotNull(service1);
			Assert.IsType<TConcrete>(service1);

			var service2 = services.GetService<TInterface>();

			Assert.NotNull(service2);
			Assert.IsType<TConcrete>(service2);

			Assert.NotEqual(service1, service2);
		}

		static void AssertSingleton<TInterface, TConcrete>(IServiceProvider services)
		{
			var service1 = services.GetService<TInterface>();

			Assert.NotNull(service1);
			Assert.IsType<TConcrete>(service1);

			var service2 = services.GetService<TInterface>();

			Assert.NotNull(service2);
			Assert.IsType<TConcrete>(service2);

			Assert.Equal(service1, service2);
		}
	}
}