using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;
using static Microsoft.Maui.UnitTests.Layouts.LayoutTestHelpers;
using static Microsoft.Maui.UnitTests.Layouts.GridLayoutManagerTests;

namespace Microsoft.Maui.UnitTests.Layouts
{
    [Category(TestCategory.Layout)]
    public class GridLayoutManagerMinMaxTests
    {
        const string GridMinMaxConstraints = "GridMinMaxConstraints";

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
            CultureInfo usCulture = new CultureInfo("en-US"); // Ensure we're using a period as the decimal separator

            gridLength = gridLength.Trim();

            if (gridLength.EndsWith("*"))
            {
                gridLength = gridLength.Substring(0, gridLength.Length - 1);

                if (gridLength.Length == 0)
                {
                    return GridLength.Star;
                }

                return new GridLength(double.Parse(gridLength, usCulture), GridUnitType.Star);
            }

            if (gridLength.ToLowerInvariant() == "auto")
            {
                return GridLength.Auto;
            }

            return new GridLength(double.Parse(gridLength, CultureInfo.InvariantCulture));
        }

        List<IGridColumnDefinition> CreateTestColumns(params string[] columnWidths)
        {
            var colDefs = new List<IGridColumnDefinition>();

            foreach (var width in columnWidths)
            {
                var gridLength = GridLengthFromString(width);
                var colDef = Substitute.For<IGridColumnDefinition>();
                colDef.Width.Returns(gridLength);
                colDef.MinWidth.Returns(-1d);
                colDef.MaxWidth.Returns(-1d);
                colDefs.Add(colDef);
            }

            return colDefs;
        }

        List<IGridColumnDefinition> CreateTestColumnsWithMinMaxConstraints(string[] columnWidths, double[] minWidths = null, double[] maxWidths = null)
        {
            var colDefs = new List<IGridColumnDefinition>();

            for (int i = 0; i < columnWidths.Length; i++)
            {
                var gridLength = GridLengthFromString(columnWidths[i]);
                var colDef = Substitute.For<IGridColumnDefinition>();
                colDef.Width.Returns(gridLength);
                
                double minWidth = (minWidths != null && i < minWidths.Length) ? minWidths[i] : -1d;
                double maxWidth = (maxWidths != null && i < maxWidths.Length) ? maxWidths[i] : -1d;
                
                colDef.MinWidth.Returns(minWidth);
                colDef.MaxWidth.Returns(maxWidth);
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
                rowDef.MinHeight.Returns(-1d);
                rowDef.MaxHeight.Returns(-1d);
                rowDefs.Add(rowDef);
            }

            return rowDefs;
        }

