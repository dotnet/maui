using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public abstract class MauiWinUIApplication : UI.Xaml.Application
	{
		IMauiContext? applicationContext;

		protected abstract MauiApp CreateMauiApp();

		protected override void OnLaunched(UI.Xaml.LaunchActivatedEventArgs args)
		{
			LaunchActivatedEventArgs = args;

			var mauiApp = CreateMauiApp();

			applicationContext = new MauiContext(mauiApp.Services, this);

			Services = mauiApp.Services;

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, applicationContext);

			var winuiWndow = CreateNativeWindow(args);

			MainWindow = winuiWndow;

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
		}

		internal UI.Xaml.Window CreateNativeWindow(IPersistedState state) =>
			CreateNativeWindow((mauiContext) => new ActivationState(mauiContext, state));

		UI.Xaml.Window CreateNativeWindow(UI.Xaml.LaunchActivatedEventArgs args) =>
			CreateNativeWindow((mauiContext) => new ActivationState(mauiContext, args));

		UI.Xaml.Window CreateNativeWindow(Func<IMauiContext, IActivationState> create)
		{
			var winuiWndow = new MauiWinUIWindow();

			var mauiContext = applicationContext!.MakeScoped(winuiWndow);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = create(mauiContext);

			var window = Application.CreateWindow(activationState);

			winuiWndow.SetWindowHandler(window, mauiContext);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnWindowCreated>(del => del(winuiWndow));

			winuiWndow.Activate();

			return winuiWndow;
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; protected set; } = null!;

		public UI.Xaml.Window MainWindow { get; protected set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
