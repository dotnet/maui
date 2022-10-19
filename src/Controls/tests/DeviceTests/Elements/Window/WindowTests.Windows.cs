using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Handlers;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
using Xunit;
using System.Linq;
using Microsoft.Maui.Graphics.Win2D;
using System;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests : HandlerTestBase
	{
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
				await AssertionExtensions.Wait(() => mauiToolBar.GetLocationOnScreen().Value.Y > 0);
				var position = mauiToolBar.GetLocationOnScreen();
				var appTitleBarHeight = GetWindowRootView(handler).AppTitleBarActualHeight;

				Assert.True(appTitleBarHeight > 0);
				Assert.True(Math.Abs(position.Value.Y - appTitleBarHeight) < 1);
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
							await AssertionExtensions.Wait(() => mauiToolBar.GetLocationOnScreen().Value.Y > 0);
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
	}
}
