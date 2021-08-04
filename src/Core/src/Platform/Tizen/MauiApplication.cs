using System;
using Tizen.Applications;
using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{

	public class MauiApplication<TStartup> : MauiApplication where TStartup : IStartup, new()
	{
		protected override void OnPreCreate()
		{
			base.OnPreCreate();

			var startup = new TStartup();

			var host = startup
				.CreateAppHostBuilder()
				.ConfigureServices(ConfigureNativeServices)
				.ConfigureUsing(startup)
				.Build();

			Services = host.Services;

			if (Services == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			Current.Services.InvokeLifecycleEvents<TizenLifecycle.OnPreCreate>(del => del(this));
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			Application = Services.GetRequiredService<IApplication>();

			if (Application == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			MainWindow = CreateNativeWindow();
			MainWindow.Show();

			Current.Services?.InvokeLifecycleEvents<TizenLifecycle.OnCreate>(del => del(this));
		}

		Window CreateNativeWindow()
		{
			var context = CoreUIAppContext.GetInstance(this);
			var mauiContext = new MauiContext(Services, context);

			Services.InvokeLifecycleEvents<TizenLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var tizenWindow = mauiContext.Window;

			if (tizenWindow == null)
				throw new InvalidOperationException($"The {nameof(tizenWindow)} instance was not found.");

			var activationState = new ActivationState(mauiContext);
			var window = Application.CreateWindow(activationState);

			_virtualWindow = new WeakReference<IWindow>(window);
			tizenWindow.SetWindow(window, mauiContext);

			return tizenWindow;
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


		// Configure native services like HandlersContext, ImageSourceHandlers etc.. 
		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
		}
	}

	public abstract class MauiApplication : CoreUIApplication
	{
		internal WeakReference<IWindow>? _virtualWindow;
		internal IWindow? VirtualWindow
		{
			get
			{
				IWindow? window = null;
				_virtualWindow?.TryGetTarget(out window);
				return window;
			}
		}

		protected MauiApplication()
		{
			Current = this;
		}

		public static new MauiApplication Current { get; private set; } = null!;

		public Window MainWindow { get; protected set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
