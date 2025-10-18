#nullable disable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class VisualAttributeTests
    {
        /// <summary>
        /// Tests that the VisualAttribute constructor properly assigns the key parameter to the Key property
        /// when provided with a valid string key and Type.
        /// </summary>
        [Theory]
        [InlineData("validKey")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("key with spaces")]
        [InlineData("key@#$%^&*()")]
        [InlineData("very_long_key_with_many_characters_that_exceeds_normal_length_expectations_and_continues_for_a_while")]
        public void Constructor_WithValidKeyAndType_AssignsKeyCorrectly(string key)
        {
            // Arrange
            var visualType = typeof(string);

            // Act
            var attribute = new VisualAttribute(key, visualType);

            // Assert
            Assert.Equal(key, attribute.Key);
        }

        /// <summary>
        /// Tests that the VisualAttribute constructor properly assigns the visual parameter to the Visual property
        /// when provided with various Type instances.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(IDisposable))]
        [InlineData(typeof(Attribute))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(System.Collections.Generic.List<>))]
        public void Constructor_WithValidTypes_AssignsVisualCorrectly(Type visualType)
        {
            // Arrange
            var key = "testKey";

            // Act
            var attribute = new VisualAttribute(key, visualType);

            // Assert
            Assert.Equal(visualType, attribute.Visual);
        }

        /// <summary>
        /// Tests that the VisualAttribute constructor accepts null key parameter
        /// and assigns it correctly to the Key property.
        /// </summary>
        [Fact]
        public void Constructor_WithNullKey_AssignsNullToKey()
        {
            // Arrange
            string key = null;
            var visualType = typeof(string);

            // Act
            var attribute = new VisualAttribute(key, visualType);

            // Assert
            Assert.Null(attribute.Key);
        }

        /// <summary>
        /// Tests that the VisualAttribute constructor accepts null visual parameter
        /// and assigns it correctly to the Visual property.
        /// </summary>
        [Fact]
        public void Constructor_WithNullVisual_AssignsNullToVisual()
        {
            // Arrange
            var key = "testKey";
            Type visual = null;

            // Act
            var attribute = new VisualAttribute(key, visual);

            // Assert
            Assert.Null(attribute.Visual);
        }

        /// <summary>
        /// Tests that the VisualAttribute constructor accepts both null parameters
        /// and assigns them correctly to their respective properties.
        /// </summary>
        [Fact]
        public void Constructor_WithBothParametersNull_AssignsBothNulls()
        {
            // Arrange
            string key = null;
            Type visual = null;

            // Act
            var attribute = new VisualAttribute(key, visual);

            // Assert
            Assert.Null(attribute.Key);
            Assert.Null(attribute.Visual);
        }

        /// <summary>
        /// Tests that the VisualAttribute constructor works correctly with various combinations
        /// of key and visual parameter values.
        /// </summary>
        [Theory]
        [InlineData("key1", typeof(int))]
        [InlineData("", typeof(string))]
        [InlineData("specialKey!@#", typeof(object))]
        [InlineData("normalKey", typeof(IDisposable))]
        public void Constructor_WithVariousKeyTypesCombinations_AssignsBothCorrectly(string key, Type visualType)
        {
            // Act
            var attribute = new VisualAttribute(key, visualType);

            // Assert
            Assert.Equal(key, attribute.Key);
            Assert.Equal(visualType, attribute.Visual);
        }
    }
}
