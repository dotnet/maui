using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	/// <summary>
	/// Defines the core behavior of a .NET MAUI application running on Android.
	/// </summary>
	public abstract class MauiApplication : Application, IPlatformApplication
	{
		protected MauiApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
			Current = this;
			IPlatformApplication.Current = this;
		}

		/// <summary>
		/// When overridden in a derived class, creates the <see cref="MauiApp"/> to be used in this application.
		/// Typically a <see cref="MauiApp"/> is created by calling <see cref="MauiApp.CreateBuilder(bool)"/>, configuring
		/// the returned <see cref="MauiAppBuilder"/>, and returning the built app by calling <see cref="MauiAppBuilder.Build"/>.
		/// </summary>
		/// <returns>The built <see cref="MauiApp"/>.</returns>
		protected abstract MauiApp CreateMauiApp();

		public override void OnCreate()
		{
			RegisterActivityLifecycleCallbacks(new ActivityLifecycleCallbacks());

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

		public override void OnLowMemory()
		{
			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationLowMemory>(del => del(this));

			base.OnLowMemory();
		}

		public override void OnTrimMemory(TrimMemory level)
		{
			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationTrimMemory>(del => del(this, level));

			base.OnTrimMemory(level);
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationConfigurationChanged>(del => del(this, newConfig));

			base.OnConfigurationChanged(newConfig);
		}

		public static MauiApplication Current { get; private set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;

		public class ActivityLifecycleCallbacks : Java.Lang.Object, IActivityLifecycleCallbacks
		{
			public void OnActivityCreated(Activity activity, Bundle? savedInstanceState) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnCreate>(del => del(activity, savedInstanceState));

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