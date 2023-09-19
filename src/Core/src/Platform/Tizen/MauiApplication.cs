using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Tizen.Applications;
using Tizen.NUI;
using IOPath = System.IO.Path;
using NView = Tizen.NUI.BaseComponents.View;
using TApplication = Tizen.Applications.Application;

namespace Microsoft.Maui
{
	public abstract class MauiApplication : NUIApplication, IPlatformApplication
	{
		const string _fontCacheFolderName = "fonts";

		internal Func<bool>? _handleBackButtonPressed;

		IMauiContext? _applicationContext = null!;

		IApplication? _application;

		IServiceProvider? _services;

		protected MauiApplication()
		{
			Current = this;
			IPlatformApplication.Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		protected override void OnPreCreate()
		{
			base.OnPreCreate();
			FocusManager.Instance.EnableDefaultAlgorithm(true);
			NView.SetDefaultGrabTouchAfterLeave(true);

			var fontResourcePath = IOPath.Combine(TApplication.Current.DirectoryInfo.Resource, _fontCacheFolderName);
			FontClient.Instance.AddCustomFontDirectory(fontResourcePath);

			var mauiApp = CreateMauiApp();
			var rootContext = new MauiContext(mauiApp.Services);

			var platformWindow = CoreAppExtensions.GetDefaultWindow();
			platformWindow.Initialize();
			rootContext.AddWeakSpecific(platformWindow);

			_applicationContext = rootContext.MakeApplicationScope(this);

			_services = _applicationContext.Services;

			if (_services == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			_services.InvokeLifecycleEvents<TizenLifecycle.OnPreCreate>(del => del(this));
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			if (_services == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			if (_applicationContext == null)
				throw new InvalidOperationException($"The {nameof(IMauiContext)} instance was not found.");

			_application = _services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(_application, _applicationContext);

			this.CreatePlatformWindow(_application);

			_services.InvokeLifecycleEvents<TizenLifecycle.OnCreate>(del => del(this));
		}

		public void SetBackButtonPressedHandler(Func<bool> handler)
		{
			_handleBackButtonPressed = handler;
		}

		protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
		{
			base.OnAppControlReceived(e);
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnAppControlReceived>(del => del(this, e));
		}

		protected override void OnDeviceOrientationChanged(DeviceOrientationEventArgs e)
		{
			base.OnDeviceOrientationChanged(e);
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnDeviceOrientationChanged>(del => del(this, e));
		}

		protected override void OnLocaleChanged(LocaleChangedEventArgs e)
		{
			base.OnLocaleChanged(e);
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnLocaleChanged>(del => del(this, e));
		}

		protected override void OnLowBattery(LowBatteryEventArgs e)
		{
			base.OnLowBattery(e);
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnLowBattery>(del => del(this, e));
		}

		protected override void OnLowMemory(LowMemoryEventArgs e)
		{
			base.OnLowMemory(e);
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnLowMemory>(del => del(this, e));
		}

		protected override void OnPause()
		{
			base.OnPause();
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnPause>(del => del(this));
		}

		protected override void OnRegionFormatChanged(RegionFormatChangedEventArgs e)
		{
			base.OnRegionFormatChanged(e);
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnRegionFormatChanged>(del => del(this, e));
		}

		protected override void OnResume()
		{
			base.OnResume();
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnResume>(del => del(this));
		}

		protected override void OnTerminate()
		{
			base.OnTerminate();
			_services?.InvokeLifecycleEvents<TizenLifecycle.OnTerminate>(del => del(this));
		}

		public static new MauiApplication Current { get; private set; } = null!;

		// TODO: we should investigate throwing an exception or changing the public API
		IServiceProvider IPlatformApplication.Services => _services!;

		IApplication IPlatformApplication.Application => _application!;

		[Obsolete("Use the IPlatformApplication.Current.Services instead.")]
		public IServiceProvider Services
		{
			get => _services!;
			protected set => _services = value;
		}

		[Obsolete("Use the IPlatformApplication.Current.Application instead.")]
		public IApplication Application
		{
			get => _application!;
			protected set => _application = value;
		}
	}
}
