using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Maui.Controls.Sample.Platform
{
	[Register("AppDelegate")]
	[Experimental ("XCODE_26_0_PREVIEW")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}