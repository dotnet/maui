using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.iOSSpecific;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
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