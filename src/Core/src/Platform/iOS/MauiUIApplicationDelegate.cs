using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui
{
	public abstract class MauiUIWindowSceneDelegate : UIWindowSceneDelegate
	{
		WeakReference<IWindow>? _virtualWindow;

		internal IWindow? VirtualWindow
			=> (_virtualWindow?.TryGetTarget(out var w) ?? false) ? w : null;

		public override UIWindow? Window { get; set; }

		public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			if (scene is UIWindowScene windowScene)
			{
				var restoreUserActivity = session?.StateRestorationActivity;

				var uiWindow = new UIWindow();
				var mauiContext = new MauiContext(MauiUIApplicationDelegate.Current.Services, uiWindow);

				MauiUIApplicationDelegate.Current.Services.InvokeLifecycleEvents<iOSLifecycle.OnMauiContextCreated>(del => del(mauiContext));

				var activationState = new ActivationState(mauiContext, restoreUserActivity);
				var window = MauiUIApplicationDelegate.Current.Application.CreateWindow(activationState);

				_virtualWindow = new WeakReference<IWindow>(window);
				uiWindow.SetWindow(window, mauiContext);

				Window = uiWindow;

				// Set the WindowScene
				Window.WindowScene = windowScene;
				Window.MakeKeyAndVisible();

				window.RestoredState(new RestoredState(mauiContext, restoreUserActivity));

				MauiUIApplicationDelegate.Current.Services.InvokeLifecycleEvents<iOSLifecycle.WillConnect>(del => del(scene, session, connectionOptions));
			}
		}

		public override NSUserActivity? GetStateRestorationActivity(UIScene scene)
		{
			var userActivity = new NSUserActivity(VirtualWindow?.GetType()?.FullName ?? "DEFAULT");
			MauiUIApplicationDelegate.Current.Services.InvokeLifecycleEvents<iOSLifecycle.GetStateRestorationActivity>(del => del(scene, userActivity));

			// Call the SavingState since the activity is going to be destroyed
			if (VirtualWindow?.Handler?.MauiContext != null)
				VirtualWindow?.SavingState(new SaveableState(VirtualWindow.Handler.MauiContext, userActivity));

			return userActivity;
		}
	}

	public abstract class MauiUIApplicationDelegate : UIApplicationDelegate, IUIApplicationDelegate
	{
		WeakReference<IWindow>? _virtualWindow;

		internal IWindow? VirtualWindow
			=> (_virtualWindow?.TryGetTarget(out var w) ?? false) ? w : null;

		protected MauiUIApplicationDelegate()
		{
			Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
			=> new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);

		public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var mauiApp = CreateMauiApp();

			Services = mauiApp.Services;

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.WillFinishLaunching>(del => del(application, launchOptions));

			return true;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var result = true;

			Application = Services.GetRequiredService<IApplication>();

			// On older than iOS13 we need to make the window here still
			if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
			{
				var uiWindow = new UIWindow();
				var mauiContext = new MauiContext(Services, uiWindow);

				Services.InvokeLifecycleEvents<iOSLifecycle.OnMauiContextCreated>(del => del(mauiContext));

				var activationState = new ActivationState(mauiContext, application.UserActivity);
				var window = Application.CreateWindow(activationState);

				_virtualWindow = new WeakReference<IWindow>(window);
				uiWindow.SetWindow(window, mauiContext);

				Window = uiWindow;
				Window.MakeKeyAndVisible();

				result = base.FinishedLaunching(application, launchOptions);
			}

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.FinishedLaunching>(del => del(application, launchOptions));

			return result;
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
