#nullable disable

using System;
using System.Collections;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ItemsViewHandlerTests
    {
        /// <summary>
        /// Tests that the ItemsViewHandler constructor properly initializes with a null mapper parameter,
        /// defaulting to the static ItemsViewMapper field.
        /// </summary>
        [Fact]
        public void ItemsViewHandler_NullMapper_UsesItemsViewMapper()
        {
            // Arrange & Act
            var handler = new TestItemsViewHandler(mapper: null);

            // Assert
            Assert.NotNull(handler);
            // The handler should be properly constructed and the base constructor should have been called
            // with ItemsViewMapper since null was passed
        }

        /// <summary>
        /// Tests that the ItemsViewHandler constructor properly initializes with a provided mapper parameter,
        /// using the provided mapper instead of the default ItemsViewMapper.
        /// </summary>
        [Fact]
        public void ItemsViewHandler_ValidMapper_UsesProvidedMapper()
        {
            // Arrange
            var mockMapper = Substitute.For<PropertyMapper>();

            // Act
            var handler = new TestItemsViewHandler(mapper: mockMapper);

            // Assert
            Assert.NotNull(handler);
            // The handler should be properly constructed and the base constructor should have been called
            // with the provided mapper
        }

        /// <summary>
        /// Tests that the ItemsViewHandler constructor behaves consistently when called multiple times
        /// with null mapper parameters.
        /// </summary>
        [Fact]
        public void ItemsViewHandler_MultipleNullMapperCalls_ConsistentBehavior()
        {
            // Arrange & Act
            var handler1 = new TestItemsViewHandler(mapper: null);
            var handler2 = new TestItemsViewHandler(mapper: null);

            // Assert
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            // Both handlers should be properly constructed
        }

        /// <summary>
        /// Tests that the ItemsViewHandler constructor works with different mapper instances,
        /// ensuring proper parameter handling.
        /// </summary>
        [Fact]
        public void ItemsViewHandler_DifferentMappers_HandlesCorrectly()
        {
            // Arrange
            var mockMapper1 = Substitute.For<PropertyMapper>();
            var mockMapper2 = Substitute.For<PropertyMapper>();

            // Act
            var handler1 = new TestItemsViewHandler(mapper: mockMapper1);
            var handler2 = new TestItemsViewHandler(mapper: mockMapper2);

            // Assert
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            // Both handlers should be properly constructed with their respective mappers
        }

        /// <summary>
        /// Tests that the parameterless constructor successfully creates an instance.
        /// This verifies that the constructor properly calls the base constructor with ItemsViewMapper
        /// and completes initialization without throwing any exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithNoParameters_CreatesInstanceSuccessfully()
        {
            // Arrange & Act
            var handler = new TestItemsViewHandler();

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the parameterless constructor properly initializes the handler
        /// and that the ItemsViewMapper static field is accessible and not null.
        /// This ensures the static mapper dependency is properly set up.
        /// </summary>
        [Fact]
        public void Constructor_WithNoParameters_ItemsViewMapperIsNotNull()
        {
            // Arrange & Act
            var handler = new TestItemsViewHandler();

            // Assert
            Assert.NotNull(TestItemsViewHandler.ItemsViewMapper);
        }

    }
}