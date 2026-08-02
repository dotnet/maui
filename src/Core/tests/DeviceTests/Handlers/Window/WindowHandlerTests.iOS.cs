using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : CoreHandlerTestBase
	{
#if !MACCATALYST
		[Fact]
		public async Task PageViewControllerReturnsDefaultStatusBarStyleWithoutWindow()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				// PageViewController without a window should return UIStatusBarStyle.Default
				// because the StatusBarTheme resolves to Default when no IWindow is available
				using var pvc = new Microsoft.Maui.Platform.PageViewController(
					new DeviceTests.Stubs.ContentViewStub(),
					MauiContext);

				Assert.Equal(UIStatusBarStyle.Default, pvc.PreferredStatusBarStyle());
			});
		}

		[Fact]
		public async Task TabBarControllerDelegatesStatusBarStyle()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var controller = new UIViewController();
				using var preferredController = new UIViewController();
				using var manager = new Microsoft.Maui.Platform.TabBarControllerManager(
					new TabBarManagerDelegateStub(controller));

				Assert.Same(controller, manager.TabBarController.ChildViewControllerForStatusBarStyle());

				manager.GetCurrentPageViewControllerFunc = () => preferredController;
				Assert.Same(preferredController, manager.TabBarController.ChildViewControllerForStatusBarStyle());
			});
		}

		sealed class TabBarManagerDelegateStub : Microsoft.Maui.Platform.ITabBarManagerDelegate
		{
			readonly UIViewController _controller;

			public TabBarManagerDelegateStub(UIViewController controller)
			{
				_controller = controller;
			}

			public UIViewController GetCurrentPageViewController() => _controller;
			public void OnTabSelected(int index) { }
			public void OnTabsReordered(UIViewController[] viewControllers) { }
			public void OnTraitCollectionDidChange(UITraitCollection previousTraitCollection) { }
			public void OnViewDidAppear() { }
			public void OnViewDidDisappear() { }
			public void OnViewDidLayoutSubviews() { }
		}
#endif

#if MACCATALYST

		[Fact(
#if CI
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
		public async Task TitleSetsOnWindow()
		{
			var window = new Window
			{
				Title = "Initial Title",
				Page = new ContentPage
				{
					Content = new Label { Text = "Yay!" }
				}
			};

			await RunWindowTest(window, handler =>
			{
				Assert.Equal("Initial Title", handler.PlatformView.WindowScene.Title);
				window.Title = "Updated Title";
				Assert.Equal("Updated Title", handler.PlatformView.WindowScene.Title);
			});
		}

		[Fact(
#if CI
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
		public async Task ContentIsSetInitially()
		{
			var window = new Window
			{
				Page = new ContentPage
				{
					Content = new Button { Text = "Yay!" }
				}
			};

			await RunWindowTest(window, handler =>
			{
				var root = handler.PlatformView.RootViewController;

				Assert.NotNull(root);
				var page = Assert.IsType<PageViewController>(root);

				Assert.NotNull(page.View);
				var content = Assert.IsType<Platform.ContentView>(root.View.Subviews[0]);
				var btn = Assert.IsType<UIButton>(content.Subviews[0].Subviews[0]);

				Assert.Equal("Yay!", btn.Title(UIControlState.Normal));
			});
		}

		[Fact(
#if CI
			Skip = "Causes Catalyst test run to hang"
#endif
		)]
		public async Task WindowSupportsEmptyPage_Platform()
		{
			var window = new Window(new ContentPage());

			await RunWindowTest(window, handler =>
			{
				var root = handler.PlatformView.RootViewController;

				Assert.NotNull(root);
				var page = Assert.IsType<PageViewController>(root);

				Assert.NotNull(page.View);
				var content = Assert.IsType<Platform.ContentView>(root.View.Subviews[0]);

				Assert.Empty(content.Subviews);
			});
		}

		void MovePlatformWindow(UIWindow window, Rect rect)
		{
			window.SetFrame(rect, true, false);
		}
#endif
	}
}