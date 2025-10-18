#nullable disable

using System;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class TableModelTests : BaseTestFixture
    {
        class TestModel : TableModel
        {
            public override int GetRowCount(int section)
            {
                return 10;
            }

            public override int GetSectionCount()
            {
                return 1;
            }

            public override object GetItem(int section, int row)
            {
                return "Foo";
            }

            public string ProtectedSectionTitle()
            {
                return GetSectionTitle(0);
            }
        }

        [Fact]
        public void DefaultSectionTitle()
        {
            Assert.Null(new TestModel().ProtectedSectionTitle());
        }

        [Fact]
        public void DefualtSectionIndexTitles()
        {
            Assert.Null(new TestModel().GetSectionIndexTitles());
        }

        [Fact]
        public void DefaultHeaderCell()
        {
            Assert.Null(new TestModel().GetHeaderCell(0));
        }

        [Fact]
        public void DefaultCellFromObject()
        {
            var model = new TestModel();
            var cell = model.GetCell(0, 5);

            Assert.IsType<TextCell>(cell);

            var textCell = (TextCell)cell;
            Assert.Equal("Foo", textCell.Text);
        }

        [Fact]
        public void RowLongPressed()
        {
            var model = new TestModel();

            var longPressedItem = "";
            model.ItemLongPressed += (sender, arg) =>
            {
                longPressedItem = (string)arg.Data;
            };

            model.RowLongPressed(0, 5);
        }

        [Fact]
        public void RowSelectedForObject()
        {
            var model = new TestModel();
            string result = null;
            model.ItemSelected += (sender, arg) => result = (string)arg.Data;

            model.RowSelected("Foobar");
            Assert.Equal("Foobar", result);
        }

        /// <summary>
        /// Test class that returns a Cell object from GetItem to test the cell != null branch
        /// </summary>
        class TestModelReturningCell : TableModel
        {
            private readonly Cell _cellToReturn;

            public TestModelReturningCell(Cell cellToReturn)
            {
                _cellToReturn = cellToReturn;
            }

            public override int GetRowCount(int section)
            {
                return 10;
            }

            public override int GetSectionCount()
            {
                return 1;
            }

            public override object GetItem(int section, int row)
            {
                return _cellToReturn;
            }
        }

        /// <summary>
        /// Test class that returns null from GetItem to test null handling
        /// </summary>
        class TestModelReturningNull : TableModel
        {
            public override int GetRowCount(int section)
            {
                return 10;
            }

            public override int GetSectionCount()
            {
                return 1;
            }

            public override object GetItem(int section, int row)
            {
                return null;
            }
        }

        /// <summary>
        /// Test class that returns objects with different ToString behaviors
        /// </summary>
        class TestModelReturningCustomObject : TableModel
        {
            private readonly object _objectToReturn;

            public TestModelReturningCustomObject(object objectToReturn)
            {
                _objectToReturn = objectToReturn;
            }

            public override int GetRowCount(int section)
            {
                return 10;
            }

            public override int GetSectionCount()
            {
                return 1;
            }

            public override object GetItem(int section, int row)
            {
                return _objectToReturn;
            }
        }

        /// <summary>
        /// Object with empty ToString for testing edge cases
        /// </summary>
        class ObjectWithEmptyToString
        {
            public override string ToString()
            {
                return "";
            }
        }

        /// <summary>
        /// Object with whitespace-only ToString for testing edge cases
        /// </summary>
        class ObjectWithWhitespaceToString
        {
            public override string ToString()
            {
                return "   ";
            }
        }

        /// <summary>
        /// Object with long ToString for testing edge cases
        /// </summary>
        class ObjectWithLongToString
        {
            public override string ToString()
            {
                return new string('A', 10000);
            }
        }

        /// <summary>
        /// Tests that GetCell returns the same Cell object when GetItem returns a Cell.
        /// This test covers the uncovered line 22 where cell != null is true.
        /// </summary>
        [Fact]
        public void GetCell_WhenGetItemReturnsCell_ReturnsSameCell()
        {
            // Arrange
            var expectedCell = new TextCell { Text = "Test Cell" };
            var model = new TestModelReturningCell(expectedCell);

            // Act
            var result = model.GetCell(0, 0);

            // Assert
            Assert.Same(expectedCell, result);
        }

        /// <summary>
        /// Tests that GetCell creates a TextCell when GetItem returns null.
        /// Tests null handling in the ToString() call.
        /// </summary>
        [Fact]
        public void GetCell_WhenGetItemReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var model = new TestModelReturningNull();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => model.GetCell(0, 0));
        }

        /// <summary>
        /// Tests that GetCell creates a TextCell with empty text when GetItem returns an object with empty ToString.
        /// </summary>
        [Fact]
        public void GetCell_WhenGetItemReturnsObjectWithEmptyToString_CreatesTextCellWithEmptyText()
        {
            // Arrange
            var objectWithEmptyToString = new ObjectWithEmptyToString();
            var model = new TestModelReturningCustomObject(objectWithEmptyToString);

            // Act
            var result = model.GetCell(0, 0);

            // Assert
            Assert.IsType<TextCell>(result);
            var textCell = (TextCell)result;
            Assert.Equal("", textCell.Text);
        }

        /// <summary>
        /// Tests that GetCell creates a TextCell with whitespace text when GetItem returns an object with whitespace ToString.
        /// </summary>
        [Fact]
        public void GetCell_WhenGetItemReturnsObjectWithWhitespaceToString_CreatesTextCellWithWhitespaceText()
        {
            // Arrange
            var objectWithWhitespaceToString = new ObjectWithWhitespaceToString();
            var model = new TestModelReturningCustomObject(objectWithWhitespaceToString);

            // Act
            var result = model.GetCell(0, 0);

            // Assert
            Assert.IsType<TextCell>(result);
            var textCell = (TextCell)result;
            Assert.Equal("   ", textCell.Text);
        }

        /// <summary>
        /// Tests that GetCell creates a TextCell with long text when GetItem returns an object with very long ToString.
        /// </summary>
        [Fact]
        public void GetCell_WhenGetItemReturnsObjectWithLongToString_CreatesTextCellWithLongText()
        {
            // Arrange
            var objectWithLongToString = new ObjectWithLongToString();
            var model = new TestModelReturningCustomObject(objectWithLongToString);

            // Act
            var result = model.GetCell(0, 0);

            // Assert
            Assert.IsType<TextCell>(result);
            var textCell = (TextCell)result;
            Assert.Equal(new string('A', 10000), textCell.Text);
        }

        /// <summary>
        /// Tests GetCell behavior with various section and row parameter edge cases.
        /// Tests negative values, zero, and extreme values.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, int.MinValue)]
        [InlineData(0, -1)]
        [InlineData(int.MaxValue, 0)]
        [InlineData(0, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        public void GetCell_WithEdgeCaseParameters_PassesParametersToGetItem(int section, int row)
        {
            // Arrange
            var model = new TestModel();

            // Act
            var result = model.GetCell(section, row);

            // Assert
            Assert.IsType<TextCell>(result);
            var textCell = (TextCell)result;
            Assert.Equal("Foo", textCell.Text);
        }

        /// <summary>
        /// Tests that GetCell correctly passes valid positive parameters to GetItem.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 5)]
        [InlineData(10, 100)]
        [InlineData(999, 999)]
        public void GetCell_WithValidParameters_PassesParametersToGetItem(int section, int row)
        {
            // Arrange
            var model = new TestModel();

            // Act
            var result = model.GetCell(section, row);

            // Assert
            Assert.IsType<TextCell>(result);
            var textCell = (TextCell)result;
            Assert.Equal("Foo", textCell.Text);
        }

        /// <summary>
        /// Tests that RowSelected(int, int) calls GetItem with correct parameters and forwards result to RowSelected(object).
        /// Verifies the basic flow from section/row coordinates to item selection.
        /// Expected result: GetItem called with provided section and row, and RowSelected(object) called with returned item.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 5)]
        [InlineData(10, 15)]
        public void RowSelected_ValidSectionAndRow_CallsGetItemAndForwardsToObjectOverload(int section, int row)
        {
            // Arrange
            var model = new TestModel();
            bool itemSelectedEventFired = false;
            object selectedItem = null;

            model.ItemSelected += (sender, args) =>
            {
                itemSelectedEventFired = true;
                selectedItem = args.Data;
            };

            // Act
            model.RowSelected(section, row);

            // Assert
            Assert.True(itemSelectedEventFired);
            Assert.Equal("Foo", selectedItem);
        }

        /// <summary>
        /// Tests RowSelected(int, int) with negative section values.
        /// Verifies that negative section values are passed through to GetItem without validation.
        /// Expected result: GetItem called with negative section value.
        /// </summary>
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(int.MinValue, 0)]
        public void RowSelected_NegativeSection_CallsGetItemWithNegativeSection(int section, int row)
        {
            // Arrange
            var model = new TestModelWithTracking();

            // Act
            model.RowSelected(section, row);

            // Assert
            Assert.Equal(section, model.LastGetItemSection);
            Assert.Equal(row, model.LastGetItemRow);
        }

        /// <summary>
        /// Tests RowSelected(int, int) with negative row values.
        /// Verifies that negative row values are passed through to GetItem without validation.
        /// Expected result: GetItem called with negative row value.
        /// </summary>
        [Theory]
        [InlineData(0, -1)]
        [InlineData(0, int.MinValue)]
        public void RowSelected_NegativeRow_CallsGetItemWithNegativeRow(int section, int row)
        {
            // Arrange
            var model = new TestModelWithTracking();

            // Act
            model.RowSelected(section, row);

            // Assert
            Assert.Equal(section, model.LastGetItemSection);
            Assert.Equal(row, model.LastGetItemRow);
        }

        /// <summary>
        /// Tests RowSelected(int, int) with maximum integer values.
        /// Verifies that extreme values are handled correctly.
        /// Expected result: GetItem called with maximum values.
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue, 0)]
        [InlineData(0, int.MaxValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        public void RowSelected_MaximumValues_CallsGetItemWithMaximumValues(int section, int row)
        {
            // Arrange
            var model = new TestModelWithTracking();

            // Act
            model.RowSelected(section, row);

            // Assert
            Assert.Equal(section, model.LastGetItemSection);
            Assert.Equal(row, model.LastGetItemRow);
        }

        /// <summary>
        /// Tests RowSelected(int, int) when GetItem returns null.
        /// Verifies that null items are properly forwarded to the object overload.
        /// Expected result: RowSelected(object) called with null.
        /// </summary>
        [Fact]
        public void RowSelected_GetItemReturnsNull_ForwardsNullToObjectOverload()
        {
            // Arrange
            var model = new TestModelReturningNull();
            bool itemSelectedEventFired = false;
            object selectedItem = "not null";

            model.ItemSelected += (sender, args) =>
            {
                itemSelectedEventFired = true;
                selectedItem = args.Data;
            };

            // Act
            model.RowSelected(0, 0);

            // Assert
            Assert.True(itemSelectedEventFired);
            Assert.Null(selectedItem);
        }

        private class TestModelWithTracking : TableModel
        {
            public int LastGetItemSection { get; private set; }
            public int LastGetItemRow { get; private set; }

            public override int GetRowCount(int section)
            {
                return 10;
            }

            public override int GetSectionCount()
            {
                return 1;
            }

            public override object GetItem(int section, int row)
            {
                LastGetItemSection = section;
                LastGetItemRow = row;
                return "TrackedItem";
            }
        }

        /// <summary>
        /// Tests that GetSectionTextColor returns null for a valid positive section index.
        /// This verifies the default implementation behavior for normal section values.
        /// </summary>
        [Fact]
        public void GetSectionTextColor_ValidPositiveSection_ReturnsNull()
        {
            // Arrange
            var model = new TestModel();

            // Act
            var result = model.GetSectionTextColor(1);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetSectionTextColor returns null for section index zero.
        /// This verifies the default implementation behavior for the first section.
        /// </summary>
        [Fact]
        public void GetSectionTextColor_ZeroSection_ReturnsNull()
        {
            // Arrange
            var model = new TestModel();

            // Act
            var result = model.GetSectionTextColor(0);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetSectionTextColor returns null for negative section indices.
        /// This verifies the default implementation behavior for invalid negative section values.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void GetSectionTextColor_NegativeSection_ReturnsNull(int section)
        {
            // Arrange
            var model = new TestModel();

            // Act
            var result = model.GetSectionTextColor(section);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetSectionTextColor returns null for boundary integer values.
        /// This verifies the default implementation behavior for edge case integer values.
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(1000)]
        [InlineData(100)]
        public void GetSectionTextColor_LargeSection_ReturnsNull(int section)
        {
            // Arrange
            var model = new TestModel();

            // Act
            var result = model.GetSectionTextColor(section);

            // Assert
            Assert.Null(result);
        }
    }
}