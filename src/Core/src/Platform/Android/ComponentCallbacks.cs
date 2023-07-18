using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	class ApplicationComponentCallbacks : ComponentCallbacks
	{
		readonly Application _application;

		public ApplicationComponentCallbacks(Application application) =>
			this._application = application;

		public override void OnConfigurationChanged(Configuration newConfig) =>
			MauiApplication.Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationConfigurationChanged>(del => del(_application, newConfig));

		public override void OnLowMemory() =>
			MauiApplication.Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationLowMemory>(del => del(_application));

		public override void OnTrimMemory(TrimMemory level) =>
			MauiApplication.Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationTrimMemory>(del => del(_application, level));
	}

	class ActivityComponentCallbacks : ComponentCallbacks
	{
		readonly WeakReference<Activity> _activity;

		public ActivityComponentCallbacks(Activity activity) =>
			this._activity = new WeakReference<Activity>(activity);

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			if (!_activity.TryGetTarget(out var target))
				return;
			MauiApplication.Current?.Application.ThemeChanged();
			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnConfigurationChanged>(del => del(target, newConfig));
		}
	}

	abstract class ComponentCallbacks : Java.Lang.Object, IComponentCallbacks2
	{
		protected ComponentCallbacks()
		{
		}

		public abstract void OnConfigurationChanged(Configuration newConfig);

		public virtual void OnLowMemory() { }

		public virtual void OnTrimMemory(TrimMemory level) { }
	}
}
