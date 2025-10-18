#nullable disable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the TemplateExtensions class.
    /// </summary>
    public class TemplateExtensionsTests
    {
        /// <summary>
        /// Tests that SetBinding throws ArgumentNullException when self parameter is null.
        /// This test exercises the null check condition and ensures proper parameter validation.
        /// Expected result: ArgumentNullException with parameter name "self".
        /// </summary>
        [Fact]
        public void SetBinding_SelfIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            DataTemplate self = null;
            var targetProperty = Substitute.For<BindableProperty>();
            string path = "TestPath";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                TemplateExtensions.SetBinding(self, targetProperty, path));
            Assert.Equal("self", exception.ParamName);
        }

        /// <summary>
        /// Tests that SetBinding throws ArgumentNullException when targetProperty parameter is null.
        /// This test verifies that the underlying DataTemplate.SetBinding method properly validates the property parameter.
        /// Expected result: ArgumentNullException with parameter name "property".
        /// </summary>
        [Fact]
        public void SetBinding_TargetPropertyIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var self = Substitute.For<DataTemplate>();
            BindableProperty targetProperty = null;
            string path = "TestPath";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                TemplateExtensions.SetBinding(self, targetProperty, path));
            Assert.Equal("property", exception.ParamName);
        }

        /// <summary>
        /// Tests that SetBinding throws ArgumentNullException when path parameter is null.
        /// This test verifies that the Binding constructor properly validates the path parameter.
        /// Expected result: ArgumentNullException with parameter name "path".
        /// </summary>
        [Fact]
        public void SetBinding_PathIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var self = Substitute.For<DataTemplate>();
            var targetProperty = Substitute.For<BindableProperty>();
            string path = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                TemplateExtensions.SetBinding(self, targetProperty, path));
            Assert.Equal("path", exception.ParamName);
        }

        /// <summary>
        /// Tests that SetBinding throws ArgumentException when path parameter is an empty string.
        /// This test verifies that the Binding constructor properly validates empty path strings.
        /// Expected result: ArgumentException with parameter name "path" and specific message.
        /// </summary>
        [Fact]
        public void SetBinding_PathIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var self = Substitute.For<DataTemplate>();
            var targetProperty = Substitute.For<BindableProperty>();
            string path = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                TemplateExtensions.SetBinding(self, targetProperty, path));
            Assert.Equal("path", exception.ParamName);
            Assert.Contains("path cannot be an empty string", exception.Message);
        }

        /// <summary>
        /// Tests that SetBinding throws ArgumentException when path parameter contains only whitespace.
        /// This test verifies that the Binding constructor properly validates whitespace-only path strings.
        /// Expected result: ArgumentException with parameter name "path" and specific message.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("   \t  \n  ")]
        public void SetBinding_PathIsWhitespace_ThrowsArgumentException(string path)
        {
            // Arrange
            var self = Substitute.For<DataTemplate>();
            var targetProperty = Substitute.For<BindableProperty>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                TemplateExtensions.SetBinding(self, targetProperty, path));
            Assert.Equal("path", exception.ParamName);
            Assert.Contains("path cannot be an empty string", exception.Message);
        }

        /// <summary>
        /// Tests that SetBinding successfully calls DataTemplate.SetBinding with a new Binding when all parameters are valid.
        /// This test verifies the happy path behavior with valid inputs.
        /// Expected result: DataTemplate.SetBinding is called with the target property and a new Binding created from the path.
        /// </summary>
        [Theory]
        [InlineData("TestPath")]
        [InlineData("Property.NestedProperty")]
        [InlineData("Items[0].Name")]
        [InlineData("a")]
        [InlineData("VeryLongPropertyPathWithManyNestedProperties.Level1.Level2.Level3.FinalProperty")]
        public void SetBinding_ValidParameters_CallsDataTemplateSetBinding(string path)
        {
            // Arrange
            var self = Substitute.For<DataTemplate>();
            var targetProperty = Substitute.For<BindableProperty>();

            // Act
            TemplateExtensions.SetBinding(self, targetProperty, path);

            // Assert
            self.Received(1).SetBinding(targetProperty, Arg.Any<Binding>());
        }
    }
}
