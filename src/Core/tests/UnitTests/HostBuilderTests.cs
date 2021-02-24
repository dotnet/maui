using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Tests;
using Microsoft.Maui.Hosting;
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

			Assert.NotNull(MauiApp.Current);
			Assert.Equal(MauiApp.Current, app);
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

			Assert.NotNull(MauiApp.Current.Services);
			Assert.Equal(app.Services, MauiApp.Current.Services);
		}

		[Fact]
		public void HandlerContextNullBeforeBuild()
		{
			var app = new AppStub();
			app.CreateBuilder();

			var handlerContext = MauiApp.Current.Context;

			Assert.Null(handlerContext);
		}

		[Fact]
		public void HandlerContextAfterBuild()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			var handlerContext = MauiApp.Current.Context;

			Assert.NotNull(handlerContext);
		}

		[Fact]
		public void CanHandlerProviderContext()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			var handlerContext = MauiApp.Current.Context;

			Assert.IsAssignableFrom<IMauiHandlersServiceProvider>(handlerContext.Handlers);
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var app = new AppStub();
			app.CreateBuilder()
				.RegisterHandler<IViewStub, ViewHandlerStub>()
				.Build(app);

			var handler = MauiApp.Current.Context.Handlers.GetHandler(typeof(IViewStub));
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

			var handler = MauiApp.Current.Context.Handlers.GetHandler(typeof(IViewStub));
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

			var handler = MauiApp.Current.Context.Handlers.GetHandler(typeof(ViewStub));
			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void DefaultHandlersAreRegistered()
		{
			var app = new AppStub();
			app.CreateBuilder().Build(app);

			var handler = MauiApp.Current.Context.Handlers.GetHandler(typeof(IButton));
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

			var defaultHandler = MauiApp.Current.Context.Handlers.GetHandler(typeof(IButton));
			var specificHandler = MauiApp.Current.Context.Handlers.GetHandler(typeof(ButtonStub));
			Assert.NotNull(defaultHandler);
			Assert.NotNull(specificHandler);
			Assert.IsType<ButtonHandler>(defaultHandler);
			Assert.IsType<ButtonHandlerStub>(specificHandler);
		}

		public void Dispose()
		{
			(App.Current as AppStub)?.ClearApp();
		}
	}
}
