using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ToolbarTests : BaseTestFixture
	{
		[Test]
		public void ToolbarExistsForNavigationPage()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			Assert.IsNotNull(toolbarElement.Toolbar);
		}

		[Test]
		public void ToolbarEmptyForContentPage()
		{
			Window window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new ContentPage();
			window.Page = startingPage;
			Assert.IsNull(toolbarElement.Toolbar);
		}

		[Test]
		public void ToolbarClearsWhenNavigationPageRemoved()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			window.Page = new ContentPage();
			Assert.IsNull(toolbarElement.Toolbar);
		}

		[Test]
		public async Task TitleAndTitleViewAreMutuallyExclusive()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var contentPage = new ContentPage() { Title = "Test Title" };
			var navigationPage = new NavigationPage(contentPage);
			window.Page = navigationPage;

			var titleView = new VerticalStackLayout();
			var toolbar = (Toolbar)toolbarElement.Toolbar;
			Assert.AreEqual("Test Title", toolbar.Title);
			NavigationPage.SetTitleView(contentPage, titleView);
			Assert.IsEmpty(toolbar.Title);
			Assert.AreEqual(titleView, toolbar.TitleView);
			NavigationPage.SetTitleView(contentPage, null);
			Assert.AreEqual("Test Title", toolbar.Title);
		}

		[Test]
		public async Task InsertPageBeforeRootPageShowsBackButton()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new TestNavigationPage(true, new ContentPage());
			window.Page = startingPage;
			startingPage.Navigation.InsertPageBefore(new ContentPage(), startingPage.RootPage);
			await Task.Delay(50);
			Assert.True(toolbarElement.Toolbar.BackButtonVisible);
		}

		[Test]
		public async Task RemoveRootPageHidesBackButton()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new TestNavigationPage(true, new ContentPage());
			window.Page = startingPage;
			startingPage.Navigation.PushAsync(new ContentPage());
			startingPage.Navigation.RemovePage(startingPage.RootPage);
			await Task.Delay(50);
			Assert.False(toolbarElement.Toolbar.BackButtonVisible);
		}

		[Test]
		public void BackButtonNotVisibleForInitialPage()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			Assert.False(toolbarElement.Toolbar.BackButtonVisible);
		}


		[Test]
		public void NestedNavigation_AppliesFromMostInnerNavigationPage()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var visibleInnerNavigationPage = new NavigationPage(new ContentPage()) { Title = "visibleInnerNavigationPage" };
			var nonVisibleNavigationPage = new NavigationPage(new ContentPage()) { Title = "nonVisibleNavigationPage" };
			var tabbedPage = new TabbedPage()
			{
				Children =
				{
					visibleInnerNavigationPage,
					nonVisibleNavigationPage
				}
			};

			var outerNavigationPage = new NavigationPage(tabbedPage) { Title = "outerNavigationPage" };
			window.Page = outerNavigationPage;

			var toolbar = (Toolbar)toolbarElement.Toolbar;

			NavigationPage.SetHasNavigationBar(tabbedPage, false);
			NavigationPage.SetHasNavigationBar(nonVisibleNavigationPage.CurrentPage, false);

			Assert.True(toolbar.IsVisible);

			NavigationPage.SetHasNavigationBar(visibleInnerNavigationPage.CurrentPage, false);

			Assert.False(toolbar.IsVisible);

			NavigationPage.SetHasNavigationBar(visibleInnerNavigationPage.CurrentPage, true);

			Assert.True(toolbar.IsVisible);
		}

		[Test]
		public void NestedNavigation_ChangingToTabWithNoNavigationPage()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var innerNavigationPage =
				new NavigationPage(new ContentPage() { Content = new Label() }) { Title = "innerNavigationPage" };

			var contentPage = new ContentPage() { Title = "contentPage" };
			var tabbedPage = new TabbedPage()
			{
				Children =
				{
					innerNavigationPage,
					contentPage
				}
			};

			var outerNavigationPage = new NavigationPage(tabbedPage) { Title = "outerNavigationPage" };
			window.Page = outerNavigationPage;

			var toolbar = (Toolbar)toolbarElement.Toolbar;
			Assert.True(toolbar.IsVisible);

			tabbedPage.CurrentPage = contentPage;

			Assert.True(toolbar.IsVisible);

			// Validate that changes to non visible navigation page don't propagate to titlebar
			NavigationPage.SetHasNavigationBar(innerNavigationPage.CurrentPage, false);
			Assert.True(toolbar.IsVisible);

			NavigationPage.SetHasNavigationBar(contentPage, false);
			Assert.False(toolbar.IsVisible);
		}

		[Test]
		public void NestedNavigation_NestedNavigationPage()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var innerNavigationPage =
				new NavigationPage(new ContentPage() { Content = new Label() }) { Title = "innerNavigationPage" };

			var contentPage = new ContentPage() { Title = "contentPage" };
			var tabbedPage = new TabbedPage()
			{
				Children =
				{
					innerNavigationPage,
					contentPage
				}
			};

			var outerNavigationPage = new NavigationPage(tabbedPage) { Title = "outerNavigationPage" };
			window.Page = outerNavigationPage;

			var toolbar = (Toolbar)toolbarElement.Toolbar;
			Assert.True(toolbar.IsVisible);

			tabbedPage.CurrentPage = contentPage;

			Assert.True(toolbar.IsVisible);

			// Validate that changes to non visible navigation page don't propagate to titlebar
			NavigationPage.SetHasNavigationBar(innerNavigationPage.CurrentPage, false);
			Assert.True(toolbar.IsVisible);

			NavigationPage.SetHasNavigationBar(contentPage, false);
			Assert.False(toolbar.IsVisible);
		}

		[Test]
		public async Task NestedNavigation_BackButtonVisibleIfAnyoneHasPages()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var innerNavigationPage =
				new NavigationPage(new ContentPage() { Content = new Label() }) { Title = "innerNavigationPage" };

			var contentPage = new ContentPage() { Title = "contentPage" };
			var tabbedPage = new TabbedPage()
			{
				Children =
				{
					contentPage,
					innerNavigationPage,
				}
			};

			var outerNavigationPage = new NavigationPage(new ContentPage()) { Title = "outerNavigationPage" };
			window.Page = outerNavigationPage;
			var toolbar = (Toolbar)toolbarElement.Toolbar;

			// push Tabbed Page on to the stack of the out nagivation page
			await outerNavigationPage.PushAsync(tabbedPage);
			Assert.True(toolbar.BackButtonVisible);

			tabbedPage.CurrentPage = innerNavigationPage;

			// even though the inner navigation page has no stack the outer one does
			// so we want to still display the navigation page
			Assert.True(toolbar.BackButtonVisible);

			await outerNavigationPage.PopAsync();
			Assert.False(toolbar.BackButtonVisible);
		}
	}
}