        List<IGridRowDefinition> CreateTestRowsWithMinMaxConstraints(string[] rowHeights, double[] minHeights = null, double[] maxHeights = null)
        {
            var rowDefs = new List<IGridRowDefinition>();

            for (int i = 0; i < rowHeights.Length; i++)
            {
                var gridLength = GridLengthFromString(rowHeights[i]);
                var rowDef = Substitute.For<IGridRowDefinition>();
                rowDef.Height.Returns(gridLength);
                
                double minHeight = (minHeights != null && i < minHeights.Length) ? minHeights[i] : -1d;
                double maxHeight = (maxHeights != null && i < maxHeights.Length) ? maxHeights[i] : -1d;
                
                rowDef.MinHeight.Returns(minHeight);
                rowDef.MaxHeight.Returns(maxHeight);
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

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void ColumnMinWidthConstraint()
        {
            // Setup a grid with a column with min width of 100
            var columnWidths = new[] { "Auto" };
            var minWidths = new[] { 100d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, minWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // A view that would naturally be smaller than the min width
            var view = CreateTestView(new Size(50, 100));
            SubstituteChildren(grid, view);

            // Set up the view to be in our constrained column
            SetLocation(grid, view, col: 0);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // The view should be expanded to match the minimum width constraint
            AssertArranged(view, 0, 0, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void ColumnMaxWidthConstraint()
        {
            // Setup a grid with a column with max width of 100
            var columnWidths = new[] { "Auto" };
            var maxWidths = new[] { 100d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, null, maxWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // A view that would naturally be larger than the max width
            var view = CreateTestView(new Size(200, 100));
            SubstituteChildren(grid, view);

            // Set up the view to be in our constrained column
            SetLocation(grid, view, col: 0);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // The view should be constrained to match the maximum width
            AssertArranged(view, 0, 0, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void RowMinHeightConstraint()
        {
            // Setup a grid with a row with min height of 100
            var rowHeights = new[] { "Auto" };
            var minHeights = new[] { 100d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, minHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // A view that would naturally be smaller than the min height
            var view = CreateTestView(new Size(100, 50));
            SubstituteChildren(grid, view);

            // Set up the view to be in our constrained row
            SetLocation(grid, view, row: 0);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // The view should be expanded to match the minimum height constraint
            AssertArranged(view, 0, 0, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void RowMaxHeightConstraint()
        {
            // Setup a grid with a row with max height of 100
            var rowHeights = new[] { "Auto" };
            var maxHeights = new[] { 100d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, null, maxHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // A view that would naturally be larger than the max height
            var view = CreateTestView(new Size(100, 200));
            SubstituteChildren(grid, view);

            // Set up the view to be in our constrained row
            SetLocation(grid, view, row: 0);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // The view should be constrained to match the maximum height
            AssertArranged(view, 0, 0, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MultipleColumnsWithMinWidthConstraints()
        {
            // Setup a grid with multiple columns, each with min width constraints
            var columnWidths = new[] { "Auto", "Auto", "Auto" };
            var minWidths = new[] { 100d, 150d, 200d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, minWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // Create views that are smaller than their min width constraints
            var view0 = CreateTestView(new Size(50, 100));
            var view1 = CreateTestView(new Size(100, 100));
            var view2 = CreateTestView(new Size(150, 100));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding column
            SetLocation(grid, view0, col: 0);
            SetLocation(grid, view1, col: 1);
            SetLocation(grid, view2, col: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Each view should have been expanded to match its column's min width
            AssertArranged(view0, 0, 0, 100, 100);
            AssertArranged(view1, 100, 0, 150, 100);
            AssertArranged(view2, 250, 0, 200, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MultipleColumnsWithMaxWidthConstraints()
        {
            // Setup a grid with multiple columns, each with max width constraints
            var columnWidths = new[] { "Auto", "Auto", "Auto" };
            var maxWidths = new[] { 50d, 75d, 100d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, null, maxWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // Create views that are larger than their max width constraints
            var view0 = CreateTestView(new Size(100, 100));
            var view1 = CreateTestView(new Size(150, 100));
            var view2 = CreateTestView(new Size(200, 100));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding column
            SetLocation(grid, view0, col: 0);
            SetLocation(grid, view1, col: 1);
            SetLocation(grid, view2, col: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Each view should have been constrained to match its column's max width
            AssertArranged(view0, 0, 0, 50, 100);
            AssertArranged(view1, 50, 0, 75, 100);
            AssertArranged(view2, 125, 0, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MultipleRowsWithMinHeightConstraints()
        {
            // Setup a grid with multiple rows, each with min height constraints
            var rowHeights = new[] { "Auto", "Auto", "Auto" };
            var minHeights = new[] { 100d, 150d, 200d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, minHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // Create views that are smaller than their min height constraints
            var view0 = CreateTestView(new Size(100, 50));
            var view1 = CreateTestView(new Size(100, 100));
            var view2 = CreateTestView(new Size(100, 150));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding row
            SetLocation(grid, view0, row: 0);
            SetLocation(grid, view1, row: 1);
            SetLocation(grid, view2, row: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Each view should have been expanded to match its row's min height
            AssertArranged(view0, 0, 0, 100, 100);
            AssertArranged(view1, 0, 100, 100, 150);
            AssertArranged(view2, 0, 250, 100, 200);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MultipleRowsWithMaxHeightConstraints()
        {
            // Setup a grid with multiple rows, each with max height constraints
            var rowHeights = new[] { "Auto", "Auto", "Auto" };
            var maxHeights = new[] { 50d, 75d, 100d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, null, maxHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // Create views that are larger than their max height constraints
            var view0 = CreateTestView(new Size(100, 100));
            var view1 = CreateTestView(new Size(100, 150));
            var view2 = CreateTestView(new Size(100, 200));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding row
            SetLocation(grid, view0, row: 0);
            SetLocation(grid, view1, row: 1);
            SetLocation(grid, view2, row: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Each view should have been constrained to match its row's max height
            AssertArranged(view0, 0, 0, 100, 50);
            AssertArranged(view1, 0, 50, 100, 75);
            AssertArranged(view2, 0, 125, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinMaxWidthConstraintsWithStarSizing()
        {
            // Setup a grid with star columns, each with min/max constraints
            var columnWidths = new[] { "*", "2*", "*" };
            var minWidths = new[] { 100d, 200d, 100d };
            var maxWidths = new[] { 200d, 300d, 200d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, minWidths, maxWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // Create views for each column
            var view0 = CreateTestView(new Size(50, 100));
            var view1 = CreateTestView(new Size(50, 100));
            var view2 = CreateTestView(new Size(50, 100));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding column
            SetLocation(grid, view0, col: 0);
            SetLocation(grid, view1, col: 1);
            SetLocation(grid, view2, col: 2);

            // Measure and arrange with fixed constraints that would:
            // - Without min constraints: make columns too small
            // - Without max constraints: make columns too large
            MeasureAndArrangeFixed(grid, 1000, 100);

            // Expected distribution:
            // - Total width: 1000
            // - Proportional distribution would be 250:500:250 for 1*:2*:1*
            // - But max constraints (200:300:200) should enforce max width limits
            // - So the actual distribution should be 200:300:200 = 700 total
            // - This means there are 300 unused points
            AssertArranged(view0, 0, 0, 200, 100);
            AssertArranged(view1, 200, 0, 300, 100);
            AssertArranged(view2, 500, 0, 200, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinMaxHeightConstraintsWithStarSizing()
        {
            // Setup a grid with star rows, each with min/max constraints
            var rowHeights = new[] { "*", "2*", "*" };
            var minHeights = new[] { 100d, 200d, 100d };
            var maxHeights = new[] { 200d, 300d, 200d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, minHeights, maxHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // Create views for each row
            var view0 = CreateTestView(new Size(100, 50));
            var view1 = CreateTestView(new Size(100, 50));
            var view2 = CreateTestView(new Size(100, 50));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding row
            SetLocation(grid, view0, row: 0);
            SetLocation(grid, view1, row: 1);
            SetLocation(grid, view2, row: 2);

            // Measure and arrange with fixed constraints that would:
            // - Without min constraints: make rows too small
            // - Without max constraints: make rows too large
            MeasureAndArrangeFixed(grid, 100, 1000);

            // Expected distribution:
            // - Total height: 1000
            // - Proportional distribution would be 250:500:250 for 1*:2*:1*
            // - But max constraints (200:300:200) should enforce max height limits
            // - So the actual distribution should be 200:300:200 = 700 total
            // - This means there are 300 unused points
            AssertArranged(view0, 0, 0, 100, 200);
            AssertArranged(view1, 0, 200, 100, 300);
            AssertArranged(view2, 0, 500, 100, 200);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinMaxWidthConstraintsWithAbsoluteSizing()
        {
            // Setup a grid with absolute columns, each with min/max constraints
            var columnWidths = new[] { "50", "100", "150" };
            var minWidths = new[] { 100d, 50d, 100d };
            var maxWidths = new[] { 200d, 200d, 125d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, minWidths, maxWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // Create views for each column
            var view0 = CreateTestView(new Size(50, 100));
            var view1 = CreateTestView(new Size(50, 100));
            var view2 = CreateTestView(new Size(50, 100));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding column
            SetLocation(grid, view0, col: 0);
            SetLocation(grid, view1, col: 1);
            SetLocation(grid, view2, col: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Expected column widths:
            // - Column 0: Min 100 overrides absolute 50
            // - Column 1: Absolute 100 is between min 50 and max 200
            // - Column 2: Max 125 overrides absolute 150
            AssertArranged(view0, 0, 0, 100, 100);
            AssertArranged(view1, 100, 0, 100, 100);
            AssertArranged(view2, 200, 0, 125, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinMaxHeightConstraintsWithAbsoluteSizing()
        {
            // Setup a grid with absolute rows, each with min/max constraints
            var rowHeights = new[] { "50", "100", "150" };
            var minHeights = new[] { 100d, 50d, 100d };
            var maxHeights = new[] { 200d, 200d, 125d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, minHeights, maxHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // Create views for each row
            var view0 = CreateTestView(new Size(100, 50));
            var view1 = CreateTestView(new Size(100, 50));
            var view2 = CreateTestView(new Size(100, 50));
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding row
            SetLocation(grid, view0, row: 0);
            SetLocation(grid, view1, row: 1);
            SetLocation(grid, view2, row: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Expected row heights:
            // - Row 0: Min 100 overrides absolute 50
            // - Row 1: Absolute 100 is between min 50 and max 200
            // - Row 2: Max 125 overrides absolute 150
            AssertArranged(view0, 0, 0, 100, 100);
            AssertArranged(view1, 0, 100, 100, 100);
            AssertArranged(view2, 0, 200, 100, 125);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinMaxWidthConstraintsWithAutoSizing()
        {
            // Setup a grid with auto columns, each with min/max constraints
            var columnWidths = new[] { "Auto", "Auto", "Auto" };
            var minWidths = new[] { 75d, 125d, 150d };
            var maxWidths = new[] { 125d, 175d, 200d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, minWidths, maxWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // Create views with varying widths
            var view0 = CreateTestView(new Size(50, 100));  // Below min
            var view1 = CreateTestView(new Size(150, 100)); // Between min and max
            var view2 = CreateTestView(new Size(250, 100)); // Above max
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding column
            SetLocation(grid, view0, col: 0);
            SetLocation(grid, view1, col: 1);
            SetLocation(grid, view2, col: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Expected column widths:
            // - Column 0: Min 75 overrides auto 50
            // - Column 1: Auto 150 is between min 125 and max 175
            // - Column 2: Max 200 overrides auto 250
            AssertArranged(view0, 0, 0, 75, 100);
            AssertArranged(view1, 75, 0, 150, 100);
            AssertArranged(view2, 225, 0, 200, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinMaxHeightConstraintsWithAutoSizing()
        {
            // Setup a grid with auto rows, each with min/max constraints
            var rowHeights = new[] { "Auto", "Auto", "Auto" };
            var minHeights = new[] { 75d, 125d, 150d };
            var maxHeights = new[] { 125d, 175d, 200d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, minHeights, maxHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // Create views with varying heights
            var view0 = CreateTestView(new Size(100, 50));  // Below min
            var view1 = CreateTestView(new Size(100, 150)); // Between min and max
            var view2 = CreateTestView(new Size(100, 250)); // Above max
            SubstituteChildren(grid, view0, view1, view2);

            // Place each view in its corresponding row
            SetLocation(grid, view0, row: 0);
            SetLocation(grid, view1, row: 1);
            SetLocation(grid, view2, row: 2);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Expected row heights:
            // - Row 0: Min 75 overrides auto 50
            // - Row 1: Auto 150 is between min 125 and max 175
            // - Row 2: Max 200 overrides auto 250
            AssertArranged(view0, 0, 0, 100, 75);
            AssertArranged(view1, 0, 75, 100, 150);
            AssertArranged(view2, 0, 225, 100, 200);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinWidthConstraintWithColumnSpan()
        {
            // Setup a grid with columns with min width constraints
            var columnWidths = new[] { "Auto", "Auto" };
            var minWidths = new[] { 75d, 100d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, minWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // Create a view that spans both columns
            var view0 = CreateTestView(new Size(50, 100));  // Would be smaller than combined min widths
            var view1 = CreateTestView(new Size(50, 100));  // For the first column
            var view2 = CreateTestView(new Size(50, 100));  // For the second column
            SubstituteChildren(grid, view0, view1, view2);

            // Setup view locations
            SetLocation(grid, view0, col: 0, colSpan: 2);
            SetLocation(grid, view1, row: 1, col: 0);
            SetLocation(grid, view2, row: 1, col: 1);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Column 0 should be 75 wide and Column 1 should be 100 wide
            // The spanning view should get the sum of both column widths (175)
            AssertArranged(view0, 0, 0, 175, 100);
            AssertArranged(view1, 0, 100, 75, 100);
            AssertArranged(view2, 75, 100, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MinHeightConstraintWithRowSpan()
        {
            // Setup a grid with rows with min height constraints
            var rowHeights = new[] { "Auto", "Auto" };
            var minHeights = new[] { 75d, 100d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, minHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // Create a view that spans both rows
            var view0 = CreateTestView(new Size(100, 50));  // Would be smaller than combined min heights
            var view1 = CreateTestView(new Size(100, 50));  // For the first row
            var view2 = CreateTestView(new Size(100, 50));  // For the second row
            SubstituteChildren(grid, view0, view1, view2);

            // Setup view locations
            SetLocation(grid, view0, row: 0, rowSpan: 2);
            SetLocation(grid, view1, row: 0, col: 1);
            SetLocation(grid, view2, row: 1, col: 1);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Row 0 should be 75 tall and Row 1 should be 100 tall
            // The spanning view should get the sum of both row heights (175)
            AssertArranged(view0, 0, 0, 100, 175);
            AssertArranged(view1, 100, 0, 100, 75);
            AssertArranged(view2, 100, 75, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MaxWidthConstraintWithColumnSpan()
        {
            // Setup a grid with columns with max width constraints
            var columnWidths = new[] { "Auto", "Auto" };
            var maxWidths = new[] { 75d, 100d };
            var colDefs = CreateTestColumnsWithMinMaxConstraints(columnWidths, null, maxWidths);
            
            var grid = CreateGridLayout();
            SubColDefs(grid, colDefs);

            // Create a view that spans both columns
            var view0 = CreateTestView(new Size(200, 100));  // Would be larger than combined max widths
            var view1 = CreateTestView(new Size(100, 100));  // For the first column
            var view2 = CreateTestView(new Size(150, 100));  // For the second column
            SubstituteChildren(grid, view0, view1, view2);

            // Setup view locations
            SetLocation(grid, view0, col: 0, colSpan: 2);
            SetLocation(grid, view1, row: 1, col: 0);
            SetLocation(grid, view2, row: 1, col: 1);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Column 0 should be capped at 75 wide and Column 1 at 100 wide
            // The spanning view should get the sum of both column widths (175)
            AssertArranged(view0, 0, 0, 175, 100);
            AssertArranged(view1, 0, 100, 75, 100);
            AssertArranged(view2, 75, 100, 100, 100);
        }

        [Category(GridMinMaxConstraints)]
        [Fact]
        public void MaxHeightConstraintWithRowSpan()
        {
            // Setup a grid with rows with max height constraints
            var rowHeights = new[] { "Auto", "Auto" };
            var maxHeights = new[] { 75d, 100d };
            var rowDefs = CreateTestRowsWithMinMaxConstraints(rowHeights, null, maxHeights);
            
            var grid = CreateGridLayout();
            SubRowDefs(grid, rowDefs);

            // Create a view that spans both rows
            var view0 = CreateTestView(new Size(100, 200));  // Would be larger than combined max heights
            var view1 = CreateTestView(new Size(100, 100));  // For the first row
            var view2 = CreateTestView(new Size(100, 150));  // For the second row
            SubstituteChildren(grid, view0, view1, view2);

            // Setup view locations
            SetLocation(grid, view0, row: 0, rowSpan: 2);
            SetLocation(grid, view1, row: 0, col: 1);
            SetLocation(grid, view2, row: 1, col: 1);

            // Measure and arrange with no constraints
            MeasureAndArrangeAuto(grid);

            // Row 0 should be capped at 75 tall and Row 1 at 100 tall
            // The spanning view should get the sum of both row heights (175)
            AssertArranged(view0, 0, 0, 100, 175);
            AssertArranged(view1, 100, 0, 100, 75);
            AssertArranged(view2, 100, 75, 100, 100);
        }
    }
}