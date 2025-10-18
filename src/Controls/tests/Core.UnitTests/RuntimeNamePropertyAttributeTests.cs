#nullable disable

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class RuntimeNamePropertyAttributeTests
    {
        /// <summary>
        /// Tests that the RuntimeNamePropertyAttribute constructor properly initializes the Name property
        /// with various valid string inputs including normal strings, empty strings, whitespace, and special characters.
        /// </summary>
        /// <param name="name">The name parameter to pass to the constructor</param>
        [Theory]
        [InlineData("ValidName")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Name with spaces")]
        [InlineData("Name_with_underscores")]
        [InlineData("Name-with-dashes")]
        [InlineData("Name123WithNumbers")]
        [InlineData("NameWithSpecialChars!@#$%^&*()")]
        [InlineData("VeryLongNameThatExceedsTypicalLengthLimitsToTestBoundaryConditionsAndEnsureTheConstructorHandlesLongStringsCorrectly")]
        [InlineData("\t\n\r")]
        public void Constructor_WithValidString_SetsNameProperty(string name)
        {
            // Arrange & Act
            var attribute = new RuntimeNamePropertyAttribute(name);

            // Assert
            Assert.Equal(name, attribute.Name);
        }

        /// <summary>
        /// Tests that the RuntimeNamePropertyAttribute constructor accepts null values
        /// and properly sets the Name property to null since nullable reference types are disabled.
        /// </summary>
        [Fact]
        public void Constructor_WithNull_SetsNamePropertyToNull()
        {
            // Arrange & Act
            var attribute = new RuntimeNamePropertyAttribute(null);

            // Assert
            Assert.Null(attribute.Name);
        }
    }
}
