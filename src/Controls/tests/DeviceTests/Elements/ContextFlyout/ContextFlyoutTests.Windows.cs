using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContextFlyoutTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Context flyout creates expected WinUI elements")]
		public async Task ContextFlyoutCreatesExpectedWinUIElements()
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem() { Text = "Toolbar Item 1" };
			var firstPage = new ContentPage();

			var window = new Window(firstPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				var labelWithContextFlyout = new Label();
				var menu1 = new MenuFlyoutItem() { Text = "Menu1" };
				var menu2 = new MenuFlyoutItem() { Text = "Menu2" };
				var menu3 = new MenuFlyoutSeparator();

				var menu4 = new MenuFlyoutSubItem() { Text = "Menu4" };
				menu4.Add(new MenuFlyoutItem() { Text = "Menu4-a" });
				menu4.Add(new MenuFlyoutItem() { Text = "Menu4-b" });

				var menuFlyout = new MenuFlyout();
				menuFlyout.Add(menu1);
				menuFlyout.Add(menu2);
				menuFlyout.Add(menu3);
				menuFlyout.Add(menu4);

				FlyoutBase.SetContextFlyout(labelWithContextFlyout, menuFlyout);

				var contentPage = new ContentPage()
				{
					Content = labelWithContextFlyout,
				};

				window.Page = contentPage;

				await OnLoadedAsync(contentPage);

				var winLabel = ((LabelHandler)labelWithContextFlyout.Handler).PlatformView;
				var contextFlyoutItems = ((Microsoft.UI.Xaml.Controls.MenuFlyout)winLabel.ContextFlyout).Items;
				Assert.Equal(4, contextFlyoutItems.Count);
				Assert.Equal("Menu1", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[0]).Text);
				Assert.Equal("Menu2", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[1]).Text);
				Assert.IsType<Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator>(contextFlyoutItems[2]);
				Assert.Equal("Menu4", ((Microsoft.UI.Xaml.Controls.MenuFlyoutSubItem)contextFlyoutItems[3]).Text);
				Assert.Equal(2, ((Microsoft.UI.Xaml.Controls.MenuFlyoutSubItem)contextFlyoutItems[3]).Items.Count);
				Assert.Equal("Menu4-a", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)((Microsoft.UI.Xaml.Controls.MenuFlyoutSubItem)contextFlyoutItems[3]).Items[0]).Text);
				Assert.Equal("Menu4-b", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)((Microsoft.UI.Xaml.Controls.MenuFlyoutSubItem)contextFlyoutItems[3]).Items[1]).Text);
			});
		}

		[Fact(DisplayName = "Context flyout doesn't crash on custom controls")]
		public async Task FlyoutAddedToCustomGridDoesntCrash()
		{
			SetupBuilder();
			var firstPage = new ContentPage();

			var window = new Window(firstPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				var theGrid = new TestCustomGrid();
				var contentPage = new ContentPage()
				{
					Content = theGrid,
				};

				window.Page = contentPage;

				await OnLoadedAsync(contentPage);

				var winGrid = ((LayoutHandler)theGrid.Handler).PlatformView;
				var contextFlyoutItems = ((Microsoft.UI.Xaml.Controls.MenuFlyout)winGrid.ContextFlyout).Items;
				Assert.Single(contextFlyoutItems);
				Assert.Equal("Hello", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[0]).Text);
			});
		}

		[Fact(DisplayName = "Context flyout adding and remove items")]
		public async Task FlyoutAddAndRemoveWorks()
		{
			SetupBuilder();
			var firstPage = new ContentPage();

			var window = new Window(firstPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				var theGrid = new TestCustomGrid();
				var contentPage = new ContentPage()
				{
					Content = theGrid,
				};

				window.Page = contentPage;

				await OnLoadedAsync(contentPage);

				var winGrid = ((LayoutHandler)theGrid.Handler).PlatformView;
				var contextFlyoutItems = ((Microsoft.UI.Xaml.Controls.MenuFlyout)winGrid.ContextFlyout).Items;
				Assert.Single(contextFlyoutItems);
				Assert.Equal("Hello", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[0]).Text);

				// Add some items
				theGrid.AddFlyoutItem("Hello World");
				theGrid.AddFlyoutItem("Hello Maui");
				Assert.Equal(3, contextFlyoutItems.Count);
				Assert.Equal("Hello World", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[1]).Text);
				Assert.Equal("Hello Maui", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[2]).Text);

				// Remove middle item
				theGrid.RemoveFlyoutItem(1);
				Assert.Equal(2, contextFlyoutItems.Count);
				Assert.Equal("Hello", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[0]).Text);
				Assert.Equal("Hello Maui", ((Microsoft.UI.Xaml.Controls.MenuFlyoutItem)contextFlyoutItems[1]).Text);
			});
		}

		private class TestCustomGrid : Grid
		{
			private MenuFlyout flyout;

			public TestCustomGrid()
			{
				flyout = new MenuFlyout();

				MenuFlyoutItem flyoutItem = new MenuFlyoutItem() { Text = "Hello" };
				flyout.Add(flyoutItem);

				FlyoutBase.SetContextFlyout(this, flyout);
			}

			public void AddFlyoutItem(string text)
			{
				MenuFlyoutItem flyoutItem = new MenuFlyoutItem() { Text = text };
				flyout.Add(flyoutItem);
			}

			public void RemoveFlyoutItem(int index)
			{
				flyout.RemoveAt(index);
			}
		}
	}
}
