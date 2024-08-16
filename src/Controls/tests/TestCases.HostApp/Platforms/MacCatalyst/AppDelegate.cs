using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;

namespace Maui.Controls.Sample.Platform
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}