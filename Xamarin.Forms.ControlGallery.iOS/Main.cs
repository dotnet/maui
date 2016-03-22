using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Controls;
#if __UNIFIED__
using UIKit;
using Foundation;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

namespace Xamarin.Forms.ControlGallery.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string [] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			#if __UNIFIED__
			if (!Debugger.IsAttached)
				Insights.Initialize (App.Secrets["InsightsApiKey"]);
			#endif
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}
