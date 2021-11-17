using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public abstract class MauiUIApplicationDelegate : UIApplicationDelegate, IUIApplicationDelegate
	{
		internal const string MauiSceneConfigurationKey = "__MAUI_DEFAULT_SCENE_CONFIGURATION__";

		IMauiContext _applicationContext = null!;

		protected MauiUIApplicationDelegate()
		{
			Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			_applicationContext = rootContext.MakeApplicationScope(this);

			Services = _applicationContext.Services;

			Services?.InvokeLifecycleEvents<iOSLifecycle.WillFinishLaunching>(del => del(application, launchOptions));

			return true;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, _applicationContext);

			// If < iOS 13, or we're not on mac/ipad, or the Info.plist does not have a scene manifest entry
			// we need to assume no multi window, and no UISceneDelegate
			if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0)
				|| (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Mac
					&& UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad)
				|| !NSBundle.MainBundle.InfoDictionary.ContainsKey(new NSString("UIApplicationSceneManifest")))
			{
				this.CreateNativeWindow(Application, application, launchOptions);
			}

			Services?.InvokeLifecycleEvents<iOSLifecycle.FinishedLaunching>(del => del(application!, launchOptions!));

			return true;
		}

		public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
			=> new(MauiUIApplicationDelegate.MauiSceneConfigurationKey, connectingSceneSession.Role);

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
