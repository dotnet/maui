#nullable disable

using System;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public sealed class ItemVisibilityEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes both Item and ItemIndex properties
        /// with valid non-null object and positive index values.
        /// Expected result: Properties should return the exact values passed to constructor.
        /// </summary>
        [Theory]
        [InlineData("test string", 0)]
        [InlineData("test string", 1)]
        [InlineData("test string", 100)]
        [InlineData(42, 5)]
        public void Constructor_WithValidObjectAndIndex_SetsPropertiesCorrectly(object item, int itemIndex)
        {
            // Arrange & Act
            var eventArgs = new ItemVisibilityEventArgs(item, itemIndex);

            // Assert
            Assert.Equal(item, eventArgs.Item);
            Assert.Equal(itemIndex, eventArgs.ItemIndex);
        }

        /// <summary>
        /// Tests that the constructor properly handles null item values.
        /// Expected result: Item property should be null and ItemIndex should be set correctly.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void Constructor_WithNullItem_SetsPropertiesCorrectly(int itemIndex)
        {
            // Arrange & Act
            var eventArgs = new ItemVisibilityEventArgs(null, itemIndex);

            // Assert
            Assert.Null(eventArgs.Item);
            Assert.Equal(itemIndex, eventArgs.ItemIndex);
        }

        /// <summary>
        /// Tests that the constructor properly handles negative ItemIndex values.
        /// Expected result: Negative index should be accepted and stored without validation.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-999)]
        public void Constructor_WithNegativeItemIndex_SetsPropertiesCorrectly(int itemIndex)
        {
            // Arrange
            var item = "test item";

            // Act
            var eventArgs = new ItemVisibilityEventArgs(item, itemIndex);

            // Assert
            Assert.Equal(item, eventArgs.Item);
            Assert.Equal(itemIndex, eventArgs.ItemIndex);
        }

        /// <summary>
        /// Tests that the constructor properly handles extreme integer values for ItemIndex.
        /// Expected result: Extreme values should be accepted and stored correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void Constructor_WithExtremeItemIndexValues_SetsPropertiesCorrectly(int itemIndex)
        {
            // Arrange
            var item = new object();

            // Act
            var eventArgs = new ItemVisibilityEventArgs(item, itemIndex);

            // Assert
            Assert.Equal(item, eventArgs.Item);
            Assert.Equal(itemIndex, eventArgs.ItemIndex);
        }

        /// <summary>
        /// Tests that the constructor works with various object types for the Item parameter.
        /// Expected result: Different object types should be stored correctly in the Item property.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentObjectTypes_SetsPropertiesCorrectly()
        {
            // Arrange & Act & Assert
            var stringItem = new ItemVisibilityEventArgs("string", 1);
            Assert.Equal("string", stringItem.Item);
            Assert.Equal(1, stringItem.ItemIndex);

            var intItem = new ItemVisibilityEventArgs(42, 2);
            Assert.Equal(42, intItem.Item);
            Assert.Equal(2, intItem.ItemIndex);

            var customObject = new { Name = "Test", Value = 123 };
            var customItem = new ItemVisibilityEventArgs(customObject, 3);
            Assert.Equal(customObject, customItem.Item);
            Assert.Equal(3, customItem.ItemIndex);

            var arrayItem = new ItemVisibilityEventArgs(new[] { 1, 2, 3 }, 4);
            Assert.Equal(new[] { 1, 2, 3 }, arrayItem.Item);
            Assert.Equal(4, arrayItem.ItemIndex);
        }

        /// <summary>
        /// Tests that the created instance properly inherits from EventArgs.
        /// Expected result: Instance should be assignable to EventArgs type.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceInheritingFromEventArgs()
        {
            // Arrange & Act
            var eventArgs = new ItemVisibilityEventArgs("test", 1);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }
}
