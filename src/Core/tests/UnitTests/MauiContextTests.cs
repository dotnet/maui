using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Hosting.Internal;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public partial class MauiContextTests
	{
		[Fact]
		public void CloneIncludeSameServices()
		{
			var obj = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(obj);
			var services = new MauiFactory(collection);

			var first = new MauiContext(services);
			var second = new MauiContext(first.Services);

			Assert.Same(obj, second.Services.GetService<TestThing>());
		}

		[Fact]
		public void AddSpecificInstanceOverridesBase()
		{
			var baseObj = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(baseObj);
			var services = new MauiFactory(collection);

			var specificObj = new TestThing();
			var context = new MauiContext(services);
			context.AddSpecific<TestThing>(specificObj);

			Assert.Same(specificObj, context.Services.GetService<TestThing>());
		}

		[Fact]
		public void AddSpecificIsNotWeak()
		{
			var collection = new MauiServiceCollection();
			var services = new MauiFactory(collection);
			var context = new MauiContext(services);

			DoAdd(context);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.NotNull(context.Services.GetService<TestThing>());

			static void DoAdd(MauiContext ctx)
			{
				var specificObj = new TestThing();
				ctx.AddSpecific<TestThing>(specificObj);
			}
		}

		[Fact]
		public void AddWeakSpecificIsWeak()
		{
			var collection = new MauiServiceCollection();
			var services = new MauiFactory(collection);
			var context = new MauiContext(services);

			DoAdd(context);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.Null(context.Services.GetService<TestThing>());

			static void DoAdd(MauiContext ctx)
			{
				var specificObj = new TestThing();
				ctx.AddWeakSpecific<TestThing>(specificObj);
			}
		}

		[Fact]
		public void CloneCanOverrideIncludeService()
		{
			var obj = new TestThing();
			var obj2 = new TestThing();

			var collection = new MauiServiceCollection();
			collection.AddSingleton(obj);
			var services = new MauiFactory(collection);

			var first = new MauiContext(services);

			var second = new MauiContext(first.Services);
			second.AddSpecific<TestThing>(obj2);

			Assert.Same(obj2, second.Services.GetService<TestThing>());
		}

		[Fact]
		public void MauiContextSupportsKeyedServices()
		{
			var collection = new ServiceCollection();
			collection.AddKeyedTransient<IFooService, FooService>("foo");
			collection.AddKeyedTransient<IFooService, FooService2>("foo2");
			var services = collection.BuildServiceProvider();

			var context = new MauiContext(services);

			var foo = context.Services.GetRequiredKeyedService<IFooService>("foo");
			Assert.IsType<FooService>(foo);

			var foo2 = context.Services.GetRequiredKeyedService<IFooService>("foo2");
			Assert.IsType<FooService2>(foo2);
		}

		[Fact]
		public void MauiContextSupportsKeyedServicesUsingAttributes()
		{
			var collection = new ServiceCollection();
			collection.AddKeyedTransient<IFooService, FooService>("foo");
			collection.AddKeyedTransient<IBarService, BarService>("bar");
			collection.AddTransient<IFooBarService, FooBarKeyedService>();
			var services = collection.BuildServiceProvider();

			var context = new MauiContext(services);

			var foobar = context.Services.GetRequiredService<IFooBarService>();
			var keyed = Assert.IsType<FooBarKeyedService>(foobar);
			Assert.NotNull(keyed.Foo);
			Assert.NotNull(keyed.Bar);
		}
		[Fact]
		public void NonKeyedProviderStaysNonKeyed()
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(new KeyedOrNonKeyedProviderFactory(false));
			var mauiApp = builder.Build();

			var context = new MauiContext(mauiApp.Services);

			Assert.IsAssignableFrom<IServiceProvider>(context.Services);
			Assert.IsNotAssignableFrom<IKeyedServiceProvider>(context.Services);

			var context2 = new MauiContext(context.Services);

			Assert.IsAssignableFrom<IServiceProvider>(context2.Services);
			Assert.IsNotAssignableFrom<IKeyedServiceProvider>(context2.Services);
		}

		[Fact]
		public void KeyedProviderStaysKeyed()
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureContainer(new KeyedOrNonKeyedProviderFactory(true));
			var mauiApp = builder.Build();

			var context = new MauiContext(mauiApp.Services);

			Assert.IsAssignableFrom<IServiceProvider>(context.Services);
			Assert.IsAssignableFrom<IKeyedServiceProvider>(context.Services);

			var context2 = new MauiContext(context.Services);

			Assert.IsAssignableFrom<IServiceProvider>(context2.Services);
			Assert.IsAssignableFrom<IKeyedServiceProvider>(context2.Services);
		}

		private class KeyedOrNonKeyedProviderFactory : IServiceProviderFactory<ServiceCollection>
		{
			public KeyedOrNonKeyedProviderFactory(bool keyed)
			{
				Keyed = keyed;
			}

			public bool Keyed { get; }

			public ServiceCollection CreateBuilder(IServiceCollection services) =>
				new() { services };

			public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder)
			{
				var real = containerBuilder.BuildServiceProvider();
				return Keyed ? new KeyedProvider(real) : new NonKeyedProvider(real);
			}
		}

		private class NonKeyedProvider : IServiceProvider
		{
			public NonKeyedProvider(ServiceProvider provider)
			{
				Provider = provider;
			}

			public ServiceProvider Provider { get; }

			public object GetService(Type serviceType) =>
				Provider.GetService(serviceType);
		}

		private class KeyedProvider : IServiceProvider, IKeyedServiceProvider
		{
			public KeyedProvider(ServiceProvider provider)
			{
				Provider = provider;
			}

			public ServiceProvider Provider { get; }

			public object GetKeyedService(Type serviceType, object serviceKey) =>
				Provider.GetKeyedService(serviceType, serviceKey);

			public object GetRequiredKeyedService(Type serviceType, object serviceKey) =>
				Provider.GetRequiredKeyedService(serviceType, serviceKey);

			public object GetService(Type serviceType) =>
				Provider.GetService(serviceType);
		}

		class TestThing
		{
		}
	}
}