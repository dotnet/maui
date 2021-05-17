using System;
using Tizen.Applications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{

	public class MauiApplication<TStartup> : MauiApplication where TStartup : IStartup, new()
	{
		protected override void OnCreate()
		{
			base.OnCreate();

			var startup = new TStartup();

			var host = startup
				.CreateAppHostBuilder()
				.ConfigureServices(ConfigureNativeServices)
				.ConfigureUsing(startup)
				.Build();

			Services = host.Services;
			Application = Services.GetRequiredService<IApplication>();

			var context = CoreUIAppContext.GetInstance(this);
			var mauiContext = new MauiContext(Services, context);

			var activationState = new ActivationState(mauiContext);
			var window = Application.CreateWindow(activationState);

			var page = window.View;

			context.SetContent(page.ToNative(mauiContext));

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

		protected override void OnPreCreate()
		{
			base.OnPreCreate();
			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnPreCreate>(del => del(this));
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


		// Configure native services like HandlersContext, ImageSourceHandlers etc.. 
		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
		}
	}

	public abstract class MauiApplication : CoreUIApplication
	{
		protected MauiApplication()
		{
			Current = this;
		}

		public static new MauiApplication Current { get; private set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
