#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PolylineHandlerTests
    {
        /// <summary>
        /// Tests that the PolylineHandler constructor with a valid IPropertyMapper parameter
        /// successfully creates an instance without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithValidMapper_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mapper = Substitute.For<IPropertyMapper>();

            // Act & Assert
            var handler = new PolylineHandler(mapper);
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the PolylineHandler constructor with a null IPropertyMapper parameter
        /// successfully creates an instance using the default static Mapper field.
        /// </summary>
        [Fact]
        public void Constructor_WithNullMapper_CreatesInstanceSuccessfully()
        {
            // Arrange
            IPropertyMapper mapper = null;

            // Act & Assert
            var handler = new PolylineHandler(mapper);
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the parameterless constructor successfully creates a PolylineHandler instance
        /// and properly initializes the handler by calling the base constructor with the static Mapper.
        /// Verifies that the created instance is not null and is of the correct type.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Arrange & Act
            var handler = new PolylineHandler();

            // Assert
            Assert.NotNull(handler);
            Assert.IsType<PolylineHandler>(handler);
            Assert.IsAssignableFrom<ShapeViewHandler>(handler);
        }

        /// <summary>
        /// Tests that the static Mapper field used by the default constructor is properly configured
        /// and contains the expected property mappings for Polyline shapes.
        /// Verifies that the Mapper is not null and contains the required property keys.
        /// </summary>
        [Fact]
        public void Constructor_Default_UsesConfiguredMapper()
        {
            // Arrange & Act
            var handler = new PolylineHandler();

            // Assert
            Assert.NotNull(PolylineHandler.Mapper);
            Assert.True(PolylineHandler.Mapper.ContainsKey(nameof(IShapeView.Shape)));
            Assert.True(PolylineHandler.Mapper.ContainsKey(nameof(Polyline.Points)));
            Assert.True(PolylineHandler.Mapper.ContainsKey(nameof(Polyline.FillRule)));
        }
    }
}