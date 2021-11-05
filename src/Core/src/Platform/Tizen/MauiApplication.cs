using System;
using Tizen.Applications;
using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public abstract class MauiApplication : CoreUIApplication
	{
		protected MauiApplication()
		{
			Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		protected override void OnPreCreate()
		{
			base.OnPreCreate();

			var mauiApp = CreateMauiApp();

			MauiApplicationContext = new MauiContext(mauiApp.Services, this, CoreUIAppContext.GetInstance(this));

			Services = mauiApp.Services;

			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnPreCreate>(del => del(this));
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			Application = Services.GetRequiredService<IApplication>();

			if (Application == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			this.SetApplicationHandler(Application, MauiApplicationContext);

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

		internal IMauiContext MauiApplicationContext { get; private set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
