using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public abstract class MauiWinUIApplication : UI.Xaml.Application
	{
		protected abstract MauiApp CreateMauiApp();

		protected override void OnLaunched(UI.Xaml.LaunchActivatedEventArgs args)
		{
			LaunchActivatedEventArgs = args;

			var mauiApp = CreateMauiApp();

			var applicationContext = new MauiApplicationContext(mauiApp.Services, this);

			Services = mauiApp.Services;

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

			var application = Services.GetRequiredService<IApplication>();

			Application = application;

			this.SetApplicationHandler(Application, applicationContext);

			var winuiWndow = CreateNativeWindow(applicationContext, args);

			MainWindow = winuiWndow;

			MainWindow.Activate();

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
		}

		UI.Xaml.Window CreateNativeWindow(MauiApplicationContext applicationContext, UI.Xaml.LaunchActivatedEventArgs? args = null)
		{
			var winuiWndow = new MauiWinUIWindow();

			var mauiContext = new MauiContext(Services, winuiWndow);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, args);
			var window = Application.CreateWindow(activationState);

			winuiWndow.SetWindowHandler(window, mauiContext);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnWindowCreated>(del => del(winuiWndow));

			return winuiWndow;
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; protected set; } = null!;

		public UI.Xaml.Window MainWindow { get; protected set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
