using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;

#if !NET6_0_OR_GREATER
using Microsoft.Maui.Controls;
#endif

namespace Maui.Controls.Sample.Platform
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}