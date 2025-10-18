#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the Configuration constructor.
    /// </summary>
    public partial class ConfigurationTests
    {
        /// <summary>
        /// Tests that the Configuration constructor properly assigns a valid element to the Element property.
        /// Verifies that the exact same reference is stored and can be retrieved.
        /// </summary>
        [Fact]
        public void Constructor_ValidElement_AssignsElementProperty()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();

            // Act
            var configuration = new Configuration<IConfigPlatform, Element>(mockElement);

            // Assert
            Assert.Same(mockElement, configuration.Element);
        }

        /// <summary>
        /// Tests that the Configuration constructor accepts a null element parameter.
        /// Since nullable reference types are disabled, null should be accepted without throwing.
        /// </summary>
        [Fact]
        public void Constructor_NullElement_AcceptsNullValue()
        {
            // Arrange
            Element nullElement = null;

            // Act
            var configuration = new Configuration<IConfigPlatform, Element>(nullElement);

            // Assert
            Assert.Null(configuration.Element);
        }

        /// <summary>
        /// Tests that the Configuration constructor maintains reference integrity.
        /// Verifies that the Element property returns the exact same object reference that was passed to the constructor.
        /// </summary>
        [Fact]
        public void Constructor_ElementReference_MaintainsReferenceIntegrity()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var originalHashCode = mockElement.GetHashCode();

            // Act
            var configuration = new Configuration<IConfigPlatform, Element>(mockElement);

            // Assert
            Assert.Same(mockElement, configuration.Element);
            Assert.Equal(originalHashCode, configuration.Element.GetHashCode());
        }

        /// <summary>
        /// Tests that multiple Configuration instances with the same element maintain independent references.
        /// Verifies that each instance stores its own reference correctly.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_MaintainIndependentReferences()
        {
            // Arrange
            var mockElement1 = Substitute.For<Element>();
            var mockElement2 = Substitute.For<Element>();

            // Act
            var configuration1 = new Configuration<IConfigPlatform, Element>(mockElement1);
            var configuration2 = new Configuration<IConfigPlatform, Element>(mockElement2);

            // Assert
            Assert.Same(mockElement1, configuration1.Element);
            Assert.Same(mockElement2, configuration2.Element);
            Assert.NotSame(configuration1.Element, configuration2.Element);
        }
    }
}
