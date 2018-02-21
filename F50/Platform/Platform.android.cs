using Android.App;
using Android.Content;
using Android.OS;

namespace Xamarin.F50
{
	public partial class Platform
	{
		static Application application;
		static ActivityLifecycleContextListener lifecycleListener;

		internal static Context CurrentContext
			=> lifecycleListener?.Context ?? application?.ApplicationContext;

		internal static Activity CurrentActivity
			=> lifecycleListener?.Activity;

		public static void Init(Activity activity, Bundle bundle)
		{
			lifecycleListener = new ActivityLifecycleContextListener();
			application = activity.Application;
			application.RegisterActivityLifecycleCallbacks(lifecycleListener);
		}
	}

	internal class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
	{
		Activity currentActivity = null;

		public Context Context 
			=> currentActivity ?? Application.Context;

		public Activity Activity
			=> currentActivity;

		public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
		{
		}

		public void OnActivityDestroyed(Activity activity)
		{
		}

		public void OnActivityPaused(Activity activity)
		{
		}

		public void OnActivityResumed(Activity activity)
		{
			currentActivity = activity;
		}

		public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
		{
		}

		public void OnActivityStarted(Activity activity)
		{
		}

		public void OnActivityStopped(Activity activity)
		{
		}
	}
}