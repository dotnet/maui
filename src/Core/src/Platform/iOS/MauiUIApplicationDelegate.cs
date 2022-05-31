using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public abstract partial class MauiUIApplicationDelegate : UIResponder, IUIApplicationDelegate, IPlatformApplication
	{
		internal const string MauiSceneConfigurationKey = "__MAUI_DEFAULT_SCENE_CONFIGURATION__";
		internal const string GetConfigurationSelectorName = "application:configurationForConnectingSceneSession:options:";

		IMauiContext _applicationContext = null!;

		protected MauiUIApplicationDelegate() : base()
		{
			Current = this;
			IPlatformApplication.Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		[Export("application:willFinishLaunchingWithOptions:")]
		public virtual bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			_applicationContext = rootContext.MakeApplicationScope(this);

			Services = _applicationContext.Services;

			Services?.InvokeLifecycleEvents<iOSLifecycle.WillFinishLaunching>(del => del(application, launchOptions));

			return true;
		}

		[Export("application:didFinishLaunchingWithOptions:")]
		public virtual bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, _applicationContext);

			// if there is no scene delegate or support for scene delegates, then we set up the window here
			if (!this.HasSceneManifest())
				this.CreatePlatformWindow(Application, application, launchOptions);

			Services?.InvokeLifecycleEvents<iOSLifecycle.FinishedLaunching>(del => del(application!, launchOptions!));

			return true;
		}

		public override bool RespondsToSelector(Selector? sel)
		{
			// if the app is not a multi-window app, then we cannot override the GetConfiguration method
			if (sel?.Name == GetConfigurationSelectorName && !this.HasSceneManifest())
				return false;

			return base.RespondsToSelector(sel);
		}

		[Export("application:configurationForConnectingSceneSession:options:")]
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.1")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.1")]
		public virtual UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
			=> new(MauiUIApplicationDelegate.MauiSceneConfigurationKey, connectingSceneSession.Role);

		[Export("application:performActionForShortcutItem:completionHandler:")]
		public virtual void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.PerformActionForShortcutItem>(del => del(application, shortcutItem, completionHandler));
		}

		[Export("application:openURL:options:")]
		public virtual bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
		{
			var wasHandled = false;

			Services?.InvokeLifecycleEvents<iOSLifecycle.OpenUrl>(del =>
			{
				wasHandled = del(application, url, options) || wasHandled;
			});

			return wasHandled;
		}

		[Export("application:continueUserActivity:restorationHandler:")]
		public virtual bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			var wasHandled = false;

			Services?.InvokeLifecycleEvents<iOSLifecycle.ContinueUserActivity>(del =>
			{
				wasHandled = del(application, userActivity, completionHandler) || wasHandled;
			});

			return wasHandled;
		}

		[Export("applicationDidBecomeActive:")]
		public virtual void OnActivated(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.OnActivated>(del => del(application));
		}

		[Export("applicationWillResignActive:")]
		public virtual void OnResignActivation(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.OnResignActivation>(del => del(application));
		}

		[Export("sceneDidDisconnect:")]
		public void DidDisconnect(UIScene scene)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.SceneDidDisconnect>(del => del(scene));
		}

		[Export("applicationWillTerminate:")]
		public virtual void WillTerminate(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.WillTerminate>(del => del(application));
		}

		[Export("applicationDidEnterBackground:")]
		public virtual void DidEnterBackground(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.DidEnterBackground>(del => del(application));
		}

		[Export("applicationWillEnterForeground:")]
		public virtual void WillEnterForeground(UIApplication application)
		{
			Services?.InvokeLifecycleEvents<iOSLifecycle.WillEnterForeground>(del => del(application));
		}

		public static MauiUIApplicationDelegate Current { get; private set; } = null!;

		[Export("window")]
		public virtual UIWindow? Window { get; set; }

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}
