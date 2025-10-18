#nullable disable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class RenderWithAttributeTests
    {
        /// <summary>
        /// Tests that the single-parameter RenderWithAttribute constructor properly initializes with a valid Type.
        /// Verifies that Type property is set to the provided type and SupportedVisuals contains DefaultVisual.
        /// </summary>
        [Fact]
        public void Constructor_ValidType_SetsPropertiesCorrectly()
        {
            // Arrange
            var testType = typeof(string);

            // Act
            var attribute = new RenderWithAttribute(testType);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the single-parameter RenderWithAttribute constructor handles null Type parameter.
        /// Verifies that Type property is set to null and SupportedVisuals still contains DefaultVisual.
        /// </summary>
        [Fact]
        public void Constructor_NullType_SetsTypeToNullAndDefaultVisuals()
        {
            // Arrange
            Type nullType = null;

            // Act
            var attribute = new RenderWithAttribute(nullType);

            // Assert
            Assert.Null(attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the single-parameter RenderWithAttribute constructor works with various Type instances.
        /// Verifies behavior with concrete classes, abstract classes, interfaces, and generic types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(IDisposable))]
        [InlineData(typeof(System.Collections.Generic.List<>))]
        [InlineData(typeof(System.Collections.Generic.List<int>))]
        public void Constructor_VariousTypes_SetsPropertiesCorrectly(Type testType)
        {
            // Arrange & Act
            var attribute = new RenderWithAttribute(testType);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the single-parameter RenderWithAttribute constructor works with abstract class types.
        /// Verifies that abstract types can be passed and are properly stored.
        /// </summary>
        [Fact]
        public void Constructor_AbstractType_SetsPropertiesCorrectly()
        {
            // Arrange
            var abstractType = typeof(System.IO.Stream);

            // Act
            var attribute = new RenderWithAttribute(abstractType);

            // Assert
            Assert.Equal(abstractType, attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the single-parameter RenderWithAttribute constructor works with interface types.
        /// Verifies that interface types can be passed and are properly stored.
        /// </summary>
        [Fact]
        public void Constructor_InterfaceType_SetsPropertiesCorrectly()
        {
            // Arrange
            var interfaceType = typeof(IComparable);

            // Act
            var attribute = new RenderWithAttribute(interfaceType);

            // Assert
            Assert.Equal(interfaceType, attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the single-parameter RenderWithAttribute constructor works with nested types.
        /// Verifies that nested class types can be passed and are properly stored.
        /// </summary>
        [Fact]
        public void Constructor_NestedType_SetsPropertiesCorrectly()
        {
            // Arrange
            var nestedType = typeof(Environment.SpecialFolder);

            // Act
            var attribute = new RenderWithAttribute(nestedType);

            // Assert
            Assert.Equal(nestedType, attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the constructor properly assigns the type parameter to the Type property
        /// and handles null supportedVisuals by setting SupportedVisuals to default array containing DefaultVisual.
        /// </summary>
        [Fact]
        public void Constructor_ValidTypeAndNullSupportedVisuals_SetsTypeAndDefaultSupportedVisuals()
        {
            // Arrange
            var testType = typeof(string);
            Type[] supportedVisuals = null;

            // Act
            var attribute = new RenderWithAttribute(testType, supportedVisuals);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the constructor properly assigns both type and supportedVisuals parameters
        /// when supportedVisuals is not null.
        /// </summary>
        [Fact]
        public void Constructor_ValidTypeAndNonNullSupportedVisuals_SetsTypeAndProvidedSupportedVisuals()
        {
            // Arrange
            var testType = typeof(int);
            var supportedVisuals = new Type[] { typeof(VisualMarker.DefaultVisual), typeof(object) };

            // Act
            var attribute = new RenderWithAttribute(testType, supportedVisuals);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.Equal(supportedVisuals, attribute.SupportedVisuals);
            Assert.Equal(2, attribute.SupportedVisuals.Length);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
            Assert.Equal(typeof(object), attribute.SupportedVisuals[1]);
        }

        /// <summary>
        /// Tests that the constructor handles null type parameter and null supportedVisuals
        /// by setting Type to null and SupportedVisuals to default array.
        /// </summary>
        [Fact]
        public void Constructor_NullTypeAndNullSupportedVisuals_SetsNullTypeAndDefaultSupportedVisuals()
        {
            // Arrange
            Type testType = null;
            Type[] supportedVisuals = null;

            // Act
            var attribute = new RenderWithAttribute(testType, supportedVisuals);

            // Assert
            Assert.Null(attribute.Type);
            Assert.NotNull(attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the constructor properly handles empty supportedVisuals array
        /// by assigning it directly without defaulting to DefaultVisual.
        /// </summary>
        [Fact]
        public void Constructor_ValidTypeAndEmptySupportedVisuals_SetsTypeAndEmptySupportedVisuals()
        {
            // Arrange
            var testType = typeof(double);
            var supportedVisuals = new Type[0];

            // Act
            var attribute = new RenderWithAttribute(testType, supportedVisuals);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.Equal(supportedVisuals, attribute.SupportedVisuals);
            Assert.Empty(attribute.SupportedVisuals);
        }

        /// <summary>
        /// Tests that the constructor handles supportedVisuals array with multiple different types
        /// and assigns them correctly to the SupportedVisuals property.
        /// </summary>
        [Fact]
        public void Constructor_ValidTypeAndMultipleSupportedVisuals_SetsTypeAndAllSupportedVisuals()
        {
            // Arrange
            var testType = typeof(RenderWithAttribute);
            var supportedVisuals = new Type[] { typeof(string), typeof(int), typeof(VisualMarker.DefaultVisual) };

            // Act
            var attribute = new RenderWithAttribute(testType, supportedVisuals);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.Equal(supportedVisuals, attribute.SupportedVisuals);
            Assert.Equal(3, attribute.SupportedVisuals.Length);
            Assert.Equal(typeof(string), attribute.SupportedVisuals[0]);
            Assert.Equal(typeof(int), attribute.SupportedVisuals[1]);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[2]);
        }

        /// <summary>
        /// Tests that the constructor handles supportedVisuals array containing null elements
        /// by preserving the null elements in the SupportedVisuals property.
        /// </summary>
        [Fact]
        public void Constructor_ValidTypeAndSupportedVisualsWithNullElements_SetsTypeAndPreservesNullElements()
        {
            // Arrange
            var testType = typeof(bool);
            var supportedVisuals = new Type[] { typeof(string), null, typeof(VisualMarker.DefaultVisual) };

            // Act
            var attribute = new RenderWithAttribute(testType, supportedVisuals);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.Equal(supportedVisuals, attribute.SupportedVisuals);
            Assert.Equal(3, attribute.SupportedVisuals.Length);
            Assert.Equal(typeof(string), attribute.SupportedVisuals[0]);
            Assert.Null(attribute.SupportedVisuals[1]);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[2]);
        }

        /// <summary>
        /// Tests that the constructor handles single element supportedVisuals array
        /// by assigning it directly to the SupportedVisuals property.
        /// </summary>
        [Fact]
        public void Constructor_ValidTypeAndSingleElementSupportedVisuals_SetsTypeAndSingleSupportedVisual()
        {
            // Arrange
            var testType = typeof(char);
            var supportedVisuals = new Type[] { typeof(object) };

            // Act
            var attribute = new RenderWithAttribute(testType, supportedVisuals);

            // Assert
            Assert.Equal(testType, attribute.Type);
            Assert.Equal(supportedVisuals, attribute.SupportedVisuals);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(object), attribute.SupportedVisuals[0]);
        }
    }
}