using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Xunit;
using static Microsoft.Maui.UnitTests.Layouts.LayoutTestHelpers;
using LayoutAlignment = Microsoft.Maui.Primitives.LayoutAlignment;

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
			string rows = null, string columns = null, IList<IView> children = null)
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

			grid.Height.Returns(Dimension.Unset);
			grid.Width.Returns(Dimension.Unset);
			grid.MinimumHeight.Returns(Dimension.Minimum);
			grid.MinimumWidth.Returns(Dimension.Minimum);
			grid.MaximumHeight.Returns(Dimension.Maximum);
			grid.MaximumWidth.Returns(Dimension.Maximum);

			grid.RowSpacing.Returns(rowSpacing);
			grid.ColumnSpacing.Returns(colSpacing);

			SubRowDefs(grid, rowDefs);
			SubColDefs(grid, colDefs);

			if (children != null)
			{
				SubstituteChildren(grid, children);
			}

			return grid;
		}

		void SubRowDefs(IGridLayout grid, IEnumerable<IGridRowDefinition> rows = null)
		{
			if (rows == null)
			{
				var rowDefs = new List<IGridRowDefinition>();
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
				var colDefs = new List<IGridColumnDefinition>();
				grid.ColumnDefinitions.Returns(colDefs);
			}
			else
			{
				grid.ColumnDefinitions.Returns(cols);
			}
		}

		static GridLength GridLengthFromString(string gridLength)
		{
			gridLength = gridLength.Trim();

			if (gridLength.EndsWith("*"))
			{
				gridLength = gridLength.Substring(0, gridLength.Length - 1);

				if (gridLength.Length == 0)
				{
					return GridLength.Star;
				}

				return new GridLength(double.Parse(gridLength), GridUnitType.Star);
			}

			if (gridLength.ToLowerInvariant() == "auto")
			{
				return GridLength.Auto;
			}

			return new GridLength(double.Parse(gridLength));
		}

		List<IGridColumnDefinition> CreateTestColumns(params string[] columnWidths)
		{
			var colDefs = new List<IGridColumnDefinition>();

			foreach (var width in columnWidths)
			{
				var gridLength = GridLengthFromString(width);
				var colDef = Substitute.For<IGridColumnDefinition>();
				colDef.Width.Returns(gridLength);
				colDefs.Add(colDef);
			}

			return colDefs;
		}

		List<IGridRowDefinition> CreateTestRows(params string[] rowHeights)
		{
			var rowDefs = new List<IGridRowDefinition>();

			foreach (var height in rowHeights)
			{
				var gridLength = GridLengthFromString(height);
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

		static Size MeasureAndArrangeFixed(IGridLayout grid, double widthConstraint, double heightConstraint, double left = 0, double top = 0)
		{
			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(widthConstraint, heightConstraint);

			var arrangeSize = new Size(widthConstraint, heightConstraint);

			manager.ArrangeChildren(new Rect(new Point(left, top), arrangeSize));

			return measuredSize;
		}

		static Size MeasureAndArrange(IGridLayout grid, double widthConstraint = double.PositiveInfinity, double heightConstraint = double.PositiveInfinity, double left = 0, double top = 0)
		{
			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(widthConstraint, heightConstraint);
			manager.ArrangeChildren(new Rect(new Point(left, top), measuredSize));

			return measuredSize;
		}

		static Size MeasureAndArrangeAuto(IGridLayout grid)
		{
			return MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity, 0, 0);
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
			SubstituteChildren(grid, view);

			// Set up the row/column values and spans
			SetLocation(grid, view);

			MeasureAndArrangeAuto(grid);

			// No rows/columns were specified, so the implied */* is used; we're measuring with infinity, so
			// we expect that the view will be arranged at its measured size
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

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			// Assuming no constraints on space
			MeasureAndArrangeAuto(grid);

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

			SubstituteChildren(grid, view0, view1, view2, view3);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, row: 1);
			SetLocation(grid, view3, row: 1, col: 1);

			// Assuming no constraints on space
			MeasureAndArrangeAuto(grid);

			// Verify that the views are getting measured at all, and that they're being measured at 
			// the appropriate sizes
			view0.Received().Measure(Arg.Is<double>(100), Arg.Is<double>(10));
			view1.Received().Measure(Arg.Is<double>(100), Arg.Is<double>(10));
			view2.Received().Measure(Arg.Is<double>(100), Arg.Is<double>(30));
			view3.Received().Measure(Arg.Is<double>(100), Arg.Is<double>(30));

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

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			// Assuming no constraints on space
			MeasureAndArrangeAuto(grid);

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

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);

			// Assuming no constraints on space
			MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			MeasureAndArrangeAuto(grid);
			AssertArranged(view, 0, 0, 100, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Two rows should include the row spacing once")]
		public void TwoRowsWithSpacing()
		{
			var grid = CreateGridLayout(rows: "100, 100", rowSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);

			MeasureAndArrangeAuto(grid);
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
			SubstituteChildren(grid, view0, view1);
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

			SubstituteChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, row: 2);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(0, 0, measure.Width, measure.Height));

			// Because the auto row has no content, we expect it to have height zero
			Assert.Equal(100 + 100, measure.Height);

			// Verify the offset for the third row
			AssertArranged(view2, 0, 100, 100, 100);
		}

		[Category(GridSpacing, GridAutoSizing)]
		[Fact(DisplayName = "Empty rows should still count for row spacing")]
		public void RowSpacingForEmptyRows()
		{
			var grid = CreateGridLayout(rows: "100, auto, 100", rowSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, row: 2);

			var measure = MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100 + 100 + 10 + 10, measure.Height);
			AssertArranged(view2, new Rect(0, 100 + 10 + 10, 100, 100));
		}

		[Category(GridSpacing, GridAutoSizing)]
		[Fact(DisplayName = "Auto rows with collapsed views should still count for row spacing")]
		public void RowSpacingForAutoRowsWithCollapsedViews()
		{
			var grid = CreateGridLayout(rows: "100, auto, 100", rowSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);
			SetLocation(grid, view2, row: 2);

			view1.Visibility.Returns(Visibility.Collapsed);

			var measure = MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100 + 100 + 10 + 10, measure.Height);
			AssertArranged(view2, new Rect(0, 100 + 10 + 10, 100, 100));
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Column spacing shouldn't affect a single-column grid")]
		public void SingleColumnIgnoresColumnSpacing()
		{
			var grid = CreateGridLayout(colSpacing: 10);
			var view = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			MeasureAndArrangeAuto(grid);
			AssertArranged(view, 0, 0, 100, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Two columns should include the column spacing once")]
		public void TwoColumnsWithSpacing()
		{
			var grid = CreateGridLayout(columns: "100, 100", colSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			MeasureAndArrangeAuto(grid);
			AssertArranged(view0, 0, 0, 100, 100);

			// With column width 100 and spacing of 10, we expect the second column to start at 110
			AssertArranged(view1, 110, 0, 100, 100);
		}

		[Category(GridSpacing)]
		[Fact(DisplayName = "Measure should include column spacing")]
		public void MeasureTwoColumnsWithSpacing()
		{
			var grid = CreateGridLayout(columns: "100, 100", colSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view0, view1);
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

			SubstituteChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, col: 2);

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(0, 0, measure.Width, measure.Height));

			// Because the auto column has no content, we expect it to have width zero
			Assert.Equal(100 + 100, measure.Width);

			// Verify the offset for the third column
			AssertArranged(view2, 100, 0, 100, 100);
		}

		[Category(GridSpacing, GridAutoSizing)]
		[Fact(DisplayName = "Empty columns still count for column spacing")]
		public void ColumnSpacingForEmptyColumns()
		{
			var grid = CreateGridLayout(columns: "auto, auto, auto", colSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view2, col: 2);

			var measure = MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100 + 100 + 10 + 10, measure.Width);
			AssertArranged(view2, new Rect(100 + 10 + 10, 0, 100, 100));
		}

		[Category(GridSpacing, GridAutoSizing)]
		[Fact(DisplayName = "Auto columns with collapsed views should still count for column spacing")]
		public void AutoColumnsWithCollapsedViews()
		{
			var grid = CreateGridLayout(columns: "100, auto, 100", colSpacing: 10);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view2);
			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, col: 2);

			view1.Visibility.Returns(Visibility.Collapsed);

			var measure = MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100 + 100 + 10 + 10, measure.Width);
			AssertArranged(view2, new Rect(100 + 10 + 10, 0, 100, 100));
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Simple row spanning")]
		public void ViewSpansRows()
		{
			var grid = CreateGridLayout(rows: "auto, auto");
			var view0 = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, rowSpan: 2);

			var measuredSize = MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

			Assert.Equal(100 + 50, measuredSize.Width);
			Assert.Equal(100, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);
			AssertArranged(view1, 100, 25, 50, 75);
		}

		[Category(GridSpacing, GridSpan)]
		[Fact(DisplayName = "Row spanning with row spacing")]
		public void RowSpanningShouldAccountForSpacing()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto", rowSpacing: 5);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 50));
			var view2 = CreateTestView(new Size(50, 50));
			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 0, col: 1);
			SetLocation(grid, view2, row: 1, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

			Assert.Equal(150, measuredSize.Width);
			Assert.Equal(50 + 50 + 5, measuredSize.Height);

			// Starts a Y = 0
			AssertArranged(view1, 100, 0, 50, 50);

			// Starts at the first row's height + the row spacing value, so Y = 50 + 5 = 55
			AssertArranged(view2, 100, 55, 50, 50);

			// We expect the height for the view spanning the rows to include the space between the rows,
			// so 50 + 5 + 50 = 105
			AssertArranged(view0, 0, 0, 100, 105);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Simple column spanning with multiple views")]
		public void ViewSpansColumnsWhenOtherViewsPresent()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto");
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(50, 50));
			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 0);
			SetLocation(grid, view2, row: 1, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

			Assert.Equal(50 + 50 + 5, measuredSize.Width);
			Assert.Equal(100 + 50, measuredSize.Height);

			// Starts a X = 0
			AssertArranged(view1, 0, 100, 50, 50);
			// Starts at the first column's width + the column spacing, so X = 50 + 5 = 55
			AssertArranged(view2, 55, 100, 50, 50);

			// We expect the width for the view spanning the columns to include the space between the columns,
			// so 50 + 5 + 50 = 105
			AssertArranged(view0, 0, 0, 105, 100);
		}

		[Category(GridSpan)]
		[Fact(DisplayName = "Row-spanning views smaller than the views confined to the row should not affect row size")]
		public void SmallerSpanningViewsShouldNotAffectRowSize()
		{
			var grid = CreateGridLayout(rows: "auto, auto", columns: "auto, auto");
			var view0 = CreateTestView(new Size(30, 30));
			var view1 = CreateTestView(new Size(50, 50));
			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 0, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 0);

			var measuredSize = MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view0);

			SetLocation(grid, view0, row: 1, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, row: 0, col: 0, colSpan: 2);
			SetLocation(grid, view1, row: 0, col: 1, rowSpan: 2);

			var measuredSize = MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, rowSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

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
			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 1);

			var measuredSize = MeasureAndArrangeAuto(grid);

			Assert.Equal(100, measuredSize.Width);
			Assert.Equal(100 + 10, measuredSize.Height);

			AssertArranged(view0, 0, 0, 100, 100);

			// The item in the second row starts at x = 80 because the auto column before it had to distribute
			// all the extra space into column 0; column 1 is absolute, so no tinkering with it to make stuff fit
			AssertArranged(view1, 80, 100, 20, 10);
		}

		[Category(GridSpan)]
		[Fact]
		public void CanSpanAbsoluteColumns()
		{
			var grid = CreateGridLayout(rows: "auto", columns: "100,100");
			var view0 = CreateTestView(new Size(150, 100));
			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, colSpan: 2);
			var manager = new GridLayoutManager(grid);

			manager.Measure(200, 100);
			manager.ArrangeChildren(new Rect(0, 0, 200, 100));

			// View should be arranged to span both columns (200 points)
			AssertArranged(view0, 0, 0, 200, 100);
		}

		[Category(GridSpan)]
		[Fact]
		public void CanSpanAbsoluteRows()
		{
			var grid = CreateGridLayout(rows: "100,100", columns: "auto");
			var view0 = CreateTestView(new Size(100, 150));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, rowSpan: 2);
			var manager = new GridLayoutManager(grid);

			manager.Measure(100, 200);
			manager.ArrangeChildren(new Rect(0, 0, 100, 200));

			// View should be arranged to span both rows (200 points)
			AssertArranged(view0, 0, 0, 100, 200);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Single star column consumes all horizontal space")]
		public void SingleStarColumn()
		{
			var screenWidth = 400;
			var screenHeight = 600;

			var grid = CreateGridLayout(rows: "auto", columns: $"*");
			var view0 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Row height is auto, so it gets the height of the view
			// Column is *, so it should get the whole width
			AssertArranged(view0, 0, 0, screenWidth, 100);
		}

		[Category(GridStarSizing)]
		[Fact]
		public void SingleWeightedStarColumn()
		{
			var screenWidth = 400;
			var screenHeight = 600;

			var grid = CreateGridLayout(rows: "auto", columns: $"3*");
			var view0 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Row height is auto, so it gets the height of the view
			// The column is 3*, but it's the only column, so it should get the full width
			AssertArranged(view0, 0, 0, screenWidth, 100);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Multiple star columns consume equal space")]
		public void MultipleStarColumns()
		{
			var screenWidth = 300;
			var screenHeight = 600;
			var viewSize = new Size(50, 50);

			var grid = CreateGridLayout(rows: "auto", columns: $"*,*,*");
			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);
			var view2 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, col: 2);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Row height is auto, so it gets the height of the view
			// Columns are *,*,*, so each view should be arranged at 1/3 the width
			var expectedWidth = screenWidth / 3;
			var expectedHeight = viewSize.Height;

			// Make sure that the views in the columns are actually getting measured at the column width,
			// and not just at the width of the whole grid
			view1.Received().Measure(Arg.Is<double>(expectedWidth), Arg.Any<double>());
			view2.Received().Measure(Arg.Is<double>(expectedWidth), Arg.Any<double>());

			AssertArranged(view0, 0, 0, expectedWidth, expectedHeight);
			AssertArranged(view1, expectedWidth, 0, expectedWidth, expectedHeight);
			AssertArranged(view2, expectedWidth * 2, 0, expectedWidth, expectedHeight);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Weighted star column gets proportional space")]
		public void WeightedStarColumn()
		{
			var screenWidth = 300;
			var screenHeight = 600;
			var viewSize = new Size(50, 50);

			var grid = CreateGridLayout(rows: "auto", columns: $"*,2*");
			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Row height is auto, so it gets the height of the view
			// First column should get 1/3 of the width, second should get 2/3
			var expectedWidth0 = screenWidth / 3;
			var expectedWidth1 = expectedWidth0 * 2;
			var expectedHeight = viewSize.Height;
			AssertArranged(view0, 0, 0, expectedWidth0, expectedHeight);
			AssertArranged(view1, expectedWidth0, 0, expectedWidth1, expectedHeight);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Totally empty star columns measured at infinite width have zero width")]
		public void EmptyStarColumnInfiniteWidthMeasure()
		{
			var grid = CreateGridLayout(rows: "auto", columns: $"*");
			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(0, measuredSize.Width);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Single star column with a view measured at infinite width gets width of the view")]
		public void StarColumnWithViewInfiniteWidthMeasure()
		{
			var grid = CreateGridLayout(rows: "auto", columns: $"*");
			var view0 = CreateTestView(new Size(100, 50));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0);

			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100, measuredSize.Width);
			Assert.Equal(50, measuredSize.Height);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Single star row consumes all vertical space")]
		public void SingleStarRow()
		{
			var screenWidth = 400;
			var screenHeight = 600;

			var grid = CreateGridLayout(rows: "*", columns: "auto");
			var view0 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Column width is auto, so it gets the width of the view
			// Row is *, so it should get the whole height
			AssertArranged(view0, 0, 0, 100, screenHeight);
		}

		[Category(GridStarSizing)]
		[Fact]
		public void SingleWeightedStarRow()
		{
			var screenWidth = 400;
			var screenHeight = 600;

			var grid = CreateGridLayout(rows: "3*", columns: "auto");
			var view0 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Column width is auto, so it gets the width of the view
			// The row is 3*, but it's the only row, so it should get the full height
			AssertArranged(view0, 0, 0, 100, screenHeight);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Multiple star rows consume equal space")]
		public void MultipleStarRows()
		{
			var screenWidth = 300;
			var screenHeight = 600;
			var viewSize = new Size(50, 50);

			var grid = CreateGridLayout(rows: "*,*,*", columns: "auto");
			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);
			var view2 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);
			SetLocation(grid, view2, row: 2);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Column width is auto, so it gets the width of the view
			// Rows are *,*,*, so each view should be arranged at 1/3 the height
			var expectedHeight = screenHeight / 3;
			var expectedWidth = viewSize.Width;
			AssertArranged(view0, 0, 0, expectedWidth, expectedHeight);
			AssertArranged(view1, 0, expectedHeight, expectedWidth, expectedHeight);
			AssertArranged(view2, 0, expectedHeight * 2, expectedWidth, expectedHeight);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Weighted star row gets proportional space")]
		public void WeightedStarRow()
		{
			var screenWidth = 300;
			var screenHeight = 600;
			var viewSize = new Size(50, 50);

			var grid = CreateGridLayout(rows: "*,2*", columns: "auto");
			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Column width is auto, so it gets the width of the view
			// First row should get 1/3 of the height, second should get 2/3
			var expectedHeight0 = screenHeight / 3;
			var expectedHeight1 = expectedHeight0 * 2;
			var expectedWidth = viewSize.Width;
			AssertArranged(view0, 0, 0, expectedWidth, expectedHeight0);
			AssertArranged(view1, 0, expectedHeight0, expectedWidth, expectedHeight1);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Totally empty star rows measured at infinite height have zero height")]
		public void EmptyStarRowInfiniteHeightMeasure()
		{
			var grid = CreateGridLayout(rows: "*", columns: $"auto");
			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(0, measuredSize.Height);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "Single star row with a view measured at infinite height gets height of the view")]
		public void StarRowWithViewInfiniteHeightMeasure()
		{
			var grid = CreateGridLayout(rows: "*", columns: $"auto");
			var view0 = CreateTestView(new Size(100, 50));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0);

			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(100, measuredSize.Width);
			Assert.Equal(50, measuredSize.Height);
		}

		[Category(GridAbsoluteSizing)]
		[Category(GridStarSizing)]
		[Fact]
		public void MixStarsAndExplicitSizes()
		{
			var screenWidth = 300;
			var screenHeight = 600;
			var viewSize = new Size(50, 50);

			var grid = CreateGridLayout(rows: "auto", columns: $"3*,100,*");
			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);
			var view2 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, col: 2);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Row height is auto, so it gets the height of the view
			// Columns are 3*,100,* 
			// So we expect the center column to be 100, leaving 200 for the stars
			// 3/4 of that goes to the first column, so 150; the remaining 50 is the last column
			var expectedStarWidth = (screenWidth - 100) / 4;
			var expectedHeight = viewSize.Height;

			AssertArranged(view0, 0, 0, expectedStarWidth * 3, expectedHeight);
			AssertArranged(view1, expectedStarWidth * 3, 0, 100, expectedHeight);
			AssertArranged(view2, (expectedStarWidth * 3) + 100, 0, expectedStarWidth, expectedHeight);
		}

		[Fact]
		public void UsesImpliedRowAndColumnIfNothingDefined()
		{
			var grid = CreateGridLayout();
			var view0 = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view0);
			SetLocation(grid, view0);

			// Using 300,300 - the implied row/column are GridLength.Star
			MeasureAndArrangeFixed(grid, 300, 300);

			// Since it's using GridLength.Star, we expect the view to be arranged at the full size of the grid
			AssertArranged(view0, 0, 0, 300, 300);
		}

		[Fact]
		public void IgnoresCollapsedViews()
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var collapsedView = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			collapsedView.Visibility.Returns(Visibility.Collapsed);

			var grid = CreateGridLayout(children: new List<IView>() { view, collapsedView });

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(100, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(Point.Zero, measure));

			// View is visible, so we expect it to be measured and arranged
			view.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			view.Received().Arrange(Arg.Any<Rect>());

			// View is collapsed, so we expect it not to be measured or arranged
			collapsedView.DidNotReceive().Measure(Arg.Any<double>(), Arg.Any<double>());
			collapsedView.DidNotReceive().Arrange(Arg.Any<Rect>());
		}

		[Fact]
		public void DoesNotIgnoreHiddenViews()
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var hiddenView = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			hiddenView.Visibility.Returns(Visibility.Hidden);

			var grid = CreateGridLayout(children: new List<IView>() { view, hiddenView });

			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(100, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(Point.Zero, measure));

			// View is visible, so we expect it to be measured and arranged
			view.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			view.Received().Arrange(Arg.Any<Rect>());

			// View is hidden, so we expect it to be measured and arranged (since it'll need to take up space)
			hiddenView.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			hiddenView.Received().Arrange(Arg.Any<Rect>());
		}

		IGridLayout BuildPaddedGrid(Thickness padding, double viewWidth, double viewHeight)
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(viewWidth, viewHeight));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.Padding.Returns(padding);

			return grid;
		}

		[Theory]
		[InlineData(0, 0, 0, 0)]
		[InlineData(10, 10, 10, 10)]
		[InlineData(10, 0, 10, 0)]
		[InlineData(0, 10, 0, 10)]
		[InlineData(23, 5, 3, 15)]
		public void MeasureAccountsForPadding(double left, double top, double right, double bottom)
		{
			var viewWidth = 100d;
			var viewHeight = 100d;
			var padding = new Thickness(left, top, right, bottom);

			var expectedHeight = padding.VerticalThickness + viewHeight;
			var expectedWidth = padding.HorizontalThickness + viewWidth;

			var grid = BuildPaddedGrid(padding, viewWidth, viewHeight);

			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measuredSize.Height);
			Assert.Equal(expectedWidth, measuredSize.Width);
		}

		[Theory]
		[InlineData(0, 0, 0, 0)]
		[InlineData(10, 10, 10, 10)]
		[InlineData(10, 0, 10, 0)]
		[InlineData(0, 10, 0, 10)]
		[InlineData(23, 5, 3, 15)]
		public void ArrangeAccountsForPadding(double left, double top, double right, double bottom)
		{
			var viewWidth = 100d;
			var viewHeight = 100d;
			var padding = new Thickness(left, top, right, bottom);

			var grid = BuildPaddedGrid(padding, viewWidth, viewHeight);

			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(Point.Zero, measuredSize));

			AssertArranged(grid[0], padding.Left, padding.Top, viewWidth, viewHeight);
		}

		[Category(GridStarSizing)]
		[Fact]
		public void StarValuesAreAutoWhenConstraintsAreInfinite()
		{
			// A one-row, one-column grid
			var grid = CreateGridLayout();

			// A 100x100 IView
			var view = CreateTestView(new Size(100, 100));

			// Set up the grid to have a single child
			SubstituteChildren(grid, view);

			// Set up the row/column values and spans
			SetLocation(grid, view);

			var size = MeasureAndArrangeAuto(grid);

			Assert.Equal(100, size.Width);
			Assert.Equal(100, size.Height);
		}

		[Category(GridAbsoluteSizing)]
		[Fact]
		public void GridMeasureShouldUseExplicitHeight()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(10, 10));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.Height.Returns(50);

			var gridLayoutManager = new GridLayoutManager(grid);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(50, measure.Height);
		}

		[Category(GridAbsoluteSizing)]
		[Fact]
		public void GridMeasureShouldUseExplicitWidth()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(10, 10));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.Width.Returns(50);

			var gridLayoutManager = new GridLayoutManager(grid);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(50, measure.Width);
		}

		[Theory]
		// at 0, 0
		[InlineData(1, 1, 0, 0, 0, 0)]
		[InlineData(1, 2, 0, 0, 0, 0)]
		[InlineData(2, 1, 0, 0, 0, 0)]
		[InlineData(2, 2, 0, 0, 0, 0)]
		// at 1, 0
		[InlineData(1, 1, 1, 0, 0, 0)]
		[InlineData(1, 2, 1, 0, 0, 0)]
		[InlineData(2, 1, 1, 0, 1, 0)]
		[InlineData(2, 2, 1, 0, 1, 0)]
		// at 0, 1
		[InlineData(1, 1, 0, 1, 0, 0)]
		[InlineData(1, 2, 0, 1, 0, 1)]
		[InlineData(2, 1, 0, 1, 0, 0)]
		[InlineData(2, 2, 0, 1, 0, 1)]
		// at 1, 1
		[InlineData(1, 1, 1, 1, 0, 0)]
		[InlineData(1, 2, 1, 1, 0, 1)]
		[InlineData(2, 1, 1, 1, 1, 0)]
		[InlineData(2, 2, 1, 1, 1, 1)]
		public void ViewOutsideRowsAndColsClampsToGrid(int rows, int cols, int row, int col, int actualRow, int actualCol)
		{
			var r = string.Join(",", Enumerable.Repeat("100", rows));
			var c = string.Join(",", Enumerable.Repeat("100", cols));

			var grid = CreateGridLayout(rows: r, columns: c);
			var view0 = CreateTestView(new Size(10, 10));
			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, row, col);

			MeasureAndArrangeFixed(grid, 100 * cols, 100 * rows);

			AssertArranged(view0, 100 * actualCol, 100 * actualRow, 100, 100);
		}

		[Theory]
		// normal
		[InlineData(0, 0, 1, 1, 0, 0, 1, 1)]
		[InlineData(1, 1, 1, 1, 1, 1, 1, 1)]
		[InlineData(1, 1, 2, 1, 1, 1, 2, 1)]
		// negative origin
		[InlineData(-1, 0, 1, 1, 0, 0, 1, 1)]
		[InlineData(0, -1, 1, 1, 0, 0, 1, 1)]
		[InlineData(-1, -1, 1, 1, 0, 0, 1, 1)]
		// negative span
		[InlineData(1, 1, -1, 0, 1, 1, 1, 1)]
		[InlineData(1, 1, 0, -1, 1, 1, 1, 1)]
		[InlineData(1, 1, -1, -1, 1, 1, 1, 1)]
		// positive origin
		[InlineData(5, 0, 1, 1, 3, 0, 1, 1)]
		[InlineData(0, 5, 1, 1, 0, 3, 1, 1)]
		[InlineData(5, 5, 1, 1, 3, 3, 1, 1)]
		// positive span
		[InlineData(0, 0, 1, 5, 0, 0, 1, 4)]
		[InlineData(0, 0, 5, 1, 0, 0, 4, 1)]
		[InlineData(0, 0, 5, 5, 0, 0, 4, 4)]
		// normal origin + positive span
		[InlineData(1, 1, 1, 5, 1, 1, 1, 3)]
		[InlineData(1, 1, 5, 1, 1, 1, 3, 1)]
		[InlineData(1, 1, 5, 5, 1, 1, 3, 3)]
		// positive origin + positive span
		[InlineData(5, 5, 1, 5, 3, 3, 1, 1)]
		[InlineData(5, 5, 5, 1, 3, 3, 1, 1)]
		[InlineData(5, 5, 5, 5, 3, 3, 1, 1)]
		public void SpansOutsideRowsAndColsClampsToGrid(int row, int col, int rowSpan, int colSpan, int actualRow, int actualCol, int actualRowSpan, int actualColSpan)
		{
			const int GridSize = 4;
			var r = string.Join(",", Enumerable.Repeat("100", GridSize));
			var c = string.Join(",", Enumerable.Repeat("100", GridSize));

			var grid = CreateGridLayout(rows: r, columns: c);
			var view0 = CreateTestView(new Size(10, 10));
			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, row, col, rowSpan, colSpan);

			MeasureAndArrangeFixed(grid, 100 * GridSize, 100 * GridSize);

			AssertArranged(
				view0,
				100 * actualCol,
				100 * actualRow,
				100 * actualColSpan,
				100 * actualRowSpan);
		}

		[Fact]
		public void ArrangeRespectsBounds()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			var measure = MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity, 10, 15);

			var expectedRectangle = new Rect(10, 15, measure.Width, measure.Height);

			view.Received().Arrange(Arg.Is(expectedRectangle));
		}

		[Category(GridAbsoluteSizing)]
		[Theory]
		[InlineData(50, 100, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(100, 50, 50)]
		[InlineData(0, 50, 0)]
		[InlineData(-1, 50, 50)]
		public void MeasureRespectsMaxHeight(double maxHeight, double viewHeight, double expectedHeight)
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, viewHeight));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.MaximumHeight.Returns(maxHeight);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measure.Height);
		}

		[Category(GridAbsoluteSizing)]
		[Theory]
		[InlineData(50, 100, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(100, 50, 50)]
		[InlineData(0, 50, 0)]
		[InlineData(-1, 50, 50)]
		public void MeasureRespectsMaxWidth(double maxWidth, double viewWidth, double expectedWidth)
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(viewWidth, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.MaximumWidth.Returns(maxWidth);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedWidth, measure.Width);
		}

		[Category(GridAbsoluteSizing)]
		[Theory]
		[InlineData(50, 10, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(10, 50, 50)]
		[InlineData(-1, 50, 50)]
		public void MeasureRespectsMinHeight(double minHeight, double viewHeight, double expectedHeight)
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, viewHeight));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.MinimumHeight.Returns(minHeight);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measure.Height);
		}

		[Theory]
		[InlineData("*", "*")]
		[InlineData("auto", "auto")]
		public void MeasureRespectsLargestChildMinimumSize(string columns, string rows)
		{
			var grid = CreateGridLayout(columns: columns, rows: rows);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(200, 200));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(200, measure.Height);
			Assert.Equal(200, measure.Width);
		}

		[Category(GridAbsoluteSizing)]
		[Theory]
		[InlineData(50, 10, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(10, 50, 50)]
		[InlineData(-1, 50, 50)]
		public void MeasureRespectsMinWidth(double minWidth, double viewWidth, double expectedWidth)
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(viewWidth, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.MinimumWidth.Returns(minWidth);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedWidth, measure.Width);
		}

		[Fact]
		[Category(GridAbsoluteSizing)]
		public void MaxWidthDominatesWidth()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.Width.Returns(75);
			grid.MaximumWidth.Returns(50);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The maximum value beats out the explicit value
			Assert.Equal(50, measure.Width);
		}

		[Fact]
		[Category(GridAbsoluteSizing)]
		public void MinWidthDominatesMaxWidth()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.MinimumWidth.Returns(75);
			grid.MaximumWidth.Returns(50);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The minimum value should beat out the maximum value
			Assert.Equal(75, measure.Width);
		}

		[Fact]
		[Category(GridAbsoluteSizing)]
		public void MaxHeightDominatesHeight()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.Height.Returns(75);
			grid.MaximumHeight.Returns(50);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The maximum value beats out the explicit value
			Assert.Equal(50, measure.Height);
		}

		[Fact]
		[Category(GridAbsoluteSizing)]
		public void MinHeightDominatesMaxHeight()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.MinimumHeight.Returns(75);
			grid.MaximumHeight.Returns(50);

			var layoutManager = new GridLayoutManager(grid);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The minimum value should beat out the maximum value
			Assert.Equal(75, measure.Height);
		}

		[Theory]
		[InlineData(100, 200, 210, 200)]
		[InlineData(200, 100, 210, 200)]
		[InlineData(100, 100, 210, 100)]
		[InlineData(100, 100, 100, 100)]
		public void AutoCellsSizeToLargestView(double view0Size, double view1Size, double constraintSize, double expectedSize)
		{
			var grid = CreateGridLayout(rows: "Auto", columns: "Auto");

			// Simulate views which size to their constraints but max out at a certain size

			var view0 = CreateTestView();
			view0.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(
				(args) => new Size((double)args[0] >= view0Size ? view0Size : (double)args[0],
									(double)args[1] >= view0Size ? view0Size : (double)args[1]));

			var view1 = CreateTestView();
			view1.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(
				(args) => new Size((double)args[0] >= view1Size ? view1Size : (double)args[0],
									(double)args[1] >= view1Size ? view1Size : (double)args[1]));

			SubstituteChildren(grid, view0, view1);

			// Put both views in row/column 0/0
			SetLocation(grid, view0);
			SetLocation(grid, view1);

			MeasureAndArrangeFixed(grid, constraintSize, constraintSize);

			var expectedRectangle = new Rect(0, 0, expectedSize, expectedSize);

			// We expect the Auto row/col to take on the size of the largest of the two views
			AssertArranged(view0, expectedRectangle);
			AssertArranged(view1, expectedRectangle);
		}

		[Fact]
		public void ArrangeAccountsForFill()
		{
			var grid = CreateGridLayout();
			var view = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view);
			SetLocation(grid, view);

			grid.HorizontalLayoutAlignment.Returns(Primitives.LayoutAlignment.Fill);
			grid.VerticalLayoutAlignment.Returns(Primitives.LayoutAlignment.Fill);

			var layoutManager = new GridLayoutManager(grid);
			_ = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var arrangedWidth = 1000;
			var arrangedHeight = 1000;

			var target = new Rect(Point.Zero, new Size(arrangedWidth, arrangedHeight));

			var actual = layoutManager.ArrangeChildren(target);

			// Since we're arranging in a space larger than needed and the layout is set to Fill in both directions,
			// we expect the returned actual arrangement size to be as large as the target space
			Assert.Equal(arrangedWidth, actual.Width);
			Assert.Equal(arrangedHeight, actual.Height);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "We can specify fractional star sizes for columns")]
		public void FractionalStarColumns()
		{
			var screenWidth = 300;
			var screenHeight = 600;
			var viewSize = new Size(50, 50);

			var grid = CreateGridLayout(rows: "auto", columns: $"*,0.5*,0.5*");
			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);
			var view2 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, col: 2);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Row height is auto, so it gets the height of the view
			var expectedHeight = viewSize.Height;

			// Columns are *,0.5*,0.5*, so the first column should be half the space
			// and the other two columns should be a quarter of the space
			var expectedWidthColumn0 = screenWidth / 2;
			var expectedWidthOthers = screenWidth / 4;

			// Make sure that the views in the columns are actually getting measured at the column width,
			// and not just at the width of the whole grid
			view0.Received().Measure(Arg.Is<double>(expectedWidthColumn0), Arg.Any<double>());
			view1.Received().Measure(Arg.Is<double>(expectedWidthOthers), Arg.Any<double>());
			view2.Received().Measure(Arg.Is<double>(expectedWidthOthers), Arg.Any<double>());

			AssertArranged(view0, 0, 0, expectedWidthColumn0, expectedHeight);
			AssertArranged(view1, expectedWidthColumn0, 0, expectedWidthOthers, expectedHeight);
			AssertArranged(view2, expectedWidthColumn0 + expectedWidthOthers, 0, expectedWidthOthers, expectedHeight);
		}

		[Category(GridStarSizing)]
		[Fact(DisplayName = "We can specify fractional star sizes for rows")]
		public void FractionalStarRows()
		{
			var screenWidth = 300;
			var screenHeight = 600;
			var viewSize = new Size(50, 50);

			var grid = CreateGridLayout(rows: "*,0.5*,0.5*", columns: "auto");
			var view0 = CreateTestView(viewSize);
			var view1 = CreateTestView(viewSize);
			var view2 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);
			SetLocation(grid, view2, row: 2);

			MeasureAndArrangeFixed(grid, screenWidth, screenHeight);

			// Column width is auto, so it gets the width of the view
			var expectedWidth = viewSize.Width;

			// Rows are *,0.5*,0.5*, so row 0 should be half the screen height
			// And the other rows should be one quarter the screen height
			var expectedHeightRow0 = screenHeight / 2;
			var expectedHeightOther = screenHeight / 4;

			// Make sure that the views in the columns are actually getting measured at the column width,
			// and not just at the width of the whole grid
			view0.Received().Measure(Arg.Any<double>(), Arg.Is<double>(expectedHeightRow0));
			view1.Received().Measure(Arg.Any<double>(), Arg.Is<double>(expectedHeightOther));
			view2.Received().Measure(Arg.Any<double>(), Arg.Is<double>(expectedHeightOther));

			AssertArranged(view0, 0, 0, expectedWidth, expectedHeightRow0);
			AssertArranged(view1, 0, expectedHeightRow0, expectedWidth, expectedHeightOther);
			AssertArranged(view2, 0, expectedHeightRow0 + expectedHeightOther, expectedWidth, expectedHeightOther);
		}

		[Category(GridSpacing, GridStarSizing)]
		[Fact("Star columns don't appropriate column spacing during measurement")]
		public void StarColumnMeasureDoesNotIncludeSpacing()
		{
			var colSpacing = 10;

			var grid = CreateGridLayout(columns: "100, *, 100", colSpacing: colSpacing);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 2);

			var manager = new GridLayoutManager(grid);
			var widthConstraint = 100 + colSpacing + 100 + colSpacing + 100;
			var measure = manager.Measure(widthConstraint, double.PositiveInfinity);

			var expectedWidthMeasure = 100 + colSpacing + 0 + colSpacing + 100;

			Assert.Equal(expectedWidthMeasure, measure.Width);

			manager.ArrangeChildren(new Rect(0, 0, widthConstraint, measure.Height));
			AssertArranged(view0, new Rect(0, 0, 100, 100));
			AssertArranged(view1, new Rect(220, 0, 100, 100));
		}

		[Category(GridSpacing, GridStarSizing)]
		[Fact("Star rows don't appropriate row spacing during measurement")]
		public void StarRowMeasureDoesNotIncludeSpacing()
		{
			var rowSpacing = 10;

			var grid = CreateGridLayout(rows: "100, *, 100", rowSpacing: rowSpacing);
			var view0 = CreateTestView(new Size(100, 100));
			var view1 = CreateTestView(new Size(100, 100));
			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 2);

			var manager = new GridLayoutManager(grid);
			var heightConstraint = 100 + rowSpacing + 100 + rowSpacing + 100;
			var measure = manager.Measure(double.PositiveInfinity, heightConstraint);

			var expectedHeightMeasure = 100 + rowSpacing + 0 + rowSpacing + 100;

			Assert.Equal(expectedHeightMeasure, measure.Height);

			manager.ArrangeChildren(new Rect(0, 0, measure.Width, heightConstraint));
			AssertArranged(view0, new Rect(0, 0, 100, 100));
			AssertArranged(view1, new Rect(0, 220, 100, 100));
		}

		[Fact]
		[Category(GridStarSizing), Category(GridAutoSizing)]
		public void AutoStarColumnSpansDoNotAffectAutoColumnSize()
		{
			var grid = CreateGridLayout(rows: "Auto, *", columns: "Auto, *");

			var view0 = CreateTestView(new Size(10, 10));

			var view1 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, col: 1);
			SetLocation(grid, view1, row: 1, colSpan: 2);

			MeasureAndArrangeFixed(grid, 200, 200);

			// We expect that column 0 has no width, so view0 will be arranged at 0,0
			AssertArranged(view0, 0, 0, 200, 10);

			// Since column 0 has no width, we expect view1 to start at 0,10 
			AssertArranged(view1, 0, 10, 200, 190);
		}

		[Fact]
		[Category(GridAbsoluteSizing)]
		public void AbsoluteRowsConstrainMeasureHeight()
		{
			var grid = CreateGridLayout(rows: "50");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0);

			// Assuming no constraints on space
			MeasureAndArrangeAuto(grid);

			// Verify that the view is getting measured at the appropriate height (50)
			view0.Received().Measure(Arg.Any<double>(), Arg.Is<double>(50));

			// And only at that height
			view0.DidNotReceive().Measure(Arg.Any<double>(), Arg.Is<double>((h) => h != 50));
		}

		[Fact]
		[Category(GridStarSizing), Category(GridAutoSizing)]
		public void AutoStarRowSpansDoNotAffectAutoRowSize()
		{
			var grid = CreateGridLayout(rows: "Auto, *", columns: "Auto, *");

			var view0 = CreateTestView(new Size(10, 10));

			var view1 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, row: 1);
			SetLocation(grid, view1, col: 1, rowSpan: 2);

			MeasureAndArrangeFixed(grid, 200, 200);

			// We expect that row 0 has no height, so view0 will be arranged at 0,0
			AssertArranged(view0, 0, 0, 10, 200);

			// Since row 0 has no height, we expect view1 to start at 10,0 
			AssertArranged(view1, 10, 0, 190, 200);
		}

		[Fact]
		[Category(GridAbsoluteSizing)]
		public void AbsoluteColumnsConstrainMeasureWidth()
		{
			var grid = CreateGridLayout(columns: "50");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0);

			// Assuming no constraints on space
			MeasureAndArrangeAuto(grid);

			// Verify that the view is getting measured at the appropriate width (50)
			view0.Received().Measure(Arg.Is<double>(50), Arg.Any<double>());

			// And only at that width
			view0.DidNotReceive().Measure(Arg.Is<double>((h) => h != 50), Arg.Any<double>());
		}

		[Fact("Children of Auto rows should be measured using an infinite height")]
		[Category(GridAutoSizing)]
		public void AutoRowsMeasureChildrenAtInfinity()
		{
			var grid = CreateGridLayout(rows: "Auto");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0);

			MeasureAndArrangeFixed(grid, 500, 500);

			// Verify that the view is getting measured at the appropriate height (infinity)
			view0.Received().Measure(Arg.Is<double>(500), Arg.Is(double.PositiveInfinity));
		}

		[Fact("Children of Auto columns should be measured using an infinite width")]
		[Category(GridAutoSizing)]
		public void AutoColumnsMeasureChildrenAtInfinity()
		{
			var grid = CreateGridLayout(columns: "Auto");

			var viewSize = new Size(10, 10);

			var view0 = CreateTestView(viewSize);

			SubstituteChildren(grid, view0);

			MeasureAndArrangeFixed(grid, 500, 500);

			// Verify that the view is getting measured at the appropriate width (infinity)
			view0.Received().Measure(Arg.Is(double.PositiveInfinity), Arg.Is<double>(500));
		}

		[Fact("Star Row Height is correct when the first child is Collapsed")]
		[Category(GridStarSizing)]
		public void StarRowHeightCorrectWhenFirstChildCollapsed()
		{
			var grid = CreateGridLayout(rows: "*");

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(10, 10));

			// Since this is collapsed, it should not count toward the star row height
			view0.Visibility.Returns(Visibility.Collapsed);

			SubstituteChildren(grid, view0, view1);

			var measuredSize = MeasureAndArrangeAuto(grid);

			Assert.Equal(10, measuredSize.Height);
		}

		[Fact("Star Column Width is correct when the first child is Collapsed")]
		[Category(GridStarSizing)]
		public void StarColumnWidthCorrectWhenFirstChildCollapsed()
		{
			var grid = CreateGridLayout(columns: "*");

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(10, 10));

			// Since this is collapsed, it should not count toward the star column width
			view0.Visibility.Returns(Visibility.Collapsed);

			SubstituteChildren(grid, view0, view1);

			var measuredSize = MeasureAndArrangeAuto(grid);

			Assert.Equal(10, measuredSize.Width);
		}

		[Fact("ArrangeChildren should arrange within measured size")]
		[Category(GridStarSizing)]
		public void ArrangeChildrenShouldArrangeWithinMeasuredSize()
		{
			var grid = CreateGridLayout(rows: "*");
			grid.Width.Returns(105);
			grid.Height.Returns(120);

			var view0 = CreateTestView(new Size(20, 20));
			SubstituteChildren(grid, view0);

			var measuredSize = MeasureAndArrange(grid, 300, double.PositiveInfinity);

			// we expect that the child will be arranged within measured size
			// TODO this test might be improperly named, and also the measuredSize.Width should probably be 300

			AssertArranged(view0, 0, 0, measuredSize.Width, measuredSize.Height);
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(LayoutAlignment.Center, true)]
		[InlineData(LayoutAlignment.Center, false)]
		[InlineData(LayoutAlignment.Start, true)]
		[InlineData(LayoutAlignment.Start, false)]
		[InlineData(LayoutAlignment.End, true)]
		[InlineData(LayoutAlignment.End, false)]
		[InlineData(LayoutAlignment.Fill, true)]
		[InlineData(LayoutAlignment.Fill, false)]
		public void GridMeasuresStarColumnToChildWidth(LayoutAlignment alignment, bool impliedColumn)
		{
			string layoutData = impliedColumn ? null : "*";

			var grid = CreateGridLayout(columns: layoutData);

			grid.HorizontalLayoutAlignment.Returns(alignment);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);

			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(100, 100);

			Assert.Equal(20, measuredSize.Width);
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(true, 100)]
		[InlineData(false, 100)]
		[InlineData(true, 15)]
		[InlineData(false, 15)]
		public void FillGridArrangesStarColumnToWidthConstraint(bool implied, double constraint)
		{
			string layoutData = implied ? null : "*";

			var grid = CreateGridLayout(columns: layoutData);

			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);

			MeasureAndArrangeFixed(grid, constraint, 100);

			AssertArranged(view0, new Rect(0, 0, constraint, 100));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(LayoutAlignment.Center, true)]
		[InlineData(LayoutAlignment.Center, false)]
		[InlineData(LayoutAlignment.Start, true)]
		[InlineData(LayoutAlignment.Start, false)]
		[InlineData(LayoutAlignment.End, true)]
		[InlineData(LayoutAlignment.End, false)]
		public void NonFillGridArrangesStarColumnToChildWidth(LayoutAlignment alignment, bool implied)
		{
			string layoutData = implied ? null : "*";

			var grid = CreateGridLayout(columns: layoutData);

			grid.HorizontalLayoutAlignment.Returns(alignment);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);

			var measure = MeasureAndArrangeFixed(grid, 100, 100);

			Assert.Equal(20, measure.Height);
			Assert.Equal(20, measure.Width);

			AssertArranged(view0, new Rect(0, 0, 20, 100));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(LayoutAlignment.Center, true)]
		[InlineData(LayoutAlignment.Center, false)]
		[InlineData(LayoutAlignment.Start, true)]
		[InlineData(LayoutAlignment.Start, false)]
		[InlineData(LayoutAlignment.End, true)]
		[InlineData(LayoutAlignment.End, false)]
		[InlineData(LayoutAlignment.Fill, true)]
		[InlineData(LayoutAlignment.Fill, false)]
		public void GridMeasuresStarRowToChildHeight(LayoutAlignment alignment, bool implied)
		{
			string layoutData = implied ? null : "*";

			var grid = CreateGridLayout(rows: layoutData);

			grid.VerticalLayoutAlignment.Returns(alignment);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);

			var manager = new GridLayoutManager(grid);
			var measuredSize = manager.Measure(100, 100);

			Assert.Equal(20, measuredSize.Height);
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(true, 100)]
		[InlineData(false, 100)]
		[InlineData(true, 15)]
		[InlineData(false, 15)]
		public void FillGridArrangesStarRowToHeightConstraint(bool implied, double constraint)
		{
			string layoutData = implied ? null : "*";

			var grid = CreateGridLayout(rows: layoutData);

			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);

			MeasureAndArrangeFixed(grid, 100, constraint);

			AssertArranged(view0, new Rect(0, 0, 100, constraint));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(LayoutAlignment.Center, true)]
		[InlineData(LayoutAlignment.Center, false)]
		[InlineData(LayoutAlignment.Start, true)]
		[InlineData(LayoutAlignment.Start, false)]
		[InlineData(LayoutAlignment.End, true)]
		[InlineData(LayoutAlignment.End, false)]
		public void NonFillGridArrangesStarRowToChildHeight(LayoutAlignment alignment, bool impliedRow)
		{
			string layoutData = impliedRow ? null : "*";

			var grid = CreateGridLayout(rows: layoutData);
			grid.VerticalLayoutAlignment.Returns(alignment);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);

			var measure = MeasureAndArrangeFixed(grid, 100, 100);

			Assert.Equal(20, measure.Height);
			Assert.Equal(20, measure.Width);

			AssertArranged(view0, new Rect(0, 0, 100, 20));
		}

		[Fact, Category(GridStarSizing)]
		public void StarRowsResizeWhenGridExpandsToFill()
		{
			var grid = CreateGridLayout(rows: "*");
			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));
			SubstituteChildren(grid, view0);

			var manager = new GridLayoutManager(grid);

			// Measuring at infinite height, we expect the Grid's only row (*) to act like an
			// Auto row and get the height of the view
			var measuredSize = manager.Measure(20, double.PositiveInfinity);
			Assert.Equal(20, measuredSize.Height);

			grid.DesiredSize.Returns(measuredSize);

			// We arrange at a height taller than the Grid's measurement; because the Grid
			// is set to vertically Fill, we expect it to expand to the arranged height
			manager.ArrangeChildren(new Rect(0, 0, 20, 100));

			// And we expect the * row to fill up that new height
			AssertArranged(view0, new Rect(0, 0, 20, 100));
		}

		[Fact, Category(GridStarSizing)]
		public void StarColumnsResizeWhenGridExpandsToFill()
		{
			var grid = CreateGridLayout(columns: "*");
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));
			SubstituteChildren(grid, view0);

			var manager = new GridLayoutManager(grid);

			// Measuring at infinite width, we expect the Grid's only column (*) to act like an
			// Auto column and get the width of the view
			var measuredSize = manager.Measure(double.PositiveInfinity, 20);
			Assert.Equal(20, measuredSize.Width);

			grid.DesiredSize.Returns(measuredSize);

			// We arrange at a width wider than the Grid's measurement; because the Grid
			// is set to horizontally Fill, we expect it to expand to the arranged width
			manager.ArrangeChildren(new Rect(0, 0, 100, 20));

			// And we expect the * column to fill up that new width
			AssertArranged(view0, new Rect(0, 0, 100, 20));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(LayoutAlignment.Center)]
		[InlineData(LayoutAlignment.Start)]
		[InlineData(LayoutAlignment.End)]
		[InlineData(LayoutAlignment.Fill)]
		public void StarRowsShouldFitExplicitDimensions(LayoutAlignment verticalAlignment)
		{
			var grid = CreateGridLayout(rows: "*");
			grid.VerticalLayoutAlignment.Returns(verticalAlignment);
			grid.Height.Returns(100);

			var view0 = CreateTestView(new Size(20, 20));
			SubstituteChildren(grid, view0);

			var manager = new GridLayoutManager(grid);
			var gridMeasure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// Because the Grid has an explicit height, we expect the measurement to have that height
			Assert.Equal(100, gridMeasure.Height);

			manager.ArrangeChildren(new Rect(Point.Zero, gridMeasure));

			// Because the child has VerticalAlignment.Fill, we expect it to fill up the 100
			// units in the Star row
			AssertArranged(view0, new Rect(0, 0, 20, 100));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(LayoutAlignment.Center)]
		[InlineData(LayoutAlignment.Start)]
		[InlineData(LayoutAlignment.End)]
		[InlineData(LayoutAlignment.Fill)]
		public void StarColumnsShouldFitExplicitDimensions(LayoutAlignment horizontalAlignment)
		{
			var grid = CreateGridLayout(columns: "*");
			grid.HorizontalLayoutAlignment.Returns(horizontalAlignment);
			grid.Width.Returns(100);

			var view0 = CreateTestView(new Size(20, 20));
			SubstituteChildren(grid, view0);

			var manager = new GridLayoutManager(grid);
			var gridMeasure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// Because the Grid has an explicit width, we expect the measurement to have that width.
			Assert.Equal(100, gridMeasure.Width);

			manager.ArrangeChildren(new Rect(Point.Zero, gridMeasure));

			// Because the child has HorizontalAlignment.Fill, we expect it to fill up the 100
			// units in the Star column
			AssertArranged(view0, new Rect(0, 0, 100, 20));
		}

		[Fact]
		[Category(GridStarSizing)]
		public void StarColumnsHaveChildWidthsWhenGridCentered()
		{
			var grid = CreateGridLayout(columns: "*,*");
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Center);

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(10, 10));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view1, col: 1);

			_ = MeasureAndArrange(grid, 200, 200);

			AssertArranged(view0, new Rect(0, 0, 20, 20));
			AssertArranged(view1, new Rect(20, 0, 10, 20));
		}

		[Fact]
		[Category(GridStarSizing, GridSpan)]
		public void MeasureStarAndExplicitColumnSpan()
		{
			var grid = CreateGridLayout(columns: "40,*");
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Center);

			var view0 = CreateTestView(new Size(100, 20));
			view0.Width.Returns(100);

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, col: 0, colSpan: 2);

			var measure = MeasureAndArrange(grid, 200, 200);

			Assert.Equal(100, measure.Width);

			AssertArranged(view0, new Rect(0, 0, 100, 20));
		}

		[Fact]
		[Category(GridStarSizing, GridSpan)]
		public void MeasureAutoAndExplicitColumnSpan()
		{
			var grid = CreateGridLayout(columns: "40,auto");
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Center);

			var view0 = CreateTestView(new Size(100, 20));
			view0.Width.Returns(100);

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, col: 0, colSpan: 2);

			var measure = MeasureAndArrange(grid, 200, 200);

			Assert.Equal(100, measure.Width);

			AssertArranged(view0, new Rect(0, 0, 100, 20));
		}

		[Fact]
		[Category(GridStarSizing, GridSpan)]
		public void MeasureStarAndExplicitRowSpan()
		{
			var grid = CreateGridLayout(rows: "40,*");
			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Center);

			var view0 = CreateTestView(new Size(20, 100));
			view0.Height.Returns(100);

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, row: 0, rowSpan: 2);

			var measure = MeasureAndArrange(grid, 200, 200);

			Assert.Equal(100, measure.Height);

			AssertArranged(view0, new Rect(0, 0, 20, 100));
		}

		[Fact]
		[Category(GridStarSizing)]
		public void ChildInStarRowWithInfiniteSpaceIsMeasuredWithInfinity()
		{
			var grid = CreateGridLayout(rows: "*");
			var view0 = CreateTestView(new Size(20, 100));
			SubstituteChildren(grid, view0);

			_ = MeasureAndArrange(grid, 200, double.PositiveInfinity);

			view0.Received().Measure(Arg.Any<double>(), Arg.Is(double.PositiveInfinity));
		}

		[Fact]
		[Category(GridStarSizing)]
		public void ChildInStarColumnWithInfiniteSpaceIsMeasuredWithInfinity()
		{
			var grid = CreateGridLayout(columns: "*");
			var view0 = CreateTestView(new Size(100, 20));
			SubstituteChildren(grid, view0);

			_ = MeasureAndArrange(grid, double.PositiveInfinity, 200);

			view0.Received().Measure(Arg.Is(double.PositiveInfinity), Arg.Any<double>());
		}

		[Fact]
		[Category(GridStarSizing)]
		public void StarColumnWidthLimitedToGridWidth()
		{
			var grid = CreateGridLayout(columns: "*", rows: "Auto, Auto");

			var screenWidth = 500;

			var view0 = CreateTestView(new Size(600, 20));
			var view1 = CreateTestView(new Size(100, 20));

			SetLocation(grid, view0);
			SetLocation(grid, view1, row: 1);

			SubstituteChildren(grid, view0, view1);

			_ = MeasureAndArrange(grid, screenWidth, 200);

			AssertArranged(view1, new Rect(0, 20, 500, 20));
		}

		[Fact]
		[Category(GridStarSizing)]
		public void StarRowHeightLimitedToGridHeight()
		{
			var grid = CreateGridLayout(rows: "*", columns: "Auto, Auto");

			var screenHeight = 500;

			var view0 = CreateTestView(new Size(20, 600));
			var view1 = CreateTestView(new Size(20, 100));

			SetLocation(grid, view0);
			SetLocation(grid, view1, col: 1);

			SubstituteChildren(grid, view0, view1);

			_ = MeasureAndArrange(grid, 200, screenHeight);

			AssertArranged(view1, new Rect(20, 0, 20, 500));
		}

		[Fact]
		[Category(GridStarSizing)]
		public void StarsExpandToFixedSizes()
		{
			var grid = CreateGridLayout();
			grid.DesiredSize.Returns(new Size(100, 120));
			grid.Width.Returns(100);
			grid.Height.Returns(120);

			var view0 = CreateTestView(new Size(20, 20));
			view0.Width.Returns(20);
			view0.Height.Returns(20);
			view0.HorizontalLayoutAlignment.Returns(LayoutAlignment.End);
			view0.VerticalLayoutAlignment.Returns(LayoutAlignment.Start);

			SetLocation(grid, view0);
			SubstituteChildren(grid, view0);

			_ = MeasureAndArrange(grid);

			AssertArranged(view0, new Rect(0, 0, 100, 120));
		}

		[Fact]
		public void AutoStarColumnsRespectUnconstrainedHeight()
		{
			var grid = CreateGridLayout(columns: "Auto, *");

			var view0 = CreateTestView(new Size(20, 20));
			view0.HorizontalLayoutAlignment.Returns(LayoutAlignment.Start);
			view0.VerticalLayoutAlignment.Returns(LayoutAlignment.Start);

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, col: 1);

			_ = MeasureAndArrange(grid, widthConstraint: 200, heightConstraint: double.PositiveInfinity);

			// The Grid only has one view; since we're measuring the Grid without height constraints,
			// and the single view does not have an explicit height, then there should have been at least
			// one measurement with an unconstrained height
			view0.Received().Measure(Arg.Any<double>(), double.PositiveInfinity);

			// The Auto column has no Views, so we expect it to have zero width; the single view should
			// be arranged at the top left corner
			AssertArranged(view0, new Rect(0, 0, 20, 20));
		}

		[Fact]
		public void AutoStarRowsRespectUnconstrainedWidth()
		{
			var grid = CreateGridLayout(rows: "Auto, *");

			var view0 = CreateTestView(new Size(20, 20));
			view0.HorizontalLayoutAlignment.Returns(LayoutAlignment.Start);
			view0.VerticalLayoutAlignment.Returns(LayoutAlignment.Start);

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, row: 1);

			_ = MeasureAndArrange(grid, widthConstraint: double.PositiveInfinity, heightConstraint: 200);

			// The Grid only has one view; since we're measuring the Grid without width constraints,
			// and the single view does not have an explicit width, then there should have been at least
			// one measurement with an unconstrained width
			view0.Received().Measure(double.PositiveInfinity, Arg.Any<double>());

			// The Auto row has no Views, so we expect it to have zero height; the single view should
			// be arranged at the top left corner
			AssertArranged(view0, new Rect(0, 0, 20, 20));
		}

		[Fact, Category(GridStarSizing)]
		public void UnconstrainedStarRowsRetainTheirHeightsWhenArrangedAtMeasuredSize()
		{
			// This test accounts for the situation where a Grid has Rows marked as "*", is measured
			// without height constraint, is vertically set to Fill, and is arranged at the measured height.
			// Basically, a situation where the Grid is inside a vertically-oriented StackLayout, 
			// and the concept of vertical "Fill" doesn't mean anything. In that situation, the rows should 
			// retain their automatic sizing, rather than being evenly distributed as usual.

			var grid = CreateGridLayout(rows: "*, *, *");
			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(20, 40));
			var view2 = CreateTestView(new Size(20, 60));

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, row: 0);
			SetLocation(grid, view1, row: 1);
			SetLocation(grid, view2, row: 2);

			// Measure the Grid with no height constraint, then arrange it using the resulting size
			// Unconstrained, we expect the views to total 20 + 40 + 60 = 120 height
			// Since we're arranging it at that same height, there's no reason for it to expand the items
			// so we expect them to be arranged at the same heights
			var measure = MeasureAndArrange(grid, widthConstraint: 200, heightConstraint: double.PositiveInfinity);

			Assert.Equal(120, measure.Height);

			AssertArranged(view0, new Rect(0, 0, 20, 20));
			AssertArranged(view1, new Rect(0, 20, 20, 40));
			AssertArranged(view2, new Rect(0, 60, 20, 60));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(0.1)]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(60)]
		[InlineData(1000)]
		[InlineData(-0.1)]
		[InlineData(-10)]
		[InlineData(-60)]
		public void ViewsInUnconstrainedStarRowsDoNotOverlapWhenArrangeHeightChanges(double heightDelta)
		{
			// Basically checking for https://github.com/dotnet/maui/issues/14694 here

			// This test accounts for the situation where a Grid has Rows marked as "*", is measured
			// without height constraint, is vertically set to Fill, and is arranged at a different height
			// than the measured height. This can happen for a couple of reasons - either because of 
			// adjustments made on the native side to handle rounding/conversion issues (e.g., Android
			// density conversions), or because of ScrollView's "Fill the viewport" behavior.

			var grid = CreateGridLayout(rows: "*, *, *");
			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(20, 40));
			var view2 = CreateTestView(new Size(20, 60));

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, row: 0);
			SetLocation(grid, view1, row: 1);
			SetLocation(grid, view2, row: 2);

			// Measure the Grid with no height constraint
			// Unconstrained, we expect the views to total 20 + 40 + 60 = 120 height
			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(200, double.PositiveInfinity);
			Assert.Equal(120, measure.Height);

			// Now arrange it at a _different_ height
			manager.ArrangeChildren(new Rect(0, 0, measure.Width, measure.Height + heightDelta));

			// Determine the destination Rect values that the manager passed in when calling Arrange() for each view
			var v0ArrangeArgs = view0.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view0Dest = (Rect)v0ArrangeArgs[0];

			var v1ArrangeArgs = view1.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view1Dest = (Rect)v1ArrangeArgs[0];

			var v2ArrangeArgs = view2.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view2Dest = (Rect)v2ArrangeArgs[0];

			// Ensure that the destination rect for each view is large enough
			// for that view (that the grid isn't somehow shrinking their destination area)
			Assert.True(view0Dest.Height >= 20);
			Assert.True(view1Dest.Height >= 40);
			Assert.True(view2Dest.Height >= 60);

			// Ensure that the destination rects for the views don't overlap
			Assert.False(view0Dest.IntersectsWith(view1Dest));
			Assert.False(view1Dest.IntersectsWith(view2Dest));
			Assert.False(view0Dest.IntersectsWith(view2Dest));

			// And ensure that the destination rects are actually tall enough to fill up the arranged height.
			// They might be taller (e.g., if a window is resized to be too small for the content), but they should
			// _not_ be _less_ than the target arrangement height.
			var destinationHeight = view0Dest.Height + view1Dest.Height + view2Dest.Height;
			Assert.True(destinationHeight >= measure.Height + heightDelta);
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(0.1)]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(60)]
		[InlineData(1000)]
		[InlineData(-0.1)]
		[InlineData(-10)]
		[InlineData(-60)]
		public void ViewsInUnconstrainedStarColumnsDoNotOverlapWhenArrangeWidthChanges(double widthDelta)
		{
			// Basically checking for https://github.com/dotnet/maui/issues/14694 here

			// This test accounts for the situation where a Grid has Columns marked as "*", is measured
			// without width constraint, is horizontally set to Fill, and is arranged at a different width
			// than the measured width. This can happen for a couple of reasons - either because of 
			// adjustments made on the native side to handle rounding/conversion issues (e.g., Android
			// density conversions), or because of ScrollView's "Fill the viewport" behavior.

			var grid = CreateGridLayout(columns: "*, *, *");
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(40, 20));
			var view2 = CreateTestView(new Size(60, 20));

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, col: 0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, col: 2);

			// Measure the Grid with no width constraint
			// Unconstrained, we expect the views to total 20 + 40 + 60 = 120 width
			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, 200);
			Assert.Equal(120, measure.Width);

			// Now arrange it at a _different_ width
			manager.ArrangeChildren(new Rect(0, 0, measure.Width + widthDelta, measure.Height));

			// Determine the destination Rect values that the manager passed in when calling Arrange() for each view
			var v0ArrangeArgs = view0.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view0Dest = (Rect)v0ArrangeArgs[0];

			var v1ArrangeArgs = view1.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view1Dest = (Rect)v1ArrangeArgs[0];

			var v2ArrangeArgs = view2.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view2Dest = (Rect)v2ArrangeArgs[0];

			// Ensure that the destination rect for each view is large enough
			// for that view (that the grid isn't somehow shrinking their destination area)
			Assert.True(view0Dest.Width >= 20);
			Assert.True(view1Dest.Width >= 40);
			Assert.True(view2Dest.Width >= 60);

			// Ensure that the destination rects for the views don't overlap
			Assert.False(view0Dest.IntersectsWith(view1Dest));
			Assert.False(view1Dest.IntersectsWith(view2Dest));
			Assert.False(view0Dest.IntersectsWith(view2Dest));
		}

		[Fact, Category(GridStarSizing)]
		public void UnconstrainedStarColumnsRetainTheirWidthsWhenArrangedAtMeasuredSize()
		{
			// This test accounts for the situation where a Grid has Columns marked as "*", is measured
			// without width constraint, is horizontally set to Fill, and is arranged at the measured width.
			// Basically, a situation where the Grid is inside a horizontally-oriented ScrollView or StackLayout, 
			// and the concept of horizontal "Fill" doesn't mean anything. In that situation, the columns should 
			// retain their automatic sizing, rather than being evenly distributed as usual.

			var grid = CreateGridLayout(columns: "*, *, *");
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(40, 20));
			var view2 = CreateTestView(new Size(60, 20));

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, col: 0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, col: 2);

			// Measure the Grid with no width constraint, then arrange it using the resulting size
			// Unconstrained, we expect the views to total 20 + 40 + 60 = 120 width
			// Since we're arranging it at that same width, there's no reason for it to expand the items
			// so we expect them to be arranged at the same widths
			var measure = MeasureAndArrange(grid, widthConstraint: double.PositiveInfinity, heightConstraint: 200);

			Assert.Equal(120, measure.Width);

			AssertArranged(view0, new Rect(0, 0, 20, 20));
			AssertArranged(view1, new Rect(20, 0, 40, 20));
			AssertArranged(view2, new Rect(60, 0, 60, 20));
		}

		// These next two tests validate cases where the Grid structure necessitates multiple 
		// measure passes (because a Star value intersects with multiple Auto values)
		// and the items being measured may have a different Auto height/width on the second pass.

		[Theory, Category(GridAutoSizing)]
		[InlineData(10, 30)] // Replicating the situation from https://github.com/dotnet/maui/issues/14296
		[InlineData(40, 30)] // Simulating something like an Image where the height shrinks as the width constraint gets tighter
		public void AutoRowIsDominatedByTallestView(double unconstrainedHeight, double constrainedHeight)
		{
			var grid = CreateGridLayout(rows: "Auto", columns: "Auto, *");

			var view0 = CreateTestView(new Size(20, 20));

			// The view changes size depending on how wide its measurement constraints are
			var view1 = CreateWidthDominatedView(100, unconstrainedHeight, new Tuple<double, double>(200, constrainedHeight));

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, row: 0, col: 0);
			SetLocation(grid, view1, row: 0, col: 1);

			var measure = MeasureAndArrange(grid, widthConstraint: 200, heightConstraint: 200);

			// We expect the Grid to grow to accommodate the full height of view1 at this width
			Assert.Equal(constrainedHeight, measure.Height);
		}

		[Theory, Category(GridAutoSizing)]
		[InlineData(10, 30)] // Replicating https://github.com/dotnet/maui/issues/14296 but for columns
		[InlineData(50, 30)] // Simulating something like an Image where the width shrinks as the height constraint gets tighter
		public void AutoColumnIsDominatedByWidestView(double unconstrainedWidth, double constrainedWidth)
		{
			var grid = CreateGridLayout(columns: "Auto", rows: "Auto, *");

			var view0 = CreateTestView(new Size(20, 20));

			// The view changes size depending on how high its measurement constraints are
			var view1 = CreateHeightDominatedView(unconstrainedWidth, 100, new Tuple<double, double>(constrainedWidth, 100));

			SubstituteChildren(grid, view0, view1);

			SetLocation(grid, view0, row: 0, col: 0);
			SetLocation(grid, view1, row: 1, col: 0);

			var measure = MeasureAndArrange(grid, widthConstraint: 100, heightConstraint: 100);

			// We expect the Grid to group to accommodate the full width of view1 at this height
			Assert.Equal(constrainedWidth, measure.Width);
		}

		[Theory]
		[InlineData(20, 100)]
		[InlineData(200, 100)]
		public void AutoStarColumnSpanMeasureIsSumOfAutoAndStar(double determinantViewWidth, double widthConstraint)
		{
			var grid = CreateGridLayout(columns: "Auto, *", rows: "Auto, Auto");
			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(determinantViewWidth, 20));
			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, row: 0, col: 0, colSpan: 2);
			SetLocation(grid, view1, row: 1, col: 0, colSpan: 1);

			var measure = MeasureAndArrange(grid, widthConstraint: widthConstraint, heightConstraint: 100);

			// view1 should make column 0 at least `determinantViewWidth` units wide
			// So view0 should be getting measured at _at least_ that value; if the widthConstraint is larger
			// than that, the * should bump the measure value up to match the widthConstraint
			var expectedMeasureWidth = Math.Max(determinantViewWidth, widthConstraint);

			view0.Received().Measure(Arg.Is<double>(expectedMeasureWidth), Arg.Any<double>());
		}

		[Theory]
		[InlineData(20, 100)]
		[InlineData(200, 100)]
		public void AutoStarRowSpanMeasureIsSumOfAutoAndStar(double determinantViewHeight, double heightConstraint)
		{
			var grid = CreateGridLayout(columns: "Auto, Auto", rows: "Auto, *");
			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(20, determinantViewHeight));
			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, row: 0, col: 0, rowSpan: 2);
			SetLocation(grid, view1, row: 0, col: 1, rowSpan: 1);

			var measure = MeasureAndArrange(grid, widthConstraint: 100, heightConstraint: heightConstraint);

			// view1 should make row 0 at least `determinantViewHeight` units tall
			// So view0 should be getting measured at _at least_ that value; if the heightConstraint is larger
			// than that, the * should bump the measure value up to match the heightConstraint
			var expectedMeasureHeight = Math.Max(determinantViewHeight, heightConstraint);

			view0.Received().Measure(Arg.Any<double>(), Arg.Is<double>(expectedMeasureHeight));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(0.1)]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(60)]
		[InlineData(1000)]
		[InlineData(-0.1)]
		[InlineData(-10)]
		[InlineData(-60)]
		public void MultipleArrangeCallsProduceConsistentResults(double delta)
		{
			var grid = CreateGridLayout(rows: "*, *, *", columns: "*, *, *");
			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 20));
			var view1 = CreateTestView(new Size(40, 20));
			var view2 = CreateTestView(new Size(60, 20));

			SubstituteChildren(grid, view0, view1, view2);

			SetLocation(grid, view0, col: 0, row: 0);
			SetLocation(grid, view1, col: 1, row: 1);
			SetLocation(grid, view2, col: 2, row: 2);

			// Measure the Grid with no constraints, then arrange it using the resulting size
			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// Now arrange it at a _different_ size
			manager.ArrangeChildren(new Rect(0, 0, measure.Width + delta, measure.Height + delta));

			// Determine the destination Rect values that the manager passed in when calling Arrange() for each view
			var v0ArrangeArgs1 = view0.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view0Dest1 = (Rect)v0ArrangeArgs1[0];

			var v1ArrangeArgs1 = view1.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view1Dest1 = (Rect)v1ArrangeArgs1[0];

			var v2ArrangeArgs1 = view2.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view2Dest1 = (Rect)v2ArrangeArgs1[0];

			view0.ClearReceivedCalls();
			view1.ClearReceivedCalls();
			view2.ClearReceivedCalls();

			// Now arrange it at the same size again
			manager.ArrangeChildren(new Rect(0, 0, measure.Width + delta, measure.Height + delta));

			// Determine the destination Rect values that the manager passed in when calling Arrange() for each view
			var v0ArrangeArgs2 = view0.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view0Dest2 = (Rect)v0ArrangeArgs2[0];

			var v1ArrangeArgs2 = view1.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view1Dest2 = (Rect)v1ArrangeArgs2[0];

			var v2ArrangeArgs2 = view2.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			var view2Dest2 = (Rect)v2ArrangeArgs2[0];

			// Ensure that Arrange was called with the same destination rect for each view both times
			Assert.Equal(view0Dest1, view0Dest2);
			Assert.Equal(view1Dest1, view1Dest2);
			Assert.Equal(view2Dest1, view2Dest2);
		}

		/*
		 * These next two test cover the specific situation from https://github.com/dotnet/maui/issues/14818
		 * Where the control in the * row/column is one that doesn't play nice with measurement. The control
		 * is determined to return a measure size larger than the constraints it's given. We have to ensure 
		 * that the sizes of the containing * row/column don't expand beyond their natural confines even if 
		 * the containing control asks for it. (The control itself may still be larger than the row/column 
		 * size, and when laid out it may exceed the boundaries of the row/column. But the row/column itself
		 * must still adhere to the constraints given by the column definitions and grid size constraints.)
		 */

		[Theory]
		[InlineData("100, *, 100")]
		[InlineData("Auto, *, Auto")]
		[InlineData("Auto, *, 100")]
		[InlineData("100, *, Auto")]
		public void StarRowsHandleGreedyMeasures(string rowDefinitions)
		{
			var grid = CreateGridLayout(rows: rowDefinitions);
			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);

			// view0 is going to be a view that returns 500 high no matter what
			var view0 = CreateTestView(new Size(100, 500));
			var view1 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, col: 0, row: 1);
			SetLocation(grid, view1, col: 0, row: 2);

			// Measure the grid with a height constraint
			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(100, 500);

			manager.ArrangeChildren(new Rect(0, 0, 100, 500));

			// At a height constraint of 500 we expect the star row to be 300 high
			// So view1 should be arranged at a Y value of 100 + 300 = 400 
			AssertArranged(view1, new Rect(0, 400, 100, 100));
		}

		[Theory]
		[InlineData("100, *, 100")]
		[InlineData("Auto, *, Auto")]
		[InlineData("Auto, *, 100")]
		[InlineData("100, *, Auto")]
		public void StarColumnsHandleGreedyMeasures(string columnDefinitions)
		{
			var grid = CreateGridLayout(columns: columnDefinitions);
			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
			grid.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);

			// view0 is going to be a view that returns 500 wide no matter what
			var view0 = CreateTestView(new Size(500, 100));
			var view1 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, col: 1, row: 0);
			SetLocation(grid, view1, col: 2, row: 0);

			// Measure the grid with a width constraint
			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(500, 100);

			manager.ArrangeChildren(new Rect(0, 0, 500, 100));

			// At a width constraint of 500, we expect the star column to be 300 wide
			// So view1 should be arranged at an X value of 100 + 300 = 400 
			AssertArranged(view1, new Rect(400, 0, 100, 100));
		}

		[Theory]
		[InlineData("100", 1, 10, 100)]
		[InlineData("100, 100", 2, 0, 100 + 0 + 100)]
		[InlineData("100, 100", 2, 10, 100 + 10 + 100)]
		[InlineData("100, 100, 50", 3, 20, 100 + 20 + 100 + 20 + 50)]
		public void SpannedColumnMeasureIncludesSpacing(string columnDefinitions, int columnSpan, double spacing, double expectedWidth)
		{
			var grid = CreateGridLayout(columns: columnDefinitions);
			grid.ColumnSpacing.Returns(spacing);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, row: 0, col: 0, colSpan: columnSpan);

			MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);

			view0.Received().Measure(Arg.Is<double>(expectedWidth), Arg.Any<Double>());
		}

		[Theory]
		[InlineData("100", 1, 10, 100)]
		[InlineData("100, 100", 2, 0, 100 + 0 + 100)]
		[InlineData("100, 100", 2, 10, 100 + 10 + 100)]
		[InlineData("100, 100, 50", 3, 20, 100 + 20 + 100 + 20 + 50)]
		public void SpannedRowMeasureIncludesSpacing(string rowDefinitions, int rowSpan, double spacing, double expectedHeight)
		{
			var grid = CreateGridLayout(rows: rowDefinitions);
			grid.RowSpacing.Returns(spacing);

			var view0 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, row: 0, col: 0, rowSpan: rowSpan);

			MeasureAndArrange(grid, double.PositiveInfinity, double.PositiveInfinity);

			view0.Received().Measure(Arg.Any<double>(), Arg.Is<Double>(expectedHeight));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(0, 0)]
		[InlineData(16, 0)]
		[InlineData(0, 16)]
		[InlineData(16, 16)]
		[InlineData(-16, 16)]
		[InlineData(-16, -16)]
		[InlineData(16, -16)]
		public void StarColumnsAccountForPadding(double left, double right)
		{
			var grid = CreateGridLayout(columns: "*,48", rows: "200");
			grid.Width.Returns(200);
			grid.Padding.Returns(new Thickness(left, 0, right, 0));

			var view0 = CreateTestView(new Size(20, 20));
			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, col: 0, row: 0, colSpan: 2);

			MeasureAndArrange(grid, 900, 900);

			// We expect the left edge of the view to be inset by the left padding,
			// and the width of the view to be the width of the Grid minus all padding
			AssertArranged(view0, new Rect(left, 0, 200 - left - right, 200));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(0, 0)]
		[InlineData(16, 0)]
		[InlineData(0, 16)]
		[InlineData(16, 16)]
		[InlineData(-16, 16)]
		[InlineData(-16, -16)]
		[InlineData(16, -16)]
		public void StarRowsAccountForPadding(double top, double bottom)
		{
			var grid = CreateGridLayout(rows: "*,48", columns: "200");
			grid.Height.Returns(200);
			grid.Padding.Returns(new Thickness(0, top, 0, bottom));

			var view0 = CreateTestView(new Size(20, 20));
			SubstituteChildren(grid, view0);
			SetLocation(grid, view0, col: 0, row: 0, rowSpan: 2);

			MeasureAndArrange(grid, 900, 900);

			// We expect the top edge of the view to be inset by the top padding,
			// and the height of the view to be the height of the Grid minus all padding
			AssertArranged(view0, new Rect(0, top, 200, 200 - top - bottom));
		}

		// Test for https://github.com/dotnet/maui/issues/16815
		// Because the padding is added to the grid's min possible size
		// and then that size is used to calculate cell sizes, we need to
		// remove the padding as the cells are not placed in the padding.
		// This means the issue appears when:
		//   grid width = view widths + (2 * padding)
		[Theory, Category(GridStarSizing)]
		[InlineData(40, 21, 24)]
		[InlineData(39, 20, 23)]
		[InlineData(38, 19, 22)]
		[InlineData(37, 18, 21)]
		public void StarRowsCalculateCorrectlyWhenGridWidthNearsMinWidth(double widthConstraint, double view0ExpectedWidth, double view1ExpectedX)
		{
			var heights = 100;

			var paddingL = 3;
			var paddingR = 6;
			var viewWidth = 10;

			var grid = CreateGridLayout(rows: "*", columns: "*, Auto");
			grid.Padding.Returns(new Thickness(paddingL, 0, paddingR, 0));

			var view0 = CreateTestView(new Size(viewWidth, heights));
			var view1 = CreateTestView(new Size(viewWidth, heights));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, col: 0);
			SetLocation(grid, view1, col: 1);

			_ = MeasureAndArrangeFixed(grid, widthConstraint: widthConstraint, heightConstraint: heights);

			AssertArranged(view0, new Rect(paddingL, 0, view0ExpectedWidth, heights));
			AssertArranged(view1, new Rect(view1ExpectedX, 0, viewWidth, heights));
		}

		[Theory, Category(GridStarSizing)]
		[InlineData(40, 21, 24)]
		[InlineData(39, 20, 23)]
		[InlineData(38, 19, 22)]
		[InlineData(37, 18, 21)]
		public void StarColsCalculateCorrectlyWhenGridHeightNearsMinHeight(double widthConstraint, double view0ExpectedWidth, double view1ExpectedX)
		{
			var widths = 100;

			var paddingT = 3;
			var paddingB = 6;
			var viewHeight = 10;

			var grid = CreateGridLayout(rows: "*, Auto", columns: "*");
			grid.Padding.Returns(new Thickness(0, paddingT, 0, paddingB));

			var view0 = CreateTestView(new Size(widths, viewHeight));
			var view1 = CreateTestView(new Size(widths, viewHeight));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, row: 0);
			SetLocation(grid, view1, row: 1);

			_ = MeasureAndArrangeFixed(grid, widthConstraint: widths, heightConstraint: widthConstraint);

			AssertArranged(view0, new Rect(0, paddingT, widths, view0ExpectedWidth));
			AssertArranged(view1, new Rect(0, view1ExpectedX, widths, viewHeight));
		}


		[Fact]
		public void StarRowExpansionWorksWithDifferingScalars()
		{
			var grid = CreateGridLayout(rows: "*, 4.5*, *, 4.5*");

			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(100, 20));
			var view1 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(100, 20));
			var view3 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view1, view2, view3);

			SetLocation(grid, view0, row: 0);
			SetLocation(grid, view1, row: 1);
			SetLocation(grid, view2, row: 2);
			SetLocation(grid, view3, row: 3);

			// Measure the Grid with no constraints			
			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// Our expected height, unconstrained, where all the views get their desired height:
			double expectedHeight = 20 + 100 + 20 + 100;
			Assert.Equal(expectedHeight, measure.Height);

			// Now we'll arrange it at a larger height (as if we were filling up the height of a layout)
			double arrangeHeight = measure.Height + 100;
			manager.ArrangeChildren(new Rect(0, 0, measure.Width, arrangeHeight));

			// Determine the destination Rect values that the manager passed in when calling Arrange() for each view
			var view0Dest = GetArrangedRect(view0);
			var view1Dest = GetArrangedRect(view1);
			var view2Dest = GetArrangedRect(view2);
			var view3Dest = GetArrangedRect(view3);

			// We have four rows: 1*, 4.5*, 1*, 4.5*
			double starCount = 1 + 4.5 + 1 + 4.5;

			// We expect the odd rows to get 1* each
			double expectedOddRowHeight = arrangeHeight / starCount;

			// And the event rows to get 4.5* each
			double expectedEvenRowHeight = expectedOddRowHeight * 4.5;

			// Verify that the views were arranged at those sizes (within tolerance)
			Assert.Equal(expectedOddRowHeight, view0Dest.Height, 1.0);
			Assert.Equal(expectedEvenRowHeight, view1Dest.Height, 1.0);
			Assert.Equal(expectedOddRowHeight, view2Dest.Height, 1.0);
			Assert.Equal(expectedEvenRowHeight, view3Dest.Height, 1.0);
		}

		[Fact]
		public void StarColumnExpansionWorksWithDifferingScalars()
		{
			var grid = CreateGridLayout(columns: "*, 4.5*, *, 4.5*");

			grid.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var view0 = CreateTestView(new Size(20, 100));
			var view1 = CreateTestView(new Size(100, 100));
			var view2 = CreateTestView(new Size(20, 100));
			var view3 = CreateTestView(new Size(100, 100));

			SubstituteChildren(grid, view0, view1, view2, view3);

			SetLocation(grid, view0, col: 0);
			SetLocation(grid, view1, col: 1);
			SetLocation(grid, view2, col: 2);
			SetLocation(grid, view3, col: 3);

			// Measure the Grid with no constraints
			var manager = new GridLayoutManager(grid);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// Our expected width, unconstrained, where all the views get their desired width:
			double expectedWidth = 20 + 100 + 20 + 100;
			Assert.Equal(expectedWidth, measure.Width);

			// Now we'll arrange it at a larger width (as if we were filling up the width of a layout)
			double arrangeWidth = measure.Width + 100;
			manager.ArrangeChildren(new Rect(0, 0, arrangeWidth, measure.Height));

			// Determine the destination Rect values that the manager passed in when calling Arrange() for each view
			var view0Dest = GetArrangedRect(view0);
			var view1Dest = GetArrangedRect(view1);
			var view2Dest = GetArrangedRect(view2);
			var view3Dest = GetArrangedRect(view3);

			// We have four columns: 1*, 4.5*, 1*, 4.5*
			double starCount = 1 + 4.5 + 1 + 4.5;

			// We expect the odd columns to get 1* each
			double expectedOddRowWidth = arrangeWidth / starCount;

			// And the event columns to get 4.5* each
			double expectedEvenRowWidth = expectedOddRowWidth * 4.5;

			// Verify that the views were arranged at those sizes (within tolerance)
			Assert.Equal(expectedOddRowWidth, view0Dest.Width, 1.0);
			Assert.Equal(expectedEvenRowWidth, view1Dest.Width, 1.0);
			Assert.Equal(expectedOddRowWidth, view2Dest.Width, 1.0);
			Assert.Equal(expectedEvenRowWidth, view3Dest.Width, 1.0);
		}

		static Rect GetArrangedRect(IView view)
		{
			var args = view.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IView.Arrange)).GetArguments();
			return (Rect)args[0];
		}

		// The next two tests look at a corner case where the Grid is measured in one dimension without constraint
		// (for instance, inside of a StackLayout); the Star in the unconstrained dimension should be treated
		// as an Auto value. The explicit Auto value in the constrained dimension forces us to make a second measure
		// pass to resolve all the measurements; we have to ensure that this second measure pass is updating
		// the measurement of the unconstrained Star if the Views measured during the second pass are larger
		// in that dimension.

		[Fact]
		public void AutoColumnIntersectionWithUnconstrainedMeasure()
		{
			var grid = CreateGridLayout(columns: "*, Auto", rows: "*");

			var view0 = CreateTestView(new Size(20, 40));
			var view1 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, col: 0);
			SetLocation(grid, view1, col: 1);

			// The infinite height means we treat the * Row as Auto
			_ = MeasureAndArrange(grid, widthConstraint: 200, heightConstraint: double.PositiveInfinity);

			// Ensure that the * Row height was updated to include the taller view
			AssertArranged(view0, new Rect(0, 0, 20, 40));
		}

		[Fact]
		public void AutoRowIntersectionWithUnconstrainedMeasure()
		{
			var grid = CreateGridLayout(rows: "*, Auto", columns: "*");

			var view0 = CreateTestView(new Size(40, 20));
			var view1 = CreateTestView(new Size(20, 20));

			SubstituteChildren(grid, view0, view1);
			SetLocation(grid, view0, row: 0);
			SetLocation(grid, view1, row: 1);

			// The infinite width means we treat the * Column as Auto
			_ = MeasureAndArrange(grid, widthConstraint: double.PositiveInfinity, heightConstraint: 200);

			// Ensure that the * Column width was updated to include the wider view
			AssertArranged(view0, new Rect(0, 0, 40, 20));
		}
	}
}
