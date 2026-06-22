using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using Windows.UI;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Swapping MainPage doesn't Crash")]
		public async Task SwappingMainPageDoesntCrash()
		{
			SetupBuilder();

			var mainPage = new ContentPage
			{
				BackgroundColor = Colors.Red
			};

			var secondaryPage = new ContentPage
			{
				BackgroundColor = Colors.Green
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, (handler) =>
			{
				var mainWindow = handler.VirtualView as Window;

				mainWindow.Page = secondaryPage;
				Assert.Equal(mainWindow.Page, secondaryPage);

				mainWindow.Page = mainPage;
				// Without exceptions, the test has passed.
				Assert.Equal(mainWindow.Page, mainPage);

				Assert.NotNull(mainWindow.Page);
			});
		}

		[Fact(DisplayName = "MauiWinUIWindow doesn't leak")]
		public async Task MauiWinUIWindowDoesntLeak()
		{
			List<WeakReference> weakReferences = new();

			SetupBuilder();

			var mainPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, async (handler) =>
			{
				for (int i = 0; i < 3; i++)
				{
					var window = new MauiWinUIWindow();
					weakReferences.Add(new WeakReference(window));

					window.Activate();
					await Task.Delay(100);
					window.Close();
				}

				// Use the standard WaitForGC pattern which tries up to 40 GC cycles with Task.Yield
				// This is the same approach used by all other memory leak tests in the repo
				await AssertionExtensions.WaitForGC(weakReferences.ToArray());
			});
		}

		[Fact]
		public async Task AdornerLayerAdded()
		{
			SetupBuilder();

			var mainPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, (handler) =>
			{
				var windowRootViewContainer = (WPanel)handler.PlatformView.Content;
				var overlayView =
					windowRootViewContainer
						.Children
						.OfType<W2DGraphicsView>()
						.SingleOrDefault();

				Assert.NotNull(overlayView);
				Assert.Equal(overlayView, windowRootViewContainer.Children.Last());
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Swapping Root Page Removes Previous Page from WindowRootViewContainer")]
		public async Task SwappingRootPageRemovesPreviousPageFromWindowRootViewContainer()
		{
			SetupBuilder();

			var mainPage = new Shell() { CurrentItem = new ContentPage() };
			var swappedInMainPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, async (handler) =>
			{
				var windowRootViewContainer = (WPanel)handler.PlatformView.Content;
				var countBeforePageSwap = windowRootViewContainer.Children.Count;

				var mainPageRootView =
					mainPage.FindMauiContext().GetNavigationRootManager().RootView;

				(handler.VirtualView as Window).Page = swappedInMainPage;
				await OnNavigatedToAsync(swappedInMainPage.CurrentPage);
				await OnFrameSetToNotEmpty(swappedInMainPage.CurrentPage);

				var countAfterPageSwap = windowRootViewContainer.Children.Count;
				Assert.Equal(countBeforePageSwap, countAfterPageSwap);
			});
		}

		[Fact]
		public async Task HeaderCorrectlyOffsetFromAppTitleBar()
		{
			SetupBuilder();

			var mainPage = new NavigationPage(new ContentPage()
			{
				Title = "title",
				ToolbarItems =
				{
					new ToolbarItem()
					{
						Text = "Item"
					}
				}
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, async (handler) =>
			{
				var mauiToolBar = GetPlatformToolbar(handler);

				Assert.NotNull(mauiToolBar);
				await AssertEventually(() => mauiToolBar.GetLocationOnScreen().Value.Y > 0);

				var position = mauiToolBar.GetLocationOnScreen();
				var appTitleBarHeight = GetWindowRootView(handler).AppTitleBarActualHeight;

				Assert.True(appTitleBarHeight > 0);
				Assert.True(Math.Abs(position.Value.Y - appTitleBarHeight) < 1);
			});
		}

		[Fact]
		public async Task WindowsBoundsWhenMaximized()
		{
			SetupBuilder();
			var mainPage = new NavigationPage(new ContentPage());

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, async (handler) =>
			{
				var appWindowPlatform = handler.PlatformView.GetAppWindow();
				Assert.NotNull(appWindowPlatform?.Presenter);
				var presenter = Assert.IsType<OverlappedPresenter>(appWindowPlatform.Presenter);

				// maximize window
				presenter.Maximize();
				var appWindow = handler.PlatformView.GetWindow();
				Assert.NotNull(appWindow);

				// Compute work-area reference values before polling so the same values
				// are used for both the wait predicate and the final assertions.
				// Compare against the monitor's work area. This correctly handles negative
				// coordinates when the window is on a monitor positioned left of or above
				// the primary display, and catches regressions beyond a simple > 0 check.
				var displayArea = DisplayArea.GetFromWindowId(appWindowPlatform.Id, DisplayAreaFallback.Nearest);
				var workArea = displayArea.WorkArea;
				var density = handler.PlatformView.GetDisplayDensity();

				// Wait until the MAUI frame reflects the maximized work-area bounds.
				// Waiting only for Height > 0 is insufficient: that condition is already true
				// before Maximize() is called, so on slow machines the assertions below would
				// execute against the pre-maximized frame and become flaky.
				await AssertEventually(() =>
					Math.Abs(appWindow.Width - workArea.Width / density) < 2 &&
					Math.Abs(appWindow.Height - workArea.Height / density) < 2);

				Assert.True(Math.Abs(appWindow.X - workArea.X / density) < 2,
					$"X should be near work area X ({workArea.X / density:F2}) but was {appWindow.X}");
				Assert.True(Math.Abs(appWindow.Y - workArea.Y / density) < 2,
					$"Y should be near work area Y ({workArea.Y / density:F2}) but was {appWindow.Y}");
				Assert.True(Math.Abs(appWindow.Width - workArea.Width / density) < 2,
					$"Width should match work area width ({workArea.Width / density:F2}) but was {appWindow.Width}");
				Assert.True(Math.Abs(appWindow.Height - workArea.Height / density) < 2,
					$"Height should match work area height ({workArea.Height / density:F2}) but was {appWindow.Height}");
			});
		}

		[Fact]
		public async Task WindowsYAndHeightCorrectWhenClosingMaximizedWindow()
		{
			SetupBuilder();
			var mainPage = new NavigationPage(new ContentPage());

			double destroyingY = double.NaN;
			double destroyingHeight = double.NaN;
			double expectedY = double.NaN;
			double expectedHeight = double.NaN;

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, async (handler) =>
			{
				var window = handler.VirtualView as Window;
				Assert.NotNull(window);

				var appWindowPlatform = handler.PlatformView.GetAppWindow();
				Assert.NotNull(appWindowPlatform?.Presenter);
				var presenter = Assert.IsType<OverlappedPresenter>(appWindowPlatform.Presenter);

				// Capture the frame values at destroy time so we can verify them after cleanup
				window.Destroying += (s, e) =>
				{
					destroyingY = window.Y;
					destroyingHeight = window.Height;
				};

				// Capture the expected work-area bounds before waiting for the maximized frame,
				// so the same reference values are used for both the wait predicate and the
				// post-cleanup assertions.
				var displayArea = DisplayArea.GetFromWindowId(appWindowPlatform.Id, DisplayAreaFallback.Nearest);
				var workArea = displayArea.WorkArea;
				var density = handler.PlatformView.GetDisplayDensity();
				expectedY = workArea.Y / density;
				expectedHeight = workArea.Height / density;

				// Maximize the window and wait until the MAUI frame reflects the maximized bounds.
				// Waiting only for Height > 0 is insufficient: that condition is already true
				// before Maximize() is called, so on slow machines the captured values at
				// Destroying time could come from the pre-maximized frame.
				// Do not assert window.Y == 0: on monitors positioned above the primary display
				// Y is legitimately negative.
				presenter.Maximize();
				await AssertEventually(() =>
					Math.Abs(window.Height - expectedHeight) < 2 &&
					Math.Abs(window.Y - expectedY) < 2);
			});

			// The window is destroyed during CreateHandlerAndAddToWindow cleanup.
			// Assert that the bounds reported at Destroying time match the monitor work area,
			// using the same < 2 tolerance as WindowsBoundsWhenMaximized.  This catches
			// regressions where Y or Height is off by ~8 pixels when closing a maximized window.
			Assert.False(double.IsNaN(destroyingHeight), "Window.Destroying event was not raised");
			Assert.True(Math.Abs(destroyingY - expectedY) < 2,
				$"Y should be near work area Y ({expectedY:F2}) when closing a maximized window, but was {destroyingY}");
			Assert.True(Math.Abs(destroyingHeight - expectedHeight) < 2,
				$"Height should match work area height ({expectedHeight:F2}) when closing a maximized window, but was {destroyingHeight}");
		}

		[Fact]
		public async Task ToggleFullscreenTitleBarWorks()
		{
			SetupBuilder();

			var mainPage = new NavigationPage(new ContentPage()
			{
				Title = "title",
				ToolbarItems =
				{
					new ToolbarItem()
					{
						Text = "Item"
					}
				}
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(mainPage, async (handler) =>
			{
				var mauiToolBar = GetPlatformToolbar(handler);
				var presenter = handler.PlatformView.GetAppWindow()?.Presenter as OverlappedPresenter;
				var rootView = GetWindowRootView(handler);
				var defaultTitleBarHeight = rootView.AppTitleBarActualHeight;
				Assert.True(defaultTitleBarHeight > 0);
				Assert.True(mauiToolBar.GetLocationOnScreen().Value.Y == 32);

				// Disable titlebar, maximize the window
				presenter.SetBorderAndTitleBar(false, false);
				presenter.Maximize();

				// Wait for maximize animation to finish
				await AssertEventually(() => mauiToolBar.GetLocationOnScreen().Value.Y == 0);

				// Now restore the window
				presenter.SetBorderAndTitleBar(true, true);
				presenter.Restore();

				await AssertEventually(() => mauiToolBar.GetLocationOnScreen().Value.Y == 32);
			});
		}

		[Theory]
		[ClassData(typeof(WindowPageSwapTestCases))]
		public async Task HeaderCorrectlyOffsetsWhenSwappingMainPage(WindowPageSwapTestCase swapOrder)
		{
			SetupBuilder();

			var firstRootPage = swapOrder.GetNextPageType();
			var window = new Window(firstRootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(swapOrder.Page);
				while (!swapOrder.IsFinished())
				{
					var nextRootPage = swapOrder.GetNextPageType();
					window.Page = nextRootPage;

					try
					{
						await OnLoadedAsync(swapOrder.Page);

						if (nextRootPage is NavigationPage || nextRootPage is Shell)
						{
							var mauiToolBar = GetPlatformToolbar(handler);

							Assert.NotNull(mauiToolBar);
							await AssertEventually(() => mauiToolBar.GetLocationOnScreen().Value.Y > 0);

							var position = mauiToolBar.GetLocationOnScreen();
							var appTitleBarHeight = GetWindowRootView(handler).AppTitleBarActualHeight;

							Assert.True(appTitleBarHeight > 0);
							Assert.True(Math.Abs(position.Value.Y - appTitleBarHeight) < 1);
						}
					}
					catch (Exception exc)
					{
						throw new Exception($"Failed to swap to {nextRootPage}", exc);
					}
				}
			});
		}

		[Theory]
		[ClassData(typeof(WindowPageSwapTestCases))]
		public async Task TitlebarWorksWhenSwitchingPage(WindowPageSwapTestCase swapOrder)
		{
			SetupBuilder();

			var firstRootPage = swapOrder.GetNextPageType();
			var window = new Window(firstRootPage)
			{
				TitleBar = new TitleBar()
				{
					Title = "Hello World",
					BackgroundColor = Colors.CornflowerBlue
				}
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await OnLoadedAsync(swapOrder.Page);
				while (!swapOrder.IsFinished())
				{
					var nextRootPage = swapOrder.GetNextPageType();
					window.Page = nextRootPage;

					try
					{
						await OnLoadedAsync(swapOrder.Page);

						var navView = GetWindowRootView(handler);
						Assert.NotNull(navView.TitleBar);
					}
					catch (Exception exc)
					{
						throw new Exception($"Failed to swap to {nextRootPage}", exc);
					}
				}
			});
		}

		// MinimizeAndThenMaximizingWorks test moved to UI tests (Issue14142) because 
		// window activation events don't fire reliably on Helix VMs which run in 
		// non-interactive Windows sessions.
	}
}
