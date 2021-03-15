using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Tests;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderTests : IDisposable
	{
		[Fact]
		public void CanBuildAHost()
		{
			var host = AppHostBuilder.CreateDefaultAppBuilder().Build();
			Assert.NotNull(host);
		}

		[Fact]
		public void CanGetServices()
		{
			var application = CreateDefaultApp();

			Assert.NotNull(application.Services);
		}

		[Fact]
		public void CanGetStaticServices()
		{
			var application = CreateDefaultApp();

			Assert.NotNull(App.Current.Services);
			Assert.Equal(application.Services, App.Current.Services);
		}

		[Fact]
		public void HandlerContextNullBeforeBuild()
		{
			var application = CreateDefaultApp(false);

			Assert.NotNull(application);

			var handlerContext = App.Current.Context;

			Assert.Null(handlerContext);
		}

		[Fact]
		public void HandlerContextAfterBuild()
		{
			var application = CreateDefaultApp();

			Assert.NotNull(application);

			var handlerContext = App.Current.Context;

			Assert.NotNull(handlerContext);
		}

		[Fact]
		public void CanHandlerProviderContext()
		{
			var application = CreateDefaultApp();

			Assert.NotNull(application);

			var handlerContext = App.Current.Context;

			Assert.IsAssignableFrom<IMauiHandlersServiceProvider>(handlerContext.Handlers);
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>();

			startup.Configure(appBuilder);

			appBuilder.Build();

			var application = new AppStub();

			appBuilder.SetServiceProvider(application);

			Assert.NotNull(application);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithDictionary()
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandlers(new Dictionary<Type, Type>
				{
					{ typeof(IViewStub), typeof(ViewHandlerStub) }
				});

			startup.Configure(appBuilder);

			appBuilder.Build();

			var application = new AppStub();

			appBuilder.SetServiceProvider(application);

			Assert.NotNull(application);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerForType()
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>();

			startup.Configure(appBuilder);

			appBuilder.Build();

			var application = new AppStub();

			appBuilder.SetServiceProvider(application);

			Assert.NotNull(application);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(ViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void DefaultHandlersAreRegistered()
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder();

			startup.Configure(appBuilder);

			appBuilder.Build();

			var application = new AppStub();

			appBuilder.SetServiceProvider(application);

			Assert.NotNull(application);

			var handler = App.Current.Context.Handlers.GetHandler(typeof(IButton));

			Assert.NotNull(handler);
			Assert.IsType<ButtonHandler>(handler);
		}

		[Fact]
		public void CanSpecifyHandler()
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.RegisterHandler<ButtonStub, ButtonHandlerStub>();

			startup.Configure(appBuilder);

			appBuilder.Build();

			var application = new AppStub();

			appBuilder.SetServiceProvider(application);

			Assert.NotNull(application);

			var defaultHandler = App.Current.Context.Handlers.GetHandler(typeof(IButton));
			var specificHandler = App.Current.Context.Handlers.GetHandler(typeof(ButtonStub));

			Assert.NotNull(defaultHandler);
			Assert.NotNull(specificHandler);
			Assert.IsType<ButtonHandler>(defaultHandler);
			Assert.IsType<ButtonHandlerStub>(specificHandler);
		}

		public void Dispose()
		{
			(App.Current as AppStub)?.ClearApp();
		}

		internal AppStub CreateDefaultApp(bool build = true)
		{
			var startup = new StartupStub();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder();

			startup.Configure(appBuilder);

			if (build)
				appBuilder.Build();

			var application = new AppStub();

			if (build)
				appBuilder.SetServiceProvider(application);

			return application;
		}
	}
}