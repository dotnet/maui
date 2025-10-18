using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ItemsLayoutTypeConverterTests : BaseTestFixture
	{
		[Fact]
		public void HorizontalListShouldReturnLinearItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalList");
			Assert.Same(LinearItemsLayout.Horizontal, result);
		}

		[Fact]
		public void VerticalListShouldReturnLinearItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalList");
			Assert.Same(LinearItemsLayout.Vertical, result);
		}

		[Fact]
		public void HorizontalGridShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalGrid");

			Assert.IsType<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.Equal(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
			Assert.Equal(1, gridItemsLayout.Span);
		}

		[Fact]
		public void VerticalGridShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalGrid");

			Assert.IsType<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.Equal(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
			Assert.Equal(1, gridItemsLayout.Span);
		}

		[Fact]
		public void HorizontalGridWithSpan4ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalGrid, 4");

			Assert.IsType<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.Equal(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
			Assert.Equal(4, gridItemsLayout.Span);
		}

		[Fact]
		public void VerticalGridWithSpan2ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalGrid,\t\t2");

			Assert.IsType<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.Equal(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
			Assert.Equal(2, gridItemsLayout.Span);
		}

		[Fact]
		public void HorizontalGridWithSpan987654ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalGrid,98654");

			Assert.IsType<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.Equal(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
			Assert.Equal(98654, gridItemsLayout.Span);
		}

		[Fact]
		public void VerticalGridWithSpan1234ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalGrid, \t 1234");

			Assert.IsType<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.Equal(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
			Assert.Equal(1234, gridItemsLayout.Span);
		}

		[Fact]
		public void HorizontalGridWithoutSpanShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalGrid,"));
		}

		[Fact]
		public void VerticalGridWithoutSpanShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("VerticalGrid,"));
		}

		[Fact]
		public void HorizontalGridWithSpanIsNotStringShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("HorizontalGrid,test"));
		}

		[Fact]
		public void VerticalGridWithSpanIs1point5ShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("VerticalGrid, 1.5"));
		}

		[Fact]
		public void VerticalGridWith2ArgumentsShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("VerticalGrid, 2, 3"));
		}

		[Fact]
		public void HorizontalGridWithSemicolonShouldShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalGrid; 2"));
		}

		[Fact]
		public void LinearItemsLayoutShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("LinearItemsLayout"));
		}

		[Fact]
		public void HorizontalListWithArgumentShouldShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalList, 1"));
		}

		[Fact]
		public void VerticalGridWithArgumentShouldShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("VerticalList, 2"));
		}

		[Fact]
		public void EmptyStringShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(string.Empty));
		}

		[Fact]
		public void NullShouldThrowArgumentNullException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<ArgumentNullException>(() => converter.ConvertFromInvariantString(null));
		}
	}
}
