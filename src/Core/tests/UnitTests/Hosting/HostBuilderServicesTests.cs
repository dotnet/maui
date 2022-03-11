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
			var builder = MauiApp.CreateBuilder();
			var mauiApp = builder.Build();

			Assert.NotNull(mauiApp.Services);
		}

		[Fact]
		public void GetServiceThrowsWhenConstructorParamTypesWereNotRegistered()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooBarService, FooBarService>();
			var mauiApp = builder.Build();

			Assert.Throws<InvalidOperationException>(() => mauiApp.Services.GetService<IFooBarService>());
		}

		[Fact]
		public void GetServiceThrowsWhenNoPublicConstructors()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, BadFooService>();
			var mauiApp = builder.Build();

			var ex = Assert.Throws<InvalidOperationException>(() => mauiApp.Services.GetService<IFooService>());
			Assert.Contains("suitable constructor", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void GetServiceHandlesFirstOfMultipleConstructors()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IFooBarService, FooDualConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var foobar = Assert.IsType<FooDualConstructor>(service);
			Assert.IsType<FooService>(foobar.Foo);
		}

		[Fact]
		public void GetServiceHandlesSecondOfMultipleConstructors()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<IFooBarService, FooDualConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var foobar = Assert.IsType<FooDualConstructor>(service);
			Assert.IsType<BarService>(foobar.Bar);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_DefaultWithNothing()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.Null(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("()", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_DefaultWithBar()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.Null(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("()", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_Foo()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("(Foo)", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_FooWithCat()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<ICatService, CatService>();
			builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.Null(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("(Foo)", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_FooBar()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.IsType<BarService>(trio.Bar);
			Assert.Null(trio.Cat);
			Assert.Equal("(Foo, Bar)", trio.Option);
		}

		[Fact]
		public void GetServiceHandlesUsesCorrectCtor_FooBarCat()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<ICatService, CatService>();
			builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var trio = Assert.IsType<FooTrioConstructor>(service);
			Assert.IsType<FooService>(trio.Foo);
			Assert.IsType<BarService>(trio.Bar);
			Assert.IsType<CatService>(trio.Cat);
			Assert.Equal("(Foo, Bar, Cat)", trio.Option);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveConstructorParams()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<IFooBarService, FooBarService>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();

			var foobar = Assert.IsType<FooBarService>(service);
			Assert.IsType<FooService>(foobar.Foo);
			Assert.IsType<BarService>(foobar.Bar);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveUnregisteredConstructorParamsButHaveDefaultValues()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooBarService, FooDefaultValueConstructor>();
			var mauiApp = builder.Build();

			var foo = mauiApp.Services.GetService<IFooBarService>();

			Assert.NotNull(foo);

			var actual = Assert.IsType<FooDefaultValueConstructor>(foo);

			Assert.Null(actual.Bar);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveRegisteredConstructorParamsAndHaveDefaultValues()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IBarService, BarService>();
			builder.Services.AddTransient<IFooBarService, FooDefaultValueConstructor>();
			var mauiApp = builder.Build();

			var foo = mauiApp.Services.GetService<IFooBarService>();

			Assert.NotNull(foo);

			var actual = Assert.IsType<FooDefaultValueConstructor>(foo);

			Assert.NotNull(actual.Bar);
			Assert.IsType<BarService>(actual.Bar);
		}

		[Fact]
		public void GetServiceCanReturnTypesThatHaveSystemDefaultValues()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooBarService, FooDefaultSystemValueConstructor>();
			var mauiApp = builder.Build();

			var foo = mauiApp.Services.GetService<IFooBarService>();

			Assert.NotNull(foo);

			var actual = Assert.IsType<FooDefaultSystemValueConstructor>(foo);

			Assert.Equal("Default Value", actual.Text);
		}

		[Fact]
		public void GetServiceCanReturnEnumerableParams()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IFooService, FooService2>();
			builder.Services.AddTransient<IFooBarService, FooEnumerableService>();
			var mauiApp = builder.Build();

			var service = mauiApp.Services.GetService<IFooBarService>();
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
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			var mauiApp = builder.Build();

			AssertTransient<IFooService, FooService>(mauiApp.Services);
		}

		[Fact]
		public void WillRetrieveSameSingletonServices()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IFooService, FooService>();
			var mauiApp = builder.Build();

			AssertSingleton<IFooService, FooService>(mauiApp.Services);
		}

		[Fact]
		public void WillRetrieveMixedServices()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IFooService, FooService>();
			builder.Services.AddTransient<IBarService, BarService>();
			var mauiApp = builder.Build();

			AssertSingleton<IFooService, FooService>(mauiApp.Services);
			AssertTransient<IBarService, BarService>(mauiApp.Services);
		}

		[Fact]
		public void WillRetrieveEnumerables()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IFooService, FooService>();
			builder.Services.AddTransient<IFooService, FooService2>();
			var mauiApp = builder.Build();

			var fooServices = mauiApp.Services
				.GetServices<IFooService>()
				.ToArray();
			Assert.Equal(2, fooServices.Length);

			var serviceTypes = fooServices
				.Select(s => s.GetType().FullName)
				.ToArray();
			Assert.Contains(typeof(FooService).FullName, serviceTypes);
			Assert.Contains(typeof(FooService2).FullName, serviceTypes);
		}

		[Fact]
		public void CanCreateLogger()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddLogging(logging => logging.AddConsole());
			var mauiApp = builder.Build();

			var factory = mauiApp.Services.GetRequiredService<ILoggerFactory>();

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