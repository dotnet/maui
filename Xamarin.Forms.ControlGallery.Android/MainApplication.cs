using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Plugin.CurrentActivity;

namespace Xamarin.Forms.ControlGallery.Android
{
	//You can specify additional application information in this attribute
    [Application]
    public class MainApplication : global::Android.App.Application, global::Android.App.Application.IActivityLifecycleCallbacks
    {
		internal static Context ActivityContext { get; private set; }

		public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
            //A great place to initialize Xamarin.Insights and Dependency Services!
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
			ActivityContext = activity;
		}

        public void OnActivityDestroyed(Activity activity)
		{
			ActivityContext = activity;
		}

        public void OnActivityPaused(Activity activity)
		{
			ActivityContext = activity;
		}

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
			ActivityContext = activity;
		}

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
		{
			ActivityContext = activity;
		}

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
			ActivityContext = activity;
		}

        public void OnActivityStopped(Activity activity)
		{
			ActivityContext = activity;
		}
    }
}