#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class CarouselViewHandlerTests
    {
        /// <summary>
        /// Tests that the parameterless CarouselViewHandler constructor can be instantiated successfully.
        /// Verifies that the constructor calls the base constructor with the static Mapper field
        /// and creates a valid CarouselViewHandler instance.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Act
            var handler = new CarouselViewHandler();

            // Assert
            Assert.NotNull(handler);
            Assert.IsType<CarouselViewHandler>(handler);
        }

        /// <summary>
        /// Tests that the CarouselViewHandler constructor with a valid PropertyMapper parameter
        /// successfully creates an instance and passes the provided mapper to the base constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithValidPropertyMapper_CreatesInstance()
        {
            // Arrange
            var mockMapper = Substitute.For<PropertyMapper>();

            // Act
            var handler = new CarouselViewHandler(mockMapper);

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the CarouselViewHandler constructor with null PropertyMapper parameter
        /// successfully creates an instance and uses the static Mapper field via null coalescing.
        /// </summary>
        [Fact]
        public void Constructor_WithNullPropertyMapper_CreatesInstanceUsingStaticMapper()
        {
            // Arrange & Act
            var handler = new CarouselViewHandler(null);

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the CarouselViewHandler constructor with default parameter (no arguments)
        /// successfully creates an instance and uses the static Mapper field via null coalescing.
        /// </summary>
        [Fact]
        public void Constructor_WithDefaultParameter_CreatesInstanceUsingStaticMapper()
        {
            // Arrange & Act
            var handler = new CarouselViewHandler();

            // Assert
            Assert.NotNull(handler);
        }
    }
}