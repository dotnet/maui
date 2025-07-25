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

		[Fact]
		public void IsVisibleBindablePropertyExists()
		{
			Assert.NotNull(ToolbarItem.IsVisibleProperty);
			Assert.Equal("IsVisible", ToolbarItem.IsVisibleProperty.PropertyName);
			Assert.Equal(typeof(bool), ToolbarItem.IsVisibleProperty.ReturnType);
			Assert.Equal(typeof(ToolbarItem), ToolbarItem.IsVisibleProperty.DeclaringType);
		}

		[Fact]
		public void IsVisiblePropertyChangeNotifies()
		{
			var toolbarItem = new ToolbarItem();
			bool propertyChanged = false;
			
			toolbarItem.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.IsVisible))
					propertyChanged = true;
			};

			toolbarItem.IsVisible = false;
			Assert.True(propertyChanged);
		}
	}
}