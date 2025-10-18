#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ParameterAttributeTests
    {
        /// <summary>
        /// Tests that the ParameterAttribute constructor properly initializes the Name property
        /// with various string inputs including edge cases like null, empty, whitespace, and special characters.
        /// </summary>
        /// <param name="name">The name parameter to pass to the constructor</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("parameterName")]
        [InlineData("MyParameter")]
        [InlineData("Parameter_With_Underscores")]
        [InlineData("Parameter-With-Dashes")]
        [InlineData("Parameter.With.Dots")]
        [InlineData("Parameter123")]
        [InlineData("123Parameter")]
        [InlineData("Параметр")] // Unicode characters
        [InlineData("参数")] // Chinese characters
        [InlineData("🎯📝")] // Emojis
        [InlineData("Special!@#$%^&*()Characters")]
        [InlineData("Very long parameter name that exceeds typical length expectations and contains many characters to test boundary conditions")]
        public void Constructor_WithVariousStringInputs_SetsNamePropertyCorrectly(string name)
        {
            // Arrange & Act
            var attribute = new ParameterAttribute(name);

            // Assert
            Assert.Equal(name, attribute.Name);
        }

        /// <summary>
        /// Tests that the ParameterAttribute constructor handles control characters correctly
        /// by setting the Name property to the exact input value including control characters.
        /// </summary>
        [Fact]
        public void Constructor_WithControlCharacters_SetsNamePropertyCorrectly()
        {
            // Arrange
            string nameWithControlChars = "Parameter\0\x01\x02Name";

            // Act
            var attribute = new ParameterAttribute(nameWithControlChars);

            // Assert
            Assert.Equal(nameWithControlChars, attribute.Name);
        }

        /// <summary>
        /// Tests that the ParameterAttribute constructor handles extremely long strings correctly
        /// by setting the Name property to the exact input value without truncation or modification.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongString_SetsNamePropertyCorrectly()
        {
            // Arrange
            string veryLongName = new string('a', 10000);

            // Act
            var attribute = new ParameterAttribute(veryLongName);

            // Assert
            Assert.Equal(veryLongName, attribute.Name);
        }
    }
}
