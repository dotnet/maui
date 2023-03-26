using System;
using Android.App;
using AndroidX.Lifecycle;
using Java.Interop;

namespace Microsoft.Maui.LifecycleEvents.Android
{
	class LifecycleObserver : Java.Lang.Object, ILifecycleObserver
	{
		readonly Application _application;

		internal LifecycleObserver(Application application)
		{
			application.RegisterComponentCallbacks(new ApplicationComponentCallbacks(application));
			_application = application;
		}

		[Export, Lifecycle.Event.OnCreate]
		public void OnCreate()
		{
		}

		[Export, Lifecycle.Event.OnDestroy]
		public void OnDestroy()
		{
		}

		[Export, Lifecycle.Event.OnPause]
		public void OnPause() =>
			MauiApplication.Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationPause>(del => del(_application));

		[Export, Lifecycle.Event.OnResume]
		public void OnResume() =>
			MauiApplication.Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationResume>(del => del(_application));

		[Export, Lifecycle.Event.OnStop]
		public void OnStop() =>
			MauiApplication.Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationStop>(del => del(_application));

		[Export, Lifecycle.Event.OnStart]
		public void OnStart() =>
			MauiApplication.Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationStart>(del => del(_application));
	}
}