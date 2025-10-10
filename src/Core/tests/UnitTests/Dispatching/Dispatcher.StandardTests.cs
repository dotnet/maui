using Microsoft.Maui.Dispatching;
using System;
using Xunit;


namespace Microsoft.Maui.Dispatching.UnitTests
{
    /// <summary>
    /// Unit tests for the Dispatcher class.
    /// </summary>
    public partial class DispatcherTests
    {
        /// <summary>
        /// Tests that the internal constructor creates a valid Dispatcher instance.
        /// Verifies that the constructor executes without throwing exceptions and produces a non-null instance of the expected type.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WhenCalled_CreatesValidDispatcherInstance()
        {
            // Arrange & Act
            var dispatcher = new Dispatcher();

            // Assert
            Assert.NotNull(dispatcher);
            Assert.IsType<Dispatcher>(dispatcher);
            Assert.IsAssignableFrom<IDispatcher>(dispatcher);
        }
    }
}
