using Foundation;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.MacCatalyst
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}