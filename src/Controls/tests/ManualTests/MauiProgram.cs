using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.ManualTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("Lobster-Regular.ttf", "Lobster");
				fonts.AddFont("ionicons.ttf", "Ionicons");
				fonts.AddFont("pe-icon-set-weather.ttf", "Weather");
				fonts.AddFont("feather.ttf", "Feather");
				fonts.AddFont("icomoon.ttf", "Icomoon");

				if (OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
				{
					fonts.AddFont("SF-Pro-Text-Thin.otf", "DefaultLightest");
					fonts.AddFont("SF-Pro-Text-Light.otf", "DefaultLighter");
					fonts.AddFont("SF-Pro-Text-Regular.otf", "Default");
					fonts.AddFont("SF-Pro-Text-Medium.otf", "DefaultStrong");
					fonts.AddFont("SF-Pro-Text-Semibold.otf", "DefaultStronger");
					fonts.AddFont("SF-Pro-Text-Bold.otf", "DefaultHeavy");
					fonts.AddFont("SF-Pro-Text-Heavy.otf", "DefaultHeavier");
					fonts.AddFont("SF-Pro-Text-Black.otf", "DefaultHeaviest");

					fonts.AddFont("SF-Compact-Text-Thin.otf", "CompactLightest");
					fonts.AddFont("SF-Compact-Text-Light.otf", "CompactLighter");
					fonts.AddFont("SF-Compact-Text-Regular.otf", "Compact");
					fonts.AddFont("SF-Compact-Text-Medium.otf", "CompactStrong");
					fonts.AddFont("SF-Compact-Text-Semibold.otf", "CompactStronger");
					fonts.AddFont("SF-Compact-Text-Bold.otf", "CompactHeavy");
				}
				else if (OperatingSystem.IsAndroid())
				{
					fonts.AddFont("Roboto-Thin.ttf", "DefaultLightest");
					fonts.AddFont("Roboto-Light.ttf", "DefaultLighter");
					fonts.AddFont("Roboto-Regular.ttf", "Default");
					fonts.AddFont("Roboto-Medium.ttf", "DefaultStrong");
					fonts.AddFont("Roboto-Bold.ttf", "DefaultStronger");
					fonts.AddFont("Roboto-Bold.ttf", "DefaultHeavy");
					fonts.AddFont("Roboto-Black.ttf", "DefaultHeavier");
					fonts.AddFont("Roboto-Black.ttf", "DefaultHeaviest");

					fonts.AddFont("RobotoCondensed-Thin.ttf", "CompactLightest");
					fonts.AddFont("RobotoCondensed-Light.ttf", "CompactLighter");
					fonts.AddFont("RobotoCondensed-Regular.ttf", "Compact");
					fonts.AddFont("RobotoCondensed-Medium.ttf", "CompactStrong");
					fonts.AddFont("RobotoCondensed-SemiBold.ttf", "CompactStronger");
					fonts.AddFont("RobotoCondensed-Bold.ttf", "CompactHeavy");
				}
				else if (OperatingSystem.IsWindows())
				{
					fonts.AddFont("segoeui-light.ttf", "DefaultLightest");
					fonts.AddFont("segoeui-semilight.ttf", "DefaultLighter");
					fonts.AddFont("segoeui-regular.ttf", "Default");
					fonts.AddFont("segoeui-semibold.ttf", "DefaultStrong");
					fonts.AddFont("segoeui-bold.ttf", "DefaultStronger");
					fonts.AddFont("segoeui-bold.ttf", "DefaultHeavy");
					fonts.AddFont("segoeui-black.ttf", "DefaultHeavier");
					fonts.AddFont("segouei-black.ttf", "DefaultHeaviest");

					fonts.AddFont("segoeui-light.ttf", "CompactLightest");
					fonts.AddFont("segoeui-semilight.ttf", "CompactLighter");
					fonts.AddFont("segoeui-regular.ttf", "Compact");
					fonts.AddFont("segoeui-semibold.ttf", "CompactStrong");
					fonts.AddFont("segoeui-bold.ttf", "CompactStronger");
					fonts.AddFont("segoeui-bold.ttf", "CompactHeavy");
					fonts.AddFont("segoeui-black.ttf", "CompactHeavier");
					fonts.AddFont("segouei-black.ttf", "CompactHeaviest");
				}
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
