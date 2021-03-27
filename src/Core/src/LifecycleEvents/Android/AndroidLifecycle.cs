using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class AndroidLifecycle
	{
		// typical lifecycle events, always call base
		public delegate void OnCreate(Activity activity, Bundle? savedInstanceState);
		public delegate void OnRestart(Activity activity);
		public delegate void OnStart(Activity activity);
		// OnRestoreInstanceState
		public delegate void OnPostCreate(Activity activity, Bundle? savedInstanceState); // System-only event
		public delegate void OnResume(Activity activity);
		public delegate void OnPostResume(Activity activity); // System-only event
		public delegate void OnPause(Activity activity);
		public delegate void OnStop(Activity activity);
		// OnSaveInstanceState
		public delegate void OnDestroy(Activity activity);

		// other events
		public delegate void OnSaveInstanceState(Activity activity, Bundle outState);
		public delegate void OnRestoreInstanceState(Activity activity, Bundle savedInstanceState);
		public delegate void OnActivityResult(Activity activity, int requestCode, Result resultCode, Intent? data);
		public delegate void OnBackPressed(Activity activity);
		public delegate void OnConfigurationChanged(Activity activity, Configuration newConfig);
	}
}