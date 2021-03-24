using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;

namespace Microsoft.Maui
{
	public class AndroidApplicationLifetime : IAndroidApplicationLifetime
	{
		public virtual void OnCreate(Activity activity, Bundle? savedInstanceState)
		{
	
		}

		public virtual void OnPostCreate(Activity activity, Bundle? savedInstanceState)
		{

		}

		public virtual void OnDestroy(Activity activity)
		{

		}

		public virtual void OnPause(Activity activity)
		{
	
		}

		public virtual void OnResume(Activity activity)
		{

		}

		public virtual void OnPostResume(Activity activity)
		{

		}

		public virtual void OnStart(Activity activity)
		{
	
		}

		public virtual void OnStop(Activity activity)
		{

		}

		public virtual void OnRestart(Activity activity)
		{

		}

		public virtual void OnSaveInstanceState(Activity activity, Bundle outState)
		{

		}

		public virtual void OnRestoreInstanceState(Activity activity, Bundle savedInstanceState)
		{

		}

		public virtual void OnConfigurationChanged(Activity activity, Configuration newConfig)
		{

		}

		public virtual void OnActivityResult(Activity activity, int requestCode, [GeneratedEnum] Result resultCode, Intent? data)
		{

		}

		public virtual void OnBackPressed(Activity activity)
		{

		}
	}
}