using Foundation;
using ObjCRuntime;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

#if !NET6_0
using Microsoft.Maui.Controls;
#endif

using Maui.Controls.Sample;

namespace Sample.MacCatalyst
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}