using Foundation;
using UIKit;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycle
	{
		public delegate bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler);
		public delegate void DidEnterBackground(UIApplication application);
		public delegate bool FinishedLaunching(UIApplication application, NSDictionary launchOptions);
		public delegate void OnActivated(UIApplication application);
		public delegate void OnResignActivation(UIApplication application);
		public delegate bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options);
		public delegate void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler);
		public delegate void WillEnterForeground(UIApplication application);
		public delegate void WillTerminate(UIApplication application);
	}
}