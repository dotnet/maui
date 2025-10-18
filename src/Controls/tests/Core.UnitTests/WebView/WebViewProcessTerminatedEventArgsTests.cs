#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class WebViewProcessTerminatedEventArgsTests
    {
        /// <summary>
        /// Tests that the internal constructor with default parameter (null) correctly sets PlatformArgs to null.
        /// This verifies the constructor behavior when called without explicitly passing arguments.
        /// </summary>
        [Fact]
        public void Constructor_WithDefaultParameter_SetsPlatformArgsToNull()
        {
            // Arrange & Act
            var eventArgs = new WebViewProcessTerminatedEventArgs();

            // Assert
            Assert.Null(eventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor with explicit null parameter correctly sets PlatformArgs to null.
        /// This verifies the constructor behavior when explicitly passing null as the argument.
        /// </summary>
        [Fact]
        public void Constructor_WithNullParameter_SetsPlatformArgsToNull()
        {
            // Arrange & Act
            var eventArgs = new WebViewProcessTerminatedEventArgs(null);

            // Assert
            Assert.Null(eventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the parameterless constructor successfully creates a new instance of WebViewProcessTerminatedEventArgs.
        /// Verifies that the object is properly instantiated and inherits from EventArgs.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesInstance()
        {
            // Act
            var eventArgs = new WebViewProcessTerminatedEventArgs();

            // Assert
            Assert.NotNull(eventArgs);
            Assert.IsType<WebViewProcessTerminatedEventArgs>(eventArgs);
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes the PlatformArgs property to null.
        /// Verifies the default state of the PlatformArgs property after construction.
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesPlatformArgsToNull()
        {
            // Act
            var eventArgs = new WebViewProcessTerminatedEventArgs();

            // Assert
            Assert.Null(eventArgs.PlatformArgs);
        }
    }
}