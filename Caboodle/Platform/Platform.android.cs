using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Microsoft.Caboodle
{
    public static partial class Platform
    {
        static Handler handler;
        static ActivityLifecycleContextListener lifecycleListener;

        internal static Context CurrentContext =>
            lifecycleListener?.Context ?? Application.Context;

        internal static Activity CurrentActivity =>
            lifecycleListener?.Activity;

        public static void Init(Application application)
        {
            lifecycleListener = new ActivityLifecycleContextListener();
            application.RegisterActivityLifecycleCallbacks(lifecycleListener);
        }

        public static void Init(Activity activity, Bundle bundle) =>
           Init(activity.Application);

        internal static bool HasPermissionInManifest(string permission)
        {
            var packageInfo = CurrentContext.PackageManager.GetPackageInfo(CurrentContext.PackageName, PackageInfoFlags.Permissions);
            var requestedPermissions = packageInfo?.RequestedPermissions;
            return requestedPermissions?.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase)) ?? false;
        }

        public static void BeginInvokeOnMainThread(Action action)
        {
            if (handler?.Looper != Looper.MainLooper)
            {
                handler = new Handler(Looper.MainLooper);
            }

            handler.Post(action);
        }
    }

    class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        WeakReference<Activity> currentActivity = new WeakReference<Activity>(null);

        public Context Context =>
            Activity ?? Application.Context;

        public Activity Activity =>
            currentActivity.TryGetTarget(out var a) ? a : null;

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
