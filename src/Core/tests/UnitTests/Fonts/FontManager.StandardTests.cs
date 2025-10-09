#nullable enable

using Microsoft;
using Microsoft.Maui;
using NSubstitute;
using System;
using Xunit;


namespace Microsoft.Maui.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="FontManager"/> class.
    /// </summary>
    public class FontManagerTests
    {
        /// <summary>
        /// Tests that the DefaultFontSize property returns -1 when accessed.
        /// This verifies the property returns the expected constant value.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void DefaultFontSize_WhenAccessed_ReturnsNegativeOne()
        {
            // Arrange
            var mockFontRegistrar = Substitute.For<IFontRegistrar>();
            var fontManager = new FontManager(mockFontRegistrar, null);

            // Act
            var result = fontManager.DefaultFontSize;

            // Assert
            Assert.Equal(-1.0, result);
        }
    }
}
