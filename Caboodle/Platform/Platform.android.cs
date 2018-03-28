using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Camera2;
using Android.Net;
using Android.Net.Wifi;
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

        public static void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) =>
            Permissions.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        internal static bool HasSystemFeature(string systemFeature)
        {
            var packageManager = CurrentContext.PackageManager;
            foreach (var feature in packageManager.GetSystemAvailableFeatures())
            {
                if (feature.Name.Equals(systemFeature, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsIntentSupported(Intent intent)
        {
            var manager = CurrentContext.PackageManager;
            var activities = manager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return activities.Any();
        }

        internal static bool HasApiLevel(BuildVersionCodes versionCode) =>
            (int)Build.VERSION.SdkInt >= (int)versionCode;

        public static void BeginInvokeOnMainThread(Action action)
        {
            if (handler?.Looper != Looper.MainLooper)
            {
                handler = new Handler(Looper.MainLooper);
            }

            handler.Post(action);
        }

        internal static CameraManager CameraManager
            => (CameraManager)Application.Context.GetSystemService(Context.CameraService);

        internal static ClipboardManager ClipboardManager
            => (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);

        internal static ConnectivityManager ConnectivityManager =>
            (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);

        internal static WifiManager WifiManager =>
            (WifiManager)Application.Context.GetSystemService(Context.WifiService);
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
