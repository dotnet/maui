using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderServicesTests
	{
		[Fact]
		public void CanGetServices()
		{
			var host = new AppHostBuilder()
				.Build();

			Assert.NotNull(host);
			Assert.NotNull(host.Services);
		}

		[Fact]
		public void GetServiceThrowsWhenConstructorParamTypesWereNotRegistered()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) => services.AddTransient<IFooBarService, FooBarService>())
				.Build();

			Assert.Throws<InvalidOperationException>(() => host.Services.GetService<IFooBarService>());
		}

		[Fact]
		public void GetServiceThrowsWhenNoPublicConstructors()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) => services.AddTransient<IFooService, BadFooService>())
				.Build();

			var ex = Assert.Throws<InvalidOperationException>(() => host.Services.GetService<IFooService>());
			Assert.Contains("public or internal constructors", ex.Message);
		}

		[Fact]
		public void GetServiceHandlesFirstOfMultipleConstructors()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IFooBarService, FooDualConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var foobar = Assert.IsType<FooDualConstructor>(service);
			Assert.IsType<FooService>(foobar.Foo);
		}

		[Fact]
		public void GetServiceHandlesSecondOfMultipleConstructors()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IBarService, BarService>();
					services.AddTransient<IFooBarService, FooDualConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var foobar = Assert.IsType<FooDualConstructor>(service);
			Assert.IsType<BarService>(foobar.Bar);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_DefaultWithNothing()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooBarService, FooTrioConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.Null(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("()", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_DefaultWithBar()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IBarService, BarService>();
					services.AddTransient<IFooBarService, FooTrioConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.Null(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("()", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_Foo()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IFooBarService, FooTrioConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("(Foo)", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_FooWithCat()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<ICatService, CatService>();
					services.AddTransient<IFooBarService, FooTrioConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("(Foo)", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_FooBar()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IBarService, BarService>();
					services.AddTransient<IFooBarService, FooTrioConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.IsType<BarService>(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("(Foo, Bar)", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_FooBarCat()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IBarService, BarService>();
					services.AddTransient<ICatService, CatService>();
					services.AddTransient<IFooBarService, FooTrioConstructor>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.IsType<BarService>(trio.Bar);
			Assert.IsType<CatService>(trio.Cat);
			Assert.Equal("(Foo, Bar, Cat)", trio.Option);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveConstructorParams()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IBarService, BarService>();
					services.AddTransient<IFooBarService, FooBarService>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();

			var foobar = Assert.IsType<FooBarService>(service);
			Assert.IsType<FooService>(foobar.Foo);
			Assert.IsType<BarService>(foobar.Bar);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveUnregisteredConstructorParamsButHaveDefaultValues()
		{
			var host = new AppHostBuilder()
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
			var host = new AppHostBuilder()
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
			var host = new AppHostBuilder()
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
		public void GetServiceCanReturnEnumerableParams()
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(true)
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IFooService, FooService2>();
					services.AddTransient<IFooBarService, FooEnumerableService>();
				})
				.Build();

			var service = host.Services.GetService<IFooBarService>();
			var foobar = Assert.IsType<FooEnumerableService>(service);

			var serviceTypes = foobar.Foos
				.Select(s => s.GetType().FullName)
				.ToArray();
			Assert.Contains(typeof(FooService).FullName, serviceTypes);
			Assert.Contains(typeof(FooService2).FullName, serviceTypes);
		}

		[Fact]
		public void WillRetrieveDifferentTransientServices()
		{
			var host = new AppHostBuilder()
				.ConfigureServices((ctx, services) => services.AddTransient<IFooService, FooService>())
				.Build();

			AssertTransient<IFooService, FooService>(host.Services);
		}

		[Fact]
		public void WillRetrieveSameSingletonServices()
		{
			var host = new AppHostBuilder()
				.ConfigureServices((ctx, services) => services.AddSingleton<IFooService, FooService>())
				.Build();

			AssertSingleton<IFooService, FooService>(host.Services);
		}

		[Fact]
		public void WillRetrieveMixedServices()
		{
			var host = new AppHostBuilder()
				.ConfigureServices((ctx, services) =>
				{
					services.AddSingleton<IFooService, FooService>();
					services.AddTransient<IBarService, BarService>();
				})
				.Build();

			AssertSingleton<IFooService, FooService>(host.Services);
			AssertTransient<IBarService, BarService>(host.Services);
		}

		[Fact]
		public void WillRetrieveEnumerables()
		{
			var host = new AppHostBuilder()
				.ConfigureServices((ctx, services) =>
				{
					services.AddTransient<IFooService, FooService>();
					services.AddTransient<IFooService, FooService2>();
				})
				.Build();

			var services = host.Services
				.GetServices<IFooService>()
				.ToArray();
			Assert.Equal(2, services.Length);

			var serviceTypes = services
				.Select(s => s.GetType().FullName)
				.ToArray();
			Assert.Contains(typeof(FooService).FullName, serviceTypes);
			Assert.Contains(typeof(FooService2).FullName, serviceTypes);
		}

		[Theory]
		//[InlineData(true)] // TODO: The MAUI provider does not support generic args
		[InlineData(false)]
		public void CanCreateLogger(bool ctorInjection)
		{
			var host = new AppHostBuilder()
				.UseMauiServiceProviderFactory(ctorInjection)
				.ConfigureServices((ctx, services) =>
				{
					services.AddLogging(logging => logging.AddConsole());
				})
				.Build();

			var factory = host.Services.GetRequiredService<ILoggerFactory>();

			var logger = factory.CreateLogger<HostBuilderServicesTests>();

			Assert.NotNull(logger);
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