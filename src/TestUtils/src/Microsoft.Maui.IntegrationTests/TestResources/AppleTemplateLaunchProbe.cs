using System;
using Foundation;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

internal static class AppleTemplateLaunchProbe
{
	static bool activated;

	public static void Configure(MauiAppBuilder builder)
	{
		builder.ConfigureLifecycleEvents(events => events.AddiOS(ios => ios
			.OnActivated(app => StartTimer())
			.SceneOnActivated(scene => StartTimer())));
	}

	static void StartTimer()
	{
		if (activated)
			return;

		activated = true;
		// Scene-based apps do not raise application activation; both paths share one timer.
		NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(15), timer =>
		{
			Console.WriteLine("__MAUI_APP_COMPLETION_MARKER__");
			Console.Out.Flush();
			Environment.Exit(0);
		});
	}
}
