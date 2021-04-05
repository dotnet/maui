using UIKit;

namespace Maui.Controls.Sample.SingleProject
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			System.Console.WriteLine("asdasdasd-iOS-main");
			System.Diagnostics.Debug.WriteLine("eiloneilon-iOS-main");
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}
	}
}