using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7701, "[Bug] iOS Large SetPrefersLargeTitles(true) Uses Incorrect BarBackgroundColor", PlatformAffected.iOS)]
	public class Issue7701 : TestNavigationPage // or TestMasterDetailPage, etc ...
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

			BarBackgroundColor = Color.Green;
			BarTextColor = Color.Red;

			On<iOS>().SetPrefersLargeTitles(true);

			PushAsync(contentPage);
		}
	}
}