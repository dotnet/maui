using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	public static class CoreDeviceTestExtensions
	{
		public static MauiAppBuilder ConfigureTestBuilder(this MauiAppBuilder mauiAppBuilder)
		{
			var builder = mauiAppBuilder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(ButtonWithContainerStub), typeof(ButtonWithContainerStubHandler));
					handlers.AddHandler(typeof(SliderStub), typeof(SliderHandler));
					handlers.AddHandler(typeof(ButtonStub), typeof(ButtonHandler));
					handlers.AddHandler(typeof(ElementStub), typeof(ElementHandlerStub));
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
					fonts.AddFont("LobsterTwo-Regular.ttf", "Lobster Two");
					fonts.AddFont("LobsterTwo-Bold.ttf", "Lobster Two Bold");
					fonts.AddFont("LobsterTwo-Italic.ttf", "Lobster Two Italic");
					fonts.AddFont("LobsterTwo-BoldItalic.ttf", "Lobster Two BoldItalic");
				});
			builder.Services.AddKeyedSingleton<IImageSourceService, CountedImageSourceServiceStub>(typeof(CountedImageSourceStub));
			builder.Services.AddKeyedSingleton<IImageSourceService>(typeof(FileImageSourceStub), (ksvcs, _) => ((IKeyedServiceProvider)ksvcs).GetRequiredKeyedService<IImageSourceService>(typeof(IFileImageSource)));

			return builder;
		}
	}
}
