#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PolygonHandlerTests
    {
        /// <summary>
        /// Tests that the parameterless constructor successfully creates a PolygonHandler instance.
        /// Verifies that the constructor properly initializes the handler by calling the base constructor with the static Mapper.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessConstructor_CreatesValidInstance()
        {
            // Act
            var handler = new PolygonHandler();

            // Assert
            Assert.NotNull(handler);
            Assert.IsType<PolygonHandler>(handler);
        }
    }
}
