using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7701, "[Bug] iOS Large SetPrefersLargeTitles(true) Uses Incorrect BarBackgroundColor", PlatformAffected.iOS)]
	public class Issue7701 : TestNavigationPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var contentPage = new ContentPage
			{
				Title = "Content Page Title",
				Content = new ScrollView
				{
					Content = new Label
					{
						Margin = new Thickness(10),
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						LineBreakMode = LineBreakMode.WordWrap,
						Text = "Verify that the navigation bar background color is green. Scroll up to shrink navigation bar size. Verify that it stays green. Verify that the bar text color is always red."
					}
				}
			};

			BarBackgroundColor = Colors.Green;
			BarTextColor = Colors.Red;

			On<iOS>().SetPrefersLargeTitles(true);

			PushAsync(contentPage);
		}
	}
}