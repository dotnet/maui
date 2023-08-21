//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 46494, "Hardware/Software back button from MainPage of type FlyoutPage causes crash 'java.lang.IllegalStateException: Activity has been destroyed'", PlatformAffected.Android)]
	public class Bugzilla46494 : TestFlyoutPage
	{
		protected override void Init()
		{
			Flyout = new ContentPage { Title = "Flyout", BackgroundColor = Colors.Blue };
			Detail = new NavigationPage(
				new ContentPage
				{
					Title = "Detail",
					BackgroundColor = Colors.Red,
					Content = new ContentView
					{
						Content = new Label
						{
							Text = "Hit Back button to destroy Activity. Disposing Fragment should not run into a race condition with Activity destroy.",
							HorizontalTextAlignment = TextAlignment.Center,
							VerticalTextAlignment = TextAlignment.Center
						}
					}
				}
			);
		}
	}
}
