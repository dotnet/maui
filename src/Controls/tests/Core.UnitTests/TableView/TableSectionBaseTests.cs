#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for TableSectionBase class focusing on the TextColor property.
    /// </summary>
    public partial class TableSectionBaseTests
    {
        /// <summary>
        /// Tests that TextColor getter returns the Color value from GetValue when a Color is set.
        /// </summary>
        [Fact]
        public void TextColor_WhenColorIsSet_ReturnsColor()
        {
            // Arrange
            var tableSectionBase = Substitute.ForPartsOf<TableSectionBase>();
            var expectedColor = new Color(0.5f, 0.3f, 0.8f, 1.0f);
            tableSectionBase.GetValue(TableSectionBase.TextColorProperty).Returns(expectedColor);

            // Act
            var actualColor = tableSectionBase.TextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that TextColor getter returns null when no color is set (default value).
        /// </summary>
        [Fact]
        public void TextColor_WhenNoColorIsSet_ReturnsNull()
        {
            // Arrange
            var tableSectionBase = Substitute.ForPartsOf<TableSectionBase>();
            tableSectionBase.GetValue(TableSectionBase.TextColorProperty).Returns((Color)null);

            // Act
            var actualColor = tableSectionBase.TextColor;

            // Assert
            Assert.Null(actualColor);
        }

        /// <summary>
        /// Tests that TextColor getter returns the default Color when GetValue returns the default value.
        /// </summary>
        [Fact]
        public void TextColor_WhenDefaultValueReturned_ReturnsDefaultColor()
        {
            // Arrange
            var tableSectionBase = Substitute.ForPartsOf<TableSectionBase>();
            var defaultColor = new Color(); // Default black color
            tableSectionBase.GetValue(TableSectionBase.TextColorProperty).Returns(defaultColor);

            // Act
            var actualColor = tableSectionBase.TextColor;

            // Assert
            Assert.Equal(defaultColor, actualColor);
        }

        /// <summary>
        /// Tests that TextColor setter calls SetValue with the correct property and color value.
        /// </summary>
        [Fact]
        public void TextColor_WhenSettingColor_CallsSetValueWithCorrectArguments()
        {
            // Arrange
            var tableSectionBase = Substitute.ForPartsOf<TableSectionBase>();
            var colorToSet = new Color(1.0f, 0.0f, 0.5f, 0.8f);

            // Act
            tableSectionBase.TextColor = colorToSet;

            // Assert
            tableSectionBase.Received(1).SetValue(TableSectionBase.TextColorProperty, colorToSet);
        }

        /// <summary>
        /// Tests that TextColor setter calls SetValue when setting null color value.
        /// </summary>
        [Fact]
        public void TextColor_WhenSettingNullColor_CallsSetValueWithNull()
        {
            // Arrange
            var tableSectionBase = Substitute.ForPartsOf<TableSectionBase>();

            // Act
            tableSectionBase.TextColor = null;

            // Assert
            tableSectionBase.Received(1).SetValue(TableSectionBase.TextColorProperty, null);
        }

        /// <summary>
        /// Tests TextColor property with various Color edge cases including transparent and extreme values.
        /// </summary>
        /// <param name="red">Red component value</param>
        /// <param name="green">Green component value</param>
        /// <param name="blue">Blue component value</param>
        /// <param name="alpha">Alpha component value</param>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // Opaque white
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        public void TextColor_WithVariousColorValues_GetterAndSetterWorkCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tableSectionBase = Substitute.ForPartsOf<TableSectionBase>();
            var testColor = new Color(red, green, blue, alpha);
            tableSectionBase.GetValue(TableSectionBase.TextColorProperty).Returns(testColor);

            // Act - Test setter
            tableSectionBase.TextColor = testColor;

            // Act - Test getter
            var retrievedColor = tableSectionBase.TextColor;

            // Assert
            tableSectionBase.Received(1).SetValue(TableSectionBase.TextColorProperty, testColor);
            Assert.Equal(testColor, retrievedColor);
        }

        /// <summary>
        /// Tests that TextColor getter properly casts the object returned from GetValue to Color type.
        /// </summary>
        [Fact]
        public void TextColor_WhenGetValueReturnsObject_CastsToColor()
        {
            // Arrange
            var tableSectionBase = Substitute.ForPartsOf<TableSectionBase>();
            var colorObject = (object)new Color(0.7f, 0.2f, 0.9f, 1.0f);
            tableSectionBase.GetValue(TableSectionBase.TextColorProperty).Returns(colorObject);

            // Act
            var actualColor = tableSectionBase.TextColor;

            // Assert
            Assert.NotNull(actualColor);
            Assert.Equal(0.7f, actualColor.Red);
            Assert.Equal(0.2f, actualColor.Green);
            Assert.Equal(0.9f, actualColor.Blue);
            Assert.Equal(1.0f, actualColor.Alpha);
        }
    }
}
