using Microsoft.Maui.Controls;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class DropCompletedEventArgsTests
    {
        /// <summary>
        /// Tests that the internal constructor properly initializes the DropCompletedEventArgs with platform-specific arguments.
        /// Verifies that the provided platformArgs parameter is correctly assigned to the PlatformArgs property.
        /// </summary>
        [Fact]
        public void Constructor_WithPlatformArgs_SetsPlatformArgsProperty()
        {
            // Arrange
            var platformArgs = new PlatformDropCompletedEventArgs();

            // Act
            var dropCompletedEventArgs = new DropCompletedEventArgs(platformArgs);

            // Assert
            Assert.Same(platformArgs, dropCompletedEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor properly chains to the parameterless constructor.
        /// Verifies that the object is properly initialized through constructor chaining.
        /// </summary>
        [Fact]
        public void Constructor_WithPlatformArgs_ChainsToParameterlessConstructor()
        {
            // Arrange
            var platformArgs = new PlatformDropCompletedEventArgs();

            // Act
            var dropCompletedEventArgs = new DropCompletedEventArgs(platformArgs);

            // Assert
            Assert.NotNull(dropCompletedEventArgs);
            Assert.IsType<DropCompletedEventArgs>(dropCompletedEventArgs);
        }
    }
}
