using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41778, "Slider Inside ScrollView Will Open FlyoutPage.Flyout", PlatformAffected.iOS)]
	public class Bugzilla41778 : TestFlyoutPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Flyout = new ContentPage
			{
				Title = "Menu",
				BackgroundColor = Colors.Blue
			};

			Detail = new DetailPage41778();
		}

		[Preserve(AllMembers = true)]
		class DetailPage41778 : ContentPage
		{
			public DetailPage41778()
			{
				var stackLayout = new StackLayout
				{
					Spacing = 20,
					Margin = 20,
					BackgroundColor = Colors.Beige,
					Orientation = StackOrientation.Vertical,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.CenterAndExpand
				};

				var label = new Label
				{
					Text = "This test is originally intended to be run on an iPad. Slide the slider back and forth quickly. Make sure that the slider thumb is moving along with your gesture."
					+ " Verify that the master detail menu does not open.",
					LineBreakMode = LineBreakMode.WordWrap,
					MaxLines = 4
				};
				stackLayout.Children.Add(label);

				var scrollView = new ScrollView { Content = new Slider() };
				scrollView.On<iOS>().SetShouldDelayContentTouches(false);
				stackLayout.Children.Add(scrollView);

				Content = stackLayout;
			}
		}
	}
}