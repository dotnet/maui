using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.Lifecycle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.LifecycleEvents.Android;

namespace Microsoft.Maui
{
	public abstract class MauiApplication : Application, IPlatformApplication
	{
		protected MauiApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
			Current = this;
			IPlatformApplication.Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		public override void OnCreate()
		{
			RegisterActivityLifecycleCallbacks(new ActivityLifecycleCallbacks());
			ProcessLifecycleOwner.Get().Lifecycle.AddObserver(new LifecycleObserver(this));

			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services, this);

			var applicationContext = rootContext.MakeApplicationScope(this);

			Services = applicationContext.Services;

			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationCreating>(del => del(this));

			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, applicationContext);

			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationCreate>(del => del(this));

			base.OnCreate();
		}

		public static MauiApplication Current { get; private set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;

		public class ActivityLifecycleCallbacks : Java.Lang.Object, IActivityLifecycleCallbacks
		{
			public void OnActivityCreated(Activity activity, Bundle? savedInstanceState)
			{
				activity.RegisterComponentCallbacks(new ActivityComponentCallbacks(activity));
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnCreate>(del => del(activity, savedInstanceState));
			}

			public void OnActivityStarted(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnStart>(del => del(activity));

			public void OnActivityResumed(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnResume>(del => del(activity));

			public void OnActivityPaused(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnPause>(del => del(activity));

			public void OnActivityStopped(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnStop>(del => del(activity));

			public void OnActivitySaveInstanceState(Activity activity, Bundle outState) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnSaveInstanceState>(del => del(activity, outState));

			public void OnActivityDestroyed(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnDestroy>(del => del(activity));
		}
	}
}