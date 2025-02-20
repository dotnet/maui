using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class HostBuilderHandlerTests
	{
		[Fact]
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
			Assert.Equal(typeof(ButtonHandler), handler.GetType());
		}

		[Fact]
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
			Assert.Equal(typeof(ButtonHandlerStub), specificHandler.GetType());
		}

		[Theory]
		[InlineData(typeof(Label), typeof(LabelHandler))]
		[InlineData(typeof(Button), typeof(ButtonHandler))]
		[InlineData(typeof(ContentPage), typeof(PageHandler))]
		[InlineData(typeof(Page), typeof(PageHandler))]
		[InlineData(typeof(TemplatedView), typeof(ContentViewHandler))]
		[InlineData(typeof(ContentView), typeof(ContentViewHandler))]
		[InlineData(typeof(MyTestCustomTemplatedView), typeof(ContentViewHandler))]
		public void VariousControlsGetCorrectHandler(Type viewType, Type handlerType)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var specificHandler = handlers.GetHandler(viewType);

			Assert.NotNull(specificHandler);
			Assert.Equal(handlerType, specificHandler.GetType());
		}

		class MyTestCustomTemplatedView : TemplatedView
		{
		}
	}
}
