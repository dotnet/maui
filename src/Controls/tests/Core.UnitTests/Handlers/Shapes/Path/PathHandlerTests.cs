#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PathHandlerTests
    {
        /// <summary>
        /// Tests the PathHandler constructor with a null mapper parameter.
        /// Should use the static Mapper field as fallback and construct successfully.
        /// </summary>
        [Fact]
        public void Constructor_NullMapper_UsesStaticMapperAndConstructsSuccessfully()
        {
            // Arrange
            IPropertyMapper nullMapper = null;

            // Act
            var pathHandler = new PathHandler(nullMapper);

            // Assert
            Assert.NotNull(pathHandler);
        }

        /// <summary>
        /// Tests the PathHandler constructor with a valid mapper parameter.
        /// Should use the provided mapper and construct successfully.
        /// </summary>
        [Fact]
        public void Constructor_ValidMapper_UsesProvidedMapperAndConstructsSuccessfully()
        {
            // Arrange
            var mockMapper = Substitute.For<IPropertyMapper>();

            // Act
            var pathHandler = new PathHandler(mockMapper);

            // Assert
            Assert.NotNull(pathHandler);
        }

        /// <summary>
        /// Tests that the parameterless PathHandler constructor successfully creates a new instance
        /// and properly initializes the object by calling the base constructor with the static Mapper.
        /// Verifies the constructor executes without throwing exceptions and creates a valid object.
        /// </summary>
        [Fact]
        public void PathHandler_Constructor_CreatesValidInstance()
        {
            // Arrange & Act
            PathHandler handler = null;
            var exception = Record.Exception(() => handler = new PathHandler());

            // Assert
            Assert.Null(exception);
            Assert.NotNull(handler);
            Assert.IsType<PathHandler>(handler);
            Assert.IsAssignableFrom<ShapeViewHandler>(handler);
        }

        /// <summary>
        /// Tests that the PathHandler constructor properly inherits from ShapeViewHandler
        /// and maintains the correct inheritance chain. Verifies type hierarchy is preserved.
        /// </summary>
        [Fact]
        public void PathHandler_Constructor_InheritsFromShapeViewHandler()
        {
            // Arrange & Act
            var handler = new PathHandler();

            // Assert
            Assert.IsAssignableFrom<IShapeViewHandler>(handler);
            Assert.IsAssignableFrom<ShapeViewHandler>(handler);
        }

        /// <summary>
        /// Tests that the static Mapper field is properly accessible and not null,
        /// ensuring the constructor can successfully pass it to the base class.
        /// </summary>
        [Fact]
        public void PathHandler_StaticMapper_IsNotNull()
        {
            // Arrange & Act & Assert
            Assert.NotNull(PathHandler.Mapper);
            Assert.IsAssignableFrom<IPropertyMapper<Path, IShapeViewHandler>>(PathHandler.Mapper);
        }
    }
}