#nullable enable

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Xunit;


namespace Microsoft.Maui.UnitTests
{
    public partial class FileImageSourceServiceTests
    {
        /// <summary>
        /// Tests that the parameterless constructor successfully creates a valid FileImageSourceService instance.
        /// This constructor delegates to the other constructor with a null logger parameter.
        /// Expected result: A non-null FileImageSourceService instance is created without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithNoParameters_CreatesValidInstance()
        {
            // Arrange & Act
            FileImageSourceService service = new FileImageSourceService();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<FileImageSourceService>(service);
        }
    }
}
