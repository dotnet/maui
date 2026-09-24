using System;
using Foundation;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

internal static class AppleTemplateLaunchProbe
{
	static bool activated;

	public static void Configure(MauiAppBuilder builder)
	{
		builder.ConfigureLifecycleEvents(events => events.AddiOS(ios => ios.OnActivated(app =>
		{
			if (activated)
				return;

			activated = true;
			// Measure the smoke-test interval on the app's run loop, not during installation.
			NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(15), timer =>
			{
				Console.WriteLine("__MAUI_APP_COMPLETION_MARKER__");
				Console.Out.Flush();
				Environment.Exit(0);
			});
		})));
	}
}
