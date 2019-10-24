using System.Diagnostics;
using UIKit;
using Xamarin.Forms.Controls;

namespace Xamarin.Forms.ControlGallery.iOS
{
	public class Application
	{
		static void Main(string[] args)
		{
			if (!Debugger.IsAttached)
				Insights.Initialize(App.InsightsApiKey);
			UIApplication.Main(args, null, "AppDelegate");
		}
	}
}
