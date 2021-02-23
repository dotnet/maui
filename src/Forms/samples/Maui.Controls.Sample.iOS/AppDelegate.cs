using Foundation;
using UIKit;
using Xamarin.Platform;

#if !NET6_0
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
#endif

using Maui.Controls.Sample;

namespace Sample.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate<MyApp>
	{

	}
}