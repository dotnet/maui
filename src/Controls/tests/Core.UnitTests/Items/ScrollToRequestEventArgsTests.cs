#nullable disable

using System;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ScrollToRequestEventArgsTests
    {
        /// <summary>
        /// Tests the element-based constructor with valid parameters to ensure all properties are set correctly
        /// and Mode is set to Element.
        /// </summary>
        /// <param name="item">The item object to scroll to</param>
        /// <param name="group">The group object containing the item</param>
        /// <param name="scrollToPosition">The position within the viewport to scroll to</param>
        /// <param name="isAnimated">Whether the scroll should be animated</param>
        [Theory]
        [InlineData("test item", "test group", ScrollToPosition.MakeVisible, true)]
        [InlineData("test item", "test group", ScrollToPosition.Start, false)]
        [InlineData("test item", "test group", ScrollToPosition.Center, true)]
        [InlineData("test item", "test group", ScrollToPosition.End, false)]
        [InlineData(123, 456, ScrollToPosition.MakeVisible, true)]
        [InlineData(123, 456, ScrollToPosition.Start, false)]
        [InlineData(123, 456, ScrollToPosition.Center, true)]
        [InlineData(123, 456, ScrollToPosition.End, false)]
        public void Constructor_ValidParameters_SetsPropertiesCorrectly(object item, object group, ScrollToPosition scrollToPosition, bool isAnimated)
        {
            // Act
            var args = new ScrollToRequestEventArgs(item, group, scrollToPosition, isAnimated);

            // Assert
            Assert.Equal(ScrollToMode.Element, args.Mode);
            Assert.Equal(item, args.Item);
            Assert.Equal(group, args.Group);
            Assert.Equal(scrollToPosition, args.ScrollToPosition);
            Assert.Equal(isAnimated, args.IsAnimated);
            Assert.IsAssignableFrom<EventArgs>(args);
        }

        /// <summary>
        /// Tests the element-based constructor with null item and group parameters to ensure
        /// null values are handled correctly.
        /// </summary>
        /// <param name="scrollToPosition">The position within the viewport to scroll to</param>
        /// <param name="isAnimated">Whether the scroll should be animated</param>
        [Theory]
        [InlineData(ScrollToPosition.MakeVisible, true)]
        [InlineData(ScrollToPosition.Start, false)]
        [InlineData(ScrollToPosition.Center, true)]
        [InlineData(ScrollToPosition.End, false)]
        public void Constructor_NullItemAndGroup_SetsPropertiesCorrectly(ScrollToPosition scrollToPosition, bool isAnimated)
        {
            // Act
            var args = new ScrollToRequestEventArgs(null, null, scrollToPosition, isAnimated);

            // Assert
            Assert.Equal(ScrollToMode.Element, args.Mode);
            Assert.Null(args.Item);
            Assert.Null(args.Group);
            Assert.Equal(scrollToPosition, args.ScrollToPosition);
            Assert.Equal(isAnimated, args.IsAnimated);
        }

        /// <summary>
        /// Tests the element-based constructor with one null parameter and one valid parameter
        /// to ensure mixed null/non-null values are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_MixedNullParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var item = "test item";
            object group = null;

            // Act
            var args = new ScrollToRequestEventArgs(item, group, ScrollToPosition.Center, true);

            // Assert
            Assert.Equal(ScrollToMode.Element, args.Mode);
            Assert.Equal(item, args.Item);
            Assert.Null(args.Group);
            Assert.Equal(ScrollToPosition.Center, args.ScrollToPosition);
            Assert.True(args.IsAnimated);
        }

        /// <summary>
        /// Tests the element-based constructor with invalid ScrollToPosition enum values
        /// to ensure the constructor handles undefined enum values.
        /// </summary>
        /// <param name="invalidEnumValue">An invalid ScrollToPosition enum value</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Constructor_InvalidScrollToPosition_SetsPropertyToInvalidValue(int invalidEnumValue)
        {
            // Arrange
            var invalidScrollToPosition = (ScrollToPosition)invalidEnumValue;
            var item = "test";
            var group = "group";

            // Act
            var args = new ScrollToRequestEventArgs(item, group, invalidScrollToPosition, false);

            // Assert
            Assert.Equal(ScrollToMode.Element, args.Mode);
            Assert.Equal(item, args.Item);
            Assert.Equal(group, args.Group);
            Assert.Equal(invalidScrollToPosition, args.ScrollToPosition);
            Assert.False(args.IsAnimated);
        }

        /// <summary>
        /// Tests the element-based constructor with complex object types to ensure
        /// it works with various object types beyond primitives.
        /// </summary>
        [Fact]
        public void Constructor_ComplexObjectTypes_SetsPropertiesCorrectly()
        {
            // Arrange
            var complexItem = new { Name = "Test", Value = 42 };
            var complexGroup = new DateTime(2023, 1, 1);

            // Act
            var args = new ScrollToRequestEventArgs(complexItem, complexGroup, ScrollToPosition.End, true);

            // Assert
            Assert.Equal(ScrollToMode.Element, args.Mode);
            Assert.Equal(complexItem, args.Item);
            Assert.Equal(complexGroup, args.Group);
            Assert.Equal(ScrollToPosition.End, args.ScrollToPosition);
            Assert.True(args.IsAnimated);
        }
    }
}
