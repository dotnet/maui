using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[Category("Layout")]
	public class GridExtensionTests
	{
		[Fact]
		public void AddViewRowColumn()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, 3, 4);

			Assert.Equal(3, grid.GetColumn(label));
			Assert.Equal(4, grid.GetRow(label));
		}

		[Fact]
		public void AddViewDefaultRow()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, 3);

			Assert.Equal(3, grid.GetColumn(label));
			Assert.Equal(0, grid.GetRow(label));
		}

		[Fact]
		public void AddViewDefaultColumn()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, row: 3);

			Assert.Equal(0, grid.GetColumn(label));
			Assert.Equal(3, grid.GetRow(label));
		}

		[Fact]
		public void AddEnsuresRows()
		{
			var grid = new Grid();
			grid.Add(new Label(), 0, 4);

			Assert.Equal(5, grid.RowDefinitions.Count);
		}

		[Fact]
		public void AddEnsuresColumns()
		{
			var grid = new Grid();
			grid.Add(new Label(), 4, 0);

			Assert.Equal(5, grid.ColumnDefinitions.Count);
		}

		[Fact]
		public void AddLRTBEnsuresRows()
		{
			var grid = new Grid();
			grid.Add(new Label(), 0, 1, 0, 5);

			Assert.Equal(5, grid.RowDefinitions.Count);
		}

		[Fact]
		public void AddLRTBEnsuresColumns()
		{
			var grid = new Grid();
			grid.Add(new Label(), 0, 5, 0, 1);

			Assert.Equal(5, grid.ColumnDefinitions.Count);
		}

		[Fact]
		public void WithSpanEnsuresRows()
		{
			var grid = new Grid();
			grid.AddWithSpan(new Label(), rowSpan: 2);

			Assert.Equal(2, grid.RowDefinitions.Count);
		}

		[Fact]
		public void WithSpanEnsuresColumns()
		{
			var grid = new Grid();
			grid.AddWithSpan(new Label(), columnSpan: 2);

			Assert.Equal(2, grid.ColumnDefinitions.Count);
		}

		[Theory]
		[InlineData(-1, 1, 0, 1)]
		[InlineData(0, 1, -1, 1)]
		public void ThrowsOnInvalidCells(int left, int right, int top, int bottom)
		{
			var grid = new Grid();
			Assert.Throws<ArgumentOutOfRangeException>(() => grid.Add(new Label(), left, right, top, bottom));
		}

		[Theory]
		[InlineData(0, 0, 0, 1)]
		[InlineData(0, 1, 0, 0)]
		[InlineData(1, 0, 0, 1)]
		[InlineData(0, 1, 1, 0)]
		public void ThrowsOnInvalidSpans(int left, int right, int top, int bottom)
		{
			var grid = new Grid();
			Assert.Throws<ArgumentOutOfRangeException>(() => grid.Add(new Label(), left, right, top, bottom));
		}
	}
}
