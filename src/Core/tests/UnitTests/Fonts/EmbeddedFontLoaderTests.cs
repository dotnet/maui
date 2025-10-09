#nullable enable

using Microsoft;
using Microsoft.Maui;
using System;
using Xunit;


namespace Microsoft.Maui.UnitTests
{
    /// <summary>
    /// Unit tests for the EmbeddedFontLoader class.
    /// </summary>
    public partial class EmbeddedFontLoaderTests
    {
        /// <summary>
        /// Tests that the parameterless constructor successfully creates an EmbeddedFontLoader instance.
        /// Verifies that the constructor chains properly to the main constructor with null parameter
        /// and creates a valid instance that implements IEmbeddedFontLoader.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_NoParameters_CreatesInstance()
        {
            // Arrange & Act
            var fontLoader = new EmbeddedFontLoader();

            // Assert
            Assert.NotNull(fontLoader);
            Assert.IsType<EmbeddedFontLoader>(fontLoader);
            Assert.IsAssignableFrom<IEmbeddedFontLoader>(fontLoader);
        }

        /// <summary>
        /// Tests that the parameterless constructor does not throw any exceptions during instantiation.
        /// Verifies that the constructor chaining mechanism works correctly without errors.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_NoParameters_DoesNotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => new EmbeddedFontLoader());

            Assert.Null(exception);
        }
    }
}
