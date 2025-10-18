#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ExportFontAttributeTests
    {
        /// <summary>
        /// Tests the ExportFontAttribute constructor with various string inputs to ensure
        /// the FontFileName property is correctly assigned.
        /// </summary>
        /// <param name="fontFileName">The font file name to test</param>
        /// <param name="expectedFontFileName">The expected value for FontFileName property</param>
        [Theory]
        [InlineData("MyFont.ttf", "MyFont.ttf")]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData("Font With Spaces.otf", "Font With Spaces.otf")]
        [InlineData("font-with-dashes.woff", "font-with-dashes.woff")]
        [InlineData("font_with_underscores.woff2", "font_with_underscores.woff2")]
        [InlineData("Font123.ttf", "Font123.ttf")]
        [InlineData("C:\\Fonts\\MyFont.ttf", "C:\\Fonts\\MyFont.ttf")]
        [InlineData("/usr/share/fonts/MyFont.ttf", "/usr/share/fonts/MyFont.ttf")]
        [InlineData("FontWithSpecialChars!@#$.ttf", "FontWithSpecialChars!@#$.ttf")]
        public void Constructor_WithVariousStringInputs_SetsFontFileNameProperty(string fontFileName, string expectedFontFileName)
        {
            // Arrange & Act
            var attribute = new ExportFontAttribute(fontFileName);

            // Assert
            Assert.Equal(expectedFontFileName, attribute.FontFileName);
        }

        /// <summary>
        /// Tests the ExportFontAttribute constructor with null input to ensure
        /// the FontFileName property is correctly assigned to null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullInput_SetsFontFileNameToNull()
        {
            // Arrange & Act
            var attribute = new ExportFontAttribute(null);

            // Assert
            Assert.Null(attribute.FontFileName);
        }

        /// <summary>
        /// Tests the ExportFontAttribute constructor with a very long string to ensure
        /// it handles large inputs correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongString_SetsFontFileNameProperty()
        {
            // Arrange
            var longFontFileName = new string('a', 10000) + ".ttf";

            // Act
            var attribute = new ExportFontAttribute(longFontFileName);

            // Assert
            Assert.Equal(longFontFileName, attribute.FontFileName);
        }

        /// <summary>
        /// Tests the ExportFontAttribute constructor with string containing control characters
        /// to ensure it handles special characters correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithControlCharacters_SetsFontFileNameProperty()
        {
            // Arrange
            var fontFileNameWithControlChars = "Font\t\r\n.ttf";

            // Act
            var attribute = new ExportFontAttribute(fontFileNameWithControlChars);

            // Assert
            Assert.Equal(fontFileNameWithControlChars, attribute.FontFileName);
        }

        /// <summary>
        /// Tests the ExportFontAttribute constructor with Unicode characters
        /// to ensure it handles international characters correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithUnicodeCharacters_SetsFontFileNameProperty()
        {
            // Arrange
            var unicodeFontFileName = "폰트파일명.ttf";

            // Act
            var attribute = new ExportFontAttribute(unicodeFontFileName);

            // Assert
            Assert.Equal(unicodeFontFileName, attribute.FontFileName);
        }
    }
}
