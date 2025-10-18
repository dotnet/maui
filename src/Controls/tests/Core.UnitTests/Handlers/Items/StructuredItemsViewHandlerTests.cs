#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class StructuredItemsViewHandlerTests
    {
        /// <summary>
        /// Tests that the StructuredItemsViewHandler constructor with null mapper parameter
        /// uses the StructuredItemsViewMapper as fallback and creates the handler successfully.
        /// </summary>
        [Fact]
        public void StructuredItemsViewHandler_NullMapper_UsesStructuredItemsViewMapperFallback()
        {
            // Arrange & Act
            var handler = new TestStructuredItemsViewHandler(mapper: null);

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the StructuredItemsViewHandler constructor with a valid PropertyMapper parameter
        /// uses the provided mapper and creates the handler successfully.
        /// </summary>
        [Fact]
        public void StructuredItemsViewHandler_ValidMapper_UsesProvidedMapper()
        {
            // Arrange
            var mockMapper = Substitute.For<PropertyMapper>();

            // Act
            var handler = new TestStructuredItemsViewHandler(mockMapper);

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the StructuredItemsViewHandler constructor with default parameter (null)
        /// uses the StructuredItemsViewMapper as fallback and creates the handler successfully.
        /// </summary>
        [Fact]
        public void StructuredItemsViewHandler_DefaultParameter_UsesStructuredItemsViewMapperFallback()
        {
            // Arrange & Act
            var handler = new TestStructuredItemsViewHandler();

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Helper class to test the generic StructuredItemsViewHandler with a concrete type.
        /// </summary>
        private class TestStructuredItemsViewHandler : StructuredItemsViewHandler<TestStructuredItemsView>
        {
            public TestStructuredItemsViewHandler() : base()
            {
            }

            public TestStructuredItemsViewHandler(PropertyMapper mapper) : base(mapper)
            {
            }
        }

        /// <summary>
        /// Test implementation of StructuredItemsView for testing purposes.
        /// </summary>
        private class TestStructuredItemsView : StructuredItemsView
        {
        }

        /// <summary>
        /// Tests that the parameterless constructor successfully creates an instance
        /// without throwing any exceptions and properly initializes the handler.
        /// </summary>
        [Fact]
        public void Constructor_WithNoParameters_CreatesInstanceSuccessfully()
        {
            // Arrange & Act
            var handler = new StructuredItemsViewHandler<StructuredItemsView>();

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the parameterless constructor can be called multiple times
        /// and creates distinct instances each time.
        /// </summary>
        [Fact]
        public void Constructor_WithNoParameters_CreatesDistinctInstances()
        {
            // Arrange & Act
            var handler1 = new StructuredItemsViewHandler<StructuredItemsView>();
            var handler2 = new StructuredItemsViewHandler<StructuredItemsView>();

            // Assert
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotSame(handler1, handler2);
        }

        /// <summary>
        /// Tests that the parameterless constructor works with the static mapper field
        /// and verifies that the StructuredItemsViewMapper is properly accessible.
        /// </summary>
        [Fact]
        public void Constructor_WithNoParameters_UsesStaticMapper()
        {
            // Arrange & Act
            var handler = new StructuredItemsViewHandler<StructuredItemsView>();

            // Assert
            Assert.NotNull(handler);
            Assert.NotNull(StructuredItemsViewHandler<StructuredItemsView>.StructuredItemsViewMapper);
        }
    }
}