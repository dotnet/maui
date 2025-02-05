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
	}
}
