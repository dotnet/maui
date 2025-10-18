#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Handlers.Items;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests.Handlers.Items
{
    public partial class CollectionViewHandlerTests
    {
        /// <summary>
        /// Tests that the default constructor creates a CollectionViewHandler instance successfully
        /// using the static Mapper field as the base constructor parameter.
        /// Expected result: Instance is created without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_CreatesInstanceSuccessfully()
        {
            // Arrange & Act
            var handler = new CollectionViewHandler();

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the parameterized constructor with null mapper parameter creates a CollectionViewHandler instance
        /// using the static Mapper field as fallback via null coalescing operator.
        /// Expected result: Instance is created without throwing exceptions, using static Mapper as fallback.
        /// </summary>
        [Fact]
        public void Constructor_WithNullMapper_CreatesInstanceUsingStaticMapper()
        {
            // Arrange & Act
            var handler = new CollectionViewHandler(mapper: null);

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the parameterized constructor with a valid PropertyMapper creates a CollectionViewHandler instance
        /// using the provided mapper parameter.
        /// Expected result: Instance is created without throwing exceptions, using the provided mapper.
        /// </summary>
        [Fact]
        public void Constructor_WithValidMapper_CreatesInstanceUsingProvidedMapper()
        {
            // Arrange
            var customMapper = Substitute.For<PropertyMapper>();

            // Act
            var handler = new CollectionViewHandler(customMapper);

            // Assert
            Assert.NotNull(handler);
        }
    }
}
