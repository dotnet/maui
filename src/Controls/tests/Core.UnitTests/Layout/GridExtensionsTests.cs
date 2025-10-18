#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class GridExtensionsTests
    {
        /// <summary>
        /// Tests that AddWithSpan throws ArgumentNullException when view parameter is null.
        /// This test covers the validation logic that checks for null view parameter.
        /// </summary>
        [Fact]
        public void AddWithSpan_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var grid = new Grid();
            IView view = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                grid.AddWithSpan(view));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddWithSpan throws ArgumentOutOfRangeException when row parameter is negative.
        /// This test verifies the row validation logic that ensures row >= 0.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void AddWithSpan_NegativeRow_ThrowsArgumentOutOfRangeException(int row)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.AddWithSpan(view, row));
            Assert.Equal("row", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddWithSpan throws ArgumentOutOfRangeException when column parameter is negative.
        /// This test verifies the column validation logic that ensures column >= 0.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void AddWithSpan_NegativeColumn_ThrowsArgumentOutOfRangeException(int column)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.AddWithSpan(view, column: column));
            Assert.Equal("column", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddWithSpan throws ArgumentOutOfRangeException when rowSpan parameter is less than 1.
        /// This test verifies the rowSpan validation logic that ensures rowSpan >= 1.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void AddWithSpan_InvalidRowSpan_ThrowsArgumentOutOfRangeException(int rowSpan)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.AddWithSpan(view, rowSpan: rowSpan));
            Assert.Equal("rowSpan", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddWithSpan throws ArgumentOutOfRangeException when columnSpan parameter is less than 1.
        /// This test verifies the columnSpan validation logic that ensures columnSpan >= 1.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void AddWithSpan_InvalidColumnSpan_ThrowsArgumentOutOfRangeException(int columnSpan)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.AddWithSpan(view, columnSpan: columnSpan));
            Assert.Equal("columnSpan", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddWithSpan successfully adds view with default parameters.
        /// This test verifies the method works with default values (row=0, column=0, rowSpan=1, columnSpan=1).
        /// </summary>
        [Fact]
        public void AddWithSpan_DefaultParameters_AddsViewSuccessfully()
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act
            grid.AddWithSpan(view);

            // Assert
            Assert.Contains(view, grid.Children);
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddWithSpan correctly sets row and column positions with custom values.
        /// This test verifies the method properly positions views at specified grid locations.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 3)]
        [InlineData(100, 200)]
        public void AddWithSpan_CustomRowAndColumn_SetsPositionCorrectly(int row, int column)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act
            grid.AddWithSpan(view, row, column);

            // Assert
            Assert.Contains(view, grid.Children);
            Assert.Equal(row, Grid.GetRow(view));
            Assert.Equal(column, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddWithSpan correctly sets row and column spans with custom values.
        /// This test verifies the method properly spans views across multiple rows and columns.
        /// </summary>
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 3)]
        [InlineData(10, 5)]
        [InlineData(100, 200)]
        public void AddWithSpan_CustomSpans_SetsSpansCorrectly(int rowSpan, int columnSpan)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act
            grid.AddWithSpan(view, rowSpan: rowSpan, columnSpan: columnSpan);

            // Assert
            Assert.Contains(view, grid.Children);
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(rowSpan, Grid.GetRowSpan(view));
            Assert.Equal(columnSpan, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddWithSpan creates necessary row definitions when view spans beyond existing rows.
        /// This test verifies the EnsureRows functionality that adds RowDefinition objects as needed.
        /// </summary>
        [Theory]
        [InlineData(0, 2, 2)]
        [InlineData(1, 3, 4)]
        [InlineData(5, 10, 15)]
        public void AddWithSpan_SpansBeyondExistingRows_CreatesRowDefinitions(int row, int rowSpan, int expectedRowCount)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();
            var initialRowCount = grid.RowDefinitions.Count;

            // Act
            grid.AddWithSpan(view, row, rowSpan: rowSpan);

            // Assert
            Assert.True(grid.RowDefinitions.Count >= expectedRowCount);
            Assert.Contains(view, grid.Children);
            Assert.Equal(row, Grid.GetRow(view));
            Assert.Equal(rowSpan, Grid.GetRowSpan(view));
        }

        /// <summary>
        /// Tests that AddWithSpan creates necessary column definitions when view spans beyond existing columns.
        /// This test verifies the EnsureColumns functionality that adds ColumnDefinition objects as needed.
        /// </summary>
        [Theory]
        [InlineData(0, 2, 2)]
        [InlineData(1, 3, 4)]
        [InlineData(5, 10, 15)]
        public void AddWithSpan_SpansBeyondExistingColumns_CreatesColumnDefinitions(int column, int columnSpan, int expectedColumnCount)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();
            var initialColumnCount = grid.ColumnDefinitions.Count;

            // Act
            grid.AddWithSpan(view, column: column, columnSpan: columnSpan);

            // Assert
            Assert.True(grid.ColumnDefinitions.Count >= expectedColumnCount);
            Assert.Contains(view, grid.Children);
            Assert.Equal(column, Grid.GetColumn(view));
            Assert.Equal(columnSpan, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddWithSpan handles boundary values correctly without throwing exceptions.
        /// This test verifies the method works with extreme but valid parameter values.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 1, 1)]
        [InlineData(int.MaxValue - 1, int.MaxValue - 1, 1, 1)]
        [InlineData(0, 0, int.MaxValue, int.MaxValue)]
        public void AddWithSpan_BoundaryValues_HandledCorrectly(int row, int column, int rowSpan, int columnSpan)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<IView>();

            // Act & Assert - Should not throw
            grid.AddWithSpan(view, row, column, rowSpan, columnSpan);

            Assert.Contains(view, grid.Children);
            Assert.Equal(row, Grid.GetRow(view));
            Assert.Equal(column, Grid.GetColumn(view));
            Assert.Equal(rowSpan, Grid.GetRowSpan(view));
            Assert.Equal(columnSpan, Grid.GetColumnSpan(view));
        }
    }
}
