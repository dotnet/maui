using System;
using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Tizen.Applications;

namespace Microsoft.Maui
{
	public abstract class MauiApplication : CoreUIApplication, IPlatformApplication
	{
		IMauiContext _applicationContext = null!;

		protected MauiApplication()
		{
			Current = this;
			IPlatformApplication.Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		protected override void OnPreCreate()
		{
			base.OnPreCreate();

			Elementary.Initialize();
			Elementary.ThemeOverlay();

			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			var platformWindow = CoreAppExtensions.GetDefaultWindow();
			platformWindow.Initialize();
			rootContext.AddWeakSpecific(platformWindow);

			_applicationContext = rootContext.MakeApplicationScope(this);

			Services = _applicationContext.Services;

			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnPreCreate>(del => del(this));
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, _applicationContext);

			this.CreateNativeWindow(Application);

			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnCreate>(del => del(this));
		}

		protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
		{
			base.OnAppControlReceived(e);
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnAppControlReceived>(del => del(this, e));
		}

		protected override void OnDeviceOrientationChanged(DeviceOrientationEventArgs e)
		{
			base.OnDeviceOrientationChanged(e);
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnDeviceOrientationChanged>(del => del(this, e));
		}

		protected override void OnLocaleChanged(LocaleChangedEventArgs e)
		{
			base.OnLocaleChanged(e);
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnLocaleChanged>(del => del(this, e));
		}

		protected override void OnLowBattery(LowBatteryEventArgs e)
		{
			base.OnLowBattery(e);
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnLowBattery>(del => del(this, e));
		}

		protected override void OnLowMemory(LowMemoryEventArgs e)
		{
			base.OnLowMemory(e);
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnLowMemory>(del => del(this, e));
		}

		protected override void OnPause()
		{
			base.OnPause();
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnPause>(del => del(this));
		}

		protected override void OnRegionFormatChanged(RegionFormatChangedEventArgs e)
		{
			base.OnRegionFormatChanged(e);
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnRegionFormatChanged>(del => del(this, e));
		}

		protected override void OnResume()
		{
			base.OnResume();
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnResume>(del => del(this));
		}

		protected override void OnTerminate()
		{
			base.OnTerminate();
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnTerminate>(del => del(this));
		}

		public static new MauiApplication Current { get; private set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
