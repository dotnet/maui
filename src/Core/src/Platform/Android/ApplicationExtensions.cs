using System;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Platform
{
	public static class ApplicationExtensions
	{
		public static void RequestNewWindow(this Application platformApplication, IApplication application, OpenWindowRequest? args)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var state = args?.State;
			var bundle = state.ToBundle();

			var pm = platformApplication.PackageManager!;
			var intent = pm.GetLaunchIntentForPackage(platformApplication.PackageName!)!;
			intent.AddFlags(ActivityFlags.NewTask);
			intent.AddFlags(ActivityFlags.MultipleTask);
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				intent.AddFlags(ActivityFlags.LaunchAdjacent);
			intent.PutExtras(bundle);

			platformApplication.StartActivity(intent);
		}

		public static void CreatePlatformWindow(this Activity activity, IApplication application, Bundle? savedInstanceState = null)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			savedInstanceState ??= activity.Intent?.Extras;

			var mauiContext = applicationContext.MakeWindowScope(activity, out var windowScope);

			applicationContext.Services.InvokeLifecycleEvents<AndroidLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, savedInstanceState);

			var window = application.CreateWindow(activationState);

			if (window.Handler?.PlatformView is Activity oldActivity && 
				oldActivity != activity &&
				!oldActivity.IsDestroyed)
			{
				throw new InvalidOperationException(
					$"This window is already associated with an active Activity ({oldActivity.GetType()}). " + 
					$"Please override CreateWindow on {application.GetType()} "  + 
					$"to add support for multiple activities https://aka.ms/maui-docs-create-window"  + 
					$"or set the LaunchMode to SingleTop on {activity.GetType()}.");
			}

			activity.SetWindowHandler(window, mauiContext);
		}

		public static Bundle ToBundle(this IPersistedState? state)
		{
			var userInfo = new Bundle();

			if (state is not null)
			{
				foreach (var pair in state)
				{
					userInfo.PutString(pair.Key, pair.Value);
				}
			}

			return userInfo;
		}

		public static void UpdateNightMode(this IApplication application)
		{
			if (application is null)
				return;

			switch (application.UserAppTheme)
			{
				case AppTheme.Light:
					AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
					break;
				case AppTheme.Dark:
					AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
					break;
				default:
					AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
					break;
			}
		}
	}
}