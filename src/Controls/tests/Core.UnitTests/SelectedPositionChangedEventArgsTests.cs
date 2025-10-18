#nullable disable

using Microsoft.Maui.Controls;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class SelectedPositionChangedEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes the SelectedPosition property with various integer values.
        /// Tests boundary values, zero, positive and negative numbers to ensure proper handling of all valid int inputs.
        /// Verifies that the int value is correctly boxed into the object property.
        /// </summary>
        /// <param name="selectedPosition">The position value to test</param>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(42)]
        [InlineData(-999)]
        [InlineData(1000)]
        public void Constructor_WithValidIntegerValues_SetsSelectedPositionProperty(int selectedPosition)
        {
            // Arrange & Act
            var eventArgs = new SelectedPositionChangedEventArgs(selectedPosition);

            // Assert
            Assert.Equal(selectedPosition, eventArgs.SelectedPosition);
            Assert.IsType<int>(eventArgs.SelectedPosition);
        }

        /// <summary>
        /// Tests that the constructor creates an instance that properly inherits from EventArgs.
        /// Verifies the inheritance chain is correctly established.
        /// </summary>
        [Fact]
        public void Constructor_WhenCalled_CreatesEventArgsInstance()
        {
            // Arrange & Act
            var eventArgs = new SelectedPositionChangedEventArgs(5);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }
}
