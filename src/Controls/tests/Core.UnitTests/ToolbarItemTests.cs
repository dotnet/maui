using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ToolbarItemTests
		: MenuItemTests
	{
		[Fact]
		public void IsVisibleDefaultValue()
		{
			var toolbarItem = new ToolbarItem();
			Assert.True(toolbarItem.IsVisible);
		}

		[Fact]
		public void IsVisibleCanBeSet()
		{
			var toolbarItem = new ToolbarItem();
			
			toolbarItem.IsVisible = false;
			Assert.False(toolbarItem.IsVisible);
			
			toolbarItem.IsVisible = true;
			Assert.True(toolbarItem.IsVisible);
		}

		[Fact]
		public void IsVisiblePropertyChangedManagesToolbarItemsCollection()
		{
			var page = new ContentPage();
			var toolbarItem = new ToolbarItem { Text = "Test" };
			
			// Add the item to the page
			page.ToolbarItems.Add(toolbarItem);
			Assert.Single(page.ToolbarItems);
			Assert.Contains(toolbarItem, page.ToolbarItems);
			
			// Setting IsVisible to false should NOT remove it from the collection
			// It should remain in the collection but be filtered out at the platform level
			toolbarItem.IsVisible = false;
			Assert.Single(page.ToolbarItems);
			Assert.Contains(toolbarItem, page.ToolbarItems);
			Assert.False(toolbarItem.IsVisible);
			
			// Setting IsVisible to true should keep it in the collection
			toolbarItem.IsVisible = true;
			Assert.Single(page.ToolbarItems);
			Assert.Contains(toolbarItem, page.ToolbarItems);
			Assert.True(toolbarItem.IsVisible);
		}

		[Fact]
		public void IsVisiblePropertyPreservesOrderByPriority()
		{
			var page = new ContentPage();
			var item1 = new ToolbarItem { Text = "Item1", Priority = 1 };
			var item2 = new ToolbarItem { Text = "Item2", Priority = 2 };
			var item3 = new ToolbarItem { Text = "Item3", Priority = 3 };
			
			// Add items to page
			page.ToolbarItems.Add(item1);
			page.ToolbarItems.Add(item2);
			page.ToolbarItems.Add(item3);
			Assert.Equal(3, page.ToolbarItems.Count);
			
			// Hide item2 (middle priority) - should remain in collection
			item2.IsVisible = false;
			Assert.Equal(3, page.ToolbarItems.Count);
			Assert.Contains(item1, page.ToolbarItems);
			Assert.Contains(item2, page.ToolbarItems);
			Assert.Contains(item3, page.ToolbarItems);
			Assert.False(item2.IsVisible);
			
			// Show item2 again - should still be in same position
			item2.IsVisible = true;
			Assert.Equal(3, page.ToolbarItems.Count);
			Assert.Equal(item1, page.ToolbarItems[0]);
			Assert.Equal(item2, page.ToolbarItems[1]);
			Assert.Equal(item3, page.ToolbarItems[2]);
			Assert.True(item2.IsVisible);
		}
	}
}