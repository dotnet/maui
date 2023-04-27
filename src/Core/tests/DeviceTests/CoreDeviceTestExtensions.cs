using System;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	public static class CoreDeviceTestExtensions
	{
		public static MauiAppBuilder ConfigureTestBuilder(this MauiAppBuilder mauiAppBuilder)
		{
			return mauiAppBuilder
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
		}
	}
}
