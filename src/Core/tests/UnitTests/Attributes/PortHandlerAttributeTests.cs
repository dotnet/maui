#nullable enable

using System;

using Microsoft;
using Microsoft.Maui;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
    /// <summary>
    /// Tests for the PortHandlerAttribute class.
    /// </summary>
    public class PortHandlerAttributeTests
    {
        /// <summary>
        /// Tests that the constructor creates a valid PortHandlerAttribute instance
        /// that inherits from System.Attribute.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithDescription_CreatesValidAttributeInstance()
        {
            // Arrange
            var description = "Test description";

            // Act
            var attribute = new PortHandlerAttribute(description);

            // Assert
            Assert.NotNull(attribute);
            Assert.IsAssignableFrom<Attribute>(attribute);
            Assert.Equal(description, attribute.Description);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid PortHandlerAttribute instance.
        /// Verifies that the constructor executes without throwing exceptions.
        /// Expected result: A valid PortHandlerAttribute instance is created.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void PortHandlerAttribute_DefaultConstructor_CreatesValidInstance()
        {
            // Arrange & Act
            var attribute = new PortHandlerAttribute();

            // Assert
            Assert.NotNull(attribute);
            Assert.IsType<PortHandlerAttribute>(attribute);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes the Description property to null.
        /// Verifies the default state of the Description property after construction.
        /// Expected result: Description property should be null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void PortHandlerAttribute_DefaultConstructor_InitializesDescriptionToNull()
        {
            // Arrange & Act
            var attribute = new PortHandlerAttribute();

            // Assert
            Assert.Null(attribute.Description);
        }

        /// <summary>
        /// Tests that the PortHandlerAttribute inherits from System.Attribute correctly.
        /// Verifies the inheritance hierarchy is properly established.
        /// Expected result: The instance should be assignable to System.Attribute.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void PortHandlerAttribute_DefaultConstructor_InheritsFromAttribute()
        {
            // Arrange & Act
            var attribute = new PortHandlerAttribute();

            // Assert
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        /// <summary>
        /// Tests that multiple instances of PortHandlerAttribute can be created independently.
        /// Verifies that the constructor can be called multiple times without issues.
        /// Expected result: Multiple distinct instances are created successfully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void PortHandlerAttribute_DefaultConstructor_AllowsMultipleInstances()
        {
            // Arrange & Act
            var attribute1 = new PortHandlerAttribute();
            var attribute2 = new PortHandlerAttribute();

            // Assert
            Assert.NotNull(attribute1);
            Assert.NotNull(attribute2);
            Assert.NotSame(attribute1, attribute2);
            Assert.Equal(attribute1.GetType(), attribute2.GetType());
        }
    }
}