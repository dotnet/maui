using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui
{
	public abstract class MauiUIApplicationDelegate : UIApplicationDelegate, IUIApplicationDelegate
	{
		WeakReference<IWindow>? _virtualWindow;
		internal IWindow? VirtualWindow
		{
			get
			{
				IWindow? window = null;
				_virtualWindow?.TryGetTarget(out window);
				return window;
			}
		}

		protected MauiUIApplicationDelegate()
		{
			Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var mauiApp = CreateMauiApp();

			Services = mauiApp.Services;

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.WillFinishLaunching>(del => del(application, launchOptions));

			return true;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Application = Services.GetRequiredService<IApplication>();

			var uiWindow = CreateNativeWindow();

			Window = uiWindow;

			Window.MakeKeyAndVisible();

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.FinishedLaunching>(del => del(application, launchOptions));

			return true;
		}

		UIWindow CreateNativeWindow()
		{
			var uiWindow = new UIWindow();

			var mauiContext = new MauiContext(Services, uiWindow);

			Services.InvokeLifecycleEvents<iOSLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext);
			var window = Application.CreateWindow(activationState);
			_virtualWindow = new WeakReference<IWindow>(window);
			uiWindow.SetWindow(window, mauiContext);

			return uiWindow;
		}

		public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.PerformActionForShortcutItem>(del => del(application, shortcutItem, completionHandler));
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
		{
			var wasHandled = false;

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.OpenUrl>(del =>
			{
				wasHandled = del(application, url, options) || wasHandled;
			});

			return wasHandled || base.OpenUrl(application, url, options);
		}

		public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			var wasHandled = false;

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.ContinueUserActivity>(del =>
			{
				wasHandled = del(application, userActivity, completionHandler) || wasHandled;
			});

			return wasHandled || base.ContinueUserActivity(application, userActivity, completionHandler);
		}

		public override void OnActivated(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.OnActivated>(del => del(application));
		}

		public override void OnResignActivation(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.OnResignActivation>(del => del(application));
		}

		public override void WillTerminate(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.WillTerminate>(del => del(application));
		}

		public override void DidEnterBackground(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.DidEnterBackground>(del => del(application));
		}

		public override void WillEnterForeground(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.WillEnterForeground>(del => del(application));
		}

		public static MauiUIApplicationDelegate Current { get; private set; } = null!;

		public override UIWindow? Window { get; set; }

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
