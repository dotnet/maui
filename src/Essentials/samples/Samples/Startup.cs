using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Samples
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

			builder
				.UseMauiApp<App>()
				.ConfigureEssentials(essentials =>
				{
					essentials.UseVersionTracking();
#if WINDOWS
					essentials.UseMapServiceToken("RJHqIE53Onrqons5CNOx~FrDr3XhjDTyEXEjng-CRoA~Aj69MhNManYUKxo6QcwZ0wmXBtyva0zwuHB04rFYAPf7qqGJ5cHb03RCDw1jIW8l");
#endif
					essentials.AddAppAction("app_info", "App Info", icon: "app_info_action_icon");
					essentials.AddAppAction("battery_info", "Battery Info");
					essentials.OnAppAction(App.HandleAppActions);
				});

			return builder.Build();
		}
	}
}
