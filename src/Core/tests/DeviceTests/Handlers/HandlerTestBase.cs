using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public class HandlerTestBase : TestBase, IDisposable
	{
		IApplication _app;
		MauiApp _mauiApp;
		IServiceProvider _servicesProvider;
		IMauiContext _context;

		public IApplication App => _app;

		public IMauiContext MauiContext => _context;

		public HandlerTestBase()
		{
			var appBuilder = MauiApp
				.CreateBuilder()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(ButtonWithContainerStub), typeof(ButtonWithContainerStubHandler));
					handlers.AddHandler(typeof(SliderStub), typeof(SliderHandler));
					handlers.AddHandler(typeof(ButtonStub), typeof(ButtonHandler));
					handlers.AddHandler(typeof(ElementStub), typeof(ElementHandlerStub));
				})
				.ConfigureImageSources(services =>
				{
					services.AddService<ICountedImageSourceStub, CountedImageSourceServiceStub>();
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
					fonts.AddFont("LobsterTwo-Regular.ttf", "Lobster Two");
					fonts.AddFont("LobsterTwo-Bold.ttf", "Lobster Two Bold");
					fonts.AddFont("LobsterTwo-Italic.ttf", "Lobster Two Italic");
					fonts.AddFont("LobsterTwo-BoldItalic.ttf", "Lobster Two BoldItalic");
				});

			_mauiApp = appBuilder.Build();
			_servicesProvider = _mauiApp.Services;

			_app = new ApplicationStub();

			_context = new ContextStub(_servicesProvider);
		}

		public void Dispose()
		{
			((IDisposable)_mauiApp).Dispose();
			_mauiApp = null;
			_servicesProvider = null;
			_app = null;
			_context = null;
		}
	}
}