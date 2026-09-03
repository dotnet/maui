using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class ElementExtensions
	{
		const string UIApplicationSceneManifestKey = "UIApplicationSceneManifest";

		public static UIViewController ToUIViewController(this IElement? view, IMauiContext context)
		{
			// The returned value is not used here, but this method is used to set 
			// up the platform view and handler. So, do not delete!
			var _ = view?.ToPlatform(context);
			if (view?.Handler is IPlatformViewHandler nvh && nvh.ViewController != null)
				return nvh.ViewController;

			return new ContainerViewController { CurrentView = view, Context = context };
		}

		// If < iOS 13 or the Info.plist does not have a scene manifest entry we need to assume no multi window, and no UISceneDelegate.
		// We cannot check for iPads/Mac because even on the iPhone it uses the scene delegate if one is specified in the manifest.
		public static bool HasSceneManifest(this IUIApplicationDelegate platformApplication) =>
			(OperatingSystem.IsIOSVersionAtLeast(13, 0) || OperatingSystem.IsTvOSVersionAtLeast(13, 0)) &&
			NSBundle.MainBundle.InfoDictionary.ContainsKey(new NSString(UIApplicationSceneManifestKey));
	}
}
