using System;
using System.Collections.Generic;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui
{
	public abstract class MauiUIApplicationDelegate : UIApplicationDelegate, IUIApplicationDelegate
	{
		internal const string MauiSceneConfigurationKey = "__MAUI_DEFAULT_SCENE_CONFIGURATION__";

		MauiContext _applicationContext = null!;
		WeakReference<IWindow>? _virtualWindow;

		internal IWindow? VirtualWindow
		{
			get
			{
				IWindow? window = null;
				_virtualWindow?.TryGetTarget(out window);
				return window;
			}
			set
			{
				if (value != null)
				{
					if (_virtualWindow == null)
						_virtualWindow = new WeakReference<IWindow>(value);
					else
						_virtualWindow.SetTarget(value);
				}
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

			_applicationContext = new MauiContext(Services, this);

			Services?.InvokeLifecycleEvents<iOSLifecycle.WillFinishLaunching>(del => del(application, launchOptions));

			return true;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, _applicationContext);

			// iOS 13+ uses scene delegates
			if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
			{
				var dicts = new List<NSDictionary>();

				if (application?.UserActivity?.UserInfo != null)
					dicts.Add(application.UserActivity.UserInfo);
				if (launchOptions != null)
					dicts.Add(launchOptions);

				var w = CreateNativeWindow(null, dicts.ToArray());

				Window = w.nativeWIndow;
				VirtualWindow = w.virtualWindow;

				Window.MakeKeyAndVisible();
			}

			Services?.InvokeLifecycleEvents<iOSLifecycle.FinishedLaunching>(del => del(application!, launchOptions!));

			return true;
		}

		public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
			=> new ("Default Configuration", connectingSceneSession.Role);

		internal (UIWindow nativeWIndow, IWindow virtualWindow) CreateNativeWindow(UIWindowScene? windowScene = null, NSDictionary[]? states = null)
		{
			var uiWindow = windowScene != null
				? new UIWindow(windowScene)
				: new UIWindow();

			var mauiContext = _applicationContext.MakeScoped(uiWindow);

			Services?.InvokeLifecycleEvents<iOSLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, states);
			var mauiWindow = Application.CreateWindow(activationState);
			_virtualWindow = new WeakReference<IWindow>(mauiWindow);

			uiWindow.SetWindowHandler(mauiWindow, mauiContext);

			return (uiWindow, mauiWindow);
		}

		public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.PerformActionForShortcutItem>(del => del(application, shortcutItem, completionHandler));
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
		{
			var wasHandled = false;

			Services?.InvokeLifecycleEvents<iOSLifecycle.OpenUrl>(del =>
			{
				wasHandled = del(application, url, options) || wasHandled;
			});

			return wasHandled || base.OpenUrl(application, url, options);
		}

		public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			var wasHandled = false;

			Services?.InvokeLifecycleEvents<iOSLifecycle.ContinueUserActivity>(del =>
			{
				wasHandled = del(application, userActivity, completionHandler) || wasHandled;
			});

			return wasHandled || base.ContinueUserActivity(application, userActivity, completionHandler);
		}

		public override void OnActivated(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.OnActivated>(del => del(application));
		}

		public override void OnResignActivation(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.OnResignActivation>(del => del(application));
		}

		public override void WillTerminate(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.WillTerminate>(del => del(application));
		}

		public override void DidEnterBackground(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.DidEnterBackground>(del => del(application));
		}

		public override void WillEnterForeground(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.WillEnterForeground>(del => del(application));
		}

		public static MauiUIApplicationDelegate Current { get; private set; } = null!;

		public override UIWindow? Window { get; set; }

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
