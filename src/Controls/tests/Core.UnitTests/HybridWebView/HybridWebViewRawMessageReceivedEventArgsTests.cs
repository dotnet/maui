using System;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class HybridWebViewRawMessageReceivedEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor properly assigns the message parameter to the Message property
        /// when provided with a null value.
        /// Expected result: Message property should be null.
        /// </summary>
        [Fact]
        public void Constructor_NullMessage_SetsMessagePropertyToNull()
        {
            // Arrange
            string message = null;

            // Act
            var eventArgs = new HybridWebViewRawMessageReceivedEventArgs(message);

            // Assert
            Assert.Null(eventArgs.Message);
        }

        /// <summary>
        /// Tests that the constructor properly assigns the message parameter to the Message property
        /// for various string inputs including empty, whitespace, and content strings.
        /// Expected result: Message property should exactly match the input parameter.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("test message")]
        [InlineData("Hello, World!")]
        [InlineData("Special chars: @#$%^&*()")]
        [InlineData("Unicode: 🚀 ñáéíóú")]
        [InlineData("Very long string with lots of content that could potentially cause issues in some implementations but should work fine here")]
        public void Constructor_ValidStringInputs_SetsMessagePropertyCorrectly(string message)
        {
            // Arrange & Act
            var eventArgs = new HybridWebViewRawMessageReceivedEventArgs(message);

            // Assert
            Assert.Equal(message, eventArgs.Message);
        }

        /// <summary>
        /// Tests that the constructor inherits from EventArgs as expected.
        /// Expected result: Instance should be assignable to EventArgs.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstance_InheritsFromEventArgs()
        {
            // Arrange & Act
            var eventArgs = new HybridWebViewRawMessageReceivedEventArgs("test");

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }
}
