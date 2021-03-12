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
			var host = Application.CreateDefaultBuilder().Build();
			Assert.NotNull(host);
		}

		[Fact]
		public void CanGetStaticApp()
		{
			var app = new ApplicationStub();
			app.CreateBuilder().Build(app);

			Assert.NotNull(Application.Current);
			Assert.Equal(Application.Current, app);
		}

		[Fact]
		public void ShouldntCreateMultipleApp()
		{
			var app = new ApplicationStub();
			Assert.Throws<InvalidOperationException>(() => new ApplicationStub());
		}

		[Fact]
		public void CanGetServices()
		{
			var app = new ApplicationStub();
			app.CreateBuilder().Build(app);

			Assert.NotNull(app.Services);
		}

		[Fact]
		public void CanGetStaticServices()
		{
			var app = new ApplicationStub();
			app.CreateBuilder().Build(app);

			Assert.NotNull(Application.Current.Services);
			Assert.Equal(app.Services, Application.Current.Services);
		}

		[Fact]
		public void HandlerContextNullBeforeBuild()
		{
			var app = new ApplicationStub();
			app.CreateBuilder();

			var handlerContext = Application.Current.Context;

			Assert.Null(handlerContext);
		}

		[Fact]
		public void HandlerContextAfterBuild()
		{
			var app = new ApplicationStub();
			app.CreateBuilder().Build(app);

			var handlerContext = Application.Current.Context;

			Assert.NotNull(handlerContext);
		}

		[Fact]
		public void CanHandlerProviderContext()
		{
			var app = new ApplicationStub();
			app.CreateBuilder().Build(app);

			var handlerContext = Application.Current.Context;

			Assert.IsAssignableFrom<IMauiHandlersServiceProvider>(handlerContext.Handlers);
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var app = new ApplicationStub();
			app.CreateBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>()
				.Build(app);

			var handler = Application.Current.Context.Handlers.GetHandler(typeof(IViewStub));
			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithDictionary()
		{
			var app = new ApplicationStub();
			app.CreateBuilder()
				.RegisterHandlers(new Dictionary<Type, Type>
				{
					{ typeof(IViewStub), typeof(ViewHandlerStub) }
				})
				.Build(app);

			var handler = Application.Current.Context.Handlers.GetHandler(typeof(IViewStub));
			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerForType()
		{
			var app = new ApplicationStub();
			app.CreateBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>()
				.Build(app);

			var handler = Application.Current.Context.Handlers.GetHandler(typeof(ViewStub));
			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void DefaultHandlersAreRegistered()
		{
			var app = new ApplicationStub();
			app.CreateBuilder().Build(app);

			var handler = Application.Current.Context.Handlers.GetHandler(typeof(IButton));
			Assert.NotNull(handler);
			Assert.IsType<ButtonHandler>(handler);
		}

		[Fact]
		public void CanSpecifyHandler()
		{
			var app = new ApplicationStub();
			app.CreateBuilder()
				.RegisterHandler<ButtonStub, ButtonHandlerStub>()
				.Build(app);

			var defaultHandler = Application.Current.Context.Handlers.GetHandler(typeof(IButton));
			var specificHandler = Application.Current.Context.Handlers.GetHandler(typeof(ButtonStub));
			Assert.NotNull(defaultHandler);
			Assert.NotNull(specificHandler);
			Assert.IsType<ButtonHandler>(defaultHandler);
			Assert.IsType<ButtonHandlerStub>(specificHandler);
		}

		[Fact]
		public void WillRetrieveDifferentTransientServices()
		{
			var app = new ApplicationStub();
			app.CreateBuilder()
				.ConfigureServices((ctx, services) => services.AddTransient<IFooService, FooService>())
				.Build(app);

			AssertTransient<IFooService, FooService>(app);
		}

		[Fact]
		public void WillRetrieveSameSingletonServices()
		{
			var app = new ApplicationStub();
			app.CreateBuilder()
				.ConfigureServices((ctx, services) => services.AddSingleton<IFooService, FooService>())
				.Build(app);

			AssertSingleton<IFooService, FooService>(app);
		}

		[Fact]
		public void WillRetrieveMixedServices()
		{
			var app = new ApplicationStub();
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
			(Application.Current as ApplicationStub)?.ClearApp();
		}

		static void AssertTransient<TInterface, TConcrete>(ApplicationStub app)
		{
			var service1 = app.Services.GetService<TInterface>();

			Assert.NotNull(service1);
			Assert.IsType<TConcrete>(service1);

			var service2 = app.Services.GetService<TInterface>();

			Assert.NotNull(service2);
			Assert.IsType<TConcrete>(service2);

			Assert.NotEqual(service1, service2);
		}

		static void AssertSingleton<TInterface, TConcrete>(ApplicationStub app)
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
