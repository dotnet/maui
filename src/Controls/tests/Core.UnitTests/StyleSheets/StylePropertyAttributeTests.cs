#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.StyleSheets;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class StylePropertyAttributeTests
    {
        /// <summary>
        /// Tests that the constructor correctly assigns all parameters to their respective properties with valid inputs.
        /// Input conditions: Valid non-null string parameters and a valid Type.
        /// Expected result: All properties are set to the provided parameter values.
        /// </summary>
        [Fact]
        public void Constructor_ValidInputs_AssignsAllPropertiesCorrectly()
        {
            // Arrange
            string cssPropertyName = "background-color";
            string bindablePropertyName = "BackgroundColor";
            Type targetType = typeof(string);

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles null cssPropertyName parameter correctly.
        /// Input conditions: Null cssPropertyName with valid other parameters.
        /// Expected result: CssPropertyName property is set to null.
        /// </summary>
        [Fact]
        public void Constructor_NullCssPropertyName_AssignsNull()
        {
            // Arrange
            string cssPropertyName = null;
            string bindablePropertyName = "BackgroundColor";
            Type targetType = typeof(int);

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Null(attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles null bindablePropertyName parameter correctly.
        /// Input conditions: Null bindablePropertyName with valid other parameters.
        /// Expected result: BindablePropertyName property is set to null.
        /// </summary>
        [Fact]
        public void Constructor_NullBindablePropertyName_AssignsNull()
        {
            // Arrange
            string cssPropertyName = "margin";
            string bindablePropertyName = null;
            Type targetType = typeof(double);

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Null(attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles null targetType parameter correctly.
        /// Input conditions: Null targetType with valid string parameters.
        /// Expected result: TargetType property is set to null.
        /// </summary>
        [Fact]
        public void Constructor_NullTargetType_AssignsNull()
        {
            // Arrange
            string cssPropertyName = "padding";
            string bindablePropertyName = "Padding";
            Type targetType = null;

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Null(attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles all null parameters correctly.
        /// Input conditions: All parameters are null.
        /// Expected result: All properties are set to null.
        /// </summary>
        [Fact]
        public void Constructor_AllNullParameters_AssignsAllNull()
        {
            // Arrange
            string cssPropertyName = null;
            string bindablePropertyName = null;
            Type targetType = null;

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Null(attribute.CssPropertyName);
            Assert.Null(attribute.BindablePropertyName);
            Assert.Null(attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles empty string parameters correctly.
        /// Input conditions: Empty strings for cssPropertyName and bindablePropertyName.
        /// Expected result: Properties are set to empty strings.
        /// </summary>
        [Theory]
        [InlineData("", "", typeof(object))]
        [InlineData("", "ValidProperty", typeof(string))]
        [InlineData("valid-css", "", typeof(int))]
        public void Constructor_EmptyStringParameters_AssignsEmptyStrings(string cssPropertyName, string bindablePropertyName, Type targetType)
        {
            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles whitespace-only string parameters correctly.
        /// Input conditions: Whitespace-only strings for cssPropertyName and bindablePropertyName.
        /// Expected result: Properties are set to the whitespace strings.
        /// </summary>
        [Theory]
        [InlineData("   ", "   ", typeof(bool))]
        [InlineData("\t", "\n", typeof(char))]
        [InlineData(" \r\n\t ", "ValidProperty", typeof(decimal))]
        public void Constructor_WhitespaceStringParameters_AssignsWhitespaceStrings(string cssPropertyName, string bindablePropertyName, Type targetType)
        {
            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles very long string parameters correctly.
        /// Input conditions: Very long strings for cssPropertyName and bindablePropertyName.
        /// Expected result: Properties are set to the long strings.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongStrings_AssignsLongStrings()
        {
            // Arrange
            string cssPropertyName = new string('a', 10000);
            string bindablePropertyName = new string('b', 10000);
            Type targetType = typeof(string);

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor handles special characters in string parameters correctly.
        /// Input conditions: Strings containing special, control, and Unicode characters.
        /// Expected result: Properties are set to the strings with special characters.
        /// </summary>
        [Theory]
        [InlineData("css-prop-!@#$%^&*()", "Property_123", typeof(float))]
        [InlineData("αβγδε", "Ωψχφυ", typeof(long))]
        [InlineData("property\0\u0001\u001F", "bindable\u007F\u0080", typeof(short))]
        public void Constructor_SpecialCharactersInStrings_AssignsSpecialCharacters(string cssPropertyName, string bindablePropertyName, Type targetType)
        {
            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor works with different types of Type parameters.
        /// Input conditions: Various Type objects including interface, abstract class, sealed class, generic types.
        /// Expected result: TargetType property is set to the provided Type.
        /// </summary>
        [Theory]
        [InlineData(typeof(IDisposable))]
        [InlineData(typeof(System.IO.Stream))]
        [InlineData(typeof(string))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(System.Collections.Generic.List<string>))]
        [InlineData(typeof(System.Nullable<int>))]
        public void Constructor_DifferentTypeParameters_AssignsCorrectType(Type targetType)
        {
            // Arrange
            string cssPropertyName = "test-property";
            string bindablePropertyName = "TestProperty";

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Equal(cssPropertyName, attribute.CssPropertyName);
            Assert.Equal(bindablePropertyName, attribute.BindablePropertyName);
            Assert.Equal(targetType, attribute.TargetType);
        }

        /// <summary>
        /// Tests that the constructor initializes settable properties to their default values.
        /// Input conditions: Valid parameters for constructor.
        /// Expected result: PropertyOwnerType is null, BindableProperty is null, Inherited is false.
        /// </summary>
        [Fact]
        public void Constructor_ValidInputs_InitializesSettablePropertiesToDefaults()
        {
            // Arrange
            string cssPropertyName = "font-size";
            string bindablePropertyName = "FontSize";
            Type targetType = typeof(double);

            // Act
            var attribute = new StylePropertyAttribute(cssPropertyName, targetType, bindablePropertyName);

            // Assert
            Assert.Null(attribute.PropertyOwnerType);
            Assert.Null(attribute.BindableProperty);
            Assert.False(attribute.Inherited);
        }
    }
}
