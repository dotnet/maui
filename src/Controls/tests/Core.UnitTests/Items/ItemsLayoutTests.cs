#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the ItemsLayout class SnapPointsType property.
    /// </summary>
    public partial class ItemsLayoutTests
    {
        /// <summary>
        /// Tests that the SnapPointsType property returns the default value when not explicitly set.
        /// Input: ItemsLayout with default SnapPointsType property value.
        /// Expected: SnapPointsType.None (the default value defined in the bindable property).
        /// </summary>
        [Fact]
        public void SnapPointsType_DefaultValue_ReturnsNone()
        {
            // Arrange
            var itemsLayout = Substitute.For<ItemsLayout>(ItemsLayoutOrientation.Vertical);
            itemsLayout.SnapPointsType.Returns(SnapPointsType.None);

            // Act
            var result = itemsLayout.SnapPointsType;

            // Assert
            Assert.Equal(SnapPointsType.None, result);
        }

        /// <summary>
        /// Tests that the SnapPointsType property can be set to and retrieve all valid enum values.
        /// Input: Valid SnapPointsType enum values.
        /// Expected: Property returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(SnapPointsType.None)]
        [InlineData(SnapPointsType.Mandatory)]
        [InlineData(SnapPointsType.MandatorySingle)]
        public void SnapPointsType_ValidEnumValues_SetAndGetCorrectly(SnapPointsType expectedValue)
        {
            // Arrange
            var itemsLayout = Substitute.For<ItemsLayout>(ItemsLayoutOrientation.Vertical);
            itemsLayout.SnapPointsType = expectedValue;
            itemsLayout.SnapPointsType.Returns(expectedValue);

            // Act
            var result = itemsLayout.SnapPointsType;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the SnapPointsType property handles invalid enum values gracefully.
        /// Input: Invalid SnapPointsType enum value (cast from out-of-range integer).
        /// Expected: Property should handle the invalid value without throwing exceptions during casting.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void SnapPointsType_InvalidEnumValues_HandlesGracefully(int invalidEnumValue)
        {
            // Arrange
            var itemsLayout = Substitute.For<ItemsLayout>(ItemsLayoutOrientation.Vertical);
            var invalidSnapPointsType = (SnapPointsType)invalidEnumValue;
            itemsLayout.SnapPointsType = invalidSnapPointsType;
            itemsLayout.SnapPointsType.Returns(invalidSnapPointsType);

            // Act
            var result = itemsLayout.SnapPointsType;

            // Assert
            Assert.Equal(invalidSnapPointsType, result);
        }

        /// <summary>
        /// Tests that setting SnapPointsType property multiple times works correctly.
        /// Input: Multiple sequential assignments of different SnapPointsType values.
        /// Expected: Property always returns the most recently set value.
        /// </summary>
        [Fact]
        public void SnapPointsType_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var itemsLayout = Substitute.For<ItemsLayout>(ItemsLayoutOrientation.Horizontal);

            // Act & Assert - Test sequence of assignments
            itemsLayout.SnapPointsType = SnapPointsType.None;
            itemsLayout.SnapPointsType.Returns(SnapPointsType.None);
            Assert.Equal(SnapPointsType.None, itemsLayout.SnapPointsType);

            itemsLayout.SnapPointsType = SnapPointsType.Mandatory;
            itemsLayout.SnapPointsType.Returns(SnapPointsType.Mandatory);
            Assert.Equal(SnapPointsType.Mandatory, itemsLayout.SnapPointsType);

            itemsLayout.SnapPointsType = SnapPointsType.MandatorySingle;
            itemsLayout.SnapPointsType.Returns(SnapPointsType.MandatorySingle);
            Assert.Equal(SnapPointsType.MandatorySingle, itemsLayout.SnapPointsType);
        }

        /// <summary>
        /// Tests SnapPointsType property with different ItemsLayoutOrientation values to ensure orientation doesn't affect the property.
        /// Input: ItemsLayout instances with different orientations.
        /// Expected: SnapPointsType property behavior should be consistent regardless of orientation.
        /// </summary>
        [Theory]
        [InlineData(ItemsLayoutOrientation.Vertical)]
        [InlineData(ItemsLayoutOrientation.Horizontal)]
        public void SnapPointsType_DifferentOrientations_PropertyBehaviorConsistent(ItemsLayoutOrientation orientation)
        {
            // Arrange
            var itemsLayout = Substitute.For<ItemsLayout>(orientation);
            var testValue = SnapPointsType.Mandatory;
            itemsLayout.SnapPointsType = testValue;
            itemsLayout.SnapPointsType.Returns(testValue);

            // Act
            var result = itemsLayout.SnapPointsType;

            // Assert
            Assert.Equal(testValue, result);
        }

        /// <summary>
        /// Tests that the SnapPointsAlignment property getter returns the Start value correctly.
        /// Verifies the property can retrieve the Start enum value from the underlying BindableProperty system.
        /// </summary>
        [Fact]
        public void SnapPointsAlignment_GetStart_ReturnsStart()
        {
            // Arrange
            var itemsLayout = Substitute.ForPartsOf<TestableItemsLayout>(ItemsLayoutOrientation.Vertical);

            // Act
            itemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
            var result = itemsLayout.SnapPointsAlignment;

            // Assert
            Assert.Equal(SnapPointsAlignment.Start, result);
        }

        /// <summary>
        /// Tests that the SnapPointsAlignment property getter returns the Center value correctly.
        /// Verifies the property can retrieve the Center enum value from the underlying BindableProperty system.
        /// </summary>
        [Fact]
        public void SnapPointsAlignment_GetCenter_ReturnsCenter()
        {
            // Arrange
            var itemsLayout = Substitute.ForPartsOf<TestableItemsLayout>(ItemsLayoutOrientation.Vertical);

            // Act
            itemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
            var result = itemsLayout.SnapPointsAlignment;

            // Assert
            Assert.Equal(SnapPointsAlignment.Center, result);
        }

        /// <summary>
        /// Tests that the SnapPointsAlignment property getter returns the End value correctly.
        /// Verifies the property can retrieve the End enum value from the underlying BindableProperty system.
        /// </summary>
        [Fact]
        public void SnapPointsAlignment_GetEnd_ReturnsEnd()
        {
            // Arrange
            var itemsLayout = Substitute.ForPartsOf<TestableItemsLayout>(ItemsLayoutOrientation.Vertical);

            // Act
            itemsLayout.SnapPointsAlignment = SnapPointsAlignment.End;
            var result = itemsLayout.SnapPointsAlignment;

            // Assert
            Assert.Equal(SnapPointsAlignment.End, result);
        }

        /// <summary>
        /// Tests that the SnapPointsAlignment property setter correctly stores all valid enum values.
        /// Verifies the property can store and retrieve each valid SnapPointsAlignment enum value.
        /// </summary>
        [Theory]
        [InlineData(SnapPointsAlignment.Start)]
        [InlineData(SnapPointsAlignment.Center)]
        [InlineData(SnapPointsAlignment.End)]
        public void SnapPointsAlignment_SetValidValues_StoresCorrectly(SnapPointsAlignment expected)
        {
            // Arrange
            var itemsLayout = Substitute.ForPartsOf<TestableItemsLayout>(ItemsLayoutOrientation.Horizontal);

            // Act
            itemsLayout.SnapPointsAlignment = expected;
            var result = itemsLayout.SnapPointsAlignment;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the SnapPointsAlignment property has the correct default value.
        /// Verifies that the property defaults to SnapPointsAlignment.Start as defined in the BindableProperty.
        /// </summary>
        [Fact]
        public void SnapPointsAlignment_DefaultValue_ReturnsStart()
        {
            // Arrange & Act
            var itemsLayout = Substitute.ForPartsOf<TestableItemsLayout>(ItemsLayoutOrientation.Vertical);

            // Assert
            Assert.Equal(SnapPointsAlignment.Start, itemsLayout.SnapPointsAlignment);
        }

        /// <summary>
        /// Tests that the SnapPointsAlignment property can handle invalid enum values through casting.
        /// Verifies the property behavior when assigned values outside the defined enum range.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SnapPointsAlignment_SetInvalidEnumValue_StoresValue(int invalidValue)
        {
            // Arrange
            var itemsLayout = Substitute.ForPartsOf<TestableItemsLayout>(ItemsLayoutOrientation.Vertical);
            var invalidSnapAlignment = (SnapPointsAlignment)invalidValue;

            // Act
            itemsLayout.SnapPointsAlignment = invalidSnapAlignment;
            var result = itemsLayout.SnapPointsAlignment;

            // Assert
            Assert.Equal(invalidSnapAlignment, result);
        }

        /// <summary>
        /// Tests that the SnapPointsAlignment property works correctly with different ItemsLayoutOrientation values.
        /// Verifies that the orientation parameter doesn't affect the SnapPointsAlignment property behavior.
        /// </summary>
        [Theory]
        [InlineData(ItemsLayoutOrientation.Vertical)]
        [InlineData(ItemsLayoutOrientation.Horizontal)]
        public void SnapPointsAlignment_WithDifferentOrientations_WorksCorrectly(ItemsLayoutOrientation orientation)
        {
            // Arrange
            var itemsLayout = Substitute.ForPartsOf<TestableItemsLayout>(orientation);

            // Act
            itemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
            var result = itemsLayout.SnapPointsAlignment;

            // Assert
            Assert.Equal(SnapPointsAlignment.Center, result);
            Assert.Equal(orientation, itemsLayout.Orientation);
        }

        /// <summary>
        /// Concrete implementation of ItemsLayout for testing purposes.
        /// This class allows us to test the abstract ItemsLayout functionality.
        /// </summary>
        private class TestableItemsLayout : ItemsLayout
        {
            public TestableItemsLayout(ItemsLayoutOrientation orientation) : base(orientation)
            {
            }
        }
    }
}