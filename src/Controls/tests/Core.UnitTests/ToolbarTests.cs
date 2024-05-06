using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ToolbarTests : BaseTestFixture
	{
		[Fact]
		public void ToolbarExistsForNavigationPage()
		{
			var window = new TestWindow();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			Assert.NotNull(toolbarElement.Toolbar);
		}

		[Fact]
		public void ToolbarEmptyForContentPage()
		{
			Window window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new ContentPage();
			window.Page = startingPage;
			Assert.Null(toolbarElement.Toolbar);
		}

		[Fact]
		public void ToolbarClearsWhenNavigationPageRemoved()
		{
			var window = new Window();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			window.Page = new ContentPage();
			Assert.Null(toolbarElement.Toolbar);
		}

		[Fact]
		public async Task TitleAndTitleViewAreMutuallyExclusive()
		{
			var window = new TestWindow();
			IToolbarElement toolbarElement = window;
			var contentPage = new ContentPage() { Title = "Test Title" };
			var navigationPage = new NavigationPage(contentPage);
			window.Page = navigationPage;

			var titleView = new VerticalStackLayout();
			var toolbar = (Toolbar)toolbarElement.Toolbar;
			Assert.Equal("Test Title", toolbar.Title);
			NavigationPage.SetTitleView(contentPage, titleView);
			Assert.Empty(toolbar.Title);
			Assert.Equal(titleView, toolbar.TitleView);
			NavigationPage.SetTitleView(contentPage, null);
			Assert.Equal("Test Title", toolbar.Title);
		}

		[Fact]
		public async Task InsertPageBeforeRootPageShowsBackButton()
		{
			var window = new TestWindow();
			IToolbarElement toolbarElement = window;
			var startingPage = new TestNavigationPage(true, new ContentPage());
			window.Page = startingPage;
			startingPage.Navigation.InsertPageBefore(new ContentPage(), startingPage.RootPage);
			await Task.Delay(50);
			Assert.True(toolbarElement.Toolbar.BackButtonVisible);
		}

		[Fact]
		public async Task RemoveRootPageHidesBackButton()
		{
			var window = new TestWindow();
			IToolbarElement toolbarElement = window;
			var startingPage = new TestNavigationPage(true, new ContentPage());
			window.Page = startingPage;
			await startingPage.Navigation.PushAsync(new ContentPage());
			startingPage.Navigation.RemovePage(startingPage.RootPage);
			await Task.Delay(50);
			Assert.False(toolbarElement.Toolbar.BackButtonVisible);
		}

		[Fact]
		public void BackButtonNotVisibleForInitialPage()
		{
			var window = new TestWindow();
			IToolbarElement toolbarElement = window;
			var startingPage = new NavigationPage(new ContentPage());
			window.Page = startingPage;
			Assert.False(toolbarElement.Toolbar.BackButtonVisible);
		}


		[Fact]
		public void NestedNavigation_AppliesFromMostInnerNavigationPage()
		{
			var window = new TestWindow();
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

		[Fact]
		public void NestedNavigation_ChangingToTabWithNoNavigationPage()
		{
			var window = new TestWindow();
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

		[Fact]
		public void NestedNavigation_NestedNavigationPage()
		{
			var window = new TestWindow();
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

		[Fact]
		public async Task NestedNavigation_BackButtonVisibleIfAnyoneHasPages()
		{
			var window = new TestWindow();
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

		[Fact]
		public async Task ToolbarDoesntSetOnWindowWhenSwappingBackToSameFlyoutPage()
		{
			var window = new TestWindow();
			var navPage = new NavigationPage(new ContentPage()) { Title = "Detail" };
			var flyoutPage = new FlyoutPage()
			{
				Detail = navPage,
				Flyout = new ContentPage() { Title = "Flyout" }
			};

			IToolbarElement windowToolbarElement = window;

			window.Page = flyoutPage;
			window.Page = new ContentPage();
			window.Page = flyoutPage;

			Assert.Null(windowToolbarElement.Toolbar);
			Assert.NotNull((flyoutPage as IToolbarElement).Toolbar);
		}

		[Fact]
		public async Task ToolbarSetsToCorrectPageWithModal()
		{
			var window = new TestWindow();
			IToolbarElement toolbarElement = window;
			var startingPage = new TestNavigationPage(true, new ContentPage());
			window.Page = startingPage;

			await startingPage.NavigatingTask;

			var rootPageToolbar = toolbarElement.Toolbar;

			var modalPage = new TestNavigationPage(true, new ContentPage());
			await startingPage.Navigation.PushModalAsync(modalPage);

			Assert.Equal(rootPageToolbar, toolbarElement.Toolbar);

			var modalPageToolBar = (modalPage as IToolbarElement).Toolbar;

			Assert.NotNull(modalPageToolBar);
			Assert.NotEqual(modalPageToolBar, rootPageToolbar);

		}
	}
}