using System;
using System.Collections.Generic;
using System.IO;
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

		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		[Category(TestCategory.Lifecycle)]
		public class WindowTestsRunInNewWindowCollection : ControlsHandlerTestBase
		{
			static void LogToFile(string msg)
			{
				try
				{
					// Write to the same log file location used by App.xaml.cs
					var cliArgs = Environment.GetCommandLineArgs();
					string logFile;
					if (cliArgs.Length > 1)
					{
						var resultsDir = Path.GetDirectoryName(cliArgs[1]);
						logFile = !string.IsNullOrEmpty(resultsDir)
							? Path.Combine(resultsDir, "maui-test-startup.log")
							: Path.Combine(Path.GetTempPath(), "maui-test-startup.log");
					}
					else
					{
						logFile = Path.Combine(Path.GetTempPath(), "maui-test-startup.log");
					}
					File.AppendAllText(logFile, $"{DateTime.Now:HH:mm:ss.fff} [MINIMIZE-TEST] {msg}{Environment.NewLine}");
				}
				catch { }
			}

			[Fact]
			public async Task MinimizeAndThenMaximizingWorks()
			{
				var window = new Window(new ContentPage());

				int activated = 0;
				int deactivated = 0;
				int resumed = 0;

				LogToFile("MinimizeAndThenMaximizingWorks starting");

				window.Activated += (_, _) =>
				{
					activated++;
					LogToFile($"Activated event fired, count now: {activated}");
				};
				window.Deactivated += (_, _) =>
				{
					deactivated++;
					LogToFile($"Deactivated event fired, count now: {deactivated}");
				};
				window.Resumed += (_, _) =>
				{
					resumed++;
					LogToFile($"Resumed event fired, count now: {resumed}");
				};

				await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
				{
					var platformWindow = window.Handler.PlatformView as UI.Xaml.Window;

					LogToFile($"Inside handler, platformWindow is null: {platformWindow is null}");

					// Get AppWindow for more detailed state info
					AppWindow appWindow = null;
					try
					{
						var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(platformWindow);
						var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
						appWindow = AppWindow.GetFromWindowId(windowId);
						LogToFile($"AppWindow obtained. IsVisible: {appWindow.IsVisible}, Presenter.Kind: {appWindow.Presenter.Kind}");
					}
					catch (Exception ex)
					{
						LogToFile($"Failed to get AppWindow: {ex.Message}");
					}

					// Wait for initial activation to complete
					await AssertEventually(() => activated >= 1, timeout: 5000,
						message: "Window should fire Activated event after creation");

					LogToFile($"After initial activation wait. activated={activated}, deactivated={deactivated}, resumed={resumed}");
					if (appWindow != null)
						LogToFile($"AppWindow state after activation: IsVisible={appWindow.IsVisible}, Presenter.Kind={appWindow.Presenter.Kind}");

					for (int i = 0; i < 2; i++)
					{
						LogToFile($"Loop iteration {i}: calling Restore()");
						platformWindow.Restore();
						// Use a delay long enough for window state transitions
						await Task.Delay(300);
						if (appWindow != null)
							LogToFile($"Loop {i} after Restore: IsVisible={appWindow.IsVisible}, Presenter.Kind={appWindow.Presenter.Kind}");
						LogToFile($"Loop iteration {i}: after Restore() delay. activated={activated}, deactivated={deactivated}, resumed={resumed}");

						LogToFile($"Loop iteration {i}: calling Minimize()");
						platformWindow.Minimize();
						await Task.Delay(300);
						if (appWindow != null)
							LogToFile($"Loop {i} after Minimize: IsVisible={appWindow.IsVisible}, Presenter.Kind={appWindow.Presenter.Kind}");
						LogToFile($"Loop iteration {i}: after Minimize() delay. activated={activated}, deactivated={deactivated}, resumed={resumed}");
					}

					LogToFile($"Exiting handler. activated={activated}, deactivated={deactivated}, resumed={resumed}");
				});

				LogToFile($"After handler completed. activated={activated}, deactivated={deactivated}, resumed={resumed}");

				// Wait for all expected events to fire with generous timeout
				// Expected: activated=2, resumed=1, deactivated=2
				await AssertEventually(() => activated >= 2, timeout: 5000,
					message: $"Expected at least 2 Activated events, got {activated}");
				await AssertEventually(() => deactivated >= 2, timeout: 5000,
					message: $"Expected at least 2 Deactivated events, got {deactivated}");

				LogToFile($"Final counts before assert. activated={activated}, deactivated={deactivated}, resumed={resumed}");

				Assert.Equal(2, activated);
				Assert.Equal(1, resumed);
				Assert.Equal(2, deactivated);
			}
		}
	}
}
