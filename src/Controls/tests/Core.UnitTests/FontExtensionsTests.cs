using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class FontExtensionsTests
    {
        /// <summary>
        /// Tests ToFont with valid FontSize and no defaultSize parameter.
        /// Should use the element's FontSize and create Font with all element properties.
        /// </summary>
        [Fact]
        public void ToFont_ValidFontSizeNoDefaultSize_UsesElementFontSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
            Assert.Equal(FontWeight.Regular, result.Weight);
            Assert.Equal(FontSlant.Default, result.Slant);
        }

        /// <summary>
        /// Tests ToFont with valid FontSize and defaultSize parameter.
        /// Should ignore defaultSize and use the element's FontSize.
        /// </summary>
        [Fact]
        public void ToFont_ValidFontSizeWithDefaultSize_UsesElementFontSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);
            double defaultSize = 12.0;

            // Act
            var result = element.ToFont(defaultSize);

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with FontSize zero and defaultSize parameter.
        /// Should use defaultSize instead of element's FontSize.
        /// </summary>
        [Fact]
        public void ToFont_FontSizeZeroWithDefaultSize_UsesDefaultSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(0.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);
            double defaultSize = 12.0;

            // Act
            var result = element.ToFont(defaultSize);

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(12.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with negative FontSize and defaultSize parameter.
        /// Should use defaultSize instead of element's FontSize.
        /// </summary>
        [Fact]
        public void ToFont_FontSizeNegativeWithDefaultSize_UsesDefaultSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(-5.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);
            double defaultSize = 12.0;

            // Act
            var result = element.ToFont(defaultSize);

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(12.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with NaN FontSize and defaultSize parameter.
        /// Should use defaultSize instead of element's FontSize.
        /// </summary>
        [Fact]
        public void ToFont_FontSizeNaNWithDefaultSize_UsesDefaultSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(double.NaN);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);
            double defaultSize = 12.0;

            // Act
            var result = element.ToFont(defaultSize);

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(12.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with FontSize zero and no defaultSize parameter.
        /// Should use element's FontSize (zero).
        /// </summary>
        [Fact]
        public void ToFont_FontSizeZeroNoDefaultSize_UsesZeroSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(0.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(0.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with negative FontSize and no defaultSize parameter.
        /// Should use element's FontSize (negative value).
        /// </summary>
        [Fact]
        public void ToFont_FontSizeNegativeNoDefaultSize_UsesNegativeSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(-5.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(-5.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with NaN FontSize and no defaultSize parameter.
        /// Should use element's FontSize (NaN).
        /// </summary>
        [Fact]
        public void ToFont_FontSizeNaNNoDefaultSize_UsesNaNSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(double.NaN);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.True(double.IsNaN(result.Size));
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with Bold font attributes.
        /// Should create Font with Bold weight and default slant.
        /// </summary>
        [Fact]
        public void ToFont_BoldFontAttributes_CreatesBoldFont()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.Bold);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.Equal(FontWeight.Bold, result.Weight);
            Assert.Equal(FontSlant.Default, result.Slant);
        }

        /// <summary>
        /// Tests ToFont with Italic font attributes.
        /// Should create Font with regular weight and italic slant.
        /// </summary>
        [Fact]
        public void ToFont_ItalicFontAttributes_CreatesItalicFont()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.Italic);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.Equal(FontWeight.Regular, result.Weight);
            Assert.Equal(FontSlant.Italic, result.Slant);
        }

        /// <summary>
        /// Tests ToFont with Bold and Italic font attributes.
        /// Should create Font with bold weight and italic slant.
        /// </summary>
        [Fact]
        public void ToFont_BoldItalicFontAttributes_CreatesBoldItalicFont()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.Bold | FontAttributes.Italic);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.Equal(FontWeight.Bold, result.Weight);
            Assert.Equal(FontSlant.Italic, result.Slant);
        }

        /// <summary>
        /// Tests ToFont with FontAutoScalingEnabled set to false.
        /// Should create Font with auto scaling disabled.
        /// </summary>
        [Fact]
        public void ToFont_FontAutoScalingDisabled_CreatesNonScalingFont()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(false);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.False(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with null FontFamily.
        /// Should create Font with null family.
        /// </summary>
        [Fact]
        public void ToFont_NullFontFamily_CreatesNullFamilyFont()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns((string)null);
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Null(result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with empty FontFamily.
        /// Should create Font with empty family.
        /// </summary>
        [Fact]
        public void ToFont_EmptyFontFamily_CreatesEmptyFamilyFont()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(14.0);
            element.FontFamily.Returns(string.Empty);
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont();

            // Assert
            Assert.Equal(string.Empty, result.Family);
            Assert.Equal(14.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with extreme double values for defaultSize.
        /// Should use defaultSize when FontSize is invalid and handle edge cases properly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ToFont_InvalidFontSizeWithExtremeDefaultSize_UsesDefaultSize(double defaultSize)
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(-1.0); // Invalid size
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);

            // Act
            var result = element.ToFont(defaultSize);

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(defaultSize, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with various invalid FontSize values and valid defaultSize.
        /// Should use defaultSize for all invalid FontSize values.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(-100.0)]
        [InlineData(double.MinValue)]
        public void ToFont_InvalidFontSizeValues_UsesDefaultSize(double invalidFontSize)
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(invalidFontSize);
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);
            double defaultSize = 12.0;

            // Act
            var result = element.ToFont(defaultSize);

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.Equal(12.0, result.Size);
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests ToFont with NaN defaultSize and invalid FontSize.
        /// Should use NaN as the size when FontSize is invalid and defaultSize is NaN.
        /// </summary>
        [Fact]
        public void ToFont_InvalidFontSizeWithNaNDefaultSize_UsesNaNSize()
        {
            // Arrange
            var element = Substitute.For<IFontElement>();
            element.FontSize.Returns(-1.0); // Invalid size
            element.FontFamily.Returns("Arial");
            element.FontAutoScalingEnabled.Returns(true);
            element.FontAttributes.Returns(FontAttributes.None);
            double defaultSize = double.NaN;

            // Act
            var result = element.ToFont(defaultSize);

            // Assert
            Assert.Equal("Arial", result.Family);
            Assert.True(double.IsNaN(result.Size));
            Assert.True(result.AutoScalingEnabled);
        }

        /// <summary>
        /// Tests GetFontAttributes method with various combinations of font weight and slant.
        /// Verifies that the method correctly converts Font properties to FontAttributes.
        /// </summary>
        /// <param name="weight">The font weight to test</param>
        /// <param name="slant">The font slant to test</param>
        /// <param name="expected">The expected FontAttributes result</param>
        [Theory]
        [InlineData(FontWeight.Bold, FontSlant.Default, FontAttributes.Bold)]
        [InlineData(FontWeight.Bold, FontSlant.Italic, FontAttributes.Bold | FontAttributes.Italic)]
        [InlineData(FontWeight.Bold, FontSlant.Oblique, FontAttributes.Bold | FontAttributes.Italic)]
        [InlineData(FontWeight.Regular, FontSlant.Default, FontAttributes.None)]
        [InlineData(FontWeight.Regular, FontSlant.Italic, FontAttributes.Italic)]
        [InlineData(FontWeight.Regular, FontSlant.Oblique, FontAttributes.Italic)]
        [InlineData(FontWeight.Thin, FontSlant.Default, FontAttributes.None)]
        [InlineData(FontWeight.Light, FontSlant.Italic, FontAttributes.Italic)]
        [InlineData(FontWeight.Medium, FontSlant.Default, FontAttributes.None)]
        [InlineData(FontWeight.Semibold, FontSlant.Italic, FontAttributes.Italic)]
        [InlineData(FontWeight.Heavy, FontSlant.Default, FontAttributes.None)]
        [InlineData(FontWeight.Black, FontSlant.Oblique, FontAttributes.Italic)]
        public void GetFontAttributes_WithVariousWeightAndSlantCombinations_ReturnsExpectedAttributes(
            FontWeight weight, FontSlant slant, FontAttributes expected)
        {
            // Arrange
            var font = Font.Default.WithWeight(weight).WithSlant(slant);

            // Act
            var result = font.GetFontAttributes();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests GetFontAttributes method with default font.
        /// Verifies that the default font returns None attributes.
        /// </summary>
        [Fact]
        public void GetFontAttributes_WithDefaultFont_ReturnsNone()
        {
            // Arrange
            var font = Font.Default;

            // Act
            var result = font.GetFontAttributes();

            // Assert
            Assert.Equal(FontAttributes.None, result);
        }

        /// <summary>
        /// Tests GetFontAttributes method with font having only bold weight.
        /// Verifies that bold weight with default slant returns Bold attributes.
        /// </summary>
        [Fact]
        public void GetFontAttributes_WithBoldWeightAndDefaultSlant_ReturnsBold()
        {
            // Arrange
            var font = Font.Default.WithWeight(FontWeight.Bold);

            // Act
            var result = font.GetFontAttributes();

            // Assert
            Assert.Equal(FontAttributes.Bold, result);
        }

        /// <summary>
        /// Tests GetFontAttributes method with font having only italic slant.
        /// Verifies that regular weight with italic slant returns Italic attributes.
        /// </summary>
        [Fact]
        public void GetFontAttributes_WithRegularWeightAndItalicSlant_ReturnsItalic()
        {
            // Arrange
            var font = Font.Default.WithSlant(FontSlant.Italic);

            // Act
            var result = font.GetFontAttributes();

            // Assert
            Assert.Equal(FontAttributes.Italic, result);
        }

        /// <summary>
        /// Tests GetFontAttributes method with font having both bold weight and italic slant.
        /// Verifies that bold weight with italic slant returns combined Bold and Italic attributes.
        /// </summary>
        [Fact]
        public void GetFontAttributes_WithBoldWeightAndItalicSlant_ReturnsBoldAndItalic()
        {
            // Arrange
            var font = Font.Default.WithWeight(FontWeight.Bold).WithSlant(FontSlant.Italic);

            // Act
            var result = font.GetFontAttributes();

            // Assert
            Assert.Equal(FontAttributes.Bold | FontAttributes.Italic, result);
        }
    }
}