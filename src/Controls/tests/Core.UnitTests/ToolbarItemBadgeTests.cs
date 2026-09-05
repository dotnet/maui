using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ToolbarItemBadgeTests : BaseTestFixture
	{
		[Fact]
		public void BadgeTextDefaultIsNull()
		{
			var item = new ToolbarItem();
			Assert.Null(item.BadgeText);
		}

		[Fact]
		public void BadgeColorDefaultIsNull()
		{
			var item = new ToolbarItem();
			Assert.Null(item.BadgeColor);
		}

		[Fact]
		public void SetBadgeTextNumeric()
		{
			var item = new ToolbarItem();
			item.BadgeText = "5";
			Assert.Equal("5", item.BadgeText);
		}

		[Fact]
		public void SetBadgeTextArbitraryString()
		{
			var item = new ToolbarItem();
			item.BadgeText = "New";
			Assert.Equal("New", item.BadgeText);
		}

		[Fact]
		public void ClearBadgeText()
		{
			var item = new ToolbarItem();
			item.BadgeText = "3";
			item.BadgeText = null;
			Assert.Null(item.BadgeText);
		}

		[Fact]
		public void SetBadgeColor()
		{
			var item = new ToolbarItem();
			item.BadgeColor = Colors.Red;
			Assert.Equal(Colors.Red, item.BadgeColor);
		}

		[Fact]
		public void ClearBadgeColor()
		{
			var item = new ToolbarItem();
			item.BadgeColor = Colors.Blue;
			item.BadgeColor = null;
			Assert.Null(item.BadgeColor);
		}

		[Fact]
		public void BadgeTextPropertyChangedFires()
		{
			var item = new ToolbarItem();
			bool fired = false;
			item.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.BadgeText))
					fired = true;
			};
			item.BadgeText = "1";
			Assert.True(fired);
		}

		[Fact]
		public void BadgeColorPropertyChangedFires()
		{
			var item = new ToolbarItem();
			bool fired = false;
			item.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.BadgeColor))
					fired = true;
			};
			item.BadgeColor = Colors.Green;
			Assert.True(fired);
		}

		[Fact]
		public void BadgeTextDoesNotFireWhenSameValue()
		{
			var item = new ToolbarItem { BadgeText = "5" };
			int fireCount = 0;
			item.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.BadgeText))
					fireCount++;
			};
			item.BadgeText = "5";
			Assert.Equal(0, fireCount);
		}

		[Fact]
		public void BadgeColorDoesNotFireWhenSameValue()
		{
			var item = new ToolbarItem { BadgeColor = Colors.Red };
			int fireCount = 0;
			item.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.BadgeColor))
					fireCount++;
			};
			item.BadgeColor = Colors.Red;
			Assert.Equal(0, fireCount);
		}

		[Fact]
		public void BadgeTextBindableProperty()
		{
			var item = new ToolbarItem();
			item.SetValue(ToolbarItem.BadgeTextProperty, "99+");
			Assert.Equal("99+", item.BadgeText);
		}

		[Fact]
		public void BadgeColorBindableProperty()
		{
			var item = new ToolbarItem();
			item.SetValue(ToolbarItem.BadgeColorProperty, Colors.Orange);
			Assert.Equal(Colors.Orange, item.BadgeColor);
		}

		[Fact]
		public void BadgeTextDataBinding()
		{
			var vm = new { Count = "42" };
			var item = new ToolbarItem();
			item.BindingContext = vm;
			item.SetBinding(ToolbarItem.BadgeTextProperty, "Count");
			Assert.Equal("42", item.BadgeText);
		}

		[Fact]
		public void BadgeColorDataBinding()
		{
			var vm = new { Highlight = Colors.Purple };
			var item = new ToolbarItem();
			item.BindingContext = vm;
			item.SetBinding(ToolbarItem.BadgeColorProperty, "Highlight");
			Assert.Equal(Colors.Purple, item.BadgeColor);
		}

		[Fact]
		public void BadgeTextEmptyStringTreatedAsValue()
		{
			var item = new ToolbarItem();
			item.BadgeText = "";
			Assert.Equal("", item.BadgeText);
		}

		[Fact]
		public void BadgePropertiesIndependent()
		{
			var item = new ToolbarItem();
			item.BadgeText = "3";
			item.BadgeColor = Colors.Red;

			// Clearing text should not affect color
			item.BadgeText = null;
			Assert.Null(item.BadgeText);
			Assert.Equal(Colors.Red, item.BadgeColor);

			// Clearing color should not affect text
			item.BadgeText = "5";
			item.BadgeColor = null;
			Assert.Equal("5", item.BadgeText);
			Assert.Null(item.BadgeColor);
		}

		[Fact]
		public void BadgeTextColorDefaultIsNull()
		{
			var item = new ToolbarItem();
			Assert.Null(item.BadgeTextColor);
		}

		[Fact]
		public void SetBadgeTextColor()
		{
			var item = new ToolbarItem();
			item.BadgeTextColor = Colors.White;
			Assert.Equal(Colors.White, item.BadgeTextColor);
		}

		[Fact]
		public void ClearBadgeTextColor()
		{
			var item = new ToolbarItem();
			item.BadgeTextColor = Colors.Black;
			item.BadgeTextColor = null;
			Assert.Null(item.BadgeTextColor);
		}

		[Fact]
		public void BadgeTextColorPropertyChangedFires()
		{
			var item = new ToolbarItem();
			bool fired = false;
			item.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.BadgeTextColor))
					fired = true;
			};
			item.BadgeTextColor = Colors.Yellow;
			Assert.True(fired);
		}

		[Fact]
		public void BadgeTextColorDoesNotFireWhenSameValue()
		{
			var item = new ToolbarItem { BadgeTextColor = Colors.White };
			int fireCount = 0;
			item.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.BadgeTextColor))
					fireCount++;
			};
			item.BadgeTextColor = Colors.White;
			Assert.Equal(0, fireCount);
		}

		[Fact]
		public void BadgeTextColorBindableProperty()
		{
			var item = new ToolbarItem();
			item.SetValue(ToolbarItem.BadgeTextColorProperty, Colors.Cyan);
			Assert.Equal(Colors.Cyan, item.BadgeTextColor);
		}

		[Fact]
		public void BadgeTextColorDataBinding()
		{
			var vm = new { TextColor = Colors.Magenta };
			var item = new ToolbarItem();
			item.BindingContext = vm;
			item.SetBinding(ToolbarItem.BadgeTextColorProperty, "TextColor");
			Assert.Equal(Colors.Magenta, item.BadgeTextColor);
		}

		[Fact]
		public void SetBadgeTextEmptyStringForDotBadge()
		{
			var item = new ToolbarItem();
			item.BadgeText = "";
			Assert.Equal("", item.BadgeText);
		}

		[Fact]
		public void EmptyStringBadgeIsDistinctFromNull()
		{
			var item = new ToolbarItem();
			item.BadgeText = "";
			Assert.NotNull(item.BadgeText);
			item.BadgeText = null;
			Assert.Null(item.BadgeText);
		}

		[Fact]
		public void EmptyStringBadgeFiresPropertyChanged()
		{
			var item = new ToolbarItem();
			bool fired = false;
			item.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(ToolbarItem.BadgeText))
					fired = true;
			};
			item.BadgeText = "";
			Assert.True(fired);
		}
	}
}
