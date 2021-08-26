using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderHandlerTests
	{
		[Fact]
		public void CanBuildAHost()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			Assert.NotNull(mauiApp.Services);
		}

		[Fact]
		public void CanGetIMauiHandlersServiceProviderFromServices()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>();
			Assert.NotNull(handlers);
			Assert.IsType<Maui.Hosting.Internal.MauiHandlersServiceProvider>(handlers);
		}

		[Fact]
		public void CanRegisterAndGetHandlerUsingType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler<IViewStub>();

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler(typeof(IViewStub), typeof(ViewHandlerStub)))
				.Build();

			var handler = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerWithDictionary()
		{
			var dic = new Dictionary<Type, Type>
			{
				{ typeof(IViewStub), typeof(ViewHandlerStub) }
			};

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandlers(dic))
				.Build();

			var handler = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(IViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanRegisterAndGetHandlerForConcreteType()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IViewStub, ViewHandlerStub>())
				.Build();

			var handler = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(ViewStub));

			Assert.NotNull(handler);
			Assert.IsType<ViewHandlerStub>(handler);
		}

		[Fact]
		public void CanChangeHandlerRegistration()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ButtonStub, ButtonHandlerStub>())
				.Build();

			var specificHandler = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(ButtonStub));
			Assert.IsType<ButtonHandlerStub>(specificHandler);

			mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetCollection().AddHandler<ButtonStub, AlternateButtonHandlerStub>();

			var alternateHandler = mauiApp.Services.GetRequiredService<IMauiHandlersServiceProvider>().GetHandler(typeof(ButtonStub));
			Assert.IsType<AlternateButtonHandlerStub>(alternateHandler);
		}
	}
}