#nullable disable

using System;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the BindablePropertyKey class.
    /// </summary>
    public sealed class BindablePropertyKeyTests
    {
        /// <summary>
        /// Tests that the BindablePropertyKey constructor throws ArgumentNullException when property parameter is null.
        /// This test exercises the null validation logic in the constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithNullProperty_ThrowsArgumentNullException()
        {
            // Arrange
            BindableProperty nullProperty = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new BindablePropertyKey(nullProperty));
            Assert.Equal("property", exception.ParamName);
        }

        /// <summary>
        /// Tests that the BindablePropertyKey constructor successfully creates an instance with valid property.
        /// This test verifies that a valid BindableProperty parameter results in correct initialization.
        /// </summary>
        [Fact]
        public void Constructor_WithValidProperty_SetsBindablePropertyCorrectly()
        {
            // Arrange
            var bindableProperty = BindableProperty.Create("TestProperty", typeof(string), typeof(BindablePropertyKeyTests));

            // Act
            var bindablePropertyKey = new BindablePropertyKey(bindableProperty);

            // Assert
            Assert.Same(bindableProperty, bindablePropertyKey.BindableProperty);
        }

        /// <summary>
        /// Tests that the BindablePropertyKey constructor works with different property types.
        /// This test verifies the constructor behavior with various BindableProperty configurations.
        /// </summary>
        [Theory]
        [InlineData(typeof(int), 42)]
        [InlineData(typeof(string), "default")]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(double), 3.14)]
        public void Constructor_WithDifferentPropertyTypes_SetsBindablePropertyCorrectly(Type propertyType, object defaultValue)
        {
            // Arrange
            var bindableProperty = BindableProperty.Create("TestProperty", propertyType, typeof(BindablePropertyKeyTests), defaultValue);

            // Act
            var bindablePropertyKey = new BindablePropertyKey(bindableProperty);

            // Assert
            Assert.Same(bindableProperty, bindablePropertyKey.BindableProperty);
            Assert.Equal(propertyType, bindablePropertyKey.BindableProperty.ReturnType);
            Assert.Equal(defaultValue, bindablePropertyKey.BindableProperty.DefaultValue);
        }
    }
}
