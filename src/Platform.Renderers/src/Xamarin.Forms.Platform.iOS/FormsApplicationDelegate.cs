using System;
using System.ComponentModel;
using System.Globalization;
using CoreSpotlight;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class FormsApplicationDelegate : UIApplicationDelegate
	{
		Application _application;
		bool _isSuspended;
		UIWindow _window;
		public override UIWindow Window
		{
			get
			{
				return _window;
			}
			set
			{
				_window = value;
			}
		}

		protected FormsApplicationDelegate()
		{
		}

		public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			CheckForAppLink(userActivity);
			return true;
		}

		// now in background
		public override void DidEnterBackground(UIApplication uiApplication)
		{
			// applicationDidEnterBackground
		}

		// finish initialization before display to user
		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{
			// check contents of launch options and evaluate why the app was launched and respond
			// initialize the important data structures
			// prepare you apps window and views for display
			// keep lightweight, anything long winded should be executed asynchronously on a secondary thread.
			// application:didFinishLaunchingWithOptions
			if (Window == null)
				Window = new UIWindow(UIScreen.MainScreen.Bounds);

			if (_application == null)
				throw new InvalidOperationException("You MUST invoke LoadApplication () before calling base.FinishedLaunching ()");

			SetMainPage();
			_application.SendStart();
			return true;
		}

		// about to become foreground, last minute preparatuin
		public override void OnActivated(UIApplication uiApplication)
		{
			// applicationDidBecomeActive
			// execute any OpenGL ES drawing calls
			if (_application != null && _isSuspended)
			{
				_isSuspended = false;
				CultureInfo.CurrentCulture.ClearCachedData();
				TimeZoneInfo.ClearCachedData();
				_application.SendResume();
			}
		}

		// transitioning to background
		public override void OnResignActivation(UIApplication uiApplication)
		{
			// applicationWillResignActive
			if (_application != null)
			{
				_isSuspended = true;
				_application.SendSleep();
			}
		}

		public override void UserActivityUpdated(UIApplication application, NSUserActivity userActivity)
		{
			CheckForAppLink(userActivity);
		}

		// from background to foreground, not yet active
		public override void WillEnterForeground(UIApplication uiApplication)
		{
			// applicationWillEnterForeground
		}

		// TODO where to execute heavy code, storing state, sending to server, etc 

		// first chance to execute code at launch time
		public override bool WillFinishLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{
			// check contents of launch options and evaluate why the app was launched and respond
			// initialize the important data structures
			// prepare you apps window and views for display
			// keep lightweight, anything long winded should be executed asynchronously on a secondary thread.
			// application:willFinishLaunchingWithOptions
			// Restore ui state here
			return true;
		}

		// app is being terminated, not called if you app is suspended
		public override void WillTerminate(UIApplication uiApplication)
		{
			// applicationWillTerminate
			//application.SendTerminate ();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _application != null)
			{
				_application.PropertyChanged -= ApplicationOnPropertyChanged;
				_application.PropertyChanging -= ApplicationOnPropertyChanging;
			}

			base.Dispose(disposing);
		}

		protected void LoadApplication(Application application)
		{
			if (application == null)
				throw new ArgumentNullException("application");

			Application.SetCurrentApplication(application);
			_application = application;
			(application as IApplicationController)?.SetAppIndexingProvider(new IOSAppIndexingProvider());

			application.PropertyChanged += ApplicationOnPropertyChanged;
			application.PropertyChanging += ApplicationOnPropertyChanging;
		}

		void ApplicationOnPropertyChanging(object sender, PropertyChangingEventArgs args)
		{
			if (args.PropertyName == nameof(_application.MainPage))
				UpdatingMainPage();
		}

		void ApplicationOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == "MainPage")
				UpdateMainPage();
		}

		void CheckForAppLink(NSUserActivity userActivity)
		{
			var strLink = string.Empty;

			switch (userActivity.ActivityType)
			{
				case "NSUserActivityTypeBrowsingWeb":
					strLink = userActivity.WebPageUrl.AbsoluteString;
					break;
				case "com.apple.corespotlightitem":
					if (userActivity.UserInfo.ContainsKey(CSSearchableItem.ActivityIdentifier))
						strLink = userActivity.UserInfo.ObjectForKey(CSSearchableItem.ActivityIdentifier).ToString();
					break;
				default:
					if (userActivity.UserInfo.ContainsKey(new NSString("link")))
						strLink = userActivity.UserInfo[new NSString("link")].ToString();
					break;
			}

			if (!string.IsNullOrEmpty(strLink))
				_application.SendOnAppLinkRequestReceived(new Uri(strLink));
		}

		void SetMainPage()
		{
			UpdateMainPage();
			Window.MakeKeyAndVisible();
		}

		void UpdatingMainPage()
		{
			if (_application.MainPage == null)
				return;

			var platformRenderer = Window.RootViewController as PlatformRenderer;
			platformRenderer.Platform.MarkForRemoval();
		}

		void UpdateMainPage()
		{
			if (_application.MainPage == null)
				return;

			var platformRenderer = Window.RootViewController as PlatformRenderer;
			if (platformRenderer != null)
				((IDisposable)platformRenderer.Platform).Dispose();

			Window.RootViewController = _application.MainPage.CreateViewController();
		}
	}
}