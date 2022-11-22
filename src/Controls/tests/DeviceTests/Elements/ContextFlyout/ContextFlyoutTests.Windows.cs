using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Handlers;
using Xunit;
using System.Linq;
using Microsoft.Maui.DeviceTests.Stubs;

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
	}
}
