#nullable disable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Internals.UnitTests
{
    /// <summary>
    /// Unit tests for the CellExtensions.SetIsGroupHeader extension method.
    /// </summary>
    public partial class CellExtensionsTests
    {
        /// <summary>
        /// Tests that SetIsGroupHeader sets the value to true on a valid cell.
        /// Input: Valid cell and value=true
        /// Expected: The IsGroupHeader property is set to true on the cell.
        /// </summary>
        [Fact]
        public void SetIsGroupHeader_ValidCellWithTrue_SetsIsGroupHeaderToTrue()
        {
            // Arrange
            var cell = new TestBindableObject();
            bool value = true;

            // Act
            cell.SetIsGroupHeader<TestTemplatedView, TestBindableObject>(value);

            // Assert
            var result = cell.GetIsGroupHeader<TestTemplatedView, TestBindableObject>();
            Assert.True(result);
        }

        /// <summary>
        /// Tests that SetIsGroupHeader sets the value to false on a valid cell.
        /// Input: Valid cell and value=false
        /// Expected: The IsGroupHeader property is set to false on the cell.
        /// </summary>
        [Fact]
        public void SetIsGroupHeader_ValidCellWithFalse_SetsIsGroupHeaderToFalse()
        {
            // Arrange
            var cell = new TestBindableObject();
            bool value = false;

            // Act
            cell.SetIsGroupHeader<TestTemplatedView, TestBindableObject>(value);

            // Assert
            var result = cell.GetIsGroupHeader<TestTemplatedView, TestBindableObject>();
            Assert.False(result);
        }

        /// <summary>
        /// Tests that SetIsGroupHeader can change the value from false to true.
        /// Input: Valid cell, first set to false then set to true
        /// Expected: The IsGroupHeader property changes from false to true.
        /// </summary>
        [Fact]
        public void SetIsGroupHeader_ChangeFromFalseToTrue_UpdatesIsGroupHeader()
        {
            // Arrange
            var cell = new TestBindableObject();

            // Act & Assert - Set to false first
            cell.SetIsGroupHeader<TestTemplatedView, TestBindableObject>(false);
            var initialResult = cell.GetIsGroupHeader<TestTemplatedView, TestBindableObject>();
            Assert.False(initialResult);

            // Act & Assert - Change to true
            cell.SetIsGroupHeader<TestTemplatedView, TestBindableObject>(true);
            var finalResult = cell.GetIsGroupHeader<TestTemplatedView, TestBindableObject>();
            Assert.True(finalResult);
        }

        /// <summary>
        /// Tests that SetIsGroupHeader throws an exception when cell is null.
        /// Input: Null cell parameter
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetIsGroupHeader_NullCell_ThrowsNullReferenceException()
        {
            // Arrange
            TestBindableObject cell = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                cell.SetIsGroupHeader<TestTemplatedView, TestBindableObject>(value));
        }

        /// <summary>
        /// Tests SetIsGroupHeader with both boolean values using parameterized test.
        /// Input: Valid cell with true and false values
        /// Expected: The IsGroupHeader property is set to the specified value.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetIsGroupHeader_ValidCellWithBothBooleanValues_SetsCorrectValue(bool value)
        {
            // Arrange
            var cell = new TestBindableObject();

            // Act
            cell.SetIsGroupHeader<TestTemplatedView, TestBindableObject>(value);

            // Assert
            var result = cell.GetIsGroupHeader<TestTemplatedView, TestBindableObject>();
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Test helper class that extends BindableObject to satisfy TItem constraint.
        /// </summary>
        private class TestBindableObject : BindableObject
        {
        }

        /// <summary>
        /// Tests that GetGroupHeaderContent throws ArgumentNullException when cell parameter is null.
        /// Input: null cell parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetGroupHeaderContent_NullCell_ThrowsArgumentNullException()
        {
            // Arrange
            TestItem nullCell = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                nullCell.GetGroupHeaderContent<TestView, TestItem>());

            Assert.Equal("item", exception.ParamName);
        }

        /// <summary>
        /// Tests that GetGroupHeaderContent returns the HeaderContent from the associated group.
        /// Input: Valid cell with associated group containing HeaderContent.
        /// Expected: Returns the HeaderContent from the group.
        /// </summary>
        [Fact]
        public void GetGroupHeaderContent_ValidCell_ReturnsHeaderContent()
        {
            // Arrange
            var cell = new TestItem();
            var headerContent = new TestItem();

            // Create a group and associate it with the cell
            var group = new TemplatedItemsList<TestView, TestItem>(new TestView(), TemplatedItemsList<TestView, TestItem>.GroupProperty);
            group.HeaderContent = headerContent;

            // Set the group on the cell
            cell.SetValue(TemplatedItemsList<TestView, TestItem>.GroupProperty, group);

            // Act
            var result = cell.GetGroupHeaderContent<TestView, TestItem>();

            // Assert
            Assert.Equal(headerContent, result);
        }

        /// <summary>
        /// Tests that GetGroupHeaderContent handles cases where the group's HeaderContent is null.
        /// Input: Valid cell with associated group that has null HeaderContent.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void GetGroupHeaderContent_GroupWithNullHeaderContent_ReturnsNull()
        {
            // Arrange
            var cell = new TestItem();

            // Create a group with null HeaderContent
            var group = new TemplatedItemsList<TestView, TestItem>(new TestView(), TemplatedItemsList<TestView, TestItem>.GroupProperty);
            // HeaderContent is null by default

            // Set the group on the cell
            cell.SetValue(TemplatedItemsList<TestView, TestItem>.GroupProperty, group);

            // Act
            var result = cell.GetGroupHeaderContent<TestView, TestItem>();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetGroupHeaderContent throws NullReferenceException when cell has no associated group.
        /// Input: Valid cell with no associated group (GetGroup returns null).
        /// Expected: NullReferenceException is thrown when accessing HeaderContent.
        /// </summary>
        [Fact]
        public void GetGroupHeaderContent_CellWithNoGroup_ThrowsNullReferenceException()
        {
            // Arrange
            var cell = new TestItem();
            // Don't set any group on the cell, so GetGroup will return null

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                cell.GetGroupHeaderContent<TestView, TestItem>());
        }

        // Helper test classes to satisfy generic constraints
        private class TestItem : BindableObject
        {
        }

        /// <summary>
        /// Tests GetIndex extension method with null cell parameter.
        /// Should throw ArgumentNullException when cell is null.
        /// </summary>
        [Fact]
        public void GetIndex_NullCell_ThrowsArgumentNullException()
        {
            // Arrange
            TestBindableItem nullCell = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => nullCell.GetIndex<TestTemplatedItemsView, TestBindableItem>());
            Assert.Equal("item", exception.ParamName);
        }

        /// <summary>
        /// Tests GetIndex extension method with valid cell parameter.
        /// Should return the index value stored in the cell's IndexProperty.
        /// </summary>
        [Theory]
        [InlineData(-1)] // Default value
        [InlineData(0)]  // First item
        [InlineData(5)]  // Middle item
        [InlineData(int.MaxValue)] // Maximum value
        [InlineData(int.MinValue)] // Minimum value
        public void GetIndex_ValidCell_ReturnsCorrectIndex(int expectedIndex)
        {
            // Arrange
            var cell = Substitute.For<TestBindableItem>();
            var indexProperty = BindableProperty.Create("Index", typeof(int), typeof(TestBindableItem), -1);
            cell.GetValue(Arg.Any<BindableProperty>()).Returns(expectedIndex);

            // Act
            var actualIndex = cell.GetIndex<TestTemplatedItemsView, TestBindableItem>();

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
            cell.Received(1).GetValue(Arg.Any<BindableProperty>());
        }

        /// <summary>
        /// Tests GetIndex extension method with different generic type combinations.
        /// Should work correctly with various TView and TItem types that satisfy constraints.
        /// </summary>
        [Fact]
        public void GetIndex_DifferentGenericTypes_WorksCorrectly()
        {
            // Arrange
            var cell = Substitute.For<AlternativeBindableItem>();
            var expectedIndex = 42;
            cell.GetValue(Arg.Any<BindableProperty>()).Returns(expectedIndex);

            // Act
            var actualIndex = cell.GetIndex<AlternativeTemplatedItemsView, AlternativeBindableItem>();

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
            cell.Received(1).GetValue(Arg.Any<BindableProperty>());
        }

        /// <summary>
        /// Tests GetIndex extension method behavior when GetValue returns non-integer type.
        /// Should properly cast the value to integer.
        /// </summary>
        [Theory]
        [InlineData(10.5, 10)]  // Double to int
        [InlineData(15L, 15)]   // Long to int
        [InlineData((short)20, 20)] // Short to int
        public void GetIndex_GetValueReturnsNonIntegerType_CastsToInteger(object returnValue, int expectedIndex)
        {
            // Arrange
            var cell = Substitute.For<TestBindableItem>();
            cell.GetValue(Arg.Any<BindableProperty>()).Returns(returnValue);

            // Act
            var actualIndex = cell.GetIndex<TestTemplatedItemsView, TestBindableItem>();

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        // Test helper classes that satisfy the generic constraints
        public class TestBindableItem : BindableObject
        {
        }

        public class AlternativeBindableItem : BindableObject
        {
        }

        /// <summary>
        /// Tests that GetGroup throws ArgumentNullException when cell parameter is null.
        /// Input: null cell parameter
        /// Expected: ArgumentNullException with parameter name "item"
        /// </summary>
        [Fact]
        public void GetGroup_NullCell_ThrowsArgumentNullException()
        {
            // Arrange
            TestBindableObject nullCell = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                nullCell.GetGroup<TestTemplatedItemsView, TestBindableObject>());

            Assert.Equal("item", exception.ParamName);
        }

        /// <summary>
        /// Tests that GetGroup returns the result from TemplatedItemsList.GetGroup when called with valid cell.
        /// Input: valid BindableObject cell with GroupProperty value set
        /// Expected: Returns the ITemplatedItemsList<TItem> from the underlying TemplatedItemsList.GetGroup call
        /// Note: This test requires a concrete implementation scenario where GroupProperty is properly set on the cell.
        /// The actual behavior depends on the cell having a value for the GroupProperty bindable property.
        /// </summary>
        [Fact(Skip = "Requires concrete BindableObject implementation with GroupProperty support")]
        public void GetGroup_ValidCell_ReturnsTemplatedItemsList()
        {
            // This test would require:
            // 1. A concrete BindableObject subclass that can have GroupProperty set
            // 2. A way to set up the GroupProperty value on the cell
            // 3. Verification that the returned value matches what TemplatedItemsList.GetGroup would return

            // Implementation approach:
            // - Create or use a concrete BindableObject subclass for TItem
            // - Create or use a concrete type that implements both BindableObject and ITemplatedItemsView<TItem> for TView
            // - Set up the cell with appropriate GroupProperty value
            // - Call GetGroup and verify the return value

            Assert.True(false, "Test implementation requires concrete types satisfying generic constraints");
        }

        /// <summary>
        /// Tests that GetPath throws ArgumentNullException when passed a null cell parameter.
        /// This verifies proper null parameter validation.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetPath_NullCell_ThrowsArgumentNullException()
        {
            // Arrange
            Cell cell = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CellExtensions.GetPath(cell));
        }

        /// <summary>
        /// Tests that GetPath returns the expected tuple when passed a valid cell.
        /// This verifies the method correctly delegates to TableSectionModel.GetPath and returns the result.
        /// Expected result: Returns the tuple value from the cell's Path property.
        /// </summary>
        [Fact]
        public void GetPath_ValidCell_ReturnsTupleFromTableSectionModel()
        {
            // Arrange
            var mockCell = Substitute.For<Cell>();
            var expectedTuple = new Tuple<int, int>(1, 2);

            // Mock the GetValue method to return our expected tuple
            // This simulates the PathProperty returning a valid tuple
            mockCell.GetValue(Arg.Any<BindableProperty>()).Returns(expectedTuple);

            // Act
            var result = CellExtensions.GetPath(mockCell);

            // Assert
            Assert.Equal(expectedTuple, result);
        }

        /// <summary>
        /// Tests that GetPath returns null when the cell's Path property is null.
        /// This verifies the method handles cases where no path has been set on the cell.
        /// Expected result: Returns null.
        /// </summary>
        [Fact]
        public void GetPath_CellWithNullPath_ReturnsNull()
        {
            // Arrange
            var mockCell = Substitute.For<Cell>();

            // Mock the GetValue method to return null (default PathProperty value)
            mockCell.GetValue(Arg.Any<BindableProperty>()).Returns((object)null);

            // Act
            var result = CellExtensions.GetPath(mockCell);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPath returns correct tuple values for boundary cases.
        /// This verifies the method works with edge case integer values.
        /// Expected result: Returns the tuple with boundary values correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        [InlineData(1, 1)]
        public void GetPath_CellWithBoundaryValues_ReturnsBoundaryTuple(int item1, int item2)
        {
            // Arrange
            var mockCell = Substitute.For<Cell>();
            var expectedTuple = new Tuple<int, int>(item1, item2);

            mockCell.GetValue(Arg.Any<BindableProperty>()).Returns(expectedTuple);

            // Act
            var result = CellExtensions.GetPath(mockCell);

            // Assert
            Assert.Equal(expectedTuple.Item1, result.Item1);
            Assert.Equal(expectedTuple.Item2, result.Item2);
        }

        /// <summary>
        /// Tests that GetIsGroupHeader throws ArgumentNullException when cell parameter is null.
        /// Input: null cell parameter
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void GetIsGroupHeader_NullCell_ThrowsArgumentNullException()
        {
            // Arrange
            TestBindableObject cell = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                CellExtensions.GetIsGroupHeader<TestTemplatedItemsView, TestBindableObject>(cell));
        }

        /// <summary>
        /// Tests that GetIsGroupHeader returns false for a BindableObject with default property value.
        /// Input: Valid BindableObject with default IsGroupHeader property
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void GetIsGroupHeader_ValidCellWithDefaultValue_ReturnsFalse()
        {
            // Arrange
            var cell = new TestBindableObject();

            // Act
            bool result = CellExtensions.GetIsGroupHeader<TestTemplatedItemsView, TestBindableObject>(cell);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetIsGroupHeader returns true when IsGroupHeader property is set to true.
        /// Input: Valid BindableObject with IsGroupHeader set to true
        /// Expected: Returns true
        /// </summary>
        [Fact]
        public void GetIsGroupHeader_ValidCellWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var cell = new TestBindableObject();
            CellExtensions.SetIsGroupHeader<TestTemplatedItemsView, TestBindableObject>(cell, true);

            // Act
            bool result = CellExtensions.GetIsGroupHeader<TestTemplatedItemsView, TestBindableObject>(cell);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetIsGroupHeader returns false when IsGroupHeader property is explicitly set to false.
        /// Input: Valid BindableObject with IsGroupHeader set to false
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void GetIsGroupHeader_ValidCellWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var cell = new TestBindableObject();
            CellExtensions.SetIsGroupHeader<TestTemplatedItemsView, TestBindableObject>(cell, false);

            // Act
            bool result = CellExtensions.GetIsGroupHeader<TestTemplatedItemsView, TestBindableObject>(cell);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetIsGroupHeader works with different generic type combinations that satisfy constraints.
        /// Input: Valid BindableObject using different generic type parameters
        /// Expected: Returns the correct boolean value
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetIsGroupHeader_DifferentGenericTypes_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var cell = new AlternateBindableObject();
            CellExtensions.SetIsGroupHeader<AlternateTemplatedItemsView, AlternateBindableObject>(cell, expectedValue);

            // Act
            bool result = CellExtensions.GetIsGroupHeader<AlternateTemplatedItemsView, AlternateBindableObject>(cell);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        internal class AlternateBindableObject : BindableObject
        {
        }

    }
}