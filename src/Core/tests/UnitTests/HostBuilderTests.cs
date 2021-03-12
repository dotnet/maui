using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Tests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public partial class HostBuilderTests : IDisposable
	{
		[Fact]
		public void CanBuildAHost()
		{
			var host = App.CreateDefaultBuilder().Build();
			Assert.NotNull(host);
		}

		[Fact]
		public void CanGetStaticApp()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			Assert.NotNull(App.Current);
			Assert.Equal(App.Current, app);
		}

		[Fact]
		public void ShouldntCreateMultipleApp()
		{
			var app = new AppStub();
			Assert.Throws<InvalidOperationException>(() => new AppStub());
		}

		[Fact]
		public void CanGetServices()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			Assert.NotNull(app.Services);
		}

		[Fact]
		public void CanGetStaticServices()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			Assert.NotNull(App.Current.Services);
			Assert.Equal(app.Services, App.Current.Services);
		}

		[Fact]
		public void HandlerContextNullBeforeBuild()
		{
			var app = new AppStub();
			app.CreateBuilder();

			var handlerContext = App.Current.Context;

			Assert.Null(handlerContext);
		}

		[Fact]
		public void HandlerContextAfterBuild()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			var handlerContext = App.Current.Context;

			Assert.NotNull(handlerContext);
		}

		[Fact]
		public void CanHandlerProviderContext()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			var handlerContext = App.Current.Context;

			Assert.IsAssignableFrom<IMauiHandlersServiceProvider>(handlerContext.Handlers);
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>()
				.Build(app);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(IViewStub));
			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithDictionary()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.RegisterHandlers(new Dictionary<Type, Type>
				{
					{ typeof(IViewStub), typeof(ViewHandlerStub) }
				})
				.Build(app);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(IViewStub));
			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerForType()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>()
				.Build(app);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(ViewStub));
			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void DefaultHandlersAreRegistered()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(IButton));
			Assert.NotNull(handler);
			Assert.IsType<ButtonHandler>(handler);
		}

		[Fact]
		public void CanSpecifyHandler()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.RegisterHandler<ButtonStub, ButtonHandlerStub>()
				.Build(app);

			var defaultHandler = App.Current.Context.Handlers.GetHandler(typeof(IButton));
			var specificHandler = App.Current.Context.Handlers.GetHandler(typeof(ButtonStub));
			Assert.NotNull(defaultHandler);
			Assert.NotNull(specificHandler);
			Assert.IsType<ButtonHandler>(defaultHandler);
			Assert.IsType<ButtonHandlerStub>(specificHandler);
		}

		[Fact]
		public void WillRetrieveDifferentTransientServices()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.ConfigureServices((ctx, services) => services.AddTransient<IFooService, FooService>())
				.Build(app);

			AssertTransient<IFooService, FooService>(app);
		}

		[Fact]
		public void WillRetrieveSameSingletonServices()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.ConfigureServices((ctx, services) => services.AddSingleton<IFooService, FooService>())
				.Build(app);

			AssertSingleton<IFooService, FooService>(app);
		}

		[Fact]
		public void WillRetrieveMixedServices()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.ConfigureServices((ctx, services) =>
				{
					services.AddSingleton<IFooService, FooService>();
					services.AddTransient<IBarService, BarService>();
				})
				.Build(app);

			AssertSingleton<IFooService, FooService>(app);
			AssertTransient<IBarService, BarService>(app);
		}

		public void Dispose()
		{
			(App.Current as AppStub)?.ClearApp();
		}

		static void AssertTransient<TInterface, TConcrete>(AppStub app)
		{
			var service1 = app.Services.GetService<TInterface>();

			Assert.NotNull(service1);
			Assert.IsType<TConcrete>(service1);

			var service2 = app.Services.GetService<TInterface>();

			Assert.NotNull(service2);
			Assert.IsType<TConcrete>(service2);

			Assert.NotEqual(service1, service2);
		}

		static void AssertSingleton<TInterface, TConcrete>(AppStub app)
		{
			var service1 = app.Services.GetService<TInterface>();

			Assert.NotNull(service1);
			Assert.IsType<TConcrete>(service1);

			var service2 = app.Services.GetService<TInterface>();

			Assert.NotNull(service2);
			Assert.IsType<TConcrete>(service2);

			Assert.Equal(service1, service2);
		}
	}
}
