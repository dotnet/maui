using Foundation;
using UIKit;

namespace Maui.Controls.Sample.Platform
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{

			return base.FinishedLaunching(uiApplication, launchOptions);
		}
	}
}