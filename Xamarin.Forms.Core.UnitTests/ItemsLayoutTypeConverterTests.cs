using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ItemsLayoutTypeConverterTests : BaseTestFixture
	{
		[Test]
		public void HorizontalListShouldReturnLinearItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalList");
			Assert.AreSame(LinearItemsLayout.Horizontal, result);
		}

		[Test]
		public void VerticalListShouldReturnLinearItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalList");
			Assert.AreSame(LinearItemsLayout.Vertical, result);
		}

		[Test]
		public void HorizontalGridShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalGrid");

			Assert.IsInstanceOf<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.AreEqual(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
			Assert.AreEqual(1, gridItemsLayout.Span);
		}

		[Test]
		public void VerticalGridShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalGrid");

			Assert.IsInstanceOf<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.AreEqual(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
			Assert.AreEqual(1, gridItemsLayout.Span);
		}

		[Test]
		public void HorizontalGridWithSpan4ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalGrid, 4");

			Assert.IsInstanceOf<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.AreEqual(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
			Assert.AreEqual(4, gridItemsLayout.Span);
		}

		[Test]
		public void VerticalGridWithSpan2ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalGrid,\t\t2");

			Assert.IsInstanceOf<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.AreEqual(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
			Assert.AreEqual(2, gridItemsLayout.Span);
		}

		[Test]
		public void HorizontalGridWithSpan987654ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("HorizontalGrid,98654");

			Assert.IsInstanceOf<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.AreEqual(ItemsLayoutOrientation.Horizontal, gridItemsLayout.Orientation);
			Assert.AreEqual(98654, gridItemsLayout.Span);
		}

		[Test]
		public void VerticalGridWithSpan1234ShouldReturnGridItemsLayout()
		{
			var converter = new ItemsLayoutTypeConverter();
			var result = converter.ConvertFromInvariantString("VerticalGrid, \t 1234");

			Assert.IsInstanceOf<GridItemsLayout>(result);
			var gridItemsLayout = (GridItemsLayout)result;
			Assert.AreEqual(ItemsLayoutOrientation.Vertical, gridItemsLayout.Orientation);
			Assert.AreEqual(1234, gridItemsLayout.Span);
		}

		[Test]
		public void HorizontalGridWithSpan0ShouldShouldThrowArgumentException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("HorizontalGrid, 0"));
		}

		[Test]
		public void VerticalGridWithSpan0ShouldShouldThrowArgumentException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("VerticalGrid, 0"));
		}

		[Test]
		public void HorizontalGridWithoutSpanShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalGrid,"));
		}

		[Test]
		public void VerticalGridWithoutSpanShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("VerticalGrid,"));
		}

		[Test]
		public void HorizontalGridWithSpanIsNotStringShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("HorizontalGrid,test"));
		}

		[Test]
		public void VerticalGridWithSpanIs1point5ShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("VerticalGrid, 1.5"));
		}

		[Test]
		public void VerticalGridWith2ArgumentsShouldShouldThrowFormatException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("VerticalGrid, 2, 3"));
		}

		[Test]
		public void HorizontalGridWithSemicolonShouldShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalGrid; 2"));
		}

		[Test]
		public void LinearItemsLayoutShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("LinearItemsLayout"));
		}

		[Test]
		public void HorizontalListWithArgumentShouldShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("HorizontalList, 1"));
		}

		[Test]
		public void VerticalGridWithArgumentShouldShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("VerticalList, 2"));
		}

		[Test]
		public void EmptyStringShouldThrowInvalidOperationException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(string.Empty));
		}

		[Test]
		public void NullShouldThrowArgumentNullException()
		{
			var converter = new ItemsLayoutTypeConverter();
			Assert.Throws<ArgumentNullException>(() => converter.ConvertFromInvariantString(null));
		}
	}
}