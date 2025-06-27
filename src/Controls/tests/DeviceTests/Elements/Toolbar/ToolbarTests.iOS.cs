using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using UIKit;

#if IOS || MACCATALYST
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Toolbar)]
	public partial class ToolbarTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Toolbar Items IsVisible Property Works on iOS")]
		public async Task ToolbarItemsIsVisiblePropertyWorksiOS()
		{
			SetupBuilder();
			var visibleItem = new ToolbarItem() { Text = "Visible Item", IsVisible = true };
			var hiddenItem = new ToolbarItem() { Text = "Hidden Item", IsVisible = false };
			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					visibleItem,
					hiddenItem
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				// Get the navigation renderer
				var navigationRenderer = (NavigationViewHandler)navPage.Handler;
				var topViewController = navigationRenderer.TopViewController;
				
				// Check that only visible item appears in the navigation bar
				var rightBarButtonItems = topViewController.NavigationItem.RightBarButtonItems;
				Assert.Single(rightBarButtonItems);
				
				// Verify the visible item is present by checking the title
				Assert.Equal("Visible Item", rightBarButtonItems[0].Title);
				
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Toolbar Items IsVisible Property Can Be Changed Dynamically on iOS")]
		public async Task ToolbarItemsIsVisiblePropertyCanBeChangedDynamicallyiOS()
		{
			SetupBuilder();
			var item1 = new ToolbarItem() { Text = "Item 1", IsVisible = true };
			var item2 = new ToolbarItem() { Text = "Item 2", IsVisible = false };
			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					item1,
					item2
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async (handler) =>
			{
				var navigationRenderer = (NavigationViewHandler)navPage.Handler;
				var topViewController = navigationRenderer.TopViewController;
				
				// Initially only item1 should be visible
				var rightBarButtonItems = topViewController.NavigationItem.RightBarButtonItems;
				Assert.Single(rightBarButtonItems);
				Assert.Equal("Item 1", rightBarButtonItems[0].Title);
				
				// Hide item1 and show item2
				await InvokeOnMainThreadAsync(() =>
				{
					item1.IsVisible = false;
					item2.IsVisible = true;
				});
				
				// Now only item2 should be visible  
				rightBarButtonItems = topViewController.NavigationItem.RightBarButtonItems;
				Assert.Single(rightBarButtonItems);
				Assert.Equal("Item 2", rightBarButtonItems[0].Title);
				
				// Show both items
				await InvokeOnMainThreadAsync(() =>
				{
					item1.IsVisible = true;
					item2.IsVisible = true;
				});
				
				// Both items should be visible now
				rightBarButtonItems = topViewController.NavigationItem.RightBarButtonItems;
				Assert.Equal(2, rightBarButtonItems.Length);
				
				// Items are reversed in iOS, so item2 (last added) comes first
				Assert.Equal("Item 2", rightBarButtonItems[0].Title);
				Assert.Equal("Item 1", rightBarButtonItems[1].Title);
			});
		}

		[Fact(DisplayName = "Secondary Toolbar Items Respect IsVisible Property on iOS")]
		public async Task SecondaryToolbarItemsRespectIsVisiblePropertyiOS()
		{
			SetupBuilder();
			var primaryItem = new ToolbarItem() { Text = "Primary", Order = ToolbarItemOrder.Primary, IsVisible = true };
			var visibleSecondary = new ToolbarItem() { Text = "Visible Secondary", Order = ToolbarItemOrder.Secondary, IsVisible = true };
			var hiddenSecondary = new ToolbarItem() { Text = "Hidden Secondary", Order = ToolbarItemOrder.Secondary, IsVisible = false };
			
			var navPage = new NavigationPage(new ContentPage()
			{
				ToolbarItems =
				{
					primaryItem,
					visibleSecondary,
					hiddenSecondary
				}
			});

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), (handler) =>
			{
				var navigationRenderer = (NavigationViewHandler)navPage.Handler;
				var topViewController = navigationRenderer.TopViewController;
				
				// Primary items go to RightBarButtonItems
				var rightBarButtonItems = topViewController.NavigationItem.RightBarButtonItems;
				Assert.Single(rightBarButtonItems);
				Assert.Equal("Primary", rightBarButtonItems[0].Title);
				
				// Secondary items go to ToolbarItems
				var toolbarItems = topViewController.ToolbarItems;
				Assert.Single(toolbarItems);
				
				return Task.CompletedTask;
			});
		}
	}
}