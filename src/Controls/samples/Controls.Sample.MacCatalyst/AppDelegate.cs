using Foundation;
using UIKit;
using Microsoft.Maui;

#if !NET6_0
using Microsoft.Maui.Controls;
#endif

using MauiControlsSample;

namespace Sample.MacCatalyst
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate<Startup>
	{
	}
}