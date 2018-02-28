using Android.App;
using Android.Content;
using Android.OS;
using System;

namespace Microsoft.Caboodle
{
	public partial class Platform
	{
		static ActivityLifecycleContextListener lifecycleListener;

		internal static Context CurrentContext =>
			lifecycleListener?.Context ?? Application.Context;

		internal static Activity CurrentActivity =>
			lifecycleListener?.Activity;

		public static void Init(Activity activity, Bundle bundle)
		{
			lifecycleListener = new ActivityLifecycleContextListener();
			activity.Application.RegisterActivityLifecycleCallbacks(lifecycleListener);
		}
	}

	class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
	{
		WeakReference<Activity> currentActivity = new WeakReference<Activity>(null);

		public Context Context =>
			Activity ?? Application.Context;

		public Activity Activity
		{
			get
			{
				Activity a;
				if (currentActivity.TryGetTarget(out a))
					return a;
				return null;
			}
		}

		public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
		{
		}

		public void OnActivityDestroyed(Activity activity)
		{
		}

		public void OnActivityPaused(Activity activity)
		{
			currentActivity.SetTarget(null);
		}

		public void OnActivityResumed(Activity activity)
		{
			currentActivity.SetTarget(activity);
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