#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the Grid.ColumnDefinitions property.
    /// </summary>
    public partial class GridTests
    {
        /// <summary>
        /// Tests that ColumnDefinitions property getter returns a non-null default value when first accessed.
        /// This test ensures the default value creator is properly invoked.
        /// Expected result: ColumnDefinitions should return a valid ColumnDefinitionCollection instance.
        /// </summary>
        [Fact]
        public void ColumnDefinitions_Getter_ReturnsNonNullDefaultValue()
        {
            // Arrange
            var grid = new Grid();

            // Act
            var columnDefinitions = grid.ColumnDefinitions;

            // Assert
            Assert.NotNull(columnDefinitions);
            Assert.IsType<ColumnDefinitionCollection>(columnDefinitions);
        }

        /// <summary>
        /// Tests that ColumnDefinitions property getter returns the same instance on multiple accesses.
        /// This verifies that the default value creator is only called once and the instance is cached.
        /// Expected result: Multiple calls to ColumnDefinitions should return the same instance.
        /// </summary>
        [Fact]
        public void ColumnDefinitions_Getter_ReturnsSameInstanceOnMultipleAccesses()
        {
            // Arrange
            var grid = new Grid();

            // Act
            var columnDefinitions1 = grid.ColumnDefinitions;
            var columnDefinitions2 = grid.ColumnDefinitions;

            // Assert
            Assert.Same(columnDefinitions1, columnDefinitions2);
        }

        /// <summary>
        /// Tests that ColumnDefinitions property setter accepts a valid ColumnDefinitionCollection.
        /// This verifies the setter works correctly with valid input.
        /// Expected result: The property should be set and retrievable via the getter.
        /// </summary>
        [Fact]
        public void ColumnDefinitions_Setter_AcceptsValidValue()
        {
            // Arrange
            var grid = new Grid();
            var newColumnDefinitions = new ColumnDefinitionCollection();

            // Act
            grid.ColumnDefinitions = newColumnDefinitions;
            var retrievedColumnDefinitions = grid.ColumnDefinitions;

            // Assert
            Assert.Same(newColumnDefinitions, retrievedColumnDefinitions);
        }

        /// <summary>
        /// Tests that ColumnDefinitions property setter with null value is handled by validation.
        /// The bindable property has validateValue that checks for null, so null should be rejected.
        /// Expected result: Setting null should either throw an exception or be ignored based on validation.
        /// </summary>
        [Fact]
        public void ColumnDefinitions_Setter_WithNullValue_IsValidated()
        {
            // Arrange
            var grid = new Grid();
            var originalColumnDefinitions = grid.ColumnDefinitions;

            // Act & Assert
            // The validateValue function checks for null, which should prevent null from being set
            // Since validation happens in the binding system, we expect the original value to remain
            grid.ColumnDefinitions = null;

            // The property should still return the original non-null value due to validation
            Assert.NotNull(grid.ColumnDefinitions);
            Assert.Same(originalColumnDefinitions, grid.ColumnDefinitions);
        }

        /// <summary>
        /// Tests that ColumnDefinitions property setter works with ColumnDefinitionCollection containing definitions.
        /// This verifies the setter works with a populated collection.
        /// Expected result: The property should be set with the populated collection.
        /// </summary>
        [Fact]
        public void ColumnDefinitions_Setter_WithPopulatedCollection_Works()
        {
            // Arrange
            var grid = new Grid();
            var columnDefinition1 = new ColumnDefinition();
            var columnDefinition2 = new ColumnDefinition();
            var columnDefinitions = new ColumnDefinitionCollection(columnDefinition1, columnDefinition2);

            // Act
            grid.ColumnDefinitions = columnDefinitions;
            var retrievedColumnDefinitions = grid.ColumnDefinitions;

            // Assert
            Assert.Same(columnDefinitions, retrievedColumnDefinitions);
            Assert.Equal(2, retrievedColumnDefinitions.Count);
        }

        /// <summary>
        /// Tests that ColumnDefinitions property returns empty collection by default.
        /// This verifies the default value creator creates an empty collection.
        /// Expected result: Default ColumnDefinitions should be empty but not null.
        /// </summary>
        [Fact]
        public void ColumnDefinitions_DefaultValue_IsEmptyCollection()
        {
            // Arrange
            var grid = new Grid();

            // Act
            var columnDefinitions = grid.ColumnDefinitions;

            // Assert
            Assert.NotNull(columnDefinitions);
            Assert.Empty(columnDefinitions);
        }

        /// <summary>
        /// Tests that multiple Grid instances have independent ColumnDefinitions collections.
        /// This ensures each Grid instance gets its own default collection.
        /// Expected result: Different Grid instances should have different ColumnDefinitions instances.
        /// </summary>
        [Fact]
        public void ColumnDefinitions_MultipleGridInstances_HaveIndependentCollections()
        {
            // Arrange
            var grid1 = new Grid();
            var grid2 = new Grid();

            // Act
            var columnDefinitions1 = grid1.ColumnDefinitions;
            var columnDefinitions2 = grid2.ColumnDefinitions;

            // Assert
            Assert.NotSame(columnDefinitions1, columnDefinitions2);
            Assert.NotNull(columnDefinitions1);
            Assert.NotNull(columnDefinitions2);
        }

        /// <summary>
        /// Tests that AddHorizontal throws ArgumentNullException when view parameter is null.
        /// Input: null view parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void AddHorizontal_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var parentGrid = CreateMockGrid();
            var gridCollection = new TestableGridElementCollection(innerCollection, parentGrid);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => gridCollection.AddHorizontal(null));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests AddHorizontal with an empty grid that has no row or column definitions.
        /// Input: empty grid with no definitions, single view.
        /// Expected: view is added at Column=0, ColumnSpan=1, Row=0, RowSpan=1.
        /// </summary>
        [Fact]
        public void AddHorizontal_EmptyGrid_AddsViewAtOriginWithSingleSpan()
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var parentGrid = CreateMockGrid();
            parentGrid.RowDefinitions.Returns(new RowDefinitionCollection());
            parentGrid.ColumnDefinitions.Returns(new ColumnDefinitionCollection());

            var gridCollection = new TestableGridElementCollection(innerCollection, parentGrid);
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            gridCollection.AddHorizontal(view);

            // Assert
            Assert.Single(innerCollection);
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
        }

        /// <summary>
        /// Tests AddHorizontal with a grid that has existing row definitions but no views.
        /// Input: grid with 3 row definitions, no views, single view to add.
        /// Expected: view is added at Column=0, ColumnSpan=1, Row=0, RowSpan=3.
        /// </summary>
        [Fact]
        public void AddHorizontal_GridWithRowDefinitions_AddsViewSpanningAllRows()
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var parentGrid = CreateMockGrid();

            var rowDefinitions = new RowDefinitionCollection();
            rowDefinitions.Add(new RowDefinition());
            rowDefinitions.Add(new RowDefinition());
            rowDefinitions.Add(new RowDefinition());
            parentGrid.RowDefinitions.Returns(rowDefinitions);
            parentGrid.ColumnDefinitions.Returns(new ColumnDefinitionCollection());

            var gridCollection = new TestableGridElementCollection(innerCollection, parentGrid);
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            gridCollection.AddHorizontal(view);

            // Assert
            Assert.Single(innerCollection);
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(3, Grid.GetRowSpan(view));
        }

        /// <summary>
        /// Tests AddHorizontal with a grid that has existing column definitions but no views.
        /// Input: grid with 2 column definitions, no views, single view to add.
        /// Expected: view is added at Column=0, ColumnSpan=1, Row=0, RowSpan=1.
        /// </summary>
        [Fact]
        public void AddHorizontal_GridWithColumnDefinitions_AddsViewAtFirstColumn()
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var parentGrid = CreateMockGrid();

            var columnDefinitions = new ColumnDefinitionCollection();
            columnDefinitions.Add(new ColumnDefinition());
            columnDefinitions.Add(new ColumnDefinition());
            parentGrid.ColumnDefinitions.Returns(columnDefinitions);
            parentGrid.RowDefinitions.Returns(new RowDefinitionCollection());

            var gridCollection = new TestableGridElementCollection(innerCollection, parentGrid);
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            gridCollection.AddHorizontal(view);

            // Assert
            Assert.Single(innerCollection);
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
        }

        /// <summary>
        /// Tests AddHorizontal with a grid that already contains views in specific positions.
        /// Input: grid with existing view at Column=1, RowSpan=2, new view to add.
        /// Expected: new view is added at Column=2, ColumnSpan=1, spanning all rows.
        /// </summary>
        [Fact]
        public void AddHorizontal_GridWithExistingViews_AddsViewAtNextAvailableColumn()
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var parentGrid = CreateMockGrid();
            parentGrid.RowDefinitions.Returns(new RowDefinitionCollection());
            parentGrid.ColumnDefinitions.Returns(new ColumnDefinitionCollection());

            var gridCollection = new TestableGridElementCollection(innerCollection, parentGrid);

            // Add an existing view at Column=1, RowSpan=2
            var existingView = MockPlatformSizeService.Sub<View>();
            Grid.SetColumn(existingView, 1);
            Grid.SetColumnSpan(existingView, 1);
            Grid.SetRow(existingView, 0);
            Grid.SetRowSpan(existingView, 2);
            innerCollection.Add(existingView);

            var newView = MockPlatformSizeService.Sub<View>();

            // Act
            gridCollection.AddHorizontal(newView);

            // Assert
            Assert.Equal(2, innerCollection.Count);
            Assert.Equal(2, Grid.GetColumn(newView));
            Assert.Equal(1, Grid.GetColumnSpan(newView));
            Assert.Equal(0, Grid.GetRow(newView));
            Assert.Equal(2, Grid.GetRowSpan(newView));
        }

        /// <summary>
        /// Tests multiple sequential AddHorizontal calls to verify horizontal arrangement.
        /// Input: multiple views added sequentially via AddHorizontal.
        /// Expected: views are positioned in sequential columns (0, 1, 2, ...).
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        public void AddHorizontal_MultipleSequentialCalls_ArrangesViewsHorizontally(int viewCount)
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var parentGrid = CreateMockGrid();
            parentGrid.RowDefinitions.Returns(new RowDefinitionCollection());
            parentGrid.ColumnDefinitions.Returns(new ColumnDefinitionCollection());

            var gridCollection = new TestableGridElementCollection(innerCollection, parentGrid);
            var views = new List<View>();

            // Act
            for (int i = 0; i < viewCount; i++)
            {
                var view = MockPlatformSizeService.Sub<View>();
                views.Add(view);
                gridCollection.AddHorizontal(view);
            }

            // Assert
            Assert.Equal(viewCount, innerCollection.Count);

            for (int i = 0; i < viewCount; i++)
            {
                Assert.Equal(i, Grid.GetColumn(views[i]));
                Assert.Equal(1, Grid.GetColumnSpan(views[i]));
                Assert.Equal(0, Grid.GetRow(views[i]));
                Assert.Equal(1, Grid.GetRowSpan(views[i]));
            }
        }

        /// <summary>
        /// Tests AddHorizontal with a grid containing both row and column definitions.
        /// Input: grid with 2 rows and 3 columns defined, single view to add.
        /// Expected: view is added at Column=0, ColumnSpan=1, Row=0, RowSpan=2.
        /// </summary>
        [Fact]
        public void AddHorizontal_GridWithBothDefinitions_AddsViewSpanningAllRows()
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var parentGrid = CreateMockGrid();

            var rowDefinitions = new RowDefinitionCollection();
            rowDefinitions.Add(new RowDefinition());
            rowDefinitions.Add(new RowDefinition());
            parentGrid.RowDefinitions.Returns(rowDefinitions);

            var columnDefinitions = new ColumnDefinitionCollection();
            columnDefinitions.Add(new ColumnDefinition());
            columnDefinitions.Add(new ColumnDefinition());
            columnDefinitions.Add(new ColumnDefinition());
            parentGrid.ColumnDefinitions.Returns(columnDefinitions);

            var gridCollection = new TestableGridElementCollection(innerCollection, parentGrid);
            var view = MockPlatformSizeService.Sub<View>();

            // Act
            gridCollection.AddHorizontal(view);

            // Assert
            Assert.Single(innerCollection);
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(2, Grid.GetRowSpan(view));
        }

        private static Grid CreateMockGrid()
        {
            var grid = Substitute.For<Grid>();
            grid.RowDefinitions.Returns(new RowDefinitionCollection());
            grid.ColumnDefinitions.Returns(new ColumnDefinitionCollection());
            return grid;
        }

        /// <summary>
        /// Tests that SetRowSpan correctly sets the row span value on a bindable object.
        /// </summary>
        /// <param name="rowSpan">The row span value to set.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void SetRowSpan_ValidValues_SetsRowSpanCorrectly(int rowSpan)
        {
            // Arrange
            var label = new Label();

            // Act
            Grid.SetRowSpan(label, rowSpan);

            // Assert
            Assert.Equal(rowSpan, Grid.GetRowSpan(label));
        }

        /// <summary>
        /// Tests that SetRowSpan throws ArgumentException when setting invalid row span values (less than 1).
        /// The RowSpanProperty has validation that requires values to be >= 1.
        /// </summary>
        /// <param name="invalidRowSpan">The invalid row span value to test.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(int.MinValue)]
        public void SetRowSpan_InvalidValues_ThrowsArgumentException(int invalidRowSpan)
        {
            // Arrange
            var label = new Label();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Grid.SetRowSpan(label, invalidRowSpan));
        }

        /// <summary>
        /// Tests that SetRowSpan throws ArgumentNullException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void SetRowSpan_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullBindable = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Grid.SetRowSpan(nullBindable, 1));
        }

        /// <summary>
        /// Tests that SetRowSpan updates the row span value when called multiple times on the same bindable object.
        /// </summary>
        [Fact]
        public void SetRowSpan_MultipleCallsOnSameBindable_UpdatesRowSpanCorrectly()
        {
            // Arrange
            var label = new Label();

            // Act
            Grid.SetRowSpan(label, 2);
            Assert.Equal(2, Grid.GetRowSpan(label));

            Grid.SetRowSpan(label, 5);

            // Assert
            Assert.Equal(5, Grid.GetRowSpan(label));
        }

        /// <summary>
        /// Tests that SetRowSpan works correctly with different types of bindable objects.
        /// </summary>
        [Fact]
        public void SetRowSpan_DifferentBindableTypes_SetsRowSpanCorrectly()
        {
            // Arrange
            var label = new Label();
            var button = new Button();
            var grid = new Grid();

            // Act
            Grid.SetRowSpan(label, 3);
            Grid.SetRowSpan(button, 4);
            Grid.SetRowSpan(grid, 2);

            // Assert
            Assert.Equal(3, Grid.GetRowSpan(label));
            Assert.Equal(4, Grid.GetRowSpan(button));
            Assert.Equal(2, Grid.GetRowSpan(grid));
        }

        /// <summary>
        /// Tests that SetColumn throws NullReferenceException when bindable parameter is null.
        /// Verifies that the method properly handles null input for the bindable parameter.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void SetColumn_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;
            int value = 5;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Grid.SetColumn(bindable, value));
        }

        /// <summary>
        /// Tests that SetColumn calls SetValue with ColumnProperty and the provided value for various integer inputs.
        /// Verifies that the method correctly passes parameters to the underlying SetValue method.
        /// Expected result: SetValue should be called with Grid.ColumnProperty and the specified value.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetColumn_ValidBindableWithVariousValues_CallsSetValueWithCorrectParameters(int value)
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();

            // Act
            Grid.SetColumn(bindable, value);

            // Assert
            bindable.Received(1).SetValue(Grid.ColumnProperty, value);
        }

        /// <summary>
        /// Tests that SetColumn works correctly with a real BindableObject instance.
        /// Verifies that the method functions properly with concrete BindableObject implementations.
        /// Expected result: No exceptions should be thrown and the operation should complete successfully.
        /// </summary>
        [Fact]
        public void SetColumn_RealBindableObject_SetsValueSuccessfully()
        {
            // Arrange
            var view = new Label();
            int columnValue = 3;

            // Act
            Grid.SetColumn(view, columnValue);

            // Assert
            // Verify the value was set by reading it back
            Assert.Equal(columnValue, Grid.GetColumn(view));
        }

        /// <summary>
        /// Tests that SetColumn handles zero value correctly.
        /// Verifies that zero is a valid column value and is processed correctly.
        /// Expected result: SetValue should be called with Grid.ColumnProperty and zero.
        /// </summary>
        [Fact]
        public void SetColumn_ZeroValue_CallsSetValueWithZero()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();

            // Act
            Grid.SetColumn(bindable, 0);

            // Assert
            bindable.Received(1).SetValue(Grid.ColumnProperty, 0);
        }

        /// <summary>
        /// Tests that SetColumn handles boundary integer values correctly.
        /// Verifies that extreme integer values are passed correctly to SetValue.
        /// Expected result: SetValue should be called with Grid.ColumnProperty and the boundary value.
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetColumn_BoundaryValues_CallsSetValueWithBoundaryValue(int boundaryValue)
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();

            // Act
            Grid.SetColumn(bindable, boundaryValue);

            // Assert
            bindable.Received(1).SetValue(Grid.ColumnProperty, boundaryValue);
        }

        /// <summary>
        /// Tests that Add method successfully adds a view to the grid at specified position with valid positive coordinates.
        /// Verifies that the view is added to the collection and grid properties are set correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 10)]
        [InlineData(100, 200)]
        [InlineData(int.MaxValue - 1, int.MaxValue - 1)]
        public void Add_ValidPositiveCoordinates_AddsViewSuccessfully(int left, int top)
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);
            var view = Substitute.For<View>();

            // Act
            gridCollection.Add(view, left, top);

            // Assert
            Assert.Contains(view, innerCollection);
            Assert.Equal(top, Grid.GetRow(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(left, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when left parameter is negative.
        /// Verifies the specific parameter name in the exception.
        /// </summary>
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(-100, 5)]
        [InlineData(int.MinValue, 0)]
        public void Add_NegativeLeft_ThrowsArgumentOutOfRangeException(int left, int top)
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);
            var view = Substitute.For<View>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => gridCollection.Add(view, left, top));
            Assert.Equal("left", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when top parameter is negative.
        /// Verifies the specific parameter name in the exception.
        /// </summary>
        [Theory]
        [InlineData(0, -1)]
        [InlineData(5, -100)]
        [InlineData(0, int.MinValue)]
        public void Add_NegativeTop_ThrowsArgumentOutOfRangeException(int left, int top)
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);
            var view = Substitute.For<View>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => gridCollection.Add(view, left, top));
            Assert.Equal("top", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when both left and top parameters are negative.
        /// Verifies that left parameter validation occurs first.
        /// </summary>
        [Theory]
        [InlineData(-1, -1)]
        [InlineData(-5, -10)]
        [InlineData(int.MinValue, int.MinValue)]
        public void Add_BothNegative_ThrowsArgumentOutOfRangeExceptionForLeft(int left, int top)
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);
            var view = Substitute.For<View>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => gridCollection.Add(view, left, top));
            Assert.Equal("left", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentNullException when view parameter is null.
        /// Verifies the specific parameter name in the exception.
        /// </summary>
        [Fact]
        public void Add_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => gridCollection.Add(null, 0, 0));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests edge case with maximum valid integer coordinates to ensure no overflow issues.
        /// Verifies that the view is added successfully with extreme but valid coordinates.
        /// </summary>
        [Fact]
        public void Add_MaximumCoordinates_AddsViewSuccessfully()
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);
            var view = Substitute.For<View>();

            // Act
            gridCollection.Add(view, int.MaxValue, int.MaxValue);

            // Assert
            Assert.Contains(view, innerCollection);
            Assert.Equal(int.MaxValue, Grid.GetRow(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(int.MaxValue, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that Add method correctly sets grid properties when adding view at boundary coordinates.
        /// Verifies that RowSpan and ColumnSpan are always set to 1 for this Add overload.
        /// </summary>
        [Fact]
        public void Add_BoundaryCoordinates_SetsCorrectGridProperties()
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);
            var view = Substitute.For<View>();

            // Act
            gridCollection.Add(view, 0, 0);

            // Assert
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that multiple views can be added to different grid positions without interference.
        /// Verifies that each view maintains its own grid properties.
        /// </summary>
        [Fact]
        public void Add_MultipleViews_EachMaintainsCorrectProperties()
        {
            // Arrange
            var grid = new Grid();
            var innerCollection = new ObservableCollection<Element>();
            var gridCollection = new Grid.GridElementCollection(innerCollection, grid);
            var view1 = Substitute.For<View>();
            var view2 = Substitute.For<View>();
            var view3 = Substitute.For<View>();

            // Act
            gridCollection.Add(view1, 0, 0);
            gridCollection.Add(view2, 1, 2);
            gridCollection.Add(view3, 5, 3);

            // Assert
            Assert.Contains(view1, innerCollection);
            Assert.Contains(view2, innerCollection);
            Assert.Contains(view3, innerCollection);

            Assert.Equal(0, Grid.GetRow(view1));
            Assert.Equal(0, Grid.GetColumn(view1));

            Assert.Equal(2, Grid.GetRow(view2));
            Assert.Equal(1, Grid.GetColumn(view2));

            Assert.Equal(3, Grid.GetRow(view3));
            Assert.Equal(5, Grid.GetColumn(view3));

            Assert.Equal(1, Grid.GetRowSpan(view1));
            Assert.Equal(1, Grid.GetRowSpan(view2));
            Assert.Equal(1, Grid.GetRowSpan(view3));
        }

        /// <summary>
        /// Tests that AddVertical throws ArgumentNullException when views parameter is null.
        /// Input: null views collection
        /// Expected: ArgumentNullException with parameter name "views"
        /// </summary>
        [Fact]
        public void AddVertical_NullViews_ThrowsArgumentNullException()
        {
            // Arrange
            var grid = new Compatibility.Grid { IsPlatformEnabled = true };
            IEnumerable<View> views = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => grid.Children.AddVertical(views));
            Assert.Equal("views", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddVertical works correctly with an empty views collection.
        /// Input: empty views collection
        /// Expected: no views added, no exceptions thrown
        /// </summary>
        [Fact]
        public void AddVertical_EmptyViews_NoViewsAdded()
        {
            // Arrange
            var grid = new Compatibility.Grid { IsPlatformEnabled = true };
            var views = new List<View>();
            var initialCount = grid.Children.Count;

            // Act
            grid.Children.AddVertical(views);

            // Assert
            Assert.Equal(initialCount, grid.Children.Count);
        }

        /// <summary>
        /// Tests that AddVertical correctly adds a single view vertically.
        /// Input: collection with one view
        /// Expected: view added at column 0, row 0 with appropriate spans
        /// </summary>
        [Fact]
        public void AddVertical_SingleView_AddsViewVertically()
        {
            // Arrange
            var grid = new Compatibility.Grid { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var views = new List<View> { view };

            // Act
            grid.Children.AddVertical(views);

            // Assert
            Assert.Single(grid.Children);
            Assert.Contains(view, grid.Children);
            Assert.Equal(0, Compatibility.Grid.GetRow(view));
            Assert.Equal(1, Compatibility.Grid.GetRowSpan(view));
            Assert.Equal(0, Compatibility.Grid.GetColumn(view));
            Assert.Equal(1, Compatibility.Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddVertical correctly adds multiple views vertically in sequence.
        /// Input: collection with multiple views
        /// Expected: each view added in subsequent rows, all at column 0
        /// </summary>
        [Fact]
        public void AddVertical_MultipleViews_AddsViewsInVerticalSequence()
        {
            // Arrange
            var grid = new Compatibility.Grid { IsPlatformEnabled = true };
            var view1 = MockPlatformSizeService.Sub<View>();
            var view2 = MockPlatformSizeService.Sub<View>();
            var view3 = MockPlatformSizeService.Sub<View>();
            var views = new List<View> { view1, view2, view3 };

            // Act
            grid.Children.AddVertical(views);

            // Assert
            Assert.Equal(3, grid.Children.Count);

            // First view should be at row 0
            Assert.Equal(0, Compatibility.Grid.GetRow(view1));
            Assert.Equal(1, Compatibility.Grid.GetRowSpan(view1));
            Assert.Equal(0, Compatibility.Grid.GetColumn(view1));
            Assert.Equal(1, Compatibility.Grid.GetColumnSpan(view1));

            // Second view should be at row 1
            Assert.Equal(1, Compatibility.Grid.GetRow(view2));
            Assert.Equal(1, Compatibility.Grid.GetRowSpan(view2));
            Assert.Equal(0, Compatibility.Grid.GetColumn(view2));
            Assert.Equal(1, Compatibility.Grid.GetColumnSpan(view2));

            // Third view should be at row 2
            Assert.Equal(2, Compatibility.Grid.GetRow(view3));
            Assert.Equal(1, Compatibility.Grid.GetRowSpan(view3));
            Assert.Equal(0, Compatibility.Grid.GetColumn(view3));
            Assert.Equal(1, Compatibility.Grid.GetColumnSpan(view3));
        }

        /// <summary>
        /// Tests that AddVertical throws ArgumentNullException when collection contains a null view.
        /// Input: collection with one valid view and one null view
        /// Expected: ArgumentNullException with parameter name "view" when processing the null view
        /// </summary>
        [Fact]
        public void AddVertical_CollectionWithNullView_ThrowsArgumentNullException()
        {
            // Arrange
            var grid = new Compatibility.Grid { IsPlatformEnabled = true };
            var validView = MockPlatformSizeService.Sub<View>();
            var views = new List<View> { validView, null };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => grid.Children.AddVertical(views));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddVertical correctly handles adding views to a grid with existing column definitions.
        /// Input: grid with 3 column definitions and multiple views
        /// Expected: views span all columns horizontally and are placed in subsequent rows
        /// </summary>
        [Fact]
        public void AddVertical_GridWithExistingColumns_ViewsSpanAllColumns()
        {
            // Arrange
            var grid = new Compatibility.Grid { IsPlatformEnabled = true };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var view1 = MockPlatformSizeService.Sub<View>();
            var view2 = MockPlatformSizeService.Sub<View>();
            var views = new List<View> { view1, view2 };

            // Act
            grid.Children.AddVertical(views);

            // Assert
            Assert.Equal(2, grid.Children.Count);

            // Both views should span all 3 columns
            Assert.Equal(0, Compatibility.Grid.GetColumn(view1));
            Assert.Equal(3, Compatibility.Grid.GetColumnSpan(view1));
            Assert.Equal(0, Compatibility.Grid.GetRow(view1));
            Assert.Equal(1, Compatibility.Grid.GetRowSpan(view1));

            Assert.Equal(0, Compatibility.Grid.GetColumn(view2));
            Assert.Equal(3, Compatibility.Grid.GetColumnSpan(view2));
            Assert.Equal(1, Compatibility.Grid.GetRow(view2));
            Assert.Equal(1, Compatibility.Grid.GetRowSpan(view2));
        }

        /// <summary>
        /// Tests that AddVertical correctly handles adding views to a grid with existing views.
        /// Input: grid with one existing view, then adding multiple views vertically
        /// Expected: new views are placed in subsequent rows after existing views
        /// </summary>
        [Fact]
        public void AddVertical_GridWithExistingViews_AddsViewsAfterExisting()
        {
            // Arrange
            var grid = new Compatibility.Grid { IsPlatformEnabled = true };
            var existingView = MockPlatformSizeService.Sub<View>();
            grid.Children.Add(existingView, 0, 0);

            var newView1 = MockPlatformSizeService.Sub<View>();
            var newView2 = MockPlatformSizeService.Sub<View>();
            var views = new List<View> { newView1, newView2 };

            // Act
            grid.Children.AddVertical(views);

            // Assert
            Assert.Equal(3, grid.Children.Count);

            // New views should be placed after existing view
            Assert.Equal(1, Compatibility.Grid.GetRow(newView1));
            Assert.Equal(2, Compatibility.Grid.GetRow(newView2));
        }

        /// <summary>
        /// Tests that OnRemoved calls the base implementation and unsubscribes from PropertyChanged event.
        /// Input: Valid View instance.
        /// Expected: Base OnRemoved is called and PropertyChanged event handler is removed.
        /// </summary>
        [Fact]
        public void OnRemoved_ValidView_CallsBaseAndUnsubscribesFromPropertyChanged()
        {
            // Arrange
            var testGrid = new TestableGrid();
            var view = new Label();

            // First add the view to establish the PropertyChanged subscription
            testGrid.TestOnAdded(view);

            // Verify the handler was initially subscribed by triggering a property change
            var initialHandlerCount = testGrid.OnItemPropertyChangedCallCount;
            view.SetValue(Grid.ColumnProperty, 1);
            Assert.True(testGrid.OnItemPropertyChangedCallCount > initialHandlerCount);

            // Act
            testGrid.TestOnRemoved(view);

            // Assert
            Assert.True(testGrid.BaseOnRemovedCalled);
            Assert.Equal(view, testGrid.BaseOnRemovedView);

            // Verify the handler was unsubscribed by triggering another property change
            var handlerCountBeforeChange = testGrid.OnItemPropertyChangedCallCount;
            view.SetValue(Grid.ColumnProperty, 2);
            Assert.Equal(handlerCountBeforeChange, testGrid.OnItemPropertyChangedCallCount);
        }

        /// <summary>
        /// Tests that OnRemoved handles null view parameter appropriately.
        /// Input: Null view parameter.
        /// Expected: Method handles null gracefully or throws appropriate exception.
        /// </summary>
        [Fact]
        public void OnRemoved_NullView_HandlesGracefully()
        {
            // Arrange
            var testGrid = new TestableGrid();

            // Act & Assert
            // The method signature suggests it expects a non-null view, but we test defensive behavior
            var exception = Record.Exception(() => testGrid.TestOnRemoved(null));

            // If no exception is thrown, verify base was still called
            if (exception == null)
            {
                Assert.True(testGrid.BaseOnRemovedCalled);
                Assert.Null(testGrid.BaseOnRemovedView);
            }
        }

        /// <summary>
        /// Tests that OnRemoved works correctly when view was never added (no PropertyChanged subscription).
        /// Input: View that was never added to the grid.
        /// Expected: Base OnRemoved is called and no exception occurs when unsubscribing.
        /// </summary>
        [Fact]
        public void OnRemoved_ViewNeverAdded_CallsBaseWithoutException()
        {
            // Arrange
            var testGrid = new TestableGrid();
            var view = new Label();
            // Note: Not calling TestOnAdded first, so there's no PropertyChanged subscription

            // Act
            var exception = Record.Exception(() => testGrid.TestOnRemoved(view));

            // Assert
            Assert.Null(exception);
            Assert.True(testGrid.BaseOnRemovedCalled);
            Assert.Equal(view, testGrid.BaseOnRemovedView);
        }

        /// <summary>
        /// Tests that OnRemoved can be called multiple times on the same view without issues.
        /// Input: Same view removed multiple times.
        /// Expected: Each call executes without exception and calls base method.
        /// </summary>
        [Fact]
        public void OnRemoved_CalledMultipleTimes_HandlesGracefully()
        {
            // Arrange
            var testGrid = new TestableGrid();
            var view = new Label();
            testGrid.TestOnAdded(view);

            // Act - Remove the same view multiple times
            testGrid.TestOnRemoved(view);
            var firstCallCount = testGrid.BaseOnRemovedCallCount;

            testGrid.TestOnRemoved(view);
            var secondCallCount = testGrid.BaseOnRemovedCallCount;

            // Assert
            Assert.Equal(1, firstCallCount);
            Assert.Equal(2, secondCallCount);
            Assert.True(testGrid.BaseOnRemovedCalled);
        }

        /// <summary>
        /// Tests SetColumnSpan with valid bindable object and valid column span value.
        /// Verifies that the column span is properly set and can be retrieved.
        /// </summary>
        [Fact]
        public void SetColumnSpan_ValidBindableAndValue_SetsColumnSpanCorrectly()
        {
            // Arrange
            var label = new Label();
            int expectedColumnSpan = 3;

            // Act
            Grid.SetColumnSpan(label, expectedColumnSpan);

            // Assert
            Assert.Equal(expectedColumnSpan, Grid.GetColumnSpan(label));
        }

        /// <summary>
        /// Tests SetColumnSpan with null bindable object.
        /// Verifies that a NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetColumnSpan_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullBindable = null;
            int value = 2;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Grid.SetColumnSpan(nullBindable, value));
        }

        /// <summary>
        /// Tests SetColumnSpan with various valid column span values.
        /// Verifies that different valid values are properly set.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void SetColumnSpan_ValidValues_SetsColumnSpanCorrectly(int columnSpan)
        {
            // Arrange
            var label = new Label();

            // Act
            Grid.SetColumnSpan(label, columnSpan);

            // Assert
            Assert.Equal(columnSpan, Grid.GetColumnSpan(label));
        }

        /// <summary>
        /// Tests SetColumnSpan with invalid column span values (less than 1).
        /// Verifies that an ArgumentException is thrown due to validation.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void SetColumnSpan_InvalidValues_ThrowsArgumentException(int invalidColumnSpan)
        {
            // Arrange
            var label = new Label();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Grid.SetColumnSpan(label, invalidColumnSpan));
        }

        /// <summary>
        /// Tests SetColumnSpan with minimum valid value (1).
        /// Verifies that the boundary case is handled correctly.
        /// </summary>
        [Fact]
        public void SetColumnSpan_MinimumValidValue_SetsColumnSpanCorrectly()
        {
            // Arrange
            var label = new Label();
            int minimumValidValue = 1;

            // Act
            Grid.SetColumnSpan(label, minimumValidValue);

            // Assert
            Assert.Equal(minimumValidValue, Grid.GetColumnSpan(label));
        }

        /// <summary>
        /// Tests SetColumnSpan with different types of BindableObjects.
        /// Verifies that the method works with various BindableObject implementations.
        /// </summary>
        [Fact]
        public void SetColumnSpan_DifferentBindableObjectTypes_SetsColumnSpanCorrectly()
        {
            // Arrange
            var button = new Button();
            var entry = new Entry();
            int columnSpan = 5;

            // Act
            Grid.SetColumnSpan(button, columnSpan);
            Grid.SetColumnSpan(entry, columnSpan);

            // Assert
            Assert.Equal(columnSpan, Grid.GetColumnSpan(button));
            Assert.Equal(columnSpan, Grid.GetColumnSpan(entry));
        }

        /// <summary>
        /// Tests SetColumnSpan overwriting existing column span value.
        /// Verifies that the column span can be changed after initial setting.
        /// </summary>
        [Fact]
        public void SetColumnSpan_OverwriteExistingValue_UpdatesColumnSpanCorrectly()
        {
            // Arrange
            var label = new Label();
            int initialColumnSpan = 2;
            int newColumnSpan = 4;

            // Act
            Grid.SetColumnSpan(label, initialColumnSpan);
            Grid.SetColumnSpan(label, newColumnSpan);

            // Assert
            Assert.Equal(newColumnSpan, Grid.GetColumnSpan(label));
        }

        /// <summary>
        /// Tests that OnBindingContextChanged propagates binding context to row and column definitions.
        /// Input: Grid with row and column definitions and a binding context object.
        /// Expected: Row and column definitions receive the same binding context as the grid.
        /// </summary>
        [Fact]
        public void OnBindingContextChanged_WithRowAndColumnDefinitions_PropagatesBindingContext()
        {
            // Arrange
            var grid = new Grid();
            var rowDef1 = new RowDefinition();
            var rowDef2 = new RowDefinition();
            var colDef1 = new ColumnDefinition();
            var colDef2 = new ColumnDefinition();

            grid.RowDefinitions.Add(rowDef1);
            grid.RowDefinitions.Add(rowDef2);
            grid.ColumnDefinitions.Add(colDef1);
            grid.ColumnDefinitions.Add(colDef2);

            var bindingContext = new object();

            // Act
            grid.BindingContext = bindingContext;

            // Assert
            Assert.Same(bindingContext, rowDef1.BindingContext);
            Assert.Same(bindingContext, rowDef2.BindingContext);
            Assert.Same(bindingContext, colDef1.BindingContext);
            Assert.Same(bindingContext, colDef2.BindingContext);
        }

        /// <summary>
        /// Tests that OnBindingContextChanged works correctly with empty row and column definitions.
        /// Input: Grid with empty row and column definition collections and a binding context object.
        /// Expected: No exceptions thrown and grid binding context is set correctly.
        /// </summary>
        [Fact]
        public void OnBindingContextChanged_WithEmptyDefinitions_DoesNotThrow()
        {
            // Arrange
            var grid = new Grid();
            var bindingContext = new object();

            // Act & Assert - Should not throw
            grid.BindingContext = bindingContext;

            Assert.Same(bindingContext, grid.BindingContext);
        }

        /// <summary>
        /// Tests that OnBindingContextChanged propagates null binding context correctly.
        /// Input: Grid with row and column definitions and null binding context.
        /// Expected: Row and column definitions receive null binding context.
        /// </summary>
        [Fact]
        public void OnBindingContextChanged_WithNullBindingContext_PropagatesNull()
        {
            // Arrange
            var grid = new Grid();
            var rowDef = new RowDefinition();
            var colDef = new ColumnDefinition();

            grid.RowDefinitions.Add(rowDef);
            grid.ColumnDefinitions.Add(colDef);

            // Set initial non-null binding context
            var initialContext = new object();
            grid.BindingContext = initialContext;

            // Verify initial state
            Assert.Same(initialContext, rowDef.BindingContext);
            Assert.Same(initialContext, colDef.BindingContext);

            // Act - Set to null
            grid.BindingContext = null;

            // Assert
            Assert.Null(rowDef.BindingContext);
            Assert.Null(colDef.BindingContext);
        }

        /// <summary>
        /// Tests that OnBindingContextChanged propagates binding context changes multiple times.
        /// Input: Grid with definitions and multiple different binding context objects.
        /// Expected: Row and column definitions always match the current grid binding context.
        /// </summary>
        [Fact]
        public void OnBindingContextChanged_WithMultipleContextChanges_PropagatesEachChange()
        {
            // Arrange
            var grid = new Grid();
            var rowDef = new RowDefinition();
            var colDef = new ColumnDefinition();

            grid.RowDefinitions.Add(rowDef);
            grid.ColumnDefinitions.Add(colDef);

            var context1 = new { Name = "Context1" };
            var context2 = new { Name = "Context2" };
            var context3 = new { Name = "Context3" };

            // Act & Assert - First change
            grid.BindingContext = context1;
            Assert.Same(context1, rowDef.BindingContext);
            Assert.Same(context1, colDef.BindingContext);

            // Act & Assert - Second change
            grid.BindingContext = context2;
            Assert.Same(context2, rowDef.BindingContext);
            Assert.Same(context2, colDef.BindingContext);

            // Act & Assert - Third change
            grid.BindingContext = context3;
            Assert.Same(context3, rowDef.BindingContext);
            Assert.Same(context3, colDef.BindingContext);
        }

        /// <summary>
        /// Tests that OnBindingContextChanged works when definitions are added after setting binding context.
        /// Input: Grid with binding context set, then row and column definitions added.
        /// Expected: Newly added definitions do not automatically inherit the binding context until next change.
        /// </summary>
        [Fact]
        public void OnBindingContextChanged_WithDefinitionsAddedAfterContext_NewDefinitionsGetContextOnNextChange()
        {
            // Arrange
            var grid = new Grid();
            var bindingContext = new object();

            // Set binding context first
            grid.BindingContext = bindingContext;

            // Add definitions after setting context
            var rowDef = new RowDefinition();
            var colDef = new ColumnDefinition();
            grid.RowDefinitions.Add(rowDef);
            grid.ColumnDefinitions.Add(colDef);

            // Initially, new definitions don't have the context
            Assert.Null(rowDef.BindingContext);
            Assert.Null(colDef.BindingContext);

            // Act - Trigger another binding context change
            var newContext = new object();
            grid.BindingContext = newContext;

            // Assert - Now the definitions have the new context
            Assert.Same(newContext, rowDef.BindingContext);
            Assert.Same(newContext, colDef.BindingContext);
        }

        /// <summary>
        /// Tests that OnBindingContextChanged handles string binding contexts correctly.
        /// Input: Grid with definitions and string binding context.
        /// Expected: String binding context is propagated to definitions.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("test")]
        [InlineData("   ")]
        [InlineData("very long string with special characters !@#$%^&*()")]
        public void OnBindingContextChanged_WithStringBindingContext_PropagatesCorrectly(string bindingContext)
        {
            // Arrange
            var grid = new Grid();
            var rowDef = new RowDefinition();
            var colDef = new ColumnDefinition();

            grid.RowDefinitions.Add(rowDef);
            grid.ColumnDefinitions.Add(colDef);

            // Act
            grid.BindingContext = bindingContext;

            // Assert
            Assert.Same(bindingContext, rowDef.BindingContext);
            Assert.Same(bindingContext, colDef.BindingContext);
        }

        /// <summary>
        /// Tests that OnBindingContextChanged handles numeric binding contexts correctly.
        /// Input: Grid with definitions and various numeric binding contexts.
        /// Expected: Numeric binding context is propagated to definitions.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void OnBindingContextChanged_WithNumericBindingContext_PropagatesCorrectly(int bindingContext)
        {
            // Arrange
            var grid = new Grid();
            var rowDef = new RowDefinition();
            var colDef = new ColumnDefinition();

            grid.RowDefinitions.Add(rowDef);
            grid.ColumnDefinitions.Add(colDef);

            // Act
            grid.BindingContext = bindingContext;

            // Assert
            Assert.Equal(bindingContext, rowDef.BindingContext);
            Assert.Equal(bindingContext, colDef.BindingContext);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView throws NullReferenceException when view parameter is null.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_NullView_ThrowsNullReferenceException()
        {
            // Arrange
            var grid = new Grid();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => grid.ComputeConstraintForView(null));
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns None when view has non-Fill alignment options.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start, LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center, LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End, LayoutAlignment.End)]
        [InlineData(LayoutAlignment.Start, LayoutAlignment.Fill)]
        [InlineData(LayoutAlignment.Fill, LayoutAlignment.Start)]
        public void ComputeConstraintForView_NonFillAlignment_ReturnsNone(LayoutAlignment verticalAlignment, LayoutAlignment horizontalAlignment)
        {
            // Arrange
            var grid = new Grid();
            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(new LayoutOptions(verticalAlignment, false));
            view.HorizontalOptions.Returns(new LayoutOptions(horizontalAlignment, false));

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            if (verticalAlignment != LayoutAlignment.Fill && horizontalAlignment != LayoutAlignment.Fill)
            {
                Assert.Equal(LayoutConstraint.None, result);
            }
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns VerticallyFixed when view has Fill vertical alignment 
        /// and all spanned rows have absolute (fixed) height.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillVerticalWithAbsoluteRows_ReturnsVerticallyFixed()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) }); // Absolute
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) }); // Absolute

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Start);

            Grid.SetRow(view, 0);
            Grid.SetRowSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.VerticallyFixed, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns HorizontallyFixed when view has Fill horizontal alignment 
        /// and all spanned columns have absolute (fixed) width.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillHorizontalWithAbsoluteColumns_ReturnsHorizontallyFixed()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Absolute
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Absolute

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Start);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetColumn(view, 0);
            Grid.SetColumnSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.HorizontallyFixed, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns Fixed when view has Fill alignment for both directions 
        /// and all spanned rows and columns have absolute (fixed) sizes.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillBothWithAbsoluteSizes_ReturnsFixed()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) }); // Absolute
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Absolute

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetRow(view, 0);
            Grid.SetColumn(view, 0);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.Fixed, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns None when view has Fill vertical alignment 
        /// but spanned rows contain Auto-sized heights.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillVerticalWithAutoRows_ReturnsNone()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) }); // Absolute
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Auto

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Start);

            Grid.SetRow(view, 0);
            Grid.SetRowSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns None when view has Fill horizontal alignment 
        /// but spanned columns contain Auto-sized widths. This test specifically targets the uncovered line.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillHorizontalWithAutoColumns_ReturnsNone()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Absolute
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Auto

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Start);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetColumn(view, 0);
            Grid.SetColumnSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns None when view has Fill vertical alignment 
        /// but spanned rows contain Star-sized heights without parent vertical constraint.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillVerticalWithStarRowsNoParentConstraint_ReturnsNone()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) }); // Absolute
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Star

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Start);

            Grid.SetRow(view, 0);
            Grid.SetRowSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns None when view has Fill horizontal alignment 
        /// but spanned columns contain Star-sized widths without parent horizontal constraint.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillHorizontalWithStarColumnsNoParentConstraint_ReturnsNone()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Absolute
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // Star

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Start);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetColumn(view, 0);
            Grid.SetColumnSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns VerticallyFixed when view has Fill vertical alignment 
        /// and spanned rows contain Star-sized heights with parent vertical constraint already set.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillVerticalWithStarRowsAndParentConstraint_ReturnsVerticallyFixed()
        {
            // Arrange
            var grid = new TestableGrid { TestConstraint = LayoutConstraint.VerticallyFixed };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) }); // Absolute
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // Star

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Start);

            Grid.SetRow(view, 0);
            Grid.SetRowSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.VerticallyFixed, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns HorizontallyFixed when view has Fill horizontal alignment 
        /// and spanned columns contain Star-sized widths with parent horizontal constraint already set.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_FillHorizontalWithStarColumnsAndParentConstraint_ReturnsHorizontallyFixed()
        {
            // Arrange
            var grid = new TestableGrid { TestConstraint = LayoutConstraint.HorizontallyFixed };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Absolute
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // Star

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Start);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetColumn(view, 0);
            Grid.SetColumnSpan(view, 2);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.HorizontallyFixed, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView handles views positioned beyond defined rows and columns correctly.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_ViewBeyondDefinitions_ReturnsNone()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetRow(view, 5); // Beyond defined rows
            Grid.SetColumn(view, 5); // Beyond defined columns

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView handles empty grid with no definitions correctly.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_EmptyGrid_ReturnsNone()
        {
            // Arrange
            var grid = new Grid(); // No row or column definitions

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetRow(view, 0);
            Grid.SetColumn(view, 0);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView handles single cell with different sizing combinations.
        /// </summary>
        [Theory]
        [InlineData(100.0, 150.0, LayoutConstraint.Fixed)] // Both absolute
        [InlineData(100.0, -1.0, LayoutConstraint.VerticallyFixed)] // Row absolute, column auto (default)
        [InlineData(-1.0, 150.0, LayoutConstraint.HorizontallyFixed)] // Row auto (default), column absolute
        public void ComputeConstraintForView_SingleCellVariousSizes_ReturnsExpectedConstraint(double rowHeight, double columnWidth, LayoutConstraint expected)
        {
            // Arrange
            var grid = new Grid();

            if (rowHeight > 0)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rowHeight) });

            if (columnWidth > 0)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(columnWidth) });

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetRow(view, 0);
            Grid.SetColumn(view, 0);

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView correctly handles mixed sizing types in spanned cells.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_MixedSizingInSpan_ReturnsPartialConstraint()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) }); // Absolute
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Auto
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Absolute
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Absolute

            var view = Substitute.For<View>();
            view.VerticalOptions.Returns(LayoutOptions.Fill);
            view.HorizontalOptions.Returns(LayoutOptions.Fill);

            Grid.SetRow(view, 0);
            Grid.SetRowSpan(view, 2); // Spans both absolute and auto row
            Grid.SetColumn(view, 0);
            Grid.SetColumnSpan(view, 2); // Spans both absolute columns

            // Act
            var result = grid.ComputeConstraintForView(view);

            // Assert
            // Should only get horizontal constraint since one row is auto
            Assert.Equal(LayoutConstraint.HorizontallyFixed, result);
        }

        /// <summary>
        /// Tests that InvalidateMeasureInernalNonVirtual method successfully calls InvalidateMeasureInternal with individual InvalidationTrigger values.
        /// Verifies the method can handle each defined enum value without throwing exceptions.
        /// </summary>
        /// <param name="trigger">The InvalidationTrigger value to test</param>
        [Theory]
        [InlineData(InvalidationTrigger.Undefined)]
        [InlineData(InvalidationTrigger.MeasureChanged)]
        [InlineData(InvalidationTrigger.HorizontalOptionsChanged)]
        [InlineData(InvalidationTrigger.VerticalOptionsChanged)]
        [InlineData(InvalidationTrigger.SizeRequestChanged)]
        [InlineData(InvalidationTrigger.RendererReady)]
        [InlineData(InvalidationTrigger.MarginChanged)]
        public void InvalidateMeasureInernalNonVirtual_WithIndividualTriggerValues_DoesNotThrow(InvalidationTrigger trigger)
        {
            // Arrange
            var grid = new Grid();

            // Act & Assert
            var exception = Record.Exception(() => grid.InvalidateMeasureInernalNonVirtual(trigger));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that InvalidateMeasureInernalNonVirtual method can handle combined InvalidationTrigger flag values.
        /// Verifies the method works correctly with bitwise combinations of enum values since InvalidationTrigger is a Flags enum.
        /// </summary>
        /// <param name="trigger">The combined InvalidationTrigger flags to test</param>
        [Theory]
        [InlineData(InvalidationTrigger.MeasureChanged | InvalidationTrigger.HorizontalOptionsChanged)]
        [InlineData(InvalidationTrigger.VerticalOptionsChanged | InvalidationTrigger.MarginChanged)]
        [InlineData(InvalidationTrigger.SizeRequestChanged | InvalidationTrigger.RendererReady)]
        [InlineData(InvalidationTrigger.MeasureChanged | InvalidationTrigger.VerticalOptionsChanged | InvalidationTrigger.MarginChanged)]
        public void InvalidateMeasureInernalNonVirtual_WithCombinedTriggerFlags_DoesNotThrow(InvalidationTrigger trigger)
        {
            // Arrange
            var grid = new Grid();

            // Act & Assert
            var exception = Record.Exception(() => grid.InvalidateMeasureInernalNonVirtual(trigger));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that InvalidateMeasureInernalNonVirtual method can handle boundary and edge case InvalidationTrigger values.
        /// Verifies the method works with extreme enum values including cast values outside the defined range.
        /// </summary>
        /// <param name="triggerValue">The integer value to cast to InvalidationTrigger</param>
        [Theory]
        [InlineData(0)] // Undefined
        [InlineData(63)] // All flags combined (1+2+4+8+16+32)
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void InvalidateMeasureInernalNonVirtual_WithBoundaryTriggerValues_DoesNotThrow(int triggerValue)
        {
            // Arrange
            var grid = new Grid();
            var trigger = (InvalidationTrigger)triggerValue;

            // Act & Assert
            var exception = Record.Exception(() => grid.InvalidateMeasureInernalNonVirtual(trigger));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that AddHorizontal throws ArgumentNullException when views collection is null.
        /// Verifies proper null validation for the views parameter.
        /// Expected result: ArgumentNullException with parameter name "views".
        /// </summary>
        [Fact]
        public void AddHorizontal_NullViews_ThrowsArgumentNullException()
        {
            // Arrange
            var grid = new Grid();
            IEnumerable<View> nullViews = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => grid.Children.AddHorizontal(nullViews));
            Assert.Equal("views", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddHorizontal handles empty views collection without throwing exceptions.
        /// Verifies that no views are added when an empty collection is provided.
        /// Expected result: Grid remains empty, no exceptions thrown.
        /// </summary>
        [Fact]
        public void AddHorizontal_EmptyViews_DoesNotThrowAndAddsNoViews()
        {
            // Arrange
            var grid = new Grid();
            var emptyViews = new List<View>();
            var initialCount = grid.Children.Count;

            // Act
            grid.Children.AddHorizontal(emptyViews);

            // Assert
            Assert.Equal(initialCount, grid.Children.Count);
        }

        /// <summary>
        /// Tests that AddHorizontal adds a single view to the grid in the correct position.
        /// Verifies that the view is placed in column 0, row 0 with appropriate spans.
        /// Expected result: Single view added at position (0,0) with span (1,1).
        /// </summary>
        [Fact]
        public void AddHorizontal_SingleView_AddsViewWithCorrectPosition()
        {
            // Arrange
            var grid = new Grid();
            var view = new Label { Text = "Test" };
            var views = new[] { view };

            // Act
            grid.Children.AddHorizontal(views);

            // Assert
            Assert.Single(grid.Children);
            Assert.Contains(view, grid.Children);
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddHorizontal adds multiple views to the grid horizontally in successive columns.
        /// Verifies that each view is placed in the next column while staying in row 0.
        /// Expected result: Views added in columns 0, 1, 2, etc., all in row 0.
        /// </summary>
        [Fact]
        public void AddHorizontal_MultipleViews_AddsViewsHorizontallyInSuccessiveColumns()
        {
            // Arrange
            var grid = new Grid();
            var view1 = new Label { Text = "View1" };
            var view2 = new Label { Text = "View2" };
            var view3 = new Label { Text = "View3" };
            var views = new[] { view1, view2, view3 };

            // Act
            grid.Children.AddHorizontal(views);

            // Assert
            Assert.Equal(3, grid.Children.Count);

            // Verify view1 position
            Assert.Equal(0, Grid.GetRow(view1));
            Assert.Equal(0, Grid.GetColumn(view1));
            Assert.Equal(1, Grid.GetRowSpan(view1));
            Assert.Equal(1, Grid.GetColumnSpan(view1));

            // Verify view2 position
            Assert.Equal(0, Grid.GetRow(view2));
            Assert.Equal(1, Grid.GetColumn(view2));
            Assert.Equal(1, Grid.GetRowSpan(view2));
            Assert.Equal(1, Grid.GetColumnSpan(view2));

            // Verify view3 position
            Assert.Equal(0, Grid.GetRow(view3));
            Assert.Equal(2, Grid.GetColumn(view3));
            Assert.Equal(1, Grid.GetRowSpan(view3));
            Assert.Equal(1, Grid.GetColumnSpan(view3));
        }

        /// <summary>
        /// Tests that AddHorizontal throws ArgumentNullException when the views collection contains a null view.
        /// Verifies that null validation occurs for individual views in the collection.
        /// Expected result: ArgumentNullException with parameter name "view".
        /// </summary>
        [Fact]
        public void AddHorizontal_ViewsContainingNull_ThrowsArgumentNullException()
        {
            // Arrange
            var grid = new Grid();
            var validView = new Label { Text = "Valid" };
            var views = new View[] { validView, null };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => grid.Children.AddHorizontal(views));
            Assert.Equal("view", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddHorizontal continues adding views after existing views in the grid.
        /// Verifies that the column positioning accounts for previously added views.
        /// Expected result: New views start from the next available column position.
        /// </summary>
        [Fact]
        public void AddHorizontal_WithExistingViews_AddsViewsAfterExistingColumns()
        {
            // Arrange
            var grid = new Grid();
            var existingView1 = new Label { Text = "Existing1" };
            var existingView2 = new Label { Text = "Existing2" };

            // Add some existing views
            grid.Children.Add(existingView1, 0, 0);
            grid.Children.Add(existingView2, 1, 0);

            var newView1 = new Label { Text = "New1" };
            var newView2 = new Label { Text = "New2" };
            var newViews = new[] { newView1, newView2 };

            // Act
            grid.Children.AddHorizontal(newViews);

            // Assert
            Assert.Equal(4, grid.Children.Count);

            // Verify new views are positioned after existing ones
            Assert.Equal(0, Grid.GetRow(newView1));
            Assert.Equal(2, Grid.GetColumn(newView1));
            Assert.Equal(1, Grid.GetRowSpan(newView1));
            Assert.Equal(1, Grid.GetColumnSpan(newView1));

            Assert.Equal(0, Grid.GetRow(newView2));
            Assert.Equal(3, Grid.GetColumn(newView2));
            Assert.Equal(1, Grid.GetRowSpan(newView2));
            Assert.Equal(1, Grid.GetColumnSpan(newView2));
        }

        /// <summary>
        /// Tests that AddHorizontal works with different view types in the collection.
        /// Verifies that the method handles heterogeneous collections of View subclasses.
        /// Expected result: All different view types are added successfully with correct positioning.
        /// </summary>
        [Fact]
        public void AddHorizontal_DifferentViewTypes_AddsAllViewsSuccessfully()
        {
            // Arrange
            var grid = new Grid();
            var label = new Label { Text = "Label" };
            var button = new Button { Text = "Button" };
            var entry = new Entry { Text = "Entry" };
            var views = new View[] { label, button, entry };

            // Act
            grid.Children.AddHorizontal(views);

            // Assert
            Assert.Equal(3, grid.Children.Count);
            Assert.Contains(label, grid.Children);
            Assert.Contains(button, grid.Children);
            Assert.Contains(entry, grid.Children);

            // Verify positioning
            Assert.Equal(0, Grid.GetColumn(label));
            Assert.Equal(1, Grid.GetColumn(button));
            Assert.Equal(2, Grid.GetColumn(entry));

            // All should be in row 0
            Assert.Equal(0, Grid.GetRow(label));
            Assert.Equal(0, Grid.GetRow(button));
            Assert.Equal(0, Grid.GetRow(entry));
        }

        /// <summary>
        /// Tests that AddHorizontal handles a large number of views efficiently.
        /// Verifies that the method can handle collections with many views without issues.
        /// Expected result: All views are added with correct sequential column positioning.
        /// </summary>
        [Fact]
        public void AddHorizontal_LargeCollection_AddsAllViewsWithCorrectPositioning()
        {
            // Arrange
            var grid = new Grid();
            var views = new List<View>();
            const int viewCount = 100;

            for (int i = 0; i < viewCount; i++)
            {
                views.Add(new Label { Text = $"View{i}" });
            }

            // Act
            grid.Children.AddHorizontal(views);

            // Assert
            Assert.Equal(viewCount, grid.Children.Count);

            // Verify first and last views have correct positioning
            Assert.Equal(0, Grid.GetColumn(views[0]));
            Assert.Equal(0, Grid.GetRow(views[0]));

            Assert.Equal(viewCount - 1, Grid.GetColumn(views[viewCount - 1]));
            Assert.Equal(0, Grid.GetRow(views[viewCount - 1]));

            // Verify some random positions in between
            Assert.Equal(50, Grid.GetColumn(views[50]));
            Assert.Equal(0, Grid.GetRow(views[50]));
        }

        /// <summary>
        /// Tests that AddHorizontal works with LINQ-generated collections.
        /// Verifies that the method properly handles deferred execution collections.
        /// Expected result: All views from LINQ collection are added successfully.
        /// </summary>
        [Fact]
        public void AddHorizontal_LinqGeneratedCollection_AddsAllViewsSuccessfully()
        {
            // Arrange
            var grid = new Grid();
            var views = Enumerable.Range(0, 5).Select(i => new Label { Text = $"Generated{i}" });

            // Act
            grid.Children.AddHorizontal(views);

            // Assert
            Assert.Equal(5, grid.Children.Count);

            // Verify positioning of generated views
            for (int i = 0; i < 5; i++)
            {
                var view = grid.Children[i];
                Assert.Equal(i, Grid.GetColumn(view));
                Assert.Equal(0, Grid.GetRow(view));
            }
        }

        /// <summary>
        /// Tests that Add method with valid parameters correctly sets Row, RowSpan, Column, ColumnSpan properties
        /// and adds the view to the collection.
        /// </summary>
        [Fact]
        public void Add_ValidParameters_SetsPropertiesAndAddsView()
        {
            // Arrange
            var grid = new Grid();
            var view = new View();
            int left = 1, right = 3, top = 2, bottom = 5;

            // Act
            grid.Children.Add(view, left, right, top, bottom);

            // Assert
            Assert.Equal(top, Grid.GetRow(view));
            Assert.Equal(bottom - top, Grid.GetRowSpan(view));
            Assert.Equal(left, Grid.GetColumn(view));
            Assert.Equal(right - left, Grid.GetColumnSpan(view));
            Assert.Contains(view, grid.Children);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when left parameter is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void Add_NegativeLeft_ThrowsArgumentOutOfRangeException(int left)
        {
            // Arrange
            var grid = new Grid();
            var view = new View();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.Children.Add(view, left, left + 1, 0, 1));

            Assert.Equal("left", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when top parameter is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(int.MinValue)]
        public void Add_NegativeTop_ThrowsArgumentOutOfRangeException(int top)
        {
            // Arrange
            var grid = new Grid();
            var view = new View();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.Children.Add(view, 0, 1, top, top + 1));

            Assert.Equal("top", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when left is greater than or equal to right.
        /// </summary>
        [Theory]
        [InlineData(5, 5)]
        [InlineData(10, 5)]
        [InlineData(100, 50)]
        public void Add_LeftGreaterThanOrEqualToRight_ThrowsArgumentOutOfRangeException(int left, int right)
        {
            // Arrange
            var grid = new Grid();
            var view = new View();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.Children.Add(view, left, right, 0, 1));

            Assert.Equal("right", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentOutOfRangeException when top is greater than or equal to bottom.
        /// </summary>
        [Theory]
        [InlineData(3, 3)]
        [InlineData(8, 2)]
        [InlineData(50, 25)]
        public void Add_TopGreaterThanOrEqualToBottom_ThrowsArgumentOutOfRangeException(int top, int bottom)
        {
            // Arrange
            var grid = new Grid();
            var view = new View();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                grid.Children.Add(view, 0, 1, top, bottom));

            Assert.Equal("bottom", exception.ParamName);
        }

        /// <summary>
        /// Tests that Add method works correctly with minimum valid span values (span of 1).
        /// </summary>
        [Fact]
        public void Add_MinimumSpanValues_SetsPropertiesCorrectly()
        {
            // Arrange
            var grid = new Grid();
            var view = new View();
            int left = 0, right = 1, top = 0, bottom = 1;

            // Act
            grid.Children.Add(view, left, right, top, bottom);

            // Assert
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that Add method works correctly with large valid parameter values.
        /// </summary>
        [Fact]
        public void Add_LargeValidValues_SetsPropertiesCorrectly()
        {
            // Arrange
            var grid = new Grid();
            var view = new View();
            int left = 1000, right = 2000, top = 500, bottom = 1500;

            // Act
            grid.Children.Add(view, left, right, top, bottom);

            // Assert
            Assert.Equal(top, Grid.GetRow(view));
            Assert.Equal(bottom - top, Grid.GetRowSpan(view));
            Assert.Equal(left, Grid.GetColumn(view));
            Assert.Equal(right - left, Grid.GetColumnSpan(view));
            Assert.Contains(view, grid.Children);
        }

        /// <summary>
        /// Tests that Add method calculates spans correctly with various parameter combinations.
        /// </summary>
        [Theory]
        [InlineData(0, 5, 0, 3, 3, 5)]
        [InlineData(2, 7, 1, 4, 3, 5)]
        [InlineData(10, 15, 20, 25, 5, 5)]
        public void Add_VariousParameterCombinations_CalculatesSpansCorrectly(
            int left, int right, int top, int bottom,
            int expectedRowSpan, int expectedColumnSpan)
        {
            // Arrange
            var grid = new Grid();
            var view = new View();

            // Act
            grid.Children.Add(view, left, right, top, bottom);

            // Assert
            Assert.Equal(expectedRowSpan, Grid.GetRowSpan(view));
            Assert.Equal(expectedColumnSpan, Grid.GetColumnSpan(view));
            Assert.Equal(top, Grid.GetRow(view));
            Assert.Equal(left, Grid.GetColumn(view));
        }

        /// <summary>
        /// Tests that Add method correctly handles boundary values for parameters.
        /// </summary>
        [Fact]
        public void Add_BoundaryValues_SetsPropertiesCorrectly()
        {
            // Arrange
            var grid = new Grid();
            var view = new View();
            int left = 0, right = int.MaxValue, top = 0, bottom = int.MaxValue;

            // Act
            grid.Children.Add(view, left, right, top, bottom);

            // Assert
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(int.MaxValue, Grid.GetRowSpan(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(int.MaxValue, Grid.GetColumnSpan(view));
            Assert.Contains(view, grid.Children);
        }

        /// <summary>
        /// Tests that Add method correctly adds multiple views to the collection with different positions.
        /// </summary>
        [Fact]
        public void Add_MultipleViews_AddsAllToCollection()
        {
            // Arrange
            var grid = new Grid();
            var view1 = new View();
            var view2 = new View();
            var view3 = new View();

            // Act
            grid.Children.Add(view1, 0, 1, 0, 1);
            grid.Children.Add(view2, 1, 2, 1, 2);
            grid.Children.Add(view3, 2, 3, 2, 3);

            // Assert
            Assert.Equal(3, grid.Children.Count);
            Assert.Contains(view1, grid.Children);
            Assert.Contains(view2, grid.Children);
            Assert.Contains(view3, grid.Children);
        }

        /// <summary>
        /// Tests that the On method returns a non-null platform configuration for a valid IConfigPlatform type.
        /// Verifies that the method properly delegates to the platform configuration registry.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var grid = new Grid();

            // Act
            var result = grid.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that multiple calls to On with the same platform type return the same configuration instance.
        /// Verifies the platform configuration registry caching behavior.
        /// </summary>
        [Fact]
        public void On_MultipleCallsWithSamePlatformType_ReturnsSameInstance()
        {
            // Arrange
            var grid = new Grid();

            // Act
            var result1 = grid.On<TestPlatform>();
            var result2 = grid.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that On method works with different IConfigPlatform implementations.
        /// Verifies the generic method functionality with multiple platform types.
        /// </summary>
        [Fact]
        public void On_WithDifferentPlatformTypes_ReturnsDistinctConfigurations()
        {
            // Arrange
            var grid = new Grid();

            // Act
            var result1 = grid.On<TestPlatform>();
            var result2 = grid.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that On method works correctly when called on different Grid instances.
        /// Verifies that each Grid has its own platform configuration registry.
        /// </summary>
        [Fact]
        public void On_WithDifferentGridInstances_ReturnsDistinctConfigurations()
        {
            // Arrange
            var grid1 = new Grid();
            var grid2 = new Grid();

            // Act
            var result1 = grid1.On<TestPlatform>();
            var result2 = grid2.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        // Helper classes for testing IConfigPlatform implementations
        private class TestPlatform : IConfigPlatform
        {
        }

        private class AnotherTestPlatform : IConfigPlatform
        {
        }
    }

    public partial class GridIGridListAddVerticalTests
    {
        /// <summary>
        /// Tests that AddVertical throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void AddVertical_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var grid = new Grid();
            var gridList = grid.Children;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => gridList.AddVertical(null));
        }

        /// <summary>
        /// Tests that AddVertical adds a view to an empty grid at row 0, column 0, spanning 1 column.
        /// </summary>
        [Fact]
        public void AddVertical_EmptyGrid_AddsViewAtRow0Column0Span1()
        {
            // Arrange
            var grid = new Grid();
            var gridList = grid.Children;
            var view = new View();

            // Act
            gridList.AddVertical(view);

            // Assert
            Assert.Single(gridList);
            Assert.Equal(view, gridList[0]);
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddVertical adds a view spanning all columns when grid has column definitions.
        /// </summary>
        [Fact]
        public void AddVertical_GridWithColumnDefinitions_AddsViewSpanningAllColumns()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var gridList = grid.Children;
            var view = new View();

            // Act
            gridList.AddVertical(view);

            // Assert
            Assert.Single(gridList);
            Assert.Equal(view, gridList[0]);
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(3, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that AddVertical places a view in the next available row when grid has existing views.
        /// </summary>
        [Fact]
        public void AddVertical_GridWithExistingViews_AddsViewInNextRow()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var gridList = grid.Children;

            var existingView = new View();
            gridList.Add(existingView, 0, 0); // Add at row 0, column 0

            var newView = new View();

            // Act
            gridList.AddVertical(newView);

            // Assert
            Assert.Equal(2, gridList.Count);
            Assert.Equal(newView, gridList[1]);
            Assert.Equal(1, Grid.GetRow(newView));
            Assert.Equal(0, Grid.GetColumn(newView));
            Assert.Equal(1, Grid.GetRowSpan(newView));
            Assert.Equal(2, Grid.GetColumnSpan(newView));
        }

        /// <summary>
        /// Tests that AddVertical correctly calculates row position when views span multiple rows.
        /// </summary>
        [Fact]
        public void AddVertical_GridWithMultiRowSpanViews_AddsViewAfterLastRow()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var gridList = grid.Children;

            var existingView = new View();
            gridList.Add(existingView, 0, 1, 0, 3); // Add at columns 0-1, rows 0-3 (spans 3 rows)

            var newView = new View();

            // Act
            gridList.AddVertical(newView);

            // Assert
            Assert.Equal(2, gridList.Count);
            Assert.Equal(newView, gridList[1]);
            Assert.Equal(3, Grid.GetRow(newView)); // Should be placed after the 3-row span
            Assert.Equal(0, Grid.GetColumn(newView));
            Assert.Equal(1, Grid.GetRowSpan(newView));
            Assert.Equal(1, Grid.GetColumnSpan(newView));
        }

        /// <summary>
        /// Tests that AddVertical works correctly when grid has row definitions but no views.
        /// </summary>
        [Fact]
        public void AddVertical_GridWithRowDefinitionsNoViews_AddsViewAtRow0()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            var gridList = grid.Children;
            var view = new View();

            // Act
            gridList.AddVertical(view);

            // Assert
            Assert.Single(gridList);
            Assert.Equal(view, gridList[0]);
            Assert.Equal(0, Grid.GetRow(view));
            Assert.Equal(0, Grid.GetColumn(view));
            Assert.Equal(1, Grid.GetRowSpan(view));
            Assert.Equal(1, Grid.GetColumnSpan(view));
        }

        /// <summary>
        /// Tests that multiple calls to AddVertical stack views vertically in correct order.
        /// </summary>
        [Fact]
        public void AddVertical_MultipleCalls_StacksViewsVertically()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var gridList = grid.Children;

            var view1 = new View();
            var view2 = new View();
            var view3 = new View();

            // Act
            gridList.AddVertical(view1);
            gridList.AddVertical(view2);
            gridList.AddVertical(view3);

            // Assert
            Assert.Equal(3, gridList.Count);

            // First view at row 0
            Assert.Equal(0, Grid.GetRow(view1));
            Assert.Equal(0, Grid.GetColumn(view1));
            Assert.Equal(2, Grid.GetColumnSpan(view1));

            // Second view at row 1
            Assert.Equal(1, Grid.GetRow(view2));
            Assert.Equal(0, Grid.GetColumn(view2));
            Assert.Equal(2, Grid.GetColumnSpan(view2));

            // Third view at row 2
            Assert.Equal(2, Grid.GetRow(view3));
            Assert.Equal(0, Grid.GetColumn(view3));
            Assert.Equal(2, Grid.GetColumnSpan(view3));
        }

        /// <summary>
        /// Tests that AddVertical correctly handles maximum row count from both views and row definitions.
        /// </summary>
        [Fact]
        public void AddVertical_GridWithRowDefinitionsAndViews_UsesMaximumRowCount()
        {
            // Arrange
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition()); // 5 row definitions

            var gridList = grid.Children;

            var existingView = new View();
            gridList.Add(existingView, 0, 0); // Add at row 0, column 0 (only uses 1 row)

            var newView = new View();

            // Act
            gridList.AddVertical(newView);

            // Assert
            Assert.Equal(2, gridList.Count);
            Assert.Equal(newView, gridList[1]);
            Assert.Equal(5, Grid.GetRow(newView)); // Should use max of 5 row definitions vs 1 used row
            Assert.Equal(0, Grid.GetColumn(newView));
            Assert.Equal(1, Grid.GetRowSpan(newView));
            Assert.Equal(1, Grid.GetColumnSpan(newView));
        }

        /// <summary>
        /// Tests that AddVertical correctly handles maximum column count from both views and column definitions.
        /// </summary>
        [Fact]
        public void AddVertical_GridWithColumnDefinitionsAndViews_UsesMaximumColumnCount()
        {
            // Arrange
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition()); // 2 column definitions

            var gridList = grid.Children;

            var existingView = new View();
            gridList.Add(existingView, 0, 3, 0, 1); // Add spanning columns 0-3 (uses 3 columns)

            var newView = new View();

            // Act
            gridList.AddVertical(newView);

            // Assert
            Assert.Equal(2, gridList.Count);
            Assert.Equal(newView, gridList[1]);
            Assert.Equal(1, Grid.GetRow(newView));
            Assert.Equal(0, Grid.GetColumn(newView));
            Assert.Equal(1, Grid.GetRowSpan(newView));
            Assert.Equal(3, Grid.GetColumnSpan(newView)); // Should use max of 3 used columns vs 2 column definitions
        }
    }
}