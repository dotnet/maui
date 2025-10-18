#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public sealed class ExportEffectAttributeTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes Type and Id properties with valid inputs.
        /// </summary>
        [Fact]
        public void Constructor_ValidInputs_SetsPropertiesCorrectly()
        {
            // Arrange
            var effectType = typeof(string);
            var uniqueName = "TestEffect";

            // Act
            var attribute = new ExportEffectAttribute(effectType, uniqueName);

            // Assert
            Assert.Equal(effectType, attribute.Type);
            Assert.Equal(uniqueName, attribute.Id);
        }

        /// <summary>
        /// Tests that the constructor accepts null effectType parameter without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_NullEffectType_SetsTypeToNull()
        {
            // Arrange
            Type effectType = null;
            var uniqueName = "TestEffect";

            // Act
            var attribute = new ExportEffectAttribute(effectType, uniqueName);

            // Assert
            Assert.Null(attribute.Type);
            Assert.Equal(uniqueName, attribute.Id);
        }

        /// <summary>
        /// Tests that the constructor accepts empty string uniqueName without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_EmptyUniqueName_SetsIdToEmptyString()
        {
            // Arrange
            var effectType = typeof(int);
            var uniqueName = "";

            // Act
            var attribute = new ExportEffectAttribute(effectType, uniqueName);

            // Assert
            Assert.Equal(effectType, attribute.Type);
            Assert.Equal("", attribute.Id);
        }

        /// <summary>
        /// Tests that the constructor accepts whitespace-only uniqueName without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_WhitespaceUniqueName_SetsIdToWhitespace()
        {
            // Arrange
            var effectType = typeof(double);
            var uniqueName = "   ";

            // Act
            var attribute = new ExportEffectAttribute(effectType, uniqueName);

            // Assert
            Assert.Equal(effectType, attribute.Type);
            Assert.Equal("   ", attribute.Id);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentException when uniqueName contains a period.
        /// </summary>
        [Theory]
        [InlineData("Effect.Name")]
        [InlineData(".EffectName")]
        [InlineData("EffectName.")]
        [InlineData("My.Effect.Name")]
        [InlineData("a.b")]
        public void Constructor_UniqueNameContainsPeriod_ThrowsArgumentException(string uniqueName)
        {
            // Arrange
            var effectType = typeof(string);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ExportEffectAttribute(effectType, uniqueName));
            Assert.Equal("uniqueName must not contain a .", exception.Message);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentException when uniqueName is null and contains period check fails.
        /// This test verifies the behavior when null is passed to IndexOf method.
        /// </summary>
        [Fact]
        public void Constructor_NullUniqueName_ThrowsArgumentException()
        {
            // Arrange
            var effectType = typeof(string);
            string uniqueName = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ExportEffectAttribute(effectType, uniqueName));
        }

        /// <summary>
        /// Tests that the constructor works correctly with various valid uniqueName values that don't contain periods.
        /// </summary>
        [Theory]
        [InlineData("SimpleEffect")]
        [InlineData("Effect123")]
        [InlineData("My_Effect")]
        [InlineData("Effect-Name")]
        [InlineData("EffectWithSpecialChars!@#$%^&*()")]
        [InlineData("VeryLongEffectNameThatShouldStillWorkFineAsLongAsItDoesNotContainAnyPeriods")]
        public void Constructor_ValidUniqueNames_SetsPropertiesCorrectly(string uniqueName)
        {
            // Arrange
            var effectType = typeof(object);

            // Act
            var attribute = new ExportEffectAttribute(effectType, uniqueName);

            // Assert
            Assert.Equal(effectType, attribute.Type);
            Assert.Equal(uniqueName, attribute.Id);
        }
    }
}
