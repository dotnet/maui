﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class AndroidLifecycle
	{
		// Events called by the Application
		public delegate void OnApplicationCreating(Application application);
		public delegate void OnApplicationCreate(Application application);
		public delegate void OnApplicationLowMemory(Application application);
		public delegate void OnApplicationTrimMemory(Application application, TrimMemory level);
		public delegate void OnApplicationConfigurationChanged(Application application, Configuration newConfig);

		// Events called by the ActivityLifecycleCallbacks
		public delegate void OnCreate(Activity activity, Bundle? savedInstanceState);
		public delegate void OnStart(Activity activity);
		public delegate void OnResume(Activity activity);
		public delegate void OnPause(Activity activity);
		public delegate void OnStop(Activity activity);
		public delegate void OnSaveInstanceState(Activity activity, Bundle outState);
		public delegate void OnDestroy(Activity activity);

		// Events called by Activity overrides (always call base)
		public delegate void OnRestart(Activity activity);
		public delegate void OnPostCreate(Activity activity, Bundle? savedInstanceState); // System-only event
		public delegate void OnPostResume(Activity activity); // System-only event

		// Events called by Activity overrides (calling base is optional)
		public delegate void OnActivityResult(Activity activity, int requestCode, Result resultCode, Intent? data);
		public delegate bool OnBackPressed(Activity activity);
		public delegate void OnConfigurationChanged(Activity activity, Configuration newConfig);
		public delegate bool OnKeyDown(object context, Keycode keyCode, KeyEvent? e);
		public delegate bool OnKeyLongPress(object context, Keycode keyCode, KeyEvent? e);
		public delegate bool OnKeyMultiple(object context, Keycode keyCode, int repeatCount, KeyEvent? e);
		public delegate bool OnKeyShortcut(object context, Keycode keyCode, KeyEvent? e);
		public delegate bool OnKeyUp(object context, Keycode keyCode, KeyEvent? e);
		public delegate void OnNewIntent(Activity activity, Intent? intent);
		public delegate void OnRequestPermissionsResult(Activity activity, int requestCode, string[] permissions, Permission[] grantResults);
		public delegate void OnRestoreInstanceState(Activity activity, Bundle savedInstanceState);

		// Internal events
		internal delegate void OnMauiContextCreated(IMauiContext mauiContext);
	}
}