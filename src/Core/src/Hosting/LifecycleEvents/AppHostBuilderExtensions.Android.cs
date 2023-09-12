using AndroidX.AppCompat.App;
using AndroidX.Window.Layout;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddAndroid(OnConfigureLifeCycle));

		internal static MauiAppBuilder ConfigureWindowEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddAndroid(OnConfigureWindow));

		static void OnConfigureLifeCycle(IAndroidLifecycleBuilder android)
		{
			android
				.OnPostCreate((activity, bundle) =>
				{
					// OnCreate is only ever called once when the activity is initially created
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
					var window = activity.GetWindow();

					// Activity is no longer visible
					window?.Stopped();

					// As of Ice Cream Sandwich, Stopped is guaranteed to be called
					// even when the activity is finishing or being destroyed
					// We check for finishing and call destroying here if so
					if (activity.IsFinishing)
						window?.Destroying();
				})
				.OnDestroy(activity =>
				{
					// If we tried to call window.Destroying() before, GetWindow() should return null
					activity.GetWindow()?.Destroying();
				})
				.OnBackPressed(activity =>
				{
					return activity.GetWindow()?.BackButtonClicked() ?? false;
				});
		}

		static void OnConfigureWindow(IAndroidLifecycleBuilder android)
		{
			android
				.OnConfigurationChanged((activity, newConfig) =>
				{
					if (IPlatformApplication.Current is IPlatformApplication platformApplication)
					{
						var application = platformApplication.Application;

						application?.UpdateNightMode();
						application?.ThemeChanged();
					}

					var mauiWindow = activity.GetWindow();
					if (mauiWindow is not null)
					{
						if (newConfig is not null)
						{
							var density = newConfig.DensityDpi / DeviceDisplay.BaseLogicalDpi;
							mauiWindow.DisplayDensityChanged(density);
						}

						var frame = activity.GetWindowFrame();
						mauiWindow.FrameChanged(frame);
					}
				});
		}
	}
}