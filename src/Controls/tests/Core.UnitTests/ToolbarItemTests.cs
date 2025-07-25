using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ToolbarItemTests
		: MenuItemTests
	{
		[Fact]
		public void IsVisibleDefaultsToTrue()
		{
			var toolbarItem = new ToolbarItem();
			Assert.True(toolbarItem.IsVisible);
		}

		[Fact]
		public void IsVisibleCanBeSetToFalse()
		{
			var toolbarItem = new ToolbarItem();
			toolbarItem.IsVisible = false;
			Assert.False(toolbarItem.IsVisible);
		}
	}
}