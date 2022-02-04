using System;
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Platform
{
	public static class ApplicationExtensions
	{
		public static void RequestNewWindow(this Application nativeApplication, IApplication application, OpenWindowRequest? args)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var state = args?.State;
			var bundle = state.ToBundle();

			var pm = nativeApplication.PackageManager!;
			var intent = pm.GetLaunchIntentForPackage(nativeApplication.PackageName!)!;
			intent.AddFlags(ActivityFlags.NewTask);
			intent.AddFlags(ActivityFlags.MultipleTask);
			if (NativeVersion.Supports(NativeApis.LaunchAdjacent))
				intent.AddFlags(ActivityFlags.LaunchAdjacent);
			intent.PutExtras(bundle);

			nativeApplication.StartActivity(intent);
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
	}
}