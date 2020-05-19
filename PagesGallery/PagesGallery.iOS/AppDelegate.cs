using Foundation;
using UIKit;
using System.Maui;
using System.Maui.Platform.iOS;

namespace PagesGallery.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			System.Maui.Maui.Init();
#if __XCODE11__
			FormsMaterial.Init();
#endif
			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}