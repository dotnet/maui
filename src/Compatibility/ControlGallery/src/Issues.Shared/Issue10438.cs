using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10438, "NavigationPage.HideNavigationBarSeparator doesn't work on iOS 13.4", PlatformAffected.iOS)]
	public class Issue10438 : TestNavigationPage
	{
		public Issue10438()
		{
			BarBackgroundColor = Color.Cornsilk;
			On<iOS>().SetPrefersLargeTitles(true);

			var page = new ContentPage
			{
				Title = "Issue 10438"
			};

			var layout = new StackLayout();

			var hideButton = new Button
			{
				Text = "Hide NavigationBarSeparator"
			};

			hideButton.Clicked += (sender, args) =>
			{
				On<iOS>().SetHideNavigationBarSeparator(true);
			};

			var showButton = new Button
			{
				Text = "Hide NavigationBarSeparator"
			};

			showButton.Clicked += (sender, args) =>
			{
				On<iOS>().SetHideNavigationBarSeparator(false);
			};

			layout.Children.Add(hideButton);
			layout.Children.Add(showButton);

			page.Content = layout;

			PushAsync(page);
		}

		protected override void Init()
		{

		}
	}
}