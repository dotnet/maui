using System;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddAndroid(OnConfigureLifeCycle));

		static void OnConfigureLifeCycle(IAndroidLifecycleBuilder android)
		{
			android
				.OnPostCreate((activity, bundle) =>
				{
					// OnCreate is only ever called once when the activity is initally created
					activity.GetWindow()?.Created();
				})
				.OnRestart(activity =>
				{
					// Restart only runs after "OnStop" and then an activity is brought back into the foreground
					activity.GetWindow()?.Resumed();
				})
				.OnResume(activity =>
				{
					// this is called right before an activity is running
					// it's called after onstart or it's called after app is unpausing
					activity.GetWindow()?.Activated();

				})
				.OnPause(activity =>
				{
					// app has been backgrounded and lost focus but still might be visible
					// think dialog prompt on top of activity
					activity.GetWindow()?.Deactivated();
				})
				.OnStop(activity =>
				{
					// Activity is no longer visible
					activity.GetWindow()?.Stopped();
				})
				.OnDestroy(activity =>
				{
					activity.GetWindow()?.Destroying();
				});
		}
	}
}
