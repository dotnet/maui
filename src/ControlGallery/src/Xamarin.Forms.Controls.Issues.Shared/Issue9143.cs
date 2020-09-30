using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.FlyoutPage)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9143, "[Android] Fix BottomNavigationItemView issue with FlyoutPage", PlatformAffected.Android)]
	public class Issue9143 : TestFlyoutPage
	{
		protected override void Init()
		{
			Title = "Issue 9143";

			DoNavigation();
		}

		void DoNavigation()
		{
			var tab1 = GetNavigationPage("One");
			var tab2 = GetNavigationPage("Two");
			var tab3 = GetNavigationPage("Three");
			var tab4 = GetNavigationPage("Four");
			var tab5 = GetNavigationPage("Five");

			var menuNavigationPage = GetNavigationPage("Menu");
			CreateAndPushPageForNavigationPage(menuNavigationPage);

			var tabbedPage = new TabbedPage();
			tabbedPage.Children.Add(tab1);
			tabbedPage.Children.Add(tab2);
			tabbedPage.Children.Add(tab3);
			tabbedPage.Children.Add(tab4);
			tabbedPage.Children.Add(tab5);

			tabbedPage.Title = "tabbed";

			tabbedPage.On<PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
			tabbedPage.On<PlatformConfiguration.Android>().SetIsSwipePagingEnabled(false);
			tabbedPage.On<PlatformConfiguration.Android>().SetIsSmoothScrollEnabled(false);
			tabbedPage.On<PlatformConfiguration.Android>().SetOffscreenPageLimit(4);

			CreateAndPushPageForNavigationPage(tab1);
			CreateAndPushPageForNavigationPage(tab2);
			CreateAndPushPageForNavigationPage(tab3);
			CreateAndPushPageForNavigationPage(tab4);
			CreateAndPushPageForNavigationPage(tab5);

			Flyout = menuNavigationPage;
			Detail = tabbedPage;
		}

		NavigationPage GetNavigationPage(string title)
		{
			var navigationPage = new NavigationPage
			{
				Title = title
			};
			return navigationPage;
		}

		void CreateAndPushPageForNavigationPage(NavigationPage navigationPage)
		{
			var aPage = new ContentPage
			{
				Title = navigationPage.Title,
				Content = new Label
				{
					Text = navigationPage.Title
				}
			};
			navigationPage.PushAsync(aPage).Wait();
		}
	}
}