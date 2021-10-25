using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Platform
{
	public static class ApplicationExtensions
	{
		public static void CreateNativeWindow(this UI.Xaml.Application nativeApplication, IApplication application, UI.Xaml.LaunchActivatedEventArgs? args)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var winuiWndow = new MauiWinUIWindow();

			var mauiContext = applicationContext!.MakeScoped(winuiWndow);

			applicationContext.Services.InvokeLifecycleEvents<WindowsLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, args);

			var window = application.CreateWindow(activationState);

			winuiWndow.SetWindowHandler(window, mauiContext);

			applicationContext.Services.InvokeLifecycleEvents<WindowsLifecycle.OnWindowCreated>(del => del(winuiWndow));

			winuiWndow.Activate();
		}
	}
}