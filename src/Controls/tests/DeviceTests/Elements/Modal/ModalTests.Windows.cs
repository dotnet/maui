using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task PushingPageWithBackgroundStacksAndUnStacksCorrectly(bool useColor)
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage() { Content = new Label() { Text = "Root Page" } });

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (handler) =>
				{
					var windowRootViewContainer = (WPanel)handler.PlatformView.Content;
					ContentPage backgroundColorContentPage = new ContentPage() { Content = new Label() { Text = "Modal Page" } };


					if (useColor)
						backgroundColorContentPage.BackgroundColor = Colors.Purple.WithAlpha(0.5f);
					else
						backgroundColorContentPage.Background = new SolidColorBrush(Colors.Purple.WithAlpha(0.5f));

					await navPage.CurrentPage.Navigation.PushModalAsync(backgroundColorContentPage);
					await OnLoadedAsync(backgroundColorContentPage);

					var modalRootView =
						backgroundColorContentPage.FindMauiContext().GetNavigationRootManager().RootView;
					var rootPageRootView =
						navPage.FindMauiContext().GetNavigationRootManager().RootView;

					Assert.Equal(1, windowRootViewContainer.Children.IndexOf(modalRootView));
					Assert.Equal(0, windowRootViewContainer.Children.IndexOf(rootPageRootView));

					await navPage.CurrentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(backgroundColorContentPage);

					Assert.Equal(0, windowRootViewContainer.Children.IndexOf(rootPageRootView));
					Assert.DoesNotContain(modalRootView, windowRootViewContainer.Children);
				});
		}

		[Fact]
		public async Task WindowTitleSetToModalTitleContainer()
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());
			var window = new Window(navPage) { Title = "Original Title" };

			await CreateHandlerAndAddToWindow<IWindowHandler>(window,
				async (handler) =>
				{
					var rootView = handler.PlatformView.Content;
					ContentPage modalPage = new ContentPage();

					var previousPageNavigationRootManager = navPage.CurrentPage
						.FindMauiContext()
						.GetNavigationRootManager();

					var previousWindowRootView = previousPageNavigationRootManager.RootView as WindowRootView;

					await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					var modalNavigationRootManager = modalPage
						.FindMauiContext()
						.GetNavigationRootManager();

					var mauiWindow = (MauiWinUIWindow)handler.PlatformView;
					Assert.Equal("Original Title", mauiWindow.Title);
					Assert.Equal(modalNavigationRootManager.WindowTitle, mauiWindow.Title);

					// Ensure previous page has hidden its titlebar
					Assert.True(previousWindowRootView.AppTitleBarContainer.Visibility == UI.Xaml.Visibility.Collapsed);
					Assert.True(previousWindowRootView.NavigationViewControl?.ButtonHolderGrid.Visibility == UI.Xaml.Visibility.Collapsed);

					window.Title = "Update Title";
					Assert.Equal("Update Title", mauiWindow.Title);
					Assert.Equal(modalNavigationRootManager.WindowTitle, mauiWindow.Title);

					// Ensure titlebar is visible after popping modal
					var modalWindowRootView = modalNavigationRootManager.RootView as WindowRootView;
					Assert.True(modalWindowRootView.AppTitleBarContainer.Visibility == UI.Xaml.Visibility.Visible);
					Assert.True(modalWindowRootView.NavigationViewControl.ButtonHolderGrid.Visibility == UI.Xaml.Visibility.Visible);
				});
		}

		[Fact]
		public async Task WindowTitleIsCorrectAfterPushAndPop()
		{
			const string OriginalTitle = "Original Title";
			const string UpdatedTitle = "Updated Title";

			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage());
			var window = new Window(navPage) { Title = OriginalTitle };

			await CreateHandlerAndAddToWindow(window,
				(Func<IWindowHandler, Task>)(async (handler) =>
				{
					var mauiWindow = handler.PlatformView;
					var currentPage = navPage.CurrentPage;
					var modalPage = new ContentPage();

					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					var currentNavigationRootManager = currentPage
						.FindMauiContext()
						.GetNavigationRootManager();
					var modalNavigationRootManager = modalPage
						.FindMauiContext()
						.GetNavigationRootManager();

					Assert.Equal(OriginalTitle, mauiWindow.Title);
					Assert.Equal(OriginalTitle, currentNavigationRootManager.WindowTitle);
					Assert.Equal(OriginalTitle, modalNavigationRootManager.WindowTitle);

					window.Title = UpdatedTitle;

					Assert.Equal(UpdatedTitle, mauiWindow.Title);
					Assert.Equal(UpdatedTitle, currentNavigationRootManager.WindowTitle);
					Assert.Equal(UpdatedTitle, modalNavigationRootManager.WindowTitle);

					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					Assert.Equal(UpdatedTitle, mauiWindow.Title);
					Assert.Equal(UpdatedTitle, currentNavigationRootManager.WindowTitle);
					Assert.Equal(UpdatedTitle, modalNavigationRootManager.WindowTitle);

					// Ensure titlebar is visible after popping modal
					var windowRootView = currentNavigationRootManager.RootView as WindowRootView;
					Assert.True(windowRootView.AppTitleBarContainer.Visibility == UI.Xaml.Visibility.Visible);
					Assert.True(windowRootView.NavigationViewControl.ButtonHolderGrid.Visibility == UI.Xaml.Visibility.Visible);
				}));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ModalPageDisablesHitTestOnUnderlyingPage(bool useColor)
		{
			SetupBuilder();

			var navPage = new NavigationPage(new ContentPage() { Content = new Label() { Text = "Root Page" } });

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (handler) =>
				{
					ContentPage modalPage = new ContentPage() { Content = new Label() { Text = "Modal Page" } };

					if (useColor)
						modalPage.BackgroundColor = Colors.Purple.WithAlpha(0.5f);
					else
						modalPage.Background = new SolidColorBrush(Colors.Purple.WithAlpha(0.5f));

					var rootPageRootView = navPage.FindMauiContext().GetNavigationRootManager().RootView;

					await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					var modalRootView = modalPage.FindMauiContext().GetNavigationRootManager().RootView;

					// The underlying page should have IsHitTestVisible disabled
					Assert.False(rootPageRootView.IsHitTestVisible,
						"Underlying page should have IsHitTestVisible=false when a modal is displayed");

					// The modal page should have IsHitTestVisible enabled
					Assert.True(modalRootView.IsHitTestVisible,
						"Modal page should have IsHitTestVisible=true");

					await navPage.CurrentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					// After popping the modal, the underlying page should be interactive again
					Assert.True(rootPageRootView.IsHitTestVisible,
						"Underlying page should have IsHitTestVisible=true after modal is dismissed");
				});
		}

		[Fact]
		public async Task ModalPageFocusTrapsAndRestoresCorrectly()
		{
			SetupBuilder();

			var button = new Button() { Text = "Test Button" };
			var rootPage = new ContentPage() { Content = button };
			var navPage = new NavigationPage(rootPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (handler) =>
				{
					var modalButton = new Button() { Text = "Modal Button" };
					var modalPage = new ContentPage()
					{
						Content = modalButton,
						BackgroundColor = Colors.Purple.WithAlpha(0.5f)
					};

					var container = (WindowRootViewContainer)handler.PlatformView.Content;

					// Push modal
					await navPage.CurrentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					var rootPageRootView = navPage.FindMauiContext().GetNavigationRootManager().RootView;
					var modalRootView = modalPage.FindMauiContext().GetNavigationRootManager().RootView;

					// Underlying page should be non-interactive
					Assert.False(rootPageRootView.IsHitTestVisible);

					// Pop modal
					await navPage.CurrentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					// After pop, the root page should be fully interactive
					Assert.True(rootPageRootView.IsHitTestVisible,
						"Root page should be hit-test visible after modal pop");

					// The root page should still be in the visual tree
					Assert.Contains(rootPageRootView, container.CachedChildren);

					// The modal should be removed
					Assert.DoesNotContain(modalRootView, container.CachedChildren);
				});
		}

		[Fact]
		public async Task NestedModalPagesMaintainHitTestVisibilityAndFocusTrap()
		{
			SetupBuilder();

			var button = new Button() { Text = "Root Button" };
			var rootPage = new ContentPage() { Content = button };
			var navPage = new NavigationPage(rootPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navPage),
				async (handler) =>
				{
					var modalButtonA = new Button() { Text = "Modal A Button" };
					var modalPageA = new ContentPage()
					{
						Content = modalButtonA,
						BackgroundColor = Colors.Green.WithAlpha(0.5f)
					};

					var modalButtonB = new Button() { Text = "Modal B Button" };
					var modalPageB = new ContentPage()
					{
						Content = modalButtonB,
						BackgroundColor = Colors.Red.WithAlpha(0.5f)
					};

					var container = (WindowRootViewContainer)handler.PlatformView.Content;

					// Push first modal (A)
					await navPage.CurrentPage.Navigation.PushModalAsync(modalPageA);
					await OnLoadedAsync(modalPageA);

					var rootPageRootView = navPage.FindMauiContext().GetNavigationRootManager().RootView;
					var modalARootView = modalPageA.FindMauiContext().GetNavigationRootManager().RootView;

					// Underlying root page should be non-interactive while modal A is showing
					Assert.False(rootPageRootView.IsHitTestVisible);
					Assert.Contains(modalARootView, container.CachedChildren);

					// Push second modal (B) on top of A
					await navPage.CurrentPage.Navigation.PushModalAsync(modalPageB);
					await OnLoadedAsync(modalPageB);

					var modalBRootView = modalPageB.FindMauiContext().GetNavigationRootManager().RootView;

					// Root should still be non-interactive with topmost modal B showing
					Assert.False(rootPageRootView.IsHitTestVisible);
					Assert.Contains(modalBRootView, container.CachedChildren);

					// Pop topmost modal (B)
					await navPage.CurrentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPageB);

					// After popping B, modal A is still visible, so the root page
					// should remain non-interactive (focus trap still active)
					Assert.False(rootPageRootView.IsHitTestVisible);
					Assert.Contains(modalARootView, container.CachedChildren);
					Assert.DoesNotContain(modalBRootView, container.CachedChildren);

					// Now pop modal A
					await navPage.CurrentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPageA);

					// After popping the last modal, the root page should be interactive again
					Assert.True(rootPageRootView.IsHitTestVisible,
						"Root page should be hit-test visible after all modals are popped");

					// The root page should still be in the visual tree
					Assert.Contains(rootPageRootView, container.CachedChildren);

					// Modal A should now be removed from the visual tree
					Assert.DoesNotContain(modalARootView, container.CachedChildren);
				});
		}
	}
}
