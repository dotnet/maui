using Foundation;
using UIKit;
using Microsoft.Maui;

#if !NET6_0
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.iOS;
#endif

using Maui.Controls.Sample;

namespace Sample.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate<MyApp>
	{

	}
}