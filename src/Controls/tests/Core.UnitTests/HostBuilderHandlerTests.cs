using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class HostBuilderHandlerTests
	{
		[Test]
		public void DefaultHandlersAreRegistered()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.NotNull(handlers);
			var handler = handlers.GetHandler(typeof(Button));

			Assert.NotNull(handler);
			Assert.AreEqual(typeof(ButtonHandler), handler.GetType());
		}

		[Test]
		public void CanSpecifyHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, ButtonHandlerStub>())
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.NotNull(handlers);

			var specificHandler = handlers.GetHandler(typeof(Button));

			Assert.NotNull(specificHandler);
			Assert.AreEqual(typeof(ButtonHandlerStub), specificHandler.GetType());
		}

		[Test]
		[TestCase(typeof(Label), typeof(LabelHandler))]
		[TestCase(typeof(Button), typeof(ButtonHandler))]
		[TestCase(typeof(ContentPage), typeof(PageHandler))]
		[TestCase(typeof(Page), typeof(PageHandler))]
		[TestCase(typeof(TemplatedView), typeof(ContentViewHandler))]
		[TestCase(typeof(ContentView), typeof(ContentViewHandler))]
		[TestCase(typeof(MyTestCustomTemplatedView), typeof(ContentViewHandler))]
		public void VariousControlsGetCorrectHandler(Type viewType, Type handlerType)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var specificHandler = handlers.GetHandler(viewType);

			Assert.NotNull(specificHandler);
			Assert.AreEqual(handlerType, specificHandler.GetType());
		}

		class MyTestCustomTemplatedView : TemplatedView
		{
		}
	}
}
