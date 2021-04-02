using Foundation;
using UIKit;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycle
	{
		public delegate bool FinishedLaunching(UIApplication application, NSDictionary launchOptions);
		public delegate void OnActivated(UIApplication application);
		public delegate void OnResignActivation(UIApplication application);
		public delegate void WillTerminate(UIApplication application);
		public delegate void DidEnterBackground(UIApplication application);
		public delegate void WillEnterForeground(UIApplication application);
	}
}