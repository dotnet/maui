﻿using System.Collections.Generic;
using NSubstitute;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using static Microsoft.Maui.UnitTests.Layouts.LayoutTestHelpers;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Layout)]
	public class GridLayoutManagerTests 
	{
		const string GridSpacing = "GridSpacing";
		const string GridAutoSizing = "GridAutoSizing";
		const string GridStarSizing = "GridStarSizing";
		const string GridAbsoluteSizing = "GridAbsoluteSizing";
		const string GridSpan = "GridSpan";

		IGridLayout CreateGridLayout(int rowSpacing = 0, int colSpacing = 0,
			string rows = null, string columns = null)
		{
			IEnumerable<IGridRowDefinition> rowDefs = null;
			IEnumerable<IGridColumnDefinition> colDefs = null;

			if (rows != null)
			{
				rowDefs = CreateTestRows(rows.Split(","));
			}

			if (columns != null)
			{
				colDefs = CreateTestColumns(columns.Split(","));
			}

			var grid = Substitute.For<IGridLayout>();

			grid.RowSpacing.Returns(rowSpacing);
			grid.ColumnSpacing.Returns(colSpacing);

			SubRowDefs(grid, rowDefs);
			SubColDefs(grid, colDefs);

			return grid;
		}

		void SubRowDefs(IGridLayout grid, IEnumerable<IGridRowDefinition> rows = null) 
		{
			if (rows == null)
			{
				var rowDef = Substitute.For<IGridRowDefinition>();
				rowDef.Height.Returns(GridLength.Auto);
				var rowDefs = new List<IGridRowDefinition>
				{
					rowDef
				};
				grid.RowDefinitions.Returns(rowDefs);
			}
			else
			{
				grid.RowDefinitions.Returns(rows);
			}
		}

		void SubColDefs(IGridLayout grid, IEnumerable<IGridColumnDefinition> cols = null)
		{
			if (cols == null)
			{
				var colDefs = CreateTestColumns("auto");
				grid.ColumnDefinitions.Returns(colDefs);
			}
			else
			{
				grid.ColumnDefinitions.Returns(cols);
			}
		}

		List<IGridColumnDefinition> CreateTestColumns(params string[] columnWidths) 
		{
			var converter = new GridLengthTypeConverter();

			var colDefs = new List<IGridColumnDefinition>();

			foreach (var width in columnWidths)
			{
				var gridLength = converter.ConvertFromInvariantString(width);
				var colDef = Substitute.For<IGridColumnDefinition>();
				colDef.Width.Returns(gridLength);
				colDefs.Add(colDef);
			}

			return colDefs;
		}

		List<IGridRowDefinition> CreateTestRows(params string[] rowHeights)
		{
			var converter = new GridLengthTypeConverter();

			var rowDefs = new List<IGridRowDefinition>();

			foreach (var height in rowHeights)
			{
				var gridLength = converter.ConvertFromInvariantString(height);
				var rowDef = Substitute.For<IGridRowDefinition>();
				rowDef.Height.Returns(gridLength);
				rowDefs.Add(rowDef);
			}

			return rowDefs;
		}

		void SetLocation(IGridLayout grid, IView view, int row = 0, int col = 0, int rowSpan = 1, int colSpan = 1) 
		{
			grid.GetRow(view).Returns(row);
			grid.GetRowSpan(view).Returns(rowSpan);
			grid.GetColumn(view).Returns(col);
			grid.GetColumnSpan(view).Returns(colSpan);
		}

		Size MeasureAndArrange(IGridLayout grid, double widthConstraint = double.PositiveInfinity, double heightConstraint = double.PositiveInfinity) 
		{
			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(widthConstraint, heightConstraint);
			manager.ArrangeChildren(new Rectangle(Point.Zero, measuredSize));

			return measuredSize;
		}

		void AssertArranged(IView view, double x, double y, double width, double height)
		{
			var expected = new Rectangle(x, y, width, height);
			view.Received().Arrange(Arg.Is(expected));
		}

		[Category(GridAutoSizing)]
		[Fact]
		public void OneAutoRowOneAutoColumn() 
		{
			// A one-row, one-column grid
			var grid = CreateGridLayout();

			// A 100x100 IView
			var view = CreateTestView(new Size(100, 100));

			// Set up the grid to have a single child
			AddChildren(grid, view);

			// Set up the row/column values and spans
			SetLocation(grid, view);

			MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);

			// We expect that the only child of the grid will be given its full size
			AssertArranged(view, 0, 0, 100, 100);
		}

		[Category(GridAbsoluteSizing)]
		[Fact]
		public void TwoAbsoluteColumnsOneAbsoluteRow()
		{
			var grid = CreateGridLayout(columns: "100, 100", rows: "10");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);

			AddChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			// Assuming no constraints on space
			MeasureAndArrange(grid, double.PositiveInfinity, double.NegativeInfinity);

			// Column width is 100, viewSize is less than that, so it should be able to layout out at full size
			AssertArranged(view0, 0, 0, 100, 10);

			// Since the first column is 100 wide, we expect the view in the second column to start at x = 100
			AssertArranged(view1, 100, 0, 100, 10);
		}

		[Category(GridAbsoluteSizing)]
		[Fact]
		public void TwoAbsoluteRowsAndColumns()
		{
			var grid = CreateGridLayout(columns: "100, 100", rows: "10, 30");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);
			var view2 = CreateTestView(viewSize);
			var view3 = CreateTestView(viewSize);

			AddChildren(grid, view0, view1, view2, view3);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, row: 1);
			SetLocation(grid, view3, row: 1, col: 1);

			// Assuming no constraints on space
			MeasureAndArrange(grid, double.PositiveInfinity, double.NegativeInfinity);

			AssertArranged(view0, 0, 0, 100, 10);

			// Since the first column is 100 wide, we expect the view in the second column to start at x = 100
			AssertArranged(view1, 100, 0, 100, 10);

			// First column, second row, so y should be 10
			AssertArranged(view2, 0, 10, 100, 30);

			// Second column, second row, so 100, 10
			AssertArranged(view3, 100, 10, 100, 30);
		}

		[Category(GridAbsoluteSizing), Category(GridAutoSizing)]
		[Fact]
		public void TwoAbsoluteColumnsOneAutoRow()
		{
			var grid = CreateGridLayout(columns: "100, 100");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);

			AddChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			// Assuming no constraints on space
			MeasureAndArrange(grid, double.PositiveInfinity, double.NegativeInfinity);

			// Column width is 100, viewSize is less, so it should be able to layout at full size
			AssertArranged(view0, 0, 0, 100, viewSize.Height);

			// Since the first column is 100 wide, we expect the view in the second column to start at x = 100
			AssertArranged(view1, 100, 0, 100, viewSize.Height);
		}

		[Category(GridAbsoluteSizing), Category(GridAutoSizing)]
		[Fact]
		public void TwoAbsoluteRowsOneAutoColumn()
		{
			var grid = CreateGridLayout(rows: "100, 100");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);

			AddChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);

			// Assuming no constraints on space
			MeasureAndArrange(grid, double.PositiveInfinity, double.NegativeInfinity);

			// Row height is 100, so full view should fit
			AssertArranged(view0, 0, 0, viewSize.Width, 100);

			// Since the first row is 100 tall, we expect the view in the second row to start at y = 100
			AssertArranged(view1, 0, 100, viewSize.Width, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Row spacing shouldn't affect a single-row grid")]
		public void SingleRowIgnoresRowSpacing()
		{
			var grid = CreateGridLayout(rowSpacing: 10);
			var view = CreateTestView(new Size(100, 100));
			AddChildren(grid, view);
			SetLocation(grid, view);

			MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);
			AssertArranged(view, 0, 0, 100, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Two rows should include the row spacing once")]
		public void TwoRowsWithSpacing()
		{
			var grid = CreateGridLayout(rows: "100, 100", rowSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			AddChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);

			MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);
			AssertArranged(view0, 0, 0, 100, 100);

			// With column width 100 and spacing of 10, we expect the second column to start at 110
			AssertArranged(view1, 0, 110, 100, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Measure should include row spacing")]
		public void MeasureTwoRowsWithSpacing()
		{
			var grid = CreateGridLayout(rows: "100, 100", rowSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			AddChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100 + 100 + 10, measure.Height);
		}

		[Category(GridAutoSizing)]
		[Fact(DisplayName = "Auto rows without content have height zero")]
		public void EmptyAutoRowsHaveNoHeight()
		{
			var grid = CreateGridLayout(rows: "100, auto, 100");
			var view0 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			AddChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, row: 2);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rectangle(0, 0, measure.Width, measure.Height));

			// Because the auto row has no content, we expect it to have height zero
			Assert.Equal(100 + 100, measure.Height);

			// Verify the offset for the third row
			AssertArranged(view2, 0, 100, 100, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Empty rows should not incur additional row spacing")]
		public void RowSpacingForEmptyRows()
		{
			var grid = CreateGridLayout(rows: "100, auto, 100", rowSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			AddChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, row: 2);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// Because the auto row has no content, we expect it to have height zero
			// and we expect that it won't add more row spacing 
			Assert.Equal(100 + 100 + 10, measure.Height);
		}

		[Fact(DisplayName = "Column spacing shouldn't affect a single-column grid")]
		public void SingleColumnIgnoresColumnSpacing()
		{
			var grid = CreateGridLayout(colSpacing: 10);
			var view = CreateTestView(new Size(100, 100));
			AddChildren(grid, view);
			SetLocation(grid, view);

			MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);
			AssertArranged(view, 0, 0, 100, 100);
		}

		[Fact(DisplayName = "Two columns should include the column spacing once")]
		public void TwoColumnsWithSpacing()
		{
			var grid = CreateGridLayout(columns: "100, 100", colSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			AddChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);
			AssertArranged(view0, 0, 0, 100, 100);

			// With column width 100 and spacing of 10, we expect the second column to start at 110
			AssertArranged(view1, 110, 0, 100, 100);
		}

		[Fact(DisplayName = "Measure should include column spacing")]
		public void MeasureTwoColumnsWithSpacing()
		{
			var grid = CreateGridLayout(columns: "100, 100", colSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			AddChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100 + 100 + 10, measure.Width);
		}

		[Category(GridAutoSizing)]
		[Fact(DisplayName = "Auto columns without content have width zero")]
		public void EmptyAutoColumnsHaveNoWidth()
		{
			var grid = CreateGridLayout(columns: "100, auto, 100");
			var view0 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			AddChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, col: 2);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rectangle(0, 0, measure.Width, measure.Height));

			// Because the auto column has no content, we expect it to have width zero
			Assert.Equal(100 + 100, measure.Width);

			// Verify the offset for the third column
			AssertArranged(view2, 100, 0, 100, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Empty columns should not incur additional column spacing")]
		public void ColumnSpacingForEmptyColumns()
		{
			var grid = CreateGridLayout(columns: "100, auto, 100", colSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			AddChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, col: 2);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// Because the auto column has no content, we expect it to have height zero
			// and we expect that it won't add more row spacing 
			Assert.Equal(100 + 100 + 10, measure.Width);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Simple row spanning")]
		public void ViewSpansRows()
		{
			var grid = CreateGridLayout(rows: "auto, auto");
			var view0 = CreateTestView(new Size(100, 100));
			AddChildren(grid, view0);
			SetLocation(grid, view0, rowSpan: 2);

			var measuredSize = MeasureAndArrange(grid);

			AssertArranged(view0, 0, 0, 100, 100);
			Assert.Equal(100, measuredSize.Width);

			// We expect the rows to each get half the view height
			Assert.Equal(100, measuredSize.Height);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Simple row spanning with multiple views")]
		public void ViewSpansRowsWhenOtherViewsPresent()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto");
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 50));
			AddChildren(grid, view0, view1);
			
			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(100 + 50, measuredSize.Width);
			Assert.Equal(100, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);
			AssertArranged(view1, 100, 25, 50, 75);
		}

		[Category(GridSpan)]
		[Category(GridSpacing)]
		[Fact(DisplayName = "Row spanning with row spacing")]
		public void RowSpanningShouldAccountForSpacing()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto", rowSpacing: 5);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 50));
			var view2 = CreateTestView(new Size(50, 50));
			AddChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 0, col: 1);
			SetLocation(grid, view2, row: 1, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(150, measuredSize.Width);
			Assert.Equal(50 + 50 + 5, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);
			AssertArranged(view1, 100, 0, 50, 50);
			AssertArranged(view2, 100, 55, 50, 50);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Simple column spanning with multiple views")]
		public void ViewSpansColumnsWhenOtherViewsPresent()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto");
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 50));
			AddChildren(grid, view0, view1);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(100, measuredSize.Width);
			Assert.Equal(100 + 50, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);
			AssertArranged(view1, 25, 100, 75, 50);
		}

		[Category(GridSpan)]
		[Category(GridSpacing)]
		[Fact(DisplayName = "Column spanning with column spacing")]
		public void ColumnSpanningShouldAccountForSpacing()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto", colSpacing: 5);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 50));
			var view2 = CreateTestView(new Size(50, 50));
			AddChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 0);
			SetLocation(grid, view2, row: 1, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(50 + 50 + 5, measuredSize.Width);
			Assert.Equal(100 + 50, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);
			AssertArranged(view1, 0, 100, 50, 50);
			AssertArranged(view2, 55, 100, 50, 50);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Row-spanning views smaller than the views confined to the row should not affect row size")]
		public void SmallerSpanningViewsShouldNotAffectRowSize() 
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto");
			var view0 = CreateTestView(new Size(30, 30));
			var view1 = CreateTestView(new Size(50, 50));
			AddChildren(grid, view0, view1);

			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 0, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(30 + 50, measuredSize.Width);
			Assert.Equal(50, measuredSize.Height);

			AssertArranged(view0, 0, 0, 30, 50);
			AssertArranged(view1, 30, 0, 50, 50);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Column-spanning views smaller than the views confined to the column should not affect column size")]
		public void SmallerSpanningViewsShouldNotAffectColumnSize()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto");
			var view0 = CreateTestView(new Size(30, 30));
			var view1 = CreateTestView(new Size(50, 50));
			AddChildren(grid, view0, view1);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 0);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(50, measuredSize.Width);
			Assert.Equal(30 + 50, measuredSize.Height);

			AssertArranged(view0, 0, 0, 50, 30);
			AssertArranged(view1, 0, 30, 50, 50);
		}


		[Category(GridAbsoluteSizing)]
		[Fact(DisplayName = "Empty absolute rows/columns still affect Grid size")]
		public void EmptyAbsoluteRowsAndColumnsAffectSize()
		{
			var grid = CreateGridLayout(rows: "10, 40", columns: "15, 85");
			var view0 = CreateTestView(new Size(30, 30));
			AddChildren(grid, view0);

			SetLocation(grid, view0, row: 1, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(15 + 85, measuredSize.Width);
			Assert.Equal(10 + 40, measuredSize.Height);

			AssertArranged(view0, 15, 10, 85, 40);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Row and column spans should be able to mix")]
		public void MixedRowAndColumnSpans()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto");
			var view0 = CreateTestView(new Size(60, 30));
			var view1 = CreateTestView(new Size(30, 60));
			AddChildren(grid, view0, view1);

			SetLocation(grid, view0, row: 0, col: 0, colSpan: 2);
			SetLocation(grid, view1, row: 0, col: 1, rowSpan: 2);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(60, measuredSize.Width);
			Assert.Equal(60, measuredSize.Height);

			AssertArranged(view0, 0, 0, 60, 45);
			AssertArranged(view1, 15, 0, 45, 60);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Row span including absolute row should not modify absolute size")]
		public void RowSpanShouldNotModifyAbsoluteRowSize()
		{
			var grid = CreateGridLayout(rows: "auto, 20", columns: "auto, auto");
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 10));
			AddChildren(grid, view0, view1);

			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(100 + 50, measuredSize.Width);
			Assert.Equal(100, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);

			// The item in the second row starts at y = 80 because the auto row above had to distribute
			// all the extra space into row 0; row 1 is absolute, so no tinkering with it to make stuff fit
			AssertArranged(view1, 100, 80, 50, 20);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Column span including absolute column should not modify absolute size")]
		public void ColumnSpanShouldNotModifyAbsoluteColumnSize()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, 20");
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 10));
			AddChildren(grid, view0, view1);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrange(grid);

			Assert.Equal(100, measuredSize.Width);
			Assert.Equal(100 + 10, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);

			// The item in the second row starts at x = 80 because the auto column before it had to distribute
			// all the extra space into column 0; column 1 is absolute, so no tinkering with it to make stuff fit
			AssertArranged(view1, 80, 100, 20, 10);
		}
	}
}
