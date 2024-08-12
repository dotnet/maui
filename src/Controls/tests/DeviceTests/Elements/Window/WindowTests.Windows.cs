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

				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.WaitForFullGCComplete();

				Assert.True(weakReferences.Count(r => r.IsAlive) == 0);
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

		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		[Category(TestCategory.Lifecycle)]
		public class WindowTestsRunInNewWindowCollection : ControlsHandlerTestBase
		{
			[Fact]
			public async Task MinimizeAndThenMaximizingWorks()
			{
				var window = new Window(new ContentPage());

				int activated = 0;
				int deactivated = 0;
				int resumed = 0;

				window.Activated += (_, _) => activated++;
				window.Deactivated += (_, _) => deactivated++;
				window.Resumed += (_, _) => resumed++;

				await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
				{
					var platformWindow = window.Handler.PlatformView as UI.Xaml.Window;

					await Task.Yield();

					for (int i = 0; i < 2; i++)
					{
						platformWindow.Restore();
						await Task.Yield();
						platformWindow.Minimize();
					}
				});

				Assert.Equal(2, activated);
				Assert.Equal(1, resumed);
				Assert.Equal(2, deactivated);
			}
		}
	}
}
