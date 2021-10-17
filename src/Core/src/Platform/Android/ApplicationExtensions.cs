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
			intent.PutExtras(bundle);

			nativeApplication.StartActivity(intent);
		}

		public static void CreateNativeWindow(this Activity activity, Bundle? savedInstanceState = null)
		{
			var mauiApp = MauiApplication.Current.Application;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			var services = MauiApplication.Current.Services;
			if (services == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			savedInstanceState ??= activity.Intent?.Extras;

			var mauiContext = MauiApplication.Current.MauiApplicationContext.MakeScoped(activity);

			services.InvokeLifecycleEvents<AndroidLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, savedInstanceState);

			var window = mauiApp.CreateWindow(activationState);

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