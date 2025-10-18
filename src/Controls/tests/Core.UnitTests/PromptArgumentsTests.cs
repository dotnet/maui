#nullable disable

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Internals.UnitTests
{
    /// <summary>
    /// Unit tests for the PromptArguments class.
    /// </summary>
    public class PromptArgumentsTests
    {
        /// <summary>
        /// Tests that SetResult sets the TaskCompletionSource result with a valid string value.
        /// Input: Valid non-empty string.
        /// Expected: TaskCompletionSource.Task.Result should contain the provided string.
        /// </summary>
        [Fact]
        public void SetResult_ValidString_SetsTaskCompletionSourceResult()
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");
            string testResult = "Test Result";

            // Act
            promptArguments.SetResult(testResult);

            // Assert
            Assert.True(promptArguments.Result.Task.IsCompleted);
            Assert.Equal(testResult, promptArguments.Result.Task.Result);
        }

        /// <summary>
        /// Tests that SetResult handles null input correctly.
        /// Input: Null string value.
        /// Expected: TaskCompletionSource.Task.Result should be null.
        /// </summary>
        [Fact]
        public void SetResult_NullString_SetsTaskCompletionSourceResultToNull()
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");

            // Act
            promptArguments.SetResult(null);

            // Assert
            Assert.True(promptArguments.Result.Task.IsCompleted);
            Assert.Null(promptArguments.Result.Task.Result);
        }

        /// <summary>
        /// Tests that SetResult handles empty string input correctly.
        /// Input: Empty string value.
        /// Expected: TaskCompletionSource.Task.Result should be empty string.
        /// </summary>
        [Fact]
        public void SetResult_EmptyString_SetsTaskCompletionSourceResult()
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");
            string emptyResult = string.Empty;

            // Act
            promptArguments.SetResult(emptyResult);

            // Assert
            Assert.True(promptArguments.Result.Task.IsCompleted);
            Assert.Equal(emptyResult, promptArguments.Result.Task.Result);
        }

        /// <summary>
        /// Tests that SetResult handles whitespace-only string input correctly.
        /// Input: Whitespace-only string value.
        /// Expected: TaskCompletionSource.Task.Result should contain the whitespace string.
        /// </summary>
        [Fact]
        public void SetResult_WhitespaceString_SetsTaskCompletionSourceResult()
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");
            string whitespaceResult = "   \t\n  ";

            // Act
            promptArguments.SetResult(whitespaceResult);

            // Assert
            Assert.True(promptArguments.Result.Task.IsCompleted);
            Assert.Equal(whitespaceResult, promptArguments.Result.Task.Result);
        }

        /// <summary>
        /// Tests that SetResult handles very long string input correctly.
        /// Input: Very long string value.
        /// Expected: TaskCompletionSource.Task.Result should contain the full long string.
        /// </summary>
        [Fact]
        public void SetResult_VeryLongString_SetsTaskCompletionSourceResult()
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");
            string longResult = new string('A', 10000);

            // Act
            promptArguments.SetResult(longResult);

            // Assert
            Assert.True(promptArguments.Result.Task.IsCompleted);
            Assert.Equal(longResult, promptArguments.Result.Task.Result);
        }

        /// <summary>
        /// Tests that SetResult handles strings with special characters correctly.
        /// Input: String containing special characters and control characters.
        /// Expected: TaskCompletionSource.Task.Result should contain the string with special characters.
        /// </summary>
        [Fact]
        public void SetResult_StringWithSpecialCharacters_SetsTaskCompletionSourceResult()
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");
            string specialResult = "Test\0\u0001\u001F\u007F\u0080\u009F€😀🌟";

            // Act
            promptArguments.SetResult(specialResult);

            // Assert
            Assert.True(promptArguments.Result.Task.IsCompleted);
            Assert.Equal(specialResult, promptArguments.Result.Task.Result);
        }

        /// <summary>
        /// Tests that calling SetResult multiple times only sets the result once (TaskCompletionSource behavior).
        /// Input: Multiple calls with different string values.
        /// Expected: Only the first call should set the result; subsequent calls should be ignored.
        /// </summary>
        [Fact]
        public void SetResult_CalledMultipleTimes_OnlyFirstCallSetsResult()
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");
            string firstResult = "First Result";
            string secondResult = "Second Result";

            // Act
            promptArguments.SetResult(firstResult);
            promptArguments.SetResult(secondResult);

            // Assert
            Assert.True(promptArguments.Result.Task.IsCompleted);
            Assert.Equal(firstResult, promptArguments.Result.Task.Result);
        }

        /// <summary>
        /// Tests that SetResult does not throw exceptions for any string input.
        /// Input: Various string values including edge cases.
        /// Expected: No exceptions should be thrown.
        /// </summary>
        [Theory]
        [InlineData("Normal text")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\r\t")]
        public void SetResult_VariousInputs_DoesNotThrowException(string input)
        {
            // Arrange
            var promptArguments = new PromptArguments("Title", "Message");

            // Act & Assert
            var exception = Record.Exception(() => promptArguments.SetResult(input));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests the constructor with only required parameters (title and message).
        /// Should assign provided values and use default values for optional parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithRequiredParametersOnly_AssignsValuesAndUsesDefaults()
        {
            // Arrange
            var title = "Test Title";
            var message = "Test Message";

            // Act
            var promptArgs = new PromptArguments(title, message);

            // Assert
            Assert.Equal(title, promptArgs.Title);
            Assert.Equal(message, promptArgs.Message);
            Assert.Equal("OK", promptArgs.Accept);
            Assert.Equal("Cancel", promptArgs.Cancel);
            Assert.Null(promptArgs.Placeholder);
            Assert.Equal("", promptArgs.InitialValue);
            Assert.Equal(-1, promptArgs.MaxLength);
            Assert.Equal(Keyboard.Default, promptArgs.Keyboard);
            Assert.NotNull(promptArgs.Result);
            Assert.IsType<TaskCompletionSource<string>>(promptArgs.Result);
        }

        /// <summary>
        /// Tests the constructor with all parameters specified.
        /// Should assign all provided values to their respective properties.
        /// </summary>
        [Fact]
        public void Constructor_WithAllParameters_AssignsAllValues()
        {
            // Arrange
            var title = "Custom Title";
            var message = "Custom Message";
            var accept = "Accept";
            var cancel = "Decline";
            var placeholder = "Enter text here";
            var maxLength = 100;
            var keyboard = Keyboard.Email;
            var initialValue = "Initial";

            // Act
            var promptArgs = new PromptArguments(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue);

            // Assert
            Assert.Equal(title, promptArgs.Title);
            Assert.Equal(message, promptArgs.Message);
            Assert.Equal(accept, promptArgs.Accept);
            Assert.Equal(cancel, promptArgs.Cancel);
            Assert.Equal(placeholder, promptArgs.Placeholder);
            Assert.Equal(initialValue, promptArgs.InitialValue);
            Assert.Equal(maxLength, promptArgs.MaxLength);
            Assert.Equal(keyboard, promptArgs.Keyboard);
            Assert.NotNull(promptArgs.Result);
            Assert.IsType<TaskCompletionSource<string>>(promptArgs.Result);
        }

        /// <summary>
        /// Tests the constructor with null keyboard parameter.
        /// Should use Keyboard.Default when keyboard parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullKeyboard_UsesDefaultKeyboard()
        {
            // Arrange
            var title = "Test Title";
            var message = "Test Message";
            Keyboard keyboard = null;

            // Act
            var promptArgs = new PromptArguments(title, message, keyboard: keyboard);

            // Assert
            Assert.Equal(Keyboard.Default, promptArgs.Keyboard);
        }

        /// <summary>
        /// Tests the constructor with edge case string values for required parameters.
        /// Should handle empty strings, whitespace, and special characters correctly.
        /// </summary>
        [Theory]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("   ", "   ")]
        [InlineData("Title with\nNewline", "Message with\tTab")]
        [InlineData("Title with special chars: !@#$%^&*()", "Message with unicode: 🚀")]
        public void Constructor_WithEdgeCaseStrings_AssignsValuesCorrectly(string title, string message)
        {
            // Act
            var promptArgs = new PromptArguments(title, message);

            // Assert
            Assert.Equal(title, promptArgs.Title);
            Assert.Equal(message, promptArgs.Message);
        }

        /// <summary>
        /// Tests the constructor with very long string values.
        /// Should handle extremely long strings without issues.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongStrings_AssignsValuesCorrectly()
        {
            // Arrange
            var longTitle = new string('T', 10000);
            var longMessage = new string('M', 10000);
            var longAccept = new string('A', 1000);
            var longCancel = new string('C', 1000);
            var longPlaceholder = new string('P', 1000);
            var longInitialValue = new string('I', 1000);

            // Act
            var promptArgs = new PromptArguments(longTitle, longMessage, longAccept, longCancel, longPlaceholder, initialValue: longInitialValue);

            // Assert
            Assert.Equal(longTitle, promptArgs.Title);
            Assert.Equal(longMessage, promptArgs.Message);
            Assert.Equal(longAccept, promptArgs.Accept);
            Assert.Equal(longCancel, promptArgs.Cancel);
            Assert.Equal(longPlaceholder, promptArgs.Placeholder);
            Assert.Equal(longInitialValue, promptArgs.InitialValue);
        }

        /// <summary>
        /// Tests the constructor with edge case integer values for maxLength parameter.
        /// Should handle minimum, maximum, zero, and negative values correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(1000)]
        public void Constructor_WithEdgeCaseMaxLengthValues_AssignsValueCorrectly(int maxLength)
        {
            // Arrange
            var title = "Test Title";
            var message = "Test Message";

            // Act
            var promptArgs = new PromptArguments(title, message, maxLength: maxLength);

            // Assert
            Assert.Equal(maxLength, promptArgs.MaxLength);
        }

        /// <summary>
        /// Tests the constructor with different keyboard types.
        /// Should assign the specified keyboard correctly.
        /// </summary>
        [Theory]
        [InlineData(nameof(Keyboard.Default))]
        [InlineData(nameof(Keyboard.Email))]
        [InlineData(nameof(Keyboard.Numeric))]
        [InlineData(nameof(Keyboard.Plain))]
        [InlineData(nameof(Keyboard.Telephone))]
        [InlineData(nameof(Keyboard.Text))]
        [InlineData(nameof(Keyboard.Url))]
        public void Constructor_WithDifferentKeyboardTypes_AssignsKeyboardCorrectly(string keyboardType)
        {
            // Arrange
            var title = "Test Title";
            var message = "Test Message";
            var keyboard = keyboardType switch
            {
                nameof(Keyboard.Default) => Keyboard.Default,
                nameof(Keyboard.Email) => Keyboard.Email,
                nameof(Keyboard.Numeric) => Keyboard.Numeric,
                nameof(Keyboard.Plain) => Keyboard.Plain,
                nameof(Keyboard.Telephone) => Keyboard.Telephone,
                nameof(Keyboard.Text) => Keyboard.Text,
                nameof(Keyboard.Url) => Keyboard.Url,
                _ => Keyboard.Default
            };

            // Act
            var promptArgs = new PromptArguments(title, message, keyboard: keyboard);

            // Assert
            Assert.Equal(keyboard, promptArgs.Keyboard);
        }

        /// <summary>
        /// Tests that the constructor always creates a new TaskCompletionSource for the Result property.
        /// Each instance should have its own unique TaskCompletionSource.
        /// </summary>
        [Fact]
        public void Constructor_AlwaysCreatesNewTaskCompletionSource()
        {
            // Arrange & Act
            var promptArgs1 = new PromptArguments("Title1", "Message1");
            var promptArgs2 = new PromptArguments("Title2", "Message2");

            // Assert
            Assert.NotNull(promptArgs1.Result);
            Assert.NotNull(promptArgs2.Result);
            Assert.NotSame(promptArgs1.Result, promptArgs2.Result);
            Assert.IsType<TaskCompletionSource<string>>(promptArgs1.Result);
            Assert.IsType<TaskCompletionSource<string>>(promptArgs2.Result);
        }

        /// <summary>
        /// Tests the constructor with null values for optional string parameters.
        /// Should handle null values appropriately based on parameter defaults.
        /// </summary>
        [Fact]
        public void Constructor_WithNullOptionalStringParameters_AssignsNullValues()
        {
            // Arrange
            var title = "Test Title";
            var message = "Test Message";

            // Act
            var promptArgs = new PromptArguments(title, message, accept: null, cancel: null, placeholder: null, initialValue: null);

            // Assert
            Assert.Equal(title, promptArgs.Title);
            Assert.Equal(message, promptArgs.Message);
            Assert.Null(promptArgs.Accept);
            Assert.Null(promptArgs.Cancel);
            Assert.Null(promptArgs.Placeholder);
            Assert.Null(promptArgs.InitialValue);
        }

        /// <summary>
        /// Tests the constructor with empty string values for optional parameters.
        /// Should assign empty strings correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyStringOptionalParameters_AssignsEmptyValues()
        {
            // Arrange
            var title = "Test Title";
            var message = "Test Message";

            // Act
            var promptArgs = new PromptArguments(title, message, accept: "", cancel: "", placeholder: "", initialValue: "");

            // Assert
            Assert.Equal("", promptArgs.Accept);
            Assert.Equal("", promptArgs.Cancel);
            Assert.Equal("", promptArgs.Placeholder);
            Assert.Equal("", promptArgs.InitialValue);
        }
    }
}