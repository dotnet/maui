#nullable disable

using System;
using System.Collections;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class KnownColorTests
    {
        /// <summary>
        /// Tests that SetAccent method correctly sets the Accent property with a valid Color instance.
        /// Verifies that the Accent property contains the expected color value after calling SetAccent.
        /// </summary>
        [Fact]
        public void SetAccent_WithValidColor_SetsAccentProperty()
        {
            // Arrange
            var expectedColor = new Color(0.5f, 0.3f, 0.8f, 1.0f);

            // Act
            KnownColor.SetAccent(expectedColor);

            // Assert
            Assert.Equal(expectedColor, KnownColor.Accent);
        }

        /// <summary>
        /// Tests that SetAccent method correctly sets the Accent property to null when passed a null value.
        /// Verifies that the Accent property becomes null after calling SetAccent with null.
        /// </summary>
        [Fact]
        public void SetAccent_WithNull_SetsAccentPropertyToNull()
        {
            // Arrange
            Color nullColor = null;

            // Act
            KnownColor.SetAccent(nullColor);

            // Assert
            Assert.Null(KnownColor.Accent);
        }

        /// <summary>
        /// Tests that SetAccent method correctly handles different Color constructor variations.
        /// Verifies that various Color instances are properly assigned to the Accent property.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 0.5f)] // Semi-transparent red
        [InlineData(0.0f, 1.0f, 0.0f, 0.0f)] // Transparent green
        public void SetAccent_WithDifferentColorValues_SetsAccentPropertyCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var color = new Color(red, green, blue, alpha);

            // Act
            KnownColor.SetAccent(color);

            // Assert
            Assert.Equal(color, KnownColor.Accent);
        }

        /// <summary>
        /// Tests that SetAccent method correctly handles a default Color instance.
        /// Verifies that a Color created with the default constructor is properly assigned to the Accent property.
        /// </summary>
        [Fact]
        public void SetAccent_WithDefaultColor_SetsAccentProperty()
        {
            // Arrange
            var defaultColor = new Color(); // Default black color

            // Act
            KnownColor.SetAccent(defaultColor);

            // Assert
            Assert.Equal(defaultColor, KnownColor.Accent);
        }

        /// <summary>
        /// Tests that SetAccent method correctly handles Color instances created with byte values.
        /// Verifies that Colors created using byte constructor overloads are properly assigned to the Accent property.
        /// </summary>
        [Theory]
        [InlineData(255, 0, 0, 255)] // Red
        [InlineData(0, 255, 0, 255)] // Green  
        [InlineData(0, 0, 255, 255)] // Blue
        [InlineData(128, 128, 128, 128)] // Gray with transparency
        public void SetAccent_WithByteColorValues_SetsAccentPropertyCorrectly(byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var color = new Color(red, green, blue, alpha);

            // Act
            KnownColor.SetAccent(color);

            // Assert
            Assert.Equal(color, KnownColor.Accent);
        }
    }
}
