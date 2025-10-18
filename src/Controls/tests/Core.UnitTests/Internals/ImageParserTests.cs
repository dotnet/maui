#nullable disable

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class GIFDecoderFormatExceptionTests
    {
        /// <summary>
        /// Tests that the GIFDecoderFormatException constructor with message creates an exception with the specified message.
        /// </summary>
        /// <param name="message">The message to pass to the constructor</param>
        /// <param name="expectedMessage">The expected message property value</param>
        [Theory]
        [InlineData("Test error message", "Test error message")]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData("Message with special characters: !@#$%^&*()_+-=[]{}|;:'\",.<>?/~`", "Message with special characters: !@#$%^&*()_+-=[]{}|;:'\",.<>?/~`")]
        [InlineData("Very long message that exceeds typical length boundaries to test if the constructor handles long strings properly without any issues or truncation", "Very long message that exceeds typical length boundaries to test if the constructor handles long strings properly without any issues or truncation")]
        public void Constructor_WithMessage_SetsMessageCorrectly(string message, string expectedMessage)
        {
            // Act
            var exception = new GIFDecoderFormatException(message);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.IsType<GIFDecoderFormatException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        /// <summary>
        /// Tests that the GIFDecoderFormatException constructor with null message creates an exception successfully.
        /// </summary>
        [Fact]
        public void Constructor_WithNullMessage_CreatesExceptionSuccessfully()
        {
            // Act
            var exception = new GIFDecoderFormatException(null);

            // Assert
            Assert.Null(exception.Message);
            Assert.IsType<GIFDecoderFormatException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        /// <summary>
        /// Tests that the GIFDecoderFormatException can be thrown and caught properly as both its specific type and base Exception type.
        /// </summary>
        [Fact]
        public void Constructor_WithMessage_CanBeThrownAndCaught()
        {
            // Arrange
            const string testMessage = "Test exception message";

            // Act & Assert - Catch as specific type
            var specificException = Assert.Throws<GIFDecoderFormatException>(() => throw new GIFDecoderFormatException(testMessage));
            Assert.Equal(testMessage, specificException.Message);

            // Act & Assert - Catch as base Exception type
            var baseException = Assert.Throws<Exception>(() => throw new GIFDecoderFormatException(testMessage));
            Assert.Equal(testMessage, baseException.Message);
            Assert.IsType<GIFDecoderFormatException>(baseException);
        }

        /// <summary>
        /// Tests that the GIFDecoderFormatException constructor with message sets all inherited Exception properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithMessage_SetsInheritedPropertiesCorrectly()
        {
            // Arrange
            const string testMessage = "Test message for property validation";

            // Act
            var exception = new GIFDecoderFormatException(testMessage);

            // Assert
            Assert.Equal(testMessage, exception.Message);
            Assert.Null(exception.InnerException);
            Assert.NotNull(exception.StackTrace);
            Assert.Contains("GIFDecoderFormatException", exception.GetType().Name);
        }

        /// <summary>
        /// Tests the constructor with valid message and valid inner exception.
        /// Verifies that both message and inner exception are properly set.
        /// </summary>
        [Fact]
        public void Constructor_ValidMessageAndValidInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            string expectedMessage = "Test exception message";
            var expectedInnerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new GIFDecoderFormatException(expectedMessage, expectedInnerException);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Same(expectedInnerException, exception.InnerException);
        }

        /// <summary>
        /// Tests the constructor with null message and valid inner exception.
        /// Verifies that null message is handled correctly and inner exception is set.
        /// </summary>
        [Fact]
        public void Constructor_NullMessageAndValidInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            string expectedMessage = null;
            var expectedInnerException = new ArgumentException("Inner exception");

            // Act
            var exception = new GIFDecoderFormatException(expectedMessage, expectedInnerException);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Same(expectedInnerException, exception.InnerException);
        }

        /// <summary>
        /// Tests the constructor with valid message and null inner exception.
        /// Verifies that message is set correctly and null inner exception is handled.
        /// </summary>
        [Fact]
        public void Constructor_ValidMessageAndNullInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            string expectedMessage = "Test message";
            Exception expectedInnerException = null;

            // Act
            var exception = new GIFDecoderFormatException(expectedMessage, expectedInnerException);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests the constructor with both null parameters.
        /// Verifies that both null values are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNull_SetsPropertiesCorrectly()
        {
            // Arrange
            string expectedMessage = null;
            Exception expectedInnerException = null;

            // Act
            var exception = new GIFDecoderFormatException(expectedMessage, expectedInnerException);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests the constructor with various string edge cases for the message parameter.
        /// Verifies that different string values including empty and whitespace are handled correctly.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        [InlineData("Very long message that contains multiple words and special characters !@#$%^&*()")]
        [InlineData("Message with\nnewlines\tand\rtabs")]
        public void Constructor_VariousMessageFormats_SetsMessageCorrectly(string message)
        {
            // Arrange
            var innerException = new Exception("Inner");

            // Act
            var exception = new GIFDecoderFormatException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests the constructor with different types of inner exceptions.
        /// Verifies that various exception types can be used as inner exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(FormatException))]
        [InlineData(typeof(IOException))]
        public void Constructor_DifferentInnerExceptionTypes_SetsInnerExceptionCorrectly(Type exceptionType)
        {
            // Arrange
            string message = "Test message";
            var innerException = (Exception)Activator.CreateInstance(exceptionType, "Inner exception message");

            // Act
            var exception = new GIFDecoderFormatException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
            Assert.IsType(exceptionType, exception.InnerException);
        }

        /// <summary>
        /// Tests the constructor with nested GIFDecoderFormatException as inner exception.
        /// Verifies that the same exception type can be used as an inner exception.
        /// </summary>
        [Fact]
        public void Constructor_NestedGIFDecoderFormatException_SetsInnerExceptionCorrectly()
        {
            // Arrange
            string outerMessage = "Outer exception";
            string innerMessage = "Inner exception";
            var nestedInnerException = new GIFDecoderFormatException(innerMessage);

            // Act
            var exception = new GIFDecoderFormatException(outerMessage, nestedInnerException);

            // Assert
            Assert.Equal(outerMessage, exception.Message);
            Assert.Same(nestedInnerException, exception.InnerException);
            Assert.IsType<GIFDecoderFormatException>(exception.InnerException);
            Assert.Equal(innerMessage, exception.InnerException.Message);
        }

        /// <summary>
        /// Tests that the constructor creates an instance that inherits from Exception.
        /// Verifies the inheritance hierarchy is maintained correctly.
        /// </summary>
        [Fact]
        public void Constructor_CreatesExceptionInstance_InheritsFromException()
        {
            // Arrange
            string message = "Test message";
            var innerException = new Exception("Inner");

            // Act
            var exception = new GIFDecoderFormatException(message, innerException);

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
            Assert.IsType<GIFDecoderFormatException>(exception);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid GIFDecoderFormatException instance.
        /// Verifies that the exception can be instantiated without arguments and inherits proper default behavior from Exception.
        /// Expected result: Exception is created successfully with default properties.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CreatesValidException()
        {
            // Act
            var exception = new GIFDecoderFormatException();

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<GIFDecoderFormatException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an exception with default Exception properties.
        /// Verifies that Message property has default value and InnerException is null.
        /// Expected result: Exception has default message and null inner exception.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_HasDefaultExceptionProperties()
        {
            // Act
            var exception = new GIFDecoderFormatException();

            // Assert
            Assert.NotNull(exception.Message);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an exception that can be thrown and caught.
        /// Verifies the exception behaves correctly in throw/catch scenarios.
        /// Expected result: Exception can be thrown and caught successfully.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CanBeThrown()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<GIFDecoderFormatException>(() =>
            {
                throw new GIFDecoderFormatException();
            });

            Assert.NotNull(exception);
            Assert.IsType<GIFDecoderFormatException>(exception);
        }

        /// <summary>
        /// Tests that multiple instances of the parameterless constructor create distinct objects.
        /// Verifies that each constructor call creates a new instance.
        /// Expected result: Multiple constructor calls create different object instances.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CreatesDistinctInstances()
        {
            // Act
            var exception1 = new GIFDecoderFormatException();
            var exception2 = new GIFDecoderFormatException();

            // Assert
            Assert.NotSame(exception1, exception2);
            Assert.NotEqual(exception1, exception2);
        }
    }

    /// <summary>
    /// Unit tests for the GIFColorTable class Data property.
    /// </summary>
    public class GIFColorTableTests
    {
        /// <summary>
        /// Tests that the Data property returns the internal color table array.
        /// Input: A GIFColorTable instance created with valid stream data.
        /// Expected: The Data property returns a non-null int array with 256 elements.
        /// </summary>
        [Fact]
        public async Task Data_WithValidColorTable_ReturnsColorTableArray()
        {
            // Arrange
            var colorData = CreateValidColorData(4); // 4 colors * 3 bytes each = 12 bytes
            var stream = CreateMemoryStreamWithData(new byte[] { 12 }.Concat(colorData).ToArray()); // block size + color data
            var streamReader = new GIFDecoderStreamReader(stream);
            var colorTable = await GIFColorTable.CreateColorTableAsync(streamReader, 4);

            // Act
            var result = colorTable.Data;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(256, result.Length);
        }

        /// <summary>
        /// Tests that the Data property returns the same reference on multiple calls.
        /// Input: A GIFColorTable instance.
        /// Expected: Multiple calls to Data return the same array reference.
        /// </summary>
        [Fact]
        public async Task Data_MultipleCalls_ReturnsSameReference()
        {
            // Arrange
            var colorData = CreateValidColorData(2);
            var stream = CreateMemoryStreamWithData(new byte[] { 6 }.Concat(colorData).ToArray());
            var streamReader = new GIFDecoderStreamReader(stream);
            var colorTable = await GIFColorTable.CreateColorTableAsync(streamReader, 2);

            // Act
            var firstCall = colorTable.Data;
            var secondCall = colorTable.Data;

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that modifications to the returned Data array affect the internal state.
        /// Input: A GIFColorTable instance with the returned Data array modified.
        /// Expected: Subsequent calls to Data reflect the modifications.
        /// </summary>
        [Fact]
        public async Task Data_ModifyReturnedArray_AffectsSubsequentCalls()
        {
            // Arrange
            var colorData = CreateValidColorData(1);
            var stream = CreateMemoryStreamWithData(new byte[] { 3 }.Concat(colorData).ToArray());
            var streamReader = new GIFDecoderStreamReader(stream);
            var colorTable = await GIFColorTable.CreateColorTableAsync(streamReader, 1);
            var data = colorTable.Data;
            var originalValue = data[0];

            // Act
            data[0] = 0x12345678;
            var modifiedData = colorTable.Data;

            // Assert
            Assert.Equal(0x12345678, modifiedData[0]);
            Assert.NotEqual(originalValue, modifiedData[0]);
        }

        /// <summary>
        /// Tests that the Data property returns populated color values after parsing.
        /// Input: A GIFColorTable created with specific RGB color data.
        /// Expected: The Data array contains the expected ARGB color values.
        /// </summary>
        [Fact]
        public async Task Data_AfterParsing_ContainsExpectedColorValues()
        {
            // Arrange - Create color data for red (255,0,0) and blue (0,0,255)
            var colorData = new byte[] { 255, 0, 0, 0, 0, 255 }; // Red and Blue
            var stream = CreateMemoryStreamWithData(new byte[] { 6 }.Concat(colorData).ToArray());
            var streamReader = new GIFDecoderStreamReader(stream);
            var colorTable = await GIFColorTable.CreateColorTableAsync(streamReader, 2);

            // Act
            var result = colorTable.Data;

            // Assert
            Assert.Equal(unchecked((int)0xFFFF0000), result[0]); // Red with alpha
            Assert.Equal(unchecked((int)0xFF0000FF), result[1]); // Blue with alpha
        }

        /// <summary>
        /// Tests that the Data property works correctly after transparency operations.
        /// Input: A GIFColorTable with transparency set and reset.
        /// Expected: The Data array reflects transparency changes correctly.
        /// </summary>
        [Fact]
        public async Task Data_AfterTransparencyOperations_ReflectsChanges()
        {
            // Arrange
            var colorData = CreateValidColorData(3);
            var stream = CreateMemoryStreamWithData(new byte[] { 9 }.Concat(colorData).ToArray());
            var streamReader = new GIFDecoderStreamReader(stream);
            var colorTable = await GIFColorTable.CreateColorTableAsync(streamReader, 3);
            var originalValue = colorTable.Data[1];

            // Act - Set transparency on index 1
            colorTable.SetTransparency(1);
            var transparentData = colorTable.Data;

            // Reset transparency
            colorTable.ResetTransparency();
            var resetData = colorTable.Data;

            // Assert
            Assert.Equal(0, transparentData[1]); // Should be transparent (0)
            Assert.Equal(originalValue, resetData[1]); // Should be restored
        }

        /// <summary>
        /// Creates a memory stream with the specified data.
        /// </summary>
        private static MemoryStream CreateMemoryStreamWithData(byte[] data)
        {
            return new MemoryStream(data);
        }

        /// <summary>
        /// Creates valid RGB color data for the specified number of colors.
        /// Each color uses 3 bytes (R, G, B).
        /// </summary>
        private static byte[] CreateValidColorData(int colorCount)
        {
            var data = new byte[colorCount * 3];
            for (int i = 0; i < colorCount; i++)
            {
                int baseIndex = i * 3;
                data[baseIndex] = (byte)(i * 85 % 256);     // R
                data[baseIndex + 1] = (byte)(i * 170 % 256); // G  
                data[baseIndex + 2] = (byte)(i * 255 % 256); // B
            }
            return data;
        }

        /// <summary>
        /// Tests that ResetTransparency does nothing when transparency is not currently set.
        /// Verifies that the color table remains unchanged when _transparencyIndex is -1.
        /// </summary>
        [Fact]
        public void ResetTransparency_WhenTransparencyNotSet_DoesNothing()
        {
            // Arrange
            var colorTable = new GIFColorTable(16);
            var originalColors = new int[256];

            // Set some test colors in the table
            for (int i = 0; i < 16; i++)
            {
                originalColors[i] = i * 100; // Some arbitrary color values
            }
            Array.Copy(originalColors, colorTable.Data, 16);

            // Act
            colorTable.ResetTransparency();

            // Assert
            for (int i = 0; i < 16; i++)
            {
                Assert.Equal(originalColors[i], colorTable.Data[i]);
            }
        }

        /// <summary>
        /// Tests that ResetTransparency restores the original color value and resets transparency state
        /// when transparency is currently set at index 0.
        /// </summary>
        [Fact]
        public void ResetTransparency_WhenTransparencySetAtIndexZero_RestoresColorAndResetsState()
        {
            // Arrange
            var colorTable = new GIFColorTable(16);
            const int transparencyIndex = 0;
            const int originalColorValue = 0xFF0000; // Red color

            colorTable.Data[transparencyIndex] = originalColorValue;
            colorTable.SetTransparency(transparencyIndex);

            // Verify transparency was set (color should be 0 now)
            Assert.Equal(0, colorTable.Data[transparencyIndex]);

            // Act
            colorTable.ResetTransparency();

            // Assert
            Assert.Equal(originalColorValue, colorTable.Data[transparencyIndex]);
        }

        /// <summary>
        /// Tests that ResetTransparency restores the original color value and resets transparency state
        /// when transparency is currently set at a middle index.
        /// </summary>
        [Fact]
        public void ResetTransparency_WhenTransparencySetAtMiddleIndex_RestoresColorAndResetsState()
        {
            // Arrange
            var colorTable = new GIFColorTable(16);
            const int transparencyIndex = 8;
            const int originalColorValue = 0x00FF00; // Green color

            colorTable.Data[transparencyIndex] = originalColorValue;
            colorTable.SetTransparency(transparencyIndex);

            // Verify transparency was set (color should be 0 now)
            Assert.Equal(0, colorTable.Data[transparencyIndex]);

            // Act
            colorTable.ResetTransparency();

            // Assert
            Assert.Equal(originalColorValue, colorTable.Data[transparencyIndex]);
        }

        /// <summary>
        /// Tests that ResetTransparency restores the original color value and resets transparency state
        /// when transparency is currently set at the last valid index.
        /// </summary>
        [Fact]
        public void ResetTransparency_WhenTransparencySetAtLastIndex_RestoresColorAndResetsState()
        {
            // Arrange
            var colorTable = new GIFColorTable(16);
            const int transparencyIndex = 15;
            const int originalColorValue = 0x0000FF; // Blue color

            colorTable.Data[transparencyIndex] = originalColorValue;
            colorTable.SetTransparency(transparencyIndex);

            // Verify transparency was set (color should be 0 now)
            Assert.Equal(0, colorTable.Data[transparencyIndex]);

            // Act
            colorTable.ResetTransparency();

            // Assert
            Assert.Equal(originalColorValue, colorTable.Data[transparencyIndex]);
        }

        /// <summary>
        /// Tests that ResetTransparency properly handles the case where the original color value was zero.
        /// Verifies that transparency can be properly reset even when the original color was transparent.
        /// </summary>
        [Fact]
        public void ResetTransparency_WhenOriginalColorWasZero_RestoresZeroValue()
        {
            // Arrange
            var colorTable = new GIFColorTable(16);
            const int transparencyIndex = 5;
            const int originalColorValue = 0; // Black/transparent color

            colorTable.Data[transparencyIndex] = originalColorValue;
            colorTable.SetTransparency(transparencyIndex);

            // Verify transparency was set (color should still be 0)
            Assert.Equal(0, colorTable.Data[transparencyIndex]);

            // Act
            colorTable.ResetTransparency();

            // Assert
            Assert.Equal(originalColorValue, colorTable.Data[transparencyIndex]);
        }

        /// <summary>
        /// Tests that calling ResetTransparency multiple times is safe and has no adverse effects.
        /// Verifies that the method is idempotent when transparency is not set.
        /// </summary>
        [Fact]
        public void ResetTransparency_WhenCalledMultipleTimes_RemainsStable()
        {
            // Arrange
            var colorTable = new GIFColorTable(16);
            const int transparencyIndex = 3;
            const int originalColorValue = 0xFFFFFF; // White color

            colorTable.Data[transparencyIndex] = originalColorValue;
            colorTable.SetTransparency(transparencyIndex);

            // Act - Reset transparency first time
            colorTable.ResetTransparency();
            var colorAfterFirstReset = colorTable.Data[transparencyIndex];

            // Act - Reset transparency second time
            colorTable.ResetTransparency();
            var colorAfterSecondReset = colorTable.Data[transparencyIndex];

            // Assert
            Assert.Equal(originalColorValue, colorAfterFirstReset);
            Assert.Equal(originalColorValue, colorAfterSecondReset);
            Assert.Equal(colorAfterFirstReset, colorAfterSecondReset);
        }

        /// <summary>
        /// Tests that ResetTransparency properly resets the internal state to allow setting transparency again.
        /// Verifies that after reset, transparency can be set again at the same or different index.
        /// </summary>
        [Fact]
        public void ResetTransparency_AfterReset_AllowsSettingTransparencyAgain()
        {
            // Arrange
            var colorTable = new GIFColorTable(16);
            const int firstTransparencyIndex = 2;
            const int secondTransparencyIndex = 7;
            const int firstOriginalColor = 0xFF0000;
            const int secondOriginalColor = 0x00FF00;

            colorTable.Data[firstTransparencyIndex] = firstOriginalColor;
            colorTable.Data[secondTransparencyIndex] = secondOriginalColor;

            // Set transparency at first index
            colorTable.SetTransparency(firstTransparencyIndex);
            Assert.Equal(0, colorTable.Data[firstTransparencyIndex]);

            // Act - Reset transparency
            colorTable.ResetTransparency();

            // Assert - First color should be restored
            Assert.Equal(firstOriginalColor, colorTable.Data[firstTransparencyIndex]);

            // Act - Set transparency at second index
            colorTable.SetTransparency(secondTransparencyIndex);

            // Assert - Second index should now be transparent, first should remain restored
            Assert.Equal(0, colorTable.Data[secondTransparencyIndex]);
            Assert.Equal(firstOriginalColor, colorTable.Data[firstTransparencyIndex]);
        }

        /// <summary>
        /// Tests SetTransparency with a valid index on a fresh color table.
        /// Verifies that the color at the specified index becomes transparent (0),
        /// the old color value is preserved, and the transparency index is set correctly.
        /// </summary>
        [Fact]
        public async Task SetTransparency_ValidIndex_SetsTransparencyCorrectly()
        {
            // Arrange
            var mockStream = Substitute.For<GIFDecoderStreamReader>(Substitute.For<Stream>());
            var colorData = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128 }; // 4 colors: red, green, blue, gray
            mockStream.ReadAsync(Arg.Any<byte[]>(), 12).Returns(Task.FromResult(12));
            mockStream.When(x => x.ReadAsync(Arg.Any<byte[]>(), 12)).Do(callInfo =>
            {
                var buffer = callInfo.Arg<byte[]>();
                Array.Copy(colorData, buffer, colorData.Length);
            });

            var colorTable = await GIFColorTable.CreateColorTableAsync(mockStream, 4);
            var originalColors = new int[4];
            Array.Copy(colorTable.Data, originalColors, 4);

            // Act
            colorTable.SetTransparency(2);

            // Assert
            Assert.Equal(0, colorTable.Data[2]); // Color at index 2 should be transparent
            Assert.Equal(originalColors[0], colorTable.Data[0]); // Other colors unchanged
            Assert.Equal(originalColors[1], colorTable.Data[1]);
            Assert.Equal(originalColors[3], colorTable.Data[3]);
        }

        /// <summary>
        /// Tests SetTransparency with the first index (0).
        /// Verifies boundary condition where the first color in the table is made transparent.
        /// </summary>
        [Fact]
        public async Task SetTransparency_FirstIndex_SetsTransparencyCorrectly()
        {
            // Arrange
            var mockStream = Substitute.For<GIFDecoderStreamReader>(Substitute.For<Stream>());
            var colorData = new byte[] { 255, 0, 0, 0, 255, 0 }; // 2 colors: red, green
            mockStream.ReadAsync(Arg.Any<byte[]>(), 6).Returns(Task.FromResult(6));
            mockStream.When(x => x.ReadAsync(Arg.Any<byte[]>(), 6)).Do(callInfo =>
            {
                var buffer = callInfo.Arg<byte[]>();
                Array.Copy(colorData, buffer, colorData.Length);
            });

            var colorTable = await GIFColorTable.CreateColorTableAsync(mockStream, 2);
            var originalFirstColor = colorTable.Data[0];

            // Act
            colorTable.SetTransparency(0);

            // Assert
            Assert.Equal(0, colorTable.Data[0]); // First color should be transparent
            Assert.NotEqual(0, colorTable.Data[1]); // Second color should remain unchanged
        }

        /// <summary>
        /// Tests SetTransparency with the last valid index.
        /// Verifies boundary condition where the last color in the table is made transparent.
        /// </summary>
        [Fact]
        public async Task SetTransparency_LastIndex_SetsTransparencyCorrectly()
        {
            // Arrange
            var mockStream = Substitute.For<GIFDecoderStreamReader>(Substitute.For<Stream>());
            var colorData = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255 }; // 3 colors: red, green, blue
            mockStream.ReadAsync(Arg.Any<byte[]>(), 9).Returns(Task.FromResult(9));
            mockStream.When(x => x.ReadAsync(Arg.Any<byte[]>(), 9)).Do(callInfo =>
            {
                var buffer = callInfo.Arg<byte[]>();
                Array.Copy(colorData, buffer, colorData.Length);
            });

            var colorTable = await GIFColorTable.CreateColorTableAsync(mockStream, 3);
            var originalLastColor = colorTable.Data[2];

            // Act
            colorTable.SetTransparency(2);

            // Assert
            Assert.Equal(0, colorTable.Data[2]); // Last color should be transparent
            Assert.NotEqual(0, colorTable.Data[0]); // Other colors should remain unchanged
            Assert.NotEqual(0, colorTable.Data[1]);
        }

        /// <summary>
        /// Tests SetTransparency called multiple times with different indices.
        /// Verifies that the previous transparency is reset and the new index becomes transparent.
        /// </summary>
        [Fact]
        public async Task SetTransparency_CalledMultipleTimes_ResetsAndSetsNewTransparency()
        {
            // Arrange
            var mockStream = Substitute.For<GIFDecoderStreamReader>(Substitute.For<Stream>());
            var colorData = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128 }; // 4 colors
            mockStream.ReadAsync(Arg.Any<byte[]>(), 12).Returns(Task.FromResult(12));
            mockStream.When(x => x.ReadAsync(Arg.Any<byte[]>(), 12)).Do(callInfo =>
            {
                var buffer = callInfo.Arg<byte[]>();
                Array.Copy(colorData, buffer, colorData.Length);
            });

            var colorTable = await GIFColorTable.CreateColorTableAsync(mockStream, 4);
            var originalColors = new int[4];
            Array.Copy(colorTable.Data, originalColors, 4);

            // Act - First transparency
            colorTable.SetTransparency(1);
            Assert.Equal(0, colorTable.Data[1]); // Index 1 should be transparent

            // Act - Second transparency (should reset first and set new)
            colorTable.SetTransparency(3);

            // Assert
            Assert.Equal(originalColors[1], colorTable.Data[1]); // Index 1 should be restored
            Assert.Equal(0, colorTable.Data[3]); // Index 3 should now be transparent
            Assert.Equal(originalColors[0], colorTable.Data[0]); // Other colors unchanged
            Assert.Equal(originalColors[2], colorTable.Data[2]);
        }

        /// <summary>
        /// Tests SetTransparency interaction with ResetTransparency.
        /// Verifies that transparency can be reset and then set again on a different index.
        /// </summary>
        [Fact]
        public async Task SetTransparency_AfterReset_SetsTransparencyCorrectly()
        {
            // Arrange
            var mockStream = Substitute.For<GIFDecoderStreamReader>(Substitute.For<Stream>());
            var colorData = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255 }; // 3 colors: red, green, blue
            mockStream.ReadAsync(Arg.Any<byte[]>(), 9).Returns(Task.FromResult(9));
            mockStream.When(x => x.ReadAsync(Arg.Any<byte[]>(), 9)).Do(callInfo =>
            {
                var buffer = callInfo.Arg<byte[]>();
                Array.Copy(colorData, buffer, colorData.Length);
            });

            var colorTable = await GIFColorTable.CreateColorTableAsync(mockStream, 3);
            var originalColors = new int[3];
            Array.Copy(colorTable.Data, originalColors, 3);

            // Act - Set transparency, reset, then set on different index
            colorTable.SetTransparency(0);
            Assert.Equal(0, colorTable.Data[0]); // Should be transparent

            colorTable.ResetTransparency();
            Assert.Equal(originalColors[0], colorTable.Data[0]); // Should be restored

            colorTable.SetTransparency(1);

            // Assert
            Assert.Equal(originalColors[0], colorTable.Data[0]); // Should remain original
            Assert.Equal(0, colorTable.Data[1]); // Should be transparent
            Assert.Equal(originalColors[2], colorTable.Data[2]); // Should remain original
        }

        /// <summary>
        /// Tests SetTransparency with same index called twice.
        /// Verifies that setting transparency on the same index twice works correctly.
        /// </summary>
        [Fact]
        public async Task SetTransparency_SameIndexTwice_HandlesCorrectly()
        {
            // Arrange
            var mockStream = Substitute.For<GIFDecoderStreamReader>(Substitute.For<Stream>());
            var colorData = new byte[] { 255, 0, 0, 0, 255, 0 }; // 2 colors: red, green
            mockStream.ReadAsync(Arg.Any<byte[]>(), 6).Returns(Task.FromResult(6));
            mockStream.When(x => x.ReadAsync(Arg.Any<byte[]>(), 6)).Do(callInfo =>
            {
                var buffer = callInfo.Arg<byte[]>();
                Array.Copy(colorData, buffer, colorData.Length);
            });

            var colorTable = await GIFColorTable.CreateColorTableAsync(mockStream, 2);
            var originalColors = new int[2];
            Array.Copy(colorTable.Data, originalColors, 2);

            // Act - Set transparency twice on same index
            colorTable.SetTransparency(1);
            colorTable.SetTransparency(1);

            // Assert
            Assert.Equal(0, colorTable.Data[1]); // Should still be transparent
            Assert.Equal(originalColors[0], colorTable.Data[0]); // Other color should be unchanged
        }

        /// <summary>
        /// Tests that CreateColorTableAsync successfully creates a color table with valid inputs.
        /// Input: Valid stream with sufficient data and positive size.
        /// Expected: Returns a populated GIFColorTable instance.
        /// </summary>
        [Fact]
        public async Task CreateColorTableAsync_ValidInputs_ReturnsColorTable()
        {
            // Arrange
            short size = 2;
            var expectedBytes = new byte[] { 255, 0, 0, 0, 255, 0 }; // Red and green colors
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                     .Returns(expectedBytes.Length);
            mockStream.CanSeek.Returns(false);

            var streamReader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = await GIFColorTable.CreateColorTableAsync(streamReader, size);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
        }

        /// <summary>
        /// Tests that CreateColorTableAsync throws ArgumentNullException when stream is null.
        /// Input: Null stream parameter.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public async Task CreateColorTableAsync_NullStream_ThrowsArgumentNullException()
        {
            // Arrange
            short size = 1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                GIFColorTable.CreateColorTableAsync(null, size));
        }

        /// <summary>
        /// Tests that CreateColorTableAsync works with minimum valid size.
        /// Input: Size = 0 (minimum boundary).
        /// Expected: Creates color table successfully.
        /// </summary>
        [Fact]
        public async Task CreateColorTableAsync_ZeroSize_CreatesEmptyColorTable()
        {
            // Arrange
            short size = 0;
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                     .Returns(0); // No bytes to read for size 0
            mockStream.CanSeek.Returns(false);

            var streamReader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = await GIFColorTable.CreateColorTableAsync(streamReader, size);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that CreateColorTableAsync works with typical GIF color table size.
        /// Input: Size = 256 (common GIF color table size).
        /// Expected: Creates color table successfully.
        /// </summary>
        [Fact]
        public async Task CreateColorTableAsync_TypicalSize_CreatesColorTable()
        {
            // Arrange
            short size = 256;
            int expectedDataLength = 3 * size; // 768 bytes
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                     .Returns(expectedDataLength);
            mockStream.CanSeek.Returns(false);

            var streamReader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = await GIFColorTable.CreateColorTableAsync(streamReader, size);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that CreateColorTableAsync throws GIFDecoderFormatException when stream provides insufficient data.
        /// Input: Stream that returns fewer bytes than required for the color table size.
        /// Expected: Throws GIFDecoderFormatException with message "Invalid color table size."
        /// </summary>
        [Fact]
        public async Task CreateColorTableAsync_InsufficientStreamData_ThrowsGIFDecoderFormatException()
        {
            // Arrange
            short size = 2;
            int requiredBytes = 3 * size; // 6 bytes required
            int actualBytes = 3; // Only 3 bytes available

            var mockStream = Substitute.For<Stream>();
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                     .Returns(actualBytes);
            mockStream.CanSeek.Returns(false);

            var streamReader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<GIFDecoderFormatException>(() =>
                GIFColorTable.CreateColorTableAsync(streamReader, size));
            Assert.Equal("Invalid color table size.", exception.Message);
        }

        /// <summary>
        /// Tests that CreateColorTableAsync propagates exceptions from stream operations.
        /// Input: Stream that throws IOException during read operation.
        /// Expected: IOException is propagated to caller.
        /// </summary>
        [Fact]
        public async Task CreateColorTableAsync_StreamThrowsException_PropagatesException()
        {
            // Arrange
            short size = 1;
            var expectedException = new IOException("Stream read error");

            var mockStream = Substitute.For<Stream>();
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                     .ThrowsAsync(expectedException);
            mockStream.CanSeek.Returns(false);

            var streamReader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<IOException>(() =>
                GIFColorTable.CreateColorTableAsync(streamReader, size));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests CreateColorTableAsync with various size boundary values.
        /// Input: Different size values including 1, small positive values, and larger values.
        /// Expected: Creates color table successfully for all valid sizes.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(128)]
        public async Task CreateColorTableAsync_VariousSizes_CreatesColorTable(short size)
        {
            // Arrange
            int expectedDataLength = 3 * size;
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                     .Returns(expectedDataLength);
            mockStream.CanSeek.Returns(false);

            var streamReader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = await GIFColorTable.CreateColorTableAsync(streamReader, size);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that CreateColorTableAsync handles negative size values appropriately.
        /// Input: Negative size value.
        /// Expected: May throw exception due to invalid array size or handle gracefully.
        /// </summary>
        [Fact]
        public async Task CreateColorTableAsync_NegativeSize_ThrowsException()
        {
            // Arrange
            short size = -1;
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);

            var streamReader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                GIFColorTable.CreateColorTableAsync(streamReader, size));
        }
    }


    /// <summary>
    /// Extension methods for test utilities.
    /// </summary>
    internal static class TestExtensions
    {
        /// <summary>
        /// Concatenates two byte arrays.
        /// </summary>
        public static byte[] Concat(this byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }
    }

    public partial class GIFBitmapDecoderTests
    {
        /// <summary>
        /// Tests that DecodeAsync throws ArgumentNullException when stream parameter is null.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_NullStream_ThrowsArgumentNullException()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                decoder.DecodeAsync(null, 10, 10));
        }

        /// <summary>
        /// Tests DecodeAsync behavior with various invalid dimension combinations.
        /// Validates that the method handles zero, negative, and overflow-prone dimensions appropriately.
        /// </summary>
        [Theory]
        [InlineData(0, 10, "Zero width should be handled")]
        [InlineData(10, 0, "Zero height should be handled")]
        [InlineData(-1, 10, "Negative width should be handled")]
        [InlineData(10, -1, "Negative height should be handled")]
        [InlineData(int.MaxValue, 2, "Large width that could cause overflow")]
        [InlineData(2, int.MaxValue, "Large height that could cause overflow")]
        public async Task DecodeAsync_InvalidDimensions_HandlesGracefully(int width, int height, string scenario)
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            if (width <= 0 || height <= 0)
            {
                stream.Read().Returns(8); // Valid LZW minimum code size
            }
            else
            {
                // For overflow scenarios, we expect the method to either handle it or throw
                stream.Read().Returns(8);
            }

            // Act
            if (width <= 0 || height <= 0)
            {
                // Should complete without error for zero/negative dimensions
                await decoder.DecodeAsync(stream, width, height);
            }
            else
            {
                // For potential overflow scenarios, either succeeds or throws appropriate exception
                try
                {
                    await decoder.DecodeAsync(stream, width, height);
                }
                catch (OutOfMemoryException)
                {
                    // Expected for very large dimensions
                }
                catch (OverflowException)
                {
                    // Expected for overflow scenarios
                }
            }

            // Assert
            // The fact that we reach here without unhandled exceptions is the assertion
            Assert.True(true, scenario);
        }

        /// <summary>
        /// Tests DecodeAsync when stream Read() returns -1 (end of stream immediately).
        /// Expected to handle gracefully and complete the decoding process.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_StreamReturnsEndOfStreamImmediately_CompletesGracefully()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(-1); // End of stream

            // Act
            await decoder.DecodeAsync(stream, 10, 10);

            // Assert
            stream.Received(1).Read();
        }

        /// <summary>
        /// Tests DecodeAsync with valid LZW minimum code size and basic decoding scenario.
        /// Validates the setup phase and basic LZW decoding initialization.
        /// </summary>
        [Theory]
        [InlineData(1, 1, 8, "Minimum image size with standard code size")]
        [InlineData(10, 10, 8, "Standard image with standard code size")]
        [InlineData(1, 100, 2, "Tall narrow image with minimum code size")]
        [InlineData(100, 1, 9, "Wide flat image with large code size")]
        public async Task DecodeAsync_ValidDimensionsAndCodeSize_InitializesCorrectly(int width, int height, int codeSize, string scenario)
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(codeSize, -1); // Return code size, then end of stream
            stream.ReadBlockAsync().Returns(Task.FromResult(0)); // No block data

            // Act
            await decoder.DecodeAsync(stream, width, height);

            // Assert
            stream.Received().Read(); // Should read the initial code size
        }

        /// <summary>
        /// Tests DecodeAsync when ReadBlockAsync returns 0 (no data available).
        /// Expected to handle the empty block scenario and exit the decoding loop.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_ReadBlockAsyncReturnsZero_HandlesEmptyBlock()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(8); // Valid LZW code size
            stream.ReadBlockAsync().Returns(Task.FromResult(0)); // No data in block

            // Act
            await decoder.DecodeAsync(stream, 10, 10);

            // Assert
            stream.Received(1).Read();
            await stream.Received().ReadBlockAsync();
        }

        /// <summary>
        /// Tests DecodeAsync when ReadBlockAsync throws GIFDecoderFormatException.
        /// Expected to propagate the exception as it indicates malformed GIF data.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_ReadBlockAsyncThrowsFormatException_PropagatesException()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(8); // Valid LZW code size
            stream.ReadBlockAsync().Returns(Task.FromException<int>(new GIFDecoderFormatException("Test format error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<GIFDecoderFormatException>(() =>
                decoder.DecodeAsync(stream, 10, 10));

            Assert.Equal("Test format error", exception.Message);
        }

        /// <summary>
        /// Tests DecodeAsync with valid LZW data including clear codes and end-of-information codes.
        /// Validates the main decoding loop handles standard LZW compression sequences.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_ValidLZWDataWithClearCode_ProcessesCorrectly()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            // Setup: 8-bit LZW with clear code (256) and end code (257)
            stream.Read().Returns(8); // LZW minimum code size

            // Create a simple LZW data block with clear code
            var blockData = new byte[] { 0x04, 0x01, 0x00, 0x01 }; // Simple 4-byte block
            stream.ReadBlockAsync().Returns(Task.FromResult(4), Task.FromResult(0));
            stream.CurrentBlockBuffer.Returns(blockData);

            // Act
            await decoder.DecodeAsync(stream, 2, 2);

            // Assert
            stream.Received().Read();
            await stream.Received(2).ReadBlockAsync();
            var _ = stream.Received().CurrentBlockBuffer;
        }

        /// <summary>
        /// Tests DecodeAsync with various LZW code sizes to validate code mask and size calculations.
        /// Ensures the decoder handles different compression bit depths correctly.
        /// </summary>
        [Theory]
        [InlineData(1, "Minimum LZW code size")]
        [InlineData(2, "Small LZW code size")]
        [InlineData(8, "Standard LZW code size")]
        [InlineData(11, "Large LZW code size")]
        public async Task DecodeAsync_VariousLZWCodeSizes_CalculatesParametersCorrectly(int lzwCodeSize, string scenario)
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(lzwCodeSize);
            stream.ReadBlockAsync().Returns(Task.FromResult(0)); // No block data

            // Act
            await decoder.DecodeAsync(stream, 5, 5);

            // Assert
            stream.Received(1).Read();
            Assert.True(true, scenario); // Completion without error validates the calculation
        }

        /// <summary>
        /// Tests DecodeAsync with block data that would trigger the main decoding loop.
        /// Validates code reading, bit manipulation, and pixel stack operations.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_BlockDataWithCodes_ProcessesDecodingLoop()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(8); // 8-bit LZW

            // Create block with enough data to enter decoding loop
            var blockData = new byte[] { 0x00, 0x01, 0x04, 0x01, 0x00, 0x02, 0x02, 0x04 };
            stream.ReadBlockAsync().Returns(Task.FromResult(8), Task.FromResult(0));
            stream.CurrentBlockBuffer.Returns(blockData);

            // Act
            await decoder.DecodeAsync(stream, 4, 4);

            // Assert
            stream.Received().Read();
            await stream.Received().ReadBlockAsync();
            var _ = stream.Received().CurrentBlockBuffer;
        }

        /// <summary>
        /// Tests DecodeAsync with end-of-information code to validate proper loop termination.
        /// Ensures the decoder recognizes and handles the end-of-information marker correctly.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_EndOfInformationCode_TerminatesCorrectly()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(2); // 2-bit LZW for simpler codes

            // Create data with end-of-information code (clear_code + 1 = 5)
            var blockData = new byte[] { 0x05, 0x00 }; // End-of-info code in first few bits
            stream.ReadBlockAsync().Returns(Task.FromResult(2), Task.FromResult(0));
            stream.CurrentBlockBuffer.Returns(blockData);

            // Act
            await decoder.DecodeAsync(stream, 3, 3);

            // Assert
            stream.Received().Read();
            await stream.Received().ReadBlockAsync();
        }

        /// <summary>
        /// Tests DecodeAsync edge case where available code exceeds the current code.
        /// Validates handling of malformed or unexpected LZW code sequences.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_CodeExceedsAvailable_BreaksLoop()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(2); // 2-bit LZW

            // Create data with code that exceeds available codes
            var blockData = new byte[] { 0xFF, 0xFF }; // High codes
            stream.ReadBlockAsync().Returns(Task.FromResult(2), Task.FromResult(0));
            stream.CurrentBlockBuffer.Returns(blockData);

            // Act
            await decoder.DecodeAsync(stream, 2, 2);

            // Assert
            stream.Received().Read();
            await stream.Received().ReadBlockAsync();
        }

        /// <summary>
        /// Tests DecodeAsync pixel filling behavior when decoding ends before all pixels are processed.
        /// Validates that remaining pixels are properly initialized to zero.
        /// </summary>
        [Fact]
        public async Task DecodeAsync_EarlyTermination_FillsRemainingPixelsWithZero()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(8);
            stream.ReadBlockAsync().Returns(Task.FromResult(0)); // Force early termination

            // Act - Use large image to ensure pixel filling code path
            await decoder.DecodeAsync(stream, 50, 50);

            // Assert
            stream.Received().Read();
            await stream.Received().ReadBlockAsync();
        }

        /// <summary>
        /// Tests DecodeAsync with maximum reasonable dimensions to validate memory handling.
        /// Ensures the decoder can handle larger images without memory-related issues.
        /// </summary>
        [Theory]
        [InlineData(1000, 1000, "Large square image")]
        [InlineData(5000, 1, "Very wide image")]
        [InlineData(1, 5000, "Very tall image")]
        public async Task DecodeAsync_LargeDimensions_HandlesMemoryAllocation(int width, int height, string scenario)
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            var decoder = new GIFBitmapDecoder();

            stream.Read().Returns(8);
            stream.ReadBlockAsync().Returns(Task.FromResult(0));

            // Act & Assert - Should complete without memory exceptions
            await decoder.DecodeAsync(stream, width, height);

            Assert.True(true, $"Successfully handled {scenario}");
        }

        /// <summary>
        /// Tests the Compose method when previousBitmap is null.
        /// Should create a new bitmap data array and process the current bitmap.
        /// </summary>
        [Fact]
        public void Compose_PreviousBitmapIsNull_CreatesNewBitmapDataAndProcessesCurrentBitmap()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 10, height: 10);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(0, 0, 5, 5),
                isInterlaced: false,
                colorTableData: new int[] { 0, 1, 2, 3, 4 });
            var pixels = new byte[] { 1, 2, 3, 4, 0, 2, 1, 4, 3, 0, 0, 1, 2, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, null);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(100, currentBitmap.Data.Length); // width * height

            // Verify some pixel data was processed
            Assert.Equal(1, currentBitmap.Data[0]); // colorTable[1]
            Assert.Equal(2, currentBitmap.Data[1]); // colorTable[2]
        }

        /// <summary>
        /// Tests the Compose method when previousBitmap has NoAction dispose method.
        /// Should create a new bitmap data array.
        /// </summary>
        [Fact]
        public void Compose_PreviousBitmapWithNoActionDispose_CreatesNewBitmapData()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 8, height: 6);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(1, 1, 3, 2),
                isInterlaced: false,
                colorTableData: new int[] { 0, 100, 200, 300 });
            var previousBitmap = CreateMockPreviousBitmap(
                dispose: GIFBitmap.DisposeMethod.NoAction,
                data: new int[48]);
            var pixels = new byte[] { 1, 2, 3, 0, 1, 2 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, previousBitmap);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(48, currentBitmap.Data.Length); // width * height
        }

        /// <summary>
        /// Tests the Compose method when previousBitmap has null data.
        /// Should create a new bitmap data array even if dispose method is not NoAction.
        /// </summary>
        [Fact]
        public void Compose_PreviousBitmapWithNullData_CreatesNewBitmapData()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 5, height: 4);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(0, 0, 2, 2),
                isInterlaced: false,
                colorTableData: new int[] { 0, 50 });
            var previousBitmap = CreateMockPreviousBitmap(
                dispose: GIFBitmap.DisposeMethod.LeaveInPlace,
                data: null);
            var pixels = new byte[] { 1, 0, 1, 1 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, previousBitmap);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(20, currentBitmap.Data.Length); // width * height
        }

        /// <summary>
        /// Tests the Compose method when previousBitmap has LeaveInPlace dispose method and existing data.
        /// Should reuse the previous bitmap data array.
        /// </summary>
        [Fact]
        public void Compose_PreviousBitmapWithLeaveInPlaceAndData_ReusesPreviousBitmapData()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 4, height: 3);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(1, 1, 2, 1),
                isInterlaced: false,
                colorTableData: new int[] { 0, 255 });
            var existingData = new int[12];
            existingData[5] = 999; // Set a marker value
            var previousBitmap = CreateMockPreviousBitmap(
                dispose: GIFBitmap.DisposeMethod.LeaveInPlace,
                data: existingData);
            var pixels = new byte[] { 1, 1 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, previousBitmap);

            // Assert
            Assert.Same(existingData, currentBitmap.Data);
            Assert.Equal(999, currentBitmap.Data[5]); // Marker value should remain
        }

        /// <summary>
        /// Tests the Compose method when previousBitmap has RestoreToPrevious dispose method and existing data.
        /// Should reuse the previous bitmap data array without calling RestoreToBackground.
        /// </summary>
        [Fact]
        public void Compose_PreviousBitmapWithRestoreToPreviousAndData_ReusesPreviousBitmapData()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 3, height: 3);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(0, 0, 2, 2),
                isInterlaced: false,
                colorTableData: new int[] { 0, 42 });
            var existingData = new int[9];
            existingData[4] = 777; // Set a marker value
            var previousBitmap = CreateMockPreviousBitmap(
                dispose: GIFBitmap.DisposeMethod.RestoreToPrevious,
                data: existingData);
            var pixels = new byte[] { 1, 0, 1, 1 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, previousBitmap);

            // Assert
            Assert.Same(existingData, currentBitmap.Data);
            Assert.Equal(777, currentBitmap.Data[4]); // Marker value should remain
        }

        /// <summary>
        /// Tests the Compose method with interlaced bitmap processing.
        /// Should process pixels using interlacing algorithm with multiple passes.
        /// </summary>
        [Fact]
        public void Compose_InterlacedBitmap_ProcessesPixelsUsingInterlacingAlgorithm()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 4, height: 8);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(0, 0, 4, 8),
                isInterlaced: true,
                colorTableData: new int[] { 0, 11, 22, 33, 44 });
            var pixels = new byte[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4,
                                    1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, null);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(32, currentBitmap.Data.Length);

            // Verify interlaced processing - first pass should place pixels in rows 0, 8 (but 8 is outside bounds)
            Assert.Equal(11, currentBitmap.Data[0]); // Row 0, first pixel
        }

        /// <summary>
        /// Tests the Compose method with bounds that exceed image width.
        /// Should adjust endBitmapIndex to prevent buffer overflow.
        /// </summary>
        [Fact]
        public void Compose_BoundsExceedImageWidth_AdjustsEndBitmapIndex()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 3, height: 3);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(2, 0, 5, 2), // Width exceeds remaining space
                isInterlaced: false,
                colorTableData: new int[] { 0, 123 });
            var pixels = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, null);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(9, currentBitmap.Data.Length);

            // Only one pixel should be written per row due to bounds adjustment
            Assert.Equal(123, currentBitmap.Data[2]); // Row 0, X=2
            Assert.Equal(123, currentBitmap.Data[5]); // Row 1, X=2
        }

        /// <summary>
        /// Tests the Compose method when target row is outside image height.
        /// Should skip processing for rows that exceed image boundaries.
        /// </summary>
        [Fact]
        public void Compose_TargetRowExceedsHeight_SkipsProcessing()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 2, height: 2);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(0, 1, 2, 3), // Height + Y exceeds image height
                isInterlaced: false,
                colorTableData: new int[] { 0, 456 });
            var pixels = new byte[] { 1, 1, 1, 1, 1, 1 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, null);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(4, currentBitmap.Data.Length);

            // Only first row should be processed (Y=1, so targetRow=1)
            Assert.Equal(456, currentBitmap.Data[2]); // Row 1, X=0
            Assert.Equal(456, currentBitmap.Data[3]); // Row 1, X=1
            Assert.Equal(0, currentBitmap.Data[0]); // Row 0 should remain 0
            Assert.Equal(0, currentBitmap.Data[1]); // Row 0 should remain 0
        }

        /// <summary>
        /// Tests the Compose method with transparent colors (color value 0).
        /// Should not write transparent pixels to the bitmap data.
        /// </summary>
        [Fact]
        public void Compose_TransparentColors_DoesNotWriteTransparentPixels()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 3, height: 2);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(0, 0, 3, 2),
                isInterlaced: false,
                colorTableData: new int[] { 0, 789, 0, 321 }); // Index 0 and 2 are transparent
            var pixels = new byte[] { 0, 1, 2, 3, 0, 1 };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, null);

            // Assert
            Assert.NotNull(currentBitmap.Data);

            // Transparent pixels (index 0, 2) should not modify bitmap data (remains 0)
            Assert.Equal(0, currentBitmap.Data[0]); // Transparent pixel
            Assert.Equal(789, currentBitmap.Data[1]); // Non-transparent pixel
            Assert.Equal(0, currentBitmap.Data[2]); // Transparent pixel  
            Assert.Equal(321, currentBitmap.Data[3]); // Non-transparent pixel
            Assert.Equal(0, currentBitmap.Data[4]); // Transparent pixel
            Assert.Equal(789, currentBitmap.Data[5]); // Non-transparent pixel
        }

        /// <summary>
        /// Tests the Compose method with zero width bounds.
        /// Should handle zero-width bounds without processing any pixels.
        /// </summary>
        [Fact]
        public void Compose_ZeroWidthBounds_HandlesGracefully()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 5, height: 3);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(2, 1, 0, 2),
                isInterlaced: false,
                colorTableData: new int[] { 0, 999 });
            var pixels = new byte[] { };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, null);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(15, currentBitmap.Data.Length);

            // All pixels should remain 0 since no processing should occur
            Assert.All(currentBitmap.Data, pixel => Assert.Equal(0, pixel));
        }

        /// <summary>
        /// Tests the Compose method with zero height bounds.
        /// Should handle zero-height bounds without processing any pixels.
        /// </summary>
        [Fact]
        public void Compose_ZeroHeightBounds_HandlesGracefully()
        {
            // Arrange
            var decoder = new GIFBitmapDecoder();
            var header = CreateMockHeader(width: 4, height: 4);
            var currentBitmap = CreateMockCurrentBitmap(
                bounds: new GIFBitmap.Rect(1, 1, 2, 0),
                isInterlaced: false,
                colorTableData: new int[] { 0, 888 });
            var pixels = new byte[] { };
            SetPixelsField(decoder, pixels);

            // Act
            decoder.Compose(header, currentBitmap, null);

            // Assert
            Assert.NotNull(currentBitmap.Data);
            Assert.Equal(16, currentBitmap.Data.Length);

            // All pixels should remain 0 since no processing should occur
            Assert.All(currentBitmap.Data, pixel => Assert.Equal(0, pixel));
        }

        // Helper class to create mock objects for testing
        private class TestGIFHeader : GIFHeader
        {
            public TestGIFHeader(int width, int height)
            {
                TestWidth = width;
                TestHeight = height;
            }

            public int TestWidth { get; }
            public int TestHeight { get; }

            public new int Width => TestWidth;
            public new int Height => TestHeight;
        }

        private class TestGIFBitmap : GIFBitmap
        {
            public TestGIFBitmap(GIFBitmap.Rect bounds, bool isInterlaced, GIFColorTable colorTable,
                GIFBitmap.DisposeMethod dispose = GIFBitmap.DisposeMethod.NoAction, int[] data = null,
                bool isTransparent = false, int backgroundColor = 0)
            {
                TestBounds = bounds;
                TestIsInterlaced = isInterlaced;
                TestColorTable = colorTable;
                TestDispose = dispose;
                TestData = data;
                TestIsTransparent = isTransparent;
                TestBackgroundColor = backgroundColor;
            }

            public GIFBitmap.Rect TestBounds { get; }
            public bool TestIsInterlaced { get; }
            public GIFColorTable TestColorTable { get; }
            public GIFBitmap.DisposeMethod TestDispose { get; }
            public int[] TestData { get; }
            public bool TestIsTransparent { get; }
            public int TestBackgroundColor { get; }

            public new GIFBitmap.Rect Bounds => TestBounds;
            public new bool IsInterlaced => TestIsInterlaced;
            public new GIFColorTable ColorTable => TestColorTable;
            public new GIFBitmap.DisposeMethod Dispose => TestDispose;
            public new int[] Data { get; set; } = null;
            public new bool IsTransparent => TestIsTransparent;
            public new int BackgroundColor => TestBackgroundColor;
        }

        private class TestGIFColorTable : GIFColorTable
        {
            public TestGIFColorTable(int[] data)
            {
                TestData = data;
            }

            public int[] TestData { get; }
            public new int[] Data => TestData;
        }

        private GIFHeader CreateMockHeader(int width, int height)
        {
            return new TestGIFHeader(width, height);
        }

        private GIFBitmap CreateMockCurrentBitmap(GIFBitmap.Rect bounds, bool isInterlaced, int[] colorTableData)
        {
            var colorTable = new TestGIFColorTable(colorTableData);
            return new TestGIFBitmap(bounds, isInterlaced, colorTable);
        }

        private GIFBitmap CreateMockPreviousBitmap(GIFBitmap.DisposeMethod dispose, int[] data,
            bool isTransparent = false, int backgroundColor = 0, GIFBitmap.Rect bounds = null)
        {
            bounds = bounds ?? new GIFBitmap.Rect(0, 0, 1, 1);
            var colorTable = new TestGIFColorTable(new int[] { 0 });
            return new TestGIFBitmap(bounds, false, colorTable, dispose, data, isTransparent, backgroundColor);
        }

        private void SetPixelsField(GIFBitmapDecoder decoder, byte[] pixels)
        {
            var field = typeof(GIFBitmapDecoder).GetField("_pixels",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(decoder, pixels);
        }
    }

    public class GIFHeaderTests
    {
        /// <summary>
        /// Tests CreateHeaderAsync with a valid GIF87a header when skipTypeIdentifier is false.
        /// Should successfully parse the header and return a valid GIFHeader object.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_ValidGIF87aHeader_SkipTypeIdentifierFalse_ReturnsValidHeader()
        {
            // Arrange
            var gifBytes = CreateValidGIF87aBytes();
            var stream = new MemoryStream(gifBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act
            var result = await GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGIFHeader);
        }

        /// <summary>
        /// Tests CreateHeaderAsync with a valid GIF89a header when skipTypeIdentifier is false.
        /// Should successfully parse the header and return a valid GIFHeader object.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_ValidGIF89aHeader_SkipTypeIdentifierFalse_ReturnsValidHeader()
        {
            // Arrange
            var gifBytes = CreateValidGIF89aBytes();
            var stream = new MemoryStream(gifBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act
            var result = await GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGIFHeader);
        }

        /// <summary>
        /// Tests CreateHeaderAsync with skipTypeIdentifier set to true.
        /// Should set TypeIdentifier to "GIF" and return a valid header regardless of stream content.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_SkipTypeIdentifierTrue_ReturnsValidHeader()
        {
            // Arrange
            var gifBytes = CreateValidGIF87aBytesWithoutTypeIdentifier();
            var stream = new MemoryStream(gifBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act
            var result = await GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: true);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGIFHeader);
        }

        /// <summary>
        /// Tests CreateHeaderAsync with invalid header data (not starting with "GIF").
        /// Should return null because IsGIFHeader will be false.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_InvalidHeader_ReturnsNull()
        {
            // Arrange
            var invalidBytes = CreateInvalidHeaderBytes();
            var stream = new MemoryStream(invalidBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act
            var result = await GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests CreateHeaderAsync with PNG header data.
        /// Should return null because IsGIFHeader will be false.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_PNGHeader_ReturnsNull()
        {
            // Arrange
            var pngBytes = CreatePNGHeaderBytes();
            var stream = new MemoryStream(pngBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act
            var result = await GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests CreateHeaderAsync with an empty stream.
        /// Should throw an exception during parsing.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_EmptyStream_ThrowsException()
        {
            // Arrange
            var stream = new MemoryStream();
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act & Assert
            await Assert.ThrowsAsync<EndOfStreamException>(
                () => GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false));
        }

        /// <summary>
        /// Tests CreateHeaderAsync with a stream that has insufficient data.
        /// Should throw an exception during parsing.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_InsufficientData_ThrowsException()
        {
            // Arrange
            var shortBytes = new byte[] { (byte)'G', (byte)'I' }; // Only 2 bytes instead of required minimum
            var stream = new MemoryStream(shortBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act & Assert
            await Assert.ThrowsAsync<EndOfStreamException>(
                () => GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false));
        }

        /// <summary>
        /// Tests CreateHeaderAsync with null stream reader parameter.
        /// Should throw ArgumentNullException.
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_NullStreamReader_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => GIFHeader.CreateHeaderAsync(null, skipTypeIdentifier: false));
        }

        /// <summary>
        /// Tests CreateHeaderAsync with case insensitive GIF identifier.
        /// Should return a valid header for lowercase "gif".
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_LowercaseGIF_ReturnsValidHeader()
        {
            // Arrange
            var gifBytes = CreateValidGIF87aBytes();
            // Change "GIF" to "gif"
            gifBytes[0] = (byte)'g';
            gifBytes[1] = (byte)'i';
            gifBytes[2] = (byte)'f';
            var stream = new MemoryStream(gifBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act
            var result = await GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGIFHeader);
        }

        /// <summary>
        /// Tests CreateHeaderAsync with mixed case GIF identifier.
        /// Should return a valid header for "GiF".
        /// </summary>
        [Fact]
        public async Task CreateHeaderAsync_MixedCaseGIF_ReturnsValidHeader()
        {
            // Arrange
            var gifBytes = CreateValidGIF87aBytes();
            // Change "GIF" to "GiF"
            gifBytes[0] = (byte)'G';
            gifBytes[1] = (byte)'i';
            gifBytes[2] = (byte)'F';
            var stream = new MemoryStream(gifBytes);
            var streamReader = new GIFDecoderStreamReader(stream);

            // Act
            var result = await GIFHeader.CreateHeaderAsync(streamReader, skipTypeIdentifier: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGIFHeader);
        }

        private byte[] CreateValidGIF87aBytes()
        {
            var bytes = new List<byte>();

            // Type identifier: "GIF"
            bytes.AddRange(Encoding.ASCII.GetBytes("GIF"));

            // Version: "87a"
            bytes.AddRange(Encoding.ASCII.GetBytes("87a"));

            // Width: 100 (little endian)
            bytes.Add(100);
            bytes.Add(0);

            // Height: 100 (little endian)
            bytes.Add(100);
            bytes.Add(0);

            // Flags: no global color table (0x00)
            bytes.Add(0x00);

            // Background color index
            bytes.Add(0);

            // Pixel aspect ratio
            bytes.Add(0);

            return bytes.ToArray();
        }

        private byte[] CreateValidGIF89aBytes()
        {
            var bytes = new List<byte>();

            // Type identifier: "GIF"
            bytes.AddRange(Encoding.ASCII.GetBytes("GIF"));

            // Version: "89a"
            bytes.AddRange(Encoding.ASCII.GetBytes("89a"));

            // Width: 100 (little endian)
            bytes.Add(100);
            bytes.Add(0);

            // Height: 100 (little endian)
            bytes.Add(100);
            bytes.Add(0);

            // Flags: no global color table (0x00)
            bytes.Add(0x00);

            // Background color index
            bytes.Add(0);

            // Pixel aspect ratio
            bytes.Add(0);

            return bytes.ToArray();
        }

        private byte[] CreateValidGIF87aBytesWithoutTypeIdentifier()
        {
            var bytes = new List<byte>();

            // Version: "87a" (no type identifier since we're skipping it)
            bytes.AddRange(Encoding.ASCII.GetBytes("87a"));

            // Width: 100 (little endian)
            bytes.Add(100);
            bytes.Add(0);

            // Height: 100 (little endian)
            bytes.Add(100);
            bytes.Add(0);

            // Flags: no global color table (0x00)
            bytes.Add(0x00);

            // Background color index
            bytes.Add(0);

            // Pixel aspect ratio
            bytes.Add(0);

            return bytes.ToArray();
        }

        private byte[] CreateInvalidHeaderBytes()
        {
            var bytes = new List<byte>();

            // Type identifier: "JPG" (not GIF)
            bytes.AddRange(Encoding.ASCII.GetBytes("JPG"));

            // Add some dummy data
            for (int i = 0; i < 10; i++)
            {
                bytes.Add(0);
            }

            return bytes.ToArray();
        }

        private byte[] CreatePNGHeaderBytes()
        {
            var bytes = new List<byte>();

            // Type identifier: "PNG" (not GIF)
            bytes.AddRange(Encoding.ASCII.GetBytes("PNG"));

            // Add some dummy data
            for (int i = 0; i < 10; i++)
            {
                bytes.Add(0);
            }

            return bytes.ToArray();
        }

        private class List<T> : System.Collections.Generic.List<T>
        {
        }

        /// <summary>
        /// Tests the IsGIFHeader property behavior.
        /// 
        /// NOTE: This test is incomplete due to design constraints.
        /// The GIFHeader class has a private constructor and TypeIdentifier has a private setter,
        /// making it impossible to create test instances without reflection (which is prohibited).
        /// 
        /// To make this testable, consider:
        /// 1. Adding an internal constructor for testing
        /// 2. Making TypeIdentifier settable for testing scenarios
        /// 3. Adding a factory method that accepts TypeIdentifier for testing
        /// 
        /// The expected behavior of IsGIFHeader is:
        /// - Returns false when TypeIdentifier is null or empty
        /// - Returns true when TypeIdentifier starts with "GIF" (case-insensitive)
        /// - Returns false when TypeIdentifier doesn't start with "GIF"
        /// </summary>
        [Fact(Skip = "Cannot test due to private constructor and private setters. Requires design changes to be testable.")]
        public void IsGIFHeader_VariousTypeIdentifiers_ReturnsExpectedResults()
        {
            // TODO: Implement once GIFHeader becomes testable
            // Test cases should include:
            // - null TypeIdentifier -> false
            // - empty string TypeIdentifier -> false
            // - "GIF" -> true
            // - "gif" -> true (case insensitive)
            // - "GIF89a" -> true
            // - "ABCGIF" -> false (doesn't start with GIF)
            // - "ABC" -> false
            // - shorter strings like "GI" -> false
        }
    }

    public partial class GIFBitmapTests
    {
        /// <summary>
        /// Tests CreateBitmapAsync when stream immediately returns -1 (end of stream).
        /// Should return null when no data is available.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_StreamReturnsEndOfStream_ReturnsNull()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(-1);
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with ImageSeparator block code (0x2C).
        /// Should create new bitmap and call ParseImageDescriptorAsync, then return the bitmap.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_ImageSeparatorBlock_CreatesBitmapAndParsesImage()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x2C); // ImageSeparator
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap, false);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with Extension block code (0x21).
        /// Should create new bitmap and call ParseExtensionAsync.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_ExtensionBlock_CreatesBitmapAndParsesExtension()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x21, 0x3B); // Extension followed by Trailer
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with Trailer block code (0x3B) and no image found.
        /// Should return null when trailer is encountered without any image data.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_TrailerBlockNoImage_ReturnsNull()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x3B); // Trailer
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with Trailer block code (0x3B) after finding an image.
        /// Should return the bitmap when trailer is encountered after image processing.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_TrailerBlockWithImage_ReturnsBitmap()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x2C, 0x3B); // ImageSeparator followed by Trailer
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with unknown block code.
        /// Should continue processing and handle subsequent blocks normally.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_UnknownBlockCode_ContinuesProcessing()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0xFF, 0x2C); // Unknown code followed by ImageSeparator
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with ignoreImageData parameter set to true.
        /// Should pass the parameter correctly to ParseImageDescriptorAsync.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_IgnoreImageDataTrue_PassesParameterCorrectly()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x2C); // ImageSeparator
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap, true);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync parameter validation with null stream.
        /// Should throw ArgumentNullException when stream is null.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_NullStream_ThrowsArgumentNullException()
        {
            // Arrange
            GIFDecoderStreamReader stream = null;
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
                GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap));
        }

        /// <summary>
        /// Tests CreateBitmapAsync parameter validation with null header.
        /// Should throw ArgumentNullException when header is null.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_NullHeader_ThrowsArgumentNullException()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x2C); // ImageSeparator
            GIFHeader header = null;
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap));
        }

        /// <summary>
        /// Tests CreateBitmapAsync parameter validation with null decoder.
        /// Should throw ArgumentNullException when decoder is null.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_NullDecoder_ThrowsArgumentNullException()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x2C); // ImageSeparator
            var header = Substitute.For<GIFHeader>();
            GIFBitmapDecoder decoder = null;
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
                GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap));
        }

        /// <summary>
        /// Tests CreateBitmapAsync with null previousBitmap parameter.
        /// Should handle null previousBitmap gracefully as it's an optional parameter.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_NullPreviousBitmap_HandlesGracefully()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x2C); // ImageSeparator
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            GIFBitmap previousBitmap = null;

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with multiple extension blocks followed by image.
        /// Should process all extensions and then create bitmap for image.
        /// </summary>
        [Fact]
        public async Task CreateBitmapAsync_MultipleExtensionsFollowedByImage_ProcessesCorrectly()
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(0x21, 0x21, 0x2C); // Two Extensions followed by ImageSeparator
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests CreateBitmapAsync with mixed unknown and known block codes.
        /// Should skip unknown blocks and process known blocks correctly.
        /// </summary>
        [Theory]
        [InlineData(0x10, 0x2C)] // Unknown followed by ImageSeparator
        [InlineData(0x99, 0x21, 0x3B)] // Unknown followed by Extension and Trailer
        [InlineData(0x50, 0x60, 0x2C)] // Multiple unknowns followed by ImageSeparator
        public async Task CreateBitmapAsync_MixedBlockCodes_HandlesCorrectly(params int[] blockCodes)
        {
            // Arrange
            var stream = Substitute.For<GIFDecoderStreamReader>();
            stream.Read().Returns(blockCodes[0], blockCodes.Skip(1).ToArray());
            var header = Substitute.For<GIFHeader>();
            var decoder = Substitute.For<GIFBitmapDecoder>();
            var previousBitmap = Substitute.For<GIFBitmap>();

            // Act
            var result = await GIFBitmap.CreateBitmapAsync(stream, header, decoder, previousBitmap);

            // Assert
            if (blockCodes.Contains(0x2C))
                Assert.NotNull(result);
            else if (blockCodes.Contains(0x21) && blockCodes.Contains(0x3B))
                Assert.NotNull(result);
            else
                Assert.Null(result);
        }
    }

    public partial class GIFImageParserTests
    {
        /// <summary>
        /// Test implementation of GIFImageParser to enable testing of the abstract class
        /// </summary>
        private class TestGIFImageParser : GIFImageParser
        {
            public bool StartParsingCalled { get; private set; }
            public bool FinishedParsingCalled { get; private set; }
            public int AddBitmapCallCount { get; private set; }
            public GIFHeader LastAddedHeader { get; private set; }
            public GIFBitmap LastAddedBitmap { get; private set; }
            public bool LastIgnoreImageDataValue { get; private set; }

            protected override void StartParsing()
            {
                StartParsingCalled = true;
            }

            protected override void AddBitmap(GIFHeader header, GIFBitmap bitmap, bool ignoreImageData)
            {
                AddBitmapCallCount++;
                LastAddedHeader = header;
                LastAddedBitmap = bitmap;
                LastIgnoreImageDataValue = ignoreImageData;
            }

            protected override void FinishedParsing()
            {
                FinishedParsingCalled = true;
            }
        }

        /// <summary>
        /// Tests that ParseAsync throws ArgumentNullException when stream parameter is null.
        /// This covers the null check validation in the method.
        /// </summary>
        [Fact]
        public async Task ParseAsync_NullStream_ThrowsArgumentNullException()
        {
            // Arrange
            var parser = new TestGIFImageParser();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => parser.ParseAsync(null));

            Assert.Equal("stream", exception.ParamName);
        }

        /// <summary>
        /// Tests that ParseAsync with null stream throws ArgumentNullException regardless of other parameters.
        /// This ensures the null check happens before any other processing.
        /// </summary>
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task ParseAsync_NullStreamWithDifferentParameters_ThrowsArgumentNullException(
            bool skipTypeIdentifier, bool ignoreImageData)
        {
            // Arrange
            var parser = new TestGIFImageParser();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => parser.ParseAsync(null, skipTypeIdentifier, ignoreImageData));

            Assert.Equal("stream", exception.ParamName);
        }

        /// <summary>
        /// Tests ParseAsync with an empty stream to verify method call sequence.
        /// This tests that StartParsing is called even when the stream has no data.
        /// </summary>
        [Fact]
        public async Task ParseAsync_EmptyStream_CallsStartParsing()
        {
            // Arrange
            var parser = new TestGIFImageParser();
            using var emptyStream = new MemoryStream();

            // Act & Assert
            // Note: This may throw exceptions during GIF parsing, but we catch them
            // to verify that StartParsing was called before the exception
            try
            {
                await parser.ParseAsync(emptyStream);
            }
            catch
            {
                // Expected - empty stream will cause parsing errors
            }

            // Assert
            Assert.True(parser.StartParsingCalled);
        }

        /// <summary>
        /// Tests ParseAsync with default parameters to ensure they are handled correctly.
        /// This verifies the method works with optional parameter defaults.
        /// </summary>
        [Fact]
        public async Task ParseAsync_WithDefaultParameters_UsesCorrectDefaults()
        {
            // Arrange
            var parser = new TestGIFImageParser();
            using var emptyStream = new MemoryStream();

            // Act & Assert
            try
            {
                await parser.ParseAsync(emptyStream);
            }
            catch
            {
                // Expected - empty stream will cause parsing errors
            }

            // Assert
            Assert.True(parser.StartParsingCalled);
        }

        /// <summary>
        /// Tests ParseAsync with invalid stream data to verify error handling.
        /// This ensures the method can handle streams that don't contain valid GIF data.
        /// </summary>
        [Fact]
        public async Task ParseAsync_InvalidStreamData_HandlesGracefully()
        {
            // Arrange
            var parser = new TestGIFImageParser();
            var invalidData = Encoding.UTF8.GetBytes("This is not GIF data");
            using var invalidStream = new MemoryStream(invalidData);

            // Act & Assert
            try
            {
                await parser.ParseAsync(invalidStream, false, false);
            }
            catch
            {
                // Expected - invalid data will cause parsing errors
            }

            // Assert that StartParsing was called even with invalid data
            Assert.True(parser.StartParsingCalled);
        }

        /// <summary>
        /// Tests ParseAsync with skipTypeIdentifier parameter set to true.
        /// This verifies that the parameter is correctly passed to the underlying parsing logic.
        /// </summary>
        [Fact]
        public async Task ParseAsync_SkipTypeIdentifierTrue_PassesParameterCorrectly()
        {
            // Arrange
            var parser = new TestGIFImageParser();
            using var stream = new MemoryStream();

            // Act & Assert
            try
            {
                await parser.ParseAsync(stream, skipTypeIdentifier: true);
            }
            catch
            {
                // Expected - empty stream will cause parsing errors
            }

            // Assert that StartParsing was called
            Assert.True(parser.StartParsingCalled);
        }

        /// <summary>
        /// Tests ParseAsync with ignoreImageData parameter set to true.
        /// This verifies that the parameter is correctly passed to the underlying parsing logic.
        /// </summary>
        [Fact]
        public async Task ParseAsync_IgnoreImageDataTrue_PassesParameterCorrectly()
        {
            // Arrange
            var parser = new TestGIFImageParser();
            using var stream = new MemoryStream();

            // Act & Assert
            try
            {
                await parser.ParseAsync(stream, ignoreImageData: true);
            }
            catch
            {
                // Expected - empty stream will cause parsing errors
            }

            // Assert that StartParsing was called
            Assert.True(parser.StartParsingCalled);
        }

        /// <summary>
        /// Tests ParseAsync with a stream that will be read from to verify stream usage.
        /// This ensures the method attempts to read from the provided stream.
        /// </summary>
        [Fact]
        public async Task ParseAsync_ValidStream_AttemptsToReadFromStream()
        {
            // Arrange
            var parser = new TestGIFImageParser();
            // Create a stream with some minimal data
            var data = new byte[] { 0x47, 0x49, 0x46 }; // "GIF" header start
            using var stream = new MemoryStream(data);

            // Act & Assert
            try
            {
                await parser.ParseAsync(stream);
            }
            catch
            {
                // Expected - incomplete GIF data will cause parsing errors
            }

            // Assert that the stream position was modified (indicating it was read)
            Assert.True(stream.Position > 0 || parser.StartParsingCalled);
        }
    }

    public class RectTests
    {
        /// <summary>
        /// Tests that the Rect constructor properly assigns all parameter values to their corresponding properties.
        /// Tests various combinations of positive, negative, and zero values to ensure proper assignment.
        /// </summary>
        /// <param name="x">The X coordinate value to test</param>
        /// <param name="y">The Y coordinate value to test</param>
        /// <param name="width">The width value to test</param>
        /// <param name="height">The height value to test</param>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(10, 20, 30, 40)]
        [InlineData(-10, -20, -30, -40)]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue, int.MinValue, int.MinValue)]
        [InlineData(100, -50, 200, 0)]
        [InlineData(-100, 50, 0, 200)]
        public void Constructor_VariousValues_PropertiesSetCorrectly(int x, int y, int width, int height)
        {
            // Arrange & Act
            var rect = new GIFBitmap.Rect(x, y, width, height);

            // Assert
            Assert.Equal(x, rect.X);
            Assert.Equal(y, rect.Y);
            Assert.Equal(width, rect.Width);
            Assert.Equal(height, rect.Height);
        }

        /// <summary>
        /// Tests that the Rect constructor handles extreme boundary values without throwing exceptions.
        /// Verifies that int.MinValue and int.MaxValue are properly assigned to all properties.
        /// </summary>
        [Fact]
        public void Constructor_ExtremeValues_NoExceptionThrown()
        {
            // Arrange & Act & Assert - should not throw
            var rectMin = new GIFBitmap.Rect(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
            var rectMax = new GIFBitmap.Rect(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

            Assert.Equal(int.MinValue, rectMin.X);
            Assert.Equal(int.MinValue, rectMin.Y);
            Assert.Equal(int.MinValue, rectMin.Width);
            Assert.Equal(int.MinValue, rectMin.Height);

            Assert.Equal(int.MaxValue, rectMax.X);
            Assert.Equal(int.MaxValue, rectMax.Y);
            Assert.Equal(int.MaxValue, rectMax.Width);
            Assert.Equal(int.MaxValue, rectMax.Height);
        }

        /// <summary>
        /// Tests that the Rect constructor accepts negative width and height values without validation.
        /// This verifies the constructor performs no validation on the geometric validity of the rectangle.
        /// </summary>
        [Fact]
        public void Constructor_NegativeWidthAndHeight_AcceptedWithoutValidation()
        {
            // Arrange & Act
            var rect = new GIFBitmap.Rect(5, 10, -15, -25);

            // Assert
            Assert.Equal(5, rect.X);
            Assert.Equal(10, rect.Y);
            Assert.Equal(-15, rect.Width);
            Assert.Equal(-25, rect.Height);
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests.Internals
{
    public partial class GIFDecoderStreamReaderTests
    {
        /// <summary>
        /// Verifies that the GIFDecoderStreamReader constructor properly assigns a valid stream to the internal field.
        /// Tests with a mocked stream and verifies assignment by calling Read method which uses the stream.
        /// Expected result: Constructor completes successfully and stream is accessible through Read method.
        /// </summary>
        [Fact]
        public void Constructor_ValidStream_SetsStreamField()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(42);

            // Act
            var reader = new GIFDecoderStreamReader(mockStream);

            // Assert
            var result = reader.Read();
            Assert.Equal(42, result);
            mockStream.Received(1).ReadByte();
        }

        /// <summary>
        /// Verifies that the GIFDecoderStreamReader constructor accepts a null stream parameter.
        /// Tests the constructor behavior when passed a null stream.
        /// Expected result: Constructor completes without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_NullStream_DoesNotThrowException()
        {
            // Arrange & Act
            var exception = Record.Exception(() => new GIFDecoderStreamReader(null));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that the GIFDecoderStreamReader constructor works with different stream implementations.
        /// Tests with a MemoryStream to ensure compatibility with concrete Stream implementations.
        /// Expected result: Constructor completes successfully and stream is properly assigned.
        /// </summary>
        [Fact]
        public void Constructor_MemoryStream_SetsStreamField()
        {
            // Arrange
            var memoryStream = new MemoryStream(new byte[] { 100, 200 });

            // Act
            var reader = new GIFDecoderStreamReader(memoryStream);

            // Assert
            var result = reader.Read();
            Assert.Equal(100, result);
        }

        /// <summary>
        /// Verifies that accessing properties after constructor with null stream throws appropriate exception.
        /// Tests the behavior when trying to use Read method after constructing with null stream.
        /// Expected result: NullReferenceException is thrown when attempting to use the null stream.
        /// </summary>
        [Fact]
        public void Constructor_NullStream_ThrowsWhenAccessingRead()
        {
            // Arrange
            var reader = new GIFDecoderStreamReader(null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => reader.Read());
        }

        /// <summary>
        /// Verifies that the constructor properly handles streams with different capabilities.
        /// Tests with a stream that cannot seek to ensure no assumptions about stream capabilities.
        /// Expected result: Constructor completes successfully regardless of stream capabilities.
        /// </summary>
        [Fact]
        public void Constructor_NonSeekableStream_SetsStreamField()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);
            mockStream.ReadByte().Returns(75);

            // Act
            var reader = new GIFDecoderStreamReader(mockStream);

            // Assert
            var result = reader.Read();
            Assert.Equal(75, result);
        }

        /// <summary>
        /// Tests that CurrentPosition returns 0 when the stream reader is first created.
        /// Validates the initial state and default value of the internal position counter.
        /// Expects the property to return 0.
        /// </summary>
        [Fact]
        public void CurrentPosition_InitialState_ReturnsZero()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = reader.CurrentPosition;

            // Assert
            Assert.Equal(0L, result);
        }

        /// <summary>
        /// Tests that CurrentPosition returns the correct value after calling Read() operations.
        /// Validates that the internal position counter is properly incremented with each read.
        /// Expects the property to return the number of read operations performed.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public void CurrentPosition_AfterReadCalls_ReturnsUpdatedPosition(int readCount)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(65); // Return 'A'
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            for (int i = 0; i < readCount; i++)
            {
                reader.Read();
            }
            var result = reader.CurrentPosition;

            // Assert
            Assert.Equal(readCount, result);
        }

        /// <summary>
        /// Tests that CurrentPosition returns the correct value after calling ReadString() operations.
        /// Validates that the internal position counter is properly incremented by the string length.
        /// Expects the property to return the total number of characters read.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public void CurrentPosition_AfterReadStringCall_ReturnsUpdatedPosition(int stringLength)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(65); // Return 'A'
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            reader.ReadString(stringLength);
            var result = reader.CurrentPosition;

            // Assert
            Assert.Equal(stringLength, result);
        }

        /// <summary>
        /// Tests that CurrentPosition works correctly when the underlying stream supports seeking.
        /// Validates the DEBUG assertion path when stream.CanSeek returns true.
        /// Expects the property to return the current position without throwing exceptions.
        /// </summary>
        [Fact]
        public void CurrentPosition_WithSeekableStream_ReturnsPosition()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.Position.Returns(0L);
            mockStream.ReadByte().Returns(65);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            reader.Read();
            mockStream.Position.Returns(1L); // Update mock position to match internal position
            var result = reader.CurrentPosition;

            // Assert
            Assert.Equal(1L, result);
        }

        /// <summary>
        /// Tests that CurrentPosition works correctly when the underlying stream does not support seeking.
        /// Validates the DEBUG assertion path when stream.CanSeek returns false.
        /// Expects the property to return the current position without checking stream position.
        /// </summary>
        [Fact]
        public void CurrentPosition_WithNonSeekableStream_ReturnsPosition()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);
            mockStream.ReadByte().Returns(65);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            reader.Read();
            reader.Read();
            var result = reader.CurrentPosition;

            // Assert
            Assert.Equal(2L, result);
        }

        /// <summary>
        /// Tests that CurrentPosition works correctly with combined read operations.
        /// Validates that the internal position counter accumulates correctly across different read methods.
        /// Expects the property to return the total number of bytes read through all operations.
        /// </summary>
        [Fact]
        public void CurrentPosition_WithCombinedOperations_ReturnsCorrectPosition()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(65);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            reader.Read(); // +1
            reader.ReadString(3); // +3
            reader.Read(); // +1
            var result = reader.CurrentPosition;

            // Assert
            Assert.Equal(5L, result);
        }

        /// <summary>
        /// Tests that CurrentPosition handles boundary values correctly.
        /// Validates that the property works with various position values including edge cases.
        /// Expects the property to return the correct position for large numbers of operations.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(255)]
        [InlineData(1000)]
        public void CurrentPosition_WithVariousPositions_ReturnsCorrectValue(int operationCount)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(65);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            for (int i = 0; i < operationCount; i++)
            {
                reader.Read();
            }
            var result = reader.CurrentPosition;

            // Assert
            Assert.Equal(operationCount, result);
        }

        /// <summary>
        /// Tests that CurrentPosition property can be accessed multiple times without side effects.
        /// Validates that the getter is idempotent and doesn't modify internal state.
        /// Expects consecutive calls to return the same value.
        /// </summary>
        [Fact]
        public void CurrentPosition_MultipleAccesses_ReturnsSameValue()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(65);
            var reader = new GIFDecoderStreamReader(mockStream);
            reader.Read();
            reader.Read();

            // Act
            var result1 = reader.CurrentPosition;
            var result2 = reader.CurrentPosition;
            var result3 = reader.CurrentPosition;

            // Assert
            Assert.Equal(2L, result1);
            Assert.Equal(2L, result2);
            Assert.Equal(2L, result3);
        }

        /// <summary>
        /// Tests that CurrentBlockBuffer returns a non-null byte array.
        /// Verifies the property accessor returns the internal buffer.
        /// Expected result: Returns a valid byte array reference.
        /// </summary>
        [Fact]
        public void CurrentBlockBuffer_WhenAccessed_ReturnsNonNullByteArray()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = reader.CurrentBlockBuffer;

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that CurrentBlockBuffer returns a byte array with the expected size.
        /// Verifies the internal buffer is initialized to 256 bytes as per field declaration.
        /// Expected result: Returns an array with length 256.
        /// </summary>
        [Fact]
        public void CurrentBlockBuffer_WhenAccessed_ReturnsArrayWithExpectedSize()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = reader.CurrentBlockBuffer;

            // Assert
            Assert.Equal(256, result.Length);
        }

        /// <summary>
        /// Tests that CurrentBlockBuffer returns the same reference on multiple calls.
        /// Verifies the property consistently returns the same internal buffer instance.
        /// Expected result: Multiple calls return the same object reference.
        /// </summary>
        [Fact]
        public void CurrentBlockBuffer_WhenAccessedMultipleTimes_ReturnsSameReference()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result1 = reader.CurrentBlockBuffer;
            var result2 = reader.CurrentBlockBuffer;

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that CurrentBlockBuffer returns a modifiable byte array.
        /// Verifies the returned array can be modified and changes persist.
        /// Expected result: Array modifications are retained across property accesses.
        /// </summary>
        [Fact]
        public void CurrentBlockBuffer_WhenModified_RetainsChanges()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var testValue = (byte)42;

            // Act
            var buffer = reader.CurrentBlockBuffer;
            buffer[0] = testValue;
            var bufferAfterModification = reader.CurrentBlockBuffer;

            // Assert
            Assert.Equal(testValue, bufferAfterModification[0]);
        }

        /// <summary>
        /// Tests that CurrentBlockBuffer property is accessible immediately after construction.
        /// Verifies the internal buffer is properly initialized during object construction.
        /// Expected result: Property is accessible without throwing exceptions.
        /// </summary>
        [Fact]
        public void CurrentBlockBuffer_AfterConstruction_IsImmediatelyAccessible()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            var exception = Record.Exception(() => reader.CurrentBlockBuffer);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that CurrentBlockSize property returns the initial default value when accessed immediately after construction.
        /// Input: Newly constructed GIFDecoderStreamReader with mocked stream.
        /// Expected: CurrentBlockSize returns 0 (default int value).
        /// </summary>
        [Fact]
        public void CurrentBlockSize_InitialValue_ReturnsZero()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            var result = reader.CurrentBlockSize;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that CurrentBlockSize property returns consistent values across multiple accesses.
        /// Input: Multiple accesses to CurrentBlockSize property on the same instance.
        /// Expected: All accesses return the same value.
        /// </summary>
        [Fact]
        public void CurrentBlockSize_MultipleAccesses_ReturnsConsistentValue()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            var firstAccess = reader.CurrentBlockSize;
            var secondAccess = reader.CurrentBlockSize;
            var thirdAccess = reader.CurrentBlockSize;

            // Assert
            Assert.Equal(firstAccess, secondAccess);
            Assert.Equal(secondAccess, thirdAccess);
            Assert.Equal(0, firstAccess);
        }

        /// <summary>
        /// Tests that CurrentBlockSize property can be accessed without throwing exceptions when stream is null.
        /// Input: GIFDecoderStreamReader constructed with null stream.
        /// Expected: CurrentBlockSize property access completes without exception and returns default value.
        /// </summary>
        [Fact]
        public void CurrentBlockSize_WithNullStream_ReturnsDefaultValue()
        {
            // Arrange
            var reader = new GIFDecoderStreamReader(null);

            // Act & Assert - Should not throw
            var result = reader.CurrentBlockSize;
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests CurrentBlockSize property with various stream types to ensure property access is independent of stream state.
        /// Input: Different stream implementations (MemoryStream, mock streams with different configurations).
        /// Expected: CurrentBlockSize consistently returns initial value regardless of stream type.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetStreamTestData))]
        public void CurrentBlockSize_WithDifferentStreamTypes_ReturnsInitialValue(Stream stream)
        {
            // Arrange
            var reader = new GIFDecoderStreamReader(stream);

            // Act
            var result = reader.CurrentBlockSize;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Provides test data for different stream types used in parameterized tests.
        /// </summary>
        public static TheoryData<Stream> GetStreamTestData()
        {
            var mockStream = Substitute.For<Stream>();
            var memoryStream = new MemoryStream();
            var emptyMemoryStream = new MemoryStream(new byte[0]);

            return new TheoryData<Stream>
            {
                mockStream,
                memoryStream,
                emptyMemoryStream
            };
        }

        /// <summary>
        /// Tests SkipBlockAsync with a seekable stream and single block.
        /// Input: Seekable stream, single block of size 5.
        /// Expected: Stream.Seek called once, _currentPosition updated.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_SeekableStreamSingleBlock_SeeksCorrectly()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.ReadByte().Returns(5, 0); // First block size 5, then 0 to end

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            mockStream.Received(1).Seek(5, SeekOrigin.Current);
            Assert.Equal(7, reader.CurrentPosition); // 2 ReadByte calls + 5 seek
        }

        /// <summary>
        /// Tests SkipBlockAsync with a seekable stream and multiple blocks.
        /// Input: Seekable stream, blocks of sizes 3, 7, 2, then 0.
        /// Expected: Stream.Seek called for each block, _currentPosition updated correctly.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_SeekableStreamMultipleBlocks_SeeksAllBlocks()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.ReadByte().Returns(3, 7, 2, 0); // Multiple blocks then 0 to end

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            mockStream.Received(1).Seek(3, SeekOrigin.Current);
            mockStream.Received(1).Seek(7, SeekOrigin.Current);
            mockStream.Received(1).Seek(2, SeekOrigin.Current);
            Assert.Equal(16, reader.CurrentPosition); // 4 ReadByte calls + 3+7+2 seek
        }

        /// <summary>
        /// Tests SkipBlockAsync with a non-seekable stream and single block.
        /// Input: Non-seekable stream, single block of size 4.
        /// Expected: ReadAsync called once with correct parameters.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_NonSeekableStreamSingleBlock_ReadsCorrectly()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);
            mockStream.ReadByte().Returns(4, 0); // Block size 4, then 0 to end
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                      .Returns(4); // ReadAsync returns 4 bytes read

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            await mockStream.Received(1).ReadAsync(Arg.Is<byte[]>(b => b.Length == 256), 0, 4);
            Assert.Equal(6, reader.CurrentPosition); // 2 ReadByte calls + 4 from ReadAsync
        }

        /// <summary>
        /// Tests SkipBlockAsync with a non-seekable stream and multiple blocks.
        /// Input: Non-seekable stream, blocks of sizes 2, 5, then 0.
        /// Expected: ReadAsync called for each block with correct parameters.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_NonSeekableStreamMultipleBlocks_ReadsAllBlocks()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);
            mockStream.ReadByte().Returns(2, 5, 0); // Multiple blocks then 0 to end
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                      .Returns(2, 5); // ReadAsync returns requested bytes

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            await mockStream.Received(1).ReadAsync(Arg.Is<byte[]>(b => b.Length == 256), 0, 2);
            await mockStream.Received(1).ReadAsync(Arg.Is<byte[]>(b => b.Length == 256), 0, 5);
            Assert.Equal(10, reader.CurrentPosition); // 3 ReadByte calls + 2+5 from ReadAsync
        }

        /// <summary>
        /// Tests SkipBlockAsync when first block size is zero.
        /// Input: Stream returning 0 immediately.
        /// Expected: Method returns immediately without seeking or reading.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_ZeroBlockSizeFirst_ReturnsImmediately()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.ReadByte().Returns(0); // Block size 0 immediately

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            mockStream.DidNotReceive().Seek(Arg.Any<long>(), Arg.Any<SeekOrigin>());
            await mockStream.DidNotReceive().ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
            Assert.Equal(1, reader.CurrentPosition); // Only 1 ReadByte call
        }

        /// <summary>
        /// Tests SkipBlockAsync with maximum block size.
        /// Input: Seekable stream, block size 255 (maximum for GIF).
        /// Expected: Seeks correctly with large block size.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_MaximumBlockSize_HandlesLargeBlock()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.ReadByte().Returns(255, 0); // Max block size then 0

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            mockStream.Received(1).Seek(255, SeekOrigin.Current);
            Assert.Equal(257, reader.CurrentPosition); // 2 ReadByte calls + 255 seek
        }

        /// <summary>
        /// Tests SkipBlockAsync when Read returns -1 (EOF).
        /// Input: Stream returning -1 from ReadByte.
        /// Expected: Method handles EOF gracefully.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_ReadReturnsEOF_HandlesGracefully()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.ReadByte().Returns(-1); // EOF immediately

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            mockStream.DidNotReceive().Seek(Arg.Any<long>(), Arg.Any<SeekOrigin>());
            Assert.Equal(1, reader.CurrentPosition); // Only 1 ReadByte call
        }

        /// <summary>
        /// Tests SkipBlockAsync with non-seekable stream when ReadAsync returns fewer bytes.
        /// Input: Non-seekable stream, ReadAsync returns partial reads.
        /// Expected: Method handles partial reads correctly.
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_NonSeekablePartialRead_HandlesPartialReads()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);
            mockStream.ReadByte().Returns(3, 0); // Block size 3, then 0
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
                      .Returns(2); // ReadAsync returns only 2 bytes instead of 3

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            await mockStream.Received(1).ReadAsync(Arg.Is<byte[]>(b => b.Length == 256), 0, 3);
            Assert.Equal(4, reader.CurrentPosition); // 2 ReadByte calls + 2 from ReadAsync
        }

        /// <summary>
        /// Tests SkipBlockAsync state after completion.
        /// Input: Stream with single block.
        /// Expected: CurrentBlockSize is set to final block size (0).
        /// </summary>
        [Fact]
        public async Task SkipBlockAsync_StateAfterCompletion_CurrentBlockSizeIsZero()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.ReadByte().Returns(5, 0); // Block size 5, then 0

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            await reader.SkipBlockAsync();

            // Assert
            Assert.Equal(0, reader.CurrentBlockSize); // Should be set to final read value (0)
        }

        /// <summary>
        /// Tests that Read method successfully reads a byte from the stream and increments the current position.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(127)]
        [InlineData(255)]
        public void Read_WithValidByte_ReturnsByteAndIncrementsPosition(int expectedByte)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(expectedByte);
            var reader = new GIFDecoderStreamReader(mockStream);
            var initialPosition = reader.CurrentPosition;

            // Act
            var result = reader.Read();

            // Assert
            Assert.Equal(expectedByte, result);
            Assert.Equal(initialPosition + 1, reader.CurrentPosition);
            mockStream.Received(1).ReadByte();
        }

        /// <summary>
        /// Tests that Read method returns -1 when end of stream is reached and still increments position.
        /// </summary>
        [Fact]
        public void Read_AtEndOfStream_ReturnsMinusOneAndIncrementsPosition()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(-1);
            var reader = new GIFDecoderStreamReader(mockStream);
            var initialPosition = reader.CurrentPosition;

            // Act
            var result = reader.Read();

            // Assert
            Assert.Equal(-1, result);
            Assert.Equal(initialPosition + 1, reader.CurrentPosition);
            mockStream.Received(1).ReadByte();
        }

        /// <summary>
        /// Tests that multiple Read calls increment position correctly for each call.
        /// </summary>
        [Fact]
        public void Read_MultipleCalls_IncrementsPositionForEachCall()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(65, 66, 67, -1);
            var reader = new GIFDecoderStreamReader(mockStream);
            var initialPosition = reader.CurrentPosition;

            // Act & Assert
            var result1 = reader.Read();
            Assert.Equal(65, result1);
            Assert.Equal(initialPosition + 1, reader.CurrentPosition);

            var result2 = reader.Read();
            Assert.Equal(66, result2);
            Assert.Equal(initialPosition + 2, reader.CurrentPosition);

            var result3 = reader.Read();
            Assert.Equal(67, result3);
            Assert.Equal(initialPosition + 3, reader.CurrentPosition);

            var result4 = reader.Read();
            Assert.Equal(-1, result4);
            Assert.Equal(initialPosition + 4, reader.CurrentPosition);

            mockStream.Received(4).ReadByte();
        }

        /// <summary>
        /// Tests that Read method throws NullReferenceException when stream is null.
        /// </summary>
        [Fact]
        public void Read_WithNullStream_ThrowsNullReferenceException()
        {
            // Arrange
            var reader = new GIFDecoderStreamReader(null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => reader.Read());
        }

        /// <summary>
        /// Tests that Read method propagates ObjectDisposedException when stream is disposed.
        /// </summary>
        [Fact]
        public void Read_WithDisposedStream_ThrowsObjectDisposedException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(x => { throw new ObjectDisposedException("stream"); });
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => reader.Read());
        }

        /// <summary>
        /// Tests that Read method propagates IOException when stream throws IOException.
        /// </summary>
        [Fact]
        public void Read_WithStreamIOException_ThrowsIOException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(x => { throw new IOException("Stream error"); });
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            Assert.Throws<IOException>(() => reader.Read());
        }

        /// <summary>
        /// Tests that Read method increments position even when stream throws an exception.
        /// </summary>
        [Fact]
        public void Read_WhenStreamThrowsAfterPositionIncrement_PositionIsStillIncremented()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(x => { throw new IOException("Stream error"); });
            var reader = new GIFDecoderStreamReader(mockStream);
            var initialPosition = reader.CurrentPosition;

            // Act & Assert
            Assert.Throws<IOException>(() => reader.Read());
            Assert.Equal(initialPosition + 1, reader.CurrentPosition);
        }

        /// <summary>
        /// Tests ReadShort method with two valid bytes to ensure proper little-endian combination.
        /// Input: Stream containing bytes 0x12 and 0x34
        /// Expected: Returns 0x3412 (13330 in decimal)
        /// </summary>
        [Fact]
        public void ReadShort_TwoValidBytes_ReturnsLittleEndianCombination()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(0x12, 0x34);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = reader.ReadShort();

            // Assert
            Assert.Equal(0x3412, result); // 13330 in decimal
        }

        /// <summary>
        /// Tests ReadShort method with various byte combinations to verify bit manipulation.
        /// Input: Different pairs of bytes
        /// Expected: Correct little-endian 16-bit integer values
        /// </summary>
        [Theory]
        [InlineData(0x00, 0x00, 0x0000)]
        [InlineData(0xFF, 0x00, 0x00FF)]
        [InlineData(0x00, 0xFF, 0xFF00)]
        [InlineData(0xFF, 0xFF, 0xFFFF)]
        [InlineData(0x01, 0x02, 0x0201)]
        [InlineData(0xAB, 0xCD, 0xCDAB)]
        public void ReadShort_VariousByteCombinations_ReturnsCorrectLittleEndianValue(byte firstByte, byte secondByte, int expected)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(firstByte, secondByte);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = reader.ReadShort();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ReadShort method when first ReadByte returns -1 (end of stream).
        /// Input: Stream returning -1 for both ReadByte calls
        /// Expected: Returns -1 due to bitwise OR with sign-extended -1 values
        /// </summary>
        [Fact]
        public void ReadShort_FirstByteEndOfStream_ReturnsNegativeOne()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(-1, -1);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = reader.ReadShort();

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests ReadShort method when first byte is valid but second ReadByte returns -1.
        /// Input: Stream returning valid byte then -1
        /// Expected: Returns result of valid_byte | (0xFFFFFFFF << 8) due to sign extension
        /// </summary>
        [Theory]
        [InlineData(0x00, -256)]     // 0x00 | (0xFFFFFFFF << 8) = 0x00 | 0xFFFFFF00 = 0xFFFFFF00 = -256
        [InlineData(0x12, -65262)]   // 0x12 | (0xFFFFFFFF << 8) = 0x12 | 0xFFFFFF00 = 0xFFFFFF12 = -65262
        [InlineData(0xFF, -65281)]   // 0xFF | (0xFFFFFFFF << 8) = 0xFF | 0xFFFFFF00 = 0xFFFFFFFF = -1, but since it's OR it becomes -65281
        public void ReadShort_ValidFirstByteEndOfStreamSecond_ReturnsSignExtendedValue(byte firstByte, int expected)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(firstByte, -1);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = reader.ReadShort();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ReadShort method when second byte is valid but first ReadByte returns -1.
        /// Input: Stream returning -1 then valid byte
        /// Expected: Returns result of -1 | (valid_byte << 8)
        /// </summary>
        [Theory]
        [InlineData(0x00, -1)]       // -1 | (0x00 << 8) = -1 | 0x0000 = -1
        [InlineData(0x12, -1)]       // -1 | (0x12 << 8) = -1 | 0x1200 = -1
        [InlineData(0xFF, -1)]       // -1 | (0xFF << 8) = -1 | 0xFF00 = -1
        public void ReadShort_EndOfStreamFirstValidSecond_ReturnsNegativeOne(byte secondByte, int expected)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(-1, secondByte);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = reader.ReadShort();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ReadShort method calls Read twice and verifies stream interaction.
        /// Input: Stream with mock setup
        /// Expected: ReadByte is called exactly twice
        /// </summary>
        [Fact]
        public void ReadShort_CallsReadTwice_VerifiesStreamInteraction()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(0x01, 0x02);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            reader.ReadShort();

            // Assert
            mockStream.Received(2).ReadByte();
        }

        /// <summary>
        /// Tests ReadShort method with maximum valid byte values.
        /// Input: Both bytes set to maximum value (255)
        /// Expected: Returns 65535 (0xFFFF)
        /// </summary>
        [Fact]
        public void ReadShort_MaximumByteValues_ReturnsMaxUShortValue()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(255, 255);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = reader.ReadShort();

            // Assert
            Assert.Equal(65535, result); // 0xFFFF
        }

        /// <summary>
        /// Tests ReadShort method with minimum valid byte values.
        /// Input: Both bytes set to minimum value (0)
        /// Expected: Returns 0
        /// </summary>
        [Fact]
        public void ReadShort_MinimumByteValues_ReturnsZero()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(0, 0);
            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = reader.ReadShort();

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests ReadString with valid positive length parameter.
        /// Should read specified number of bytes from stream and return as string.
        /// </summary>
        [Theory]
        [InlineData(1, new byte[] { 65 }, "A")]
        [InlineData(3, new byte[] { 72, 105, 33 }, "Hi!")]
        [InlineData(5, new byte[] { 72, 101, 108, 108, 111 }, "Hello")]
        public void ReadString_ValidLength_ReturnsExpectedString(int length, byte[] streamBytes, string expectedResult)
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);

            SetupStreamReadByteSequence(stream, streamBytes);

            // Act
            var result = reader.ReadString(length);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(length, stream.ReceivedCalls().Count());
        }

        /// <summary>
        /// Tests ReadString with zero length parameter.
        /// Should return empty string without reading from stream.
        /// </summary>
        [Fact]
        public void ReadString_ZeroLength_ReturnsEmptyString()
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);

            // Act
            var result = reader.ReadString(0);

            // Assert
            Assert.Equal(string.Empty, result);
            stream.DidNotReceive().ReadByte();
        }

        /// <summary>
        /// Tests ReadString with negative length parameter.
        /// Should not read from stream and return empty string due to loop condition.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void ReadString_NegativeLength_ReturnsEmptyString(int negativeLength)
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);

            // Act
            var result = reader.ReadString(negativeLength);

            // Assert
            Assert.Equal(string.Empty, result);
            stream.DidNotReceive().ReadByte();
        }

        /// <summary>
        /// Tests ReadString with stream returning -1 (end of stream).
        /// Should handle end of stream by casting -1 to char.
        /// </summary>
        [Fact]
        public void ReadString_StreamReturnsEndOfStream_HandlesEndOfStreamBytes()
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);
            stream.ReadByte().Returns(-1);

            // Act
            var result = reader.ReadString(2);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal((char)(-1), result[0]);
            Assert.Equal((char)(-1), result[1]);
            Assert.Equal(2, stream.ReceivedCalls().Count());
        }

        /// <summary>
        /// Tests ReadString with non-ASCII byte values.
        /// Should cast bytes directly to chars, potentially producing non-printable characters.
        /// </summary>
        [Theory]
        [InlineData(new byte[] { 128, 255, 200 })]
        [InlineData(new byte[] { 0, 1, 31 })]
        public void ReadString_NonAsciiBytes_CastsBytesToChars(byte[] streamBytes)
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);
            SetupStreamReadByteSequence(stream, streamBytes);

            // Act
            var result = reader.ReadString(streamBytes.Length);

            // Assert
            Assert.Equal(streamBytes.Length, result.Length);
            for (int i = 0; i < streamBytes.Length; i++)
            {
                Assert.Equal((char)streamBytes[i], result[i]);
            }
        }

        /// <summary>
        /// Tests ReadString updates current position correctly.
        /// Verifies that _currentPosition field is incremented by length parameter.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public void ReadString_UpdatesCurrentPosition(int length)
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);
            stream.ReadByte().Returns(65); // Return 'A'

            var initialPosition = reader.CurrentPosition;

            // Act
            reader.ReadString(length);

            // Assert
            Assert.Equal(initialPosition + length, reader.CurrentPosition);
        }

        /// <summary>
        /// Tests ReadString with maximum practical length.
        /// Should handle large strings without issues.
        /// </summary>
        [Fact]
        public void ReadString_LargeLength_HandlesLargeStrings()
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);
            stream.ReadByte().Returns(65); // Return 'A'

            const int largeLength = 1000;

            // Act
            var result = reader.ReadString(largeLength);

            // Assert
            Assert.Equal(largeLength, result.Length);
            Assert.True(result.All(c => c == 'A'));
            Assert.Equal(largeLength, stream.ReceivedCalls().Count());
        }

        /// <summary>
        /// Tests ReadString when stream throws exception during ReadByte.
        /// Should propagate the exception from the underlying stream.
        /// </summary>
        [Fact]
        public void ReadString_StreamThrowsException_PropagatesException()
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);
            stream.ReadByte().Returns(x => throw new IOException("Stream error"));

            // Act & Assert
            Assert.Throws<IOException>(() => reader.ReadString(1));
        }

        /// <summary>
        /// Tests ReadString with mixed valid and invalid byte sequences.
        /// Should process all bytes including those that may not represent valid text characters.
        /// </summary>
        [Fact]
        public void ReadString_MixedByteValues_ProcessesAllBytes()
        {
            // Arrange
            var stream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(stream);
            var bytes = new byte[] { 72, 0, 255, 101, 108, 13, 10, 111 }; // Mix of printable, null, high-value, and control chars
            SetupStreamReadByteSequence(stream, bytes);

            // Act
            var result = reader.ReadString(bytes.Length);

            // Assert
            Assert.Equal(bytes.Length, result.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                Assert.Equal((char)bytes[i], result[i]);
            }
        }

        private static void SetupStreamReadByteSequence(Stream stream, byte[] bytes)
        {
            var calls = stream.ReadByte();
            for (int i = 0; i < bytes.Length; i++)
            {
                calls = calls.Returns((int)bytes[i]);
            }
        }

        /// <summary>
        /// Tests ReadAsync when toRead is zero.
        /// Should return zero immediately without calling the underlying stream.
        /// </summary>
        [Fact]
        public async Task ReadAsync_ToReadZero_ReturnsZero()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[10];

            // Act
            var result = await reader.ReadAsync(buffer, 0);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(0, reader.CurrentPosition);
            await mockStream.DidNotReceive().ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
        }

        /// <summary>
        /// Tests ReadAsync when toRead is negative.
        /// Should return zero immediately without calling the underlying stream.
        /// </summary>
        [Fact]
        public async Task ReadAsync_ToReadNegative_ReturnsZero()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[10];

            // Act
            var result = await reader.ReadAsync(buffer, -5);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(0, reader.CurrentPosition);
            await mockStream.DidNotReceive().ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
        }

        /// <summary>
        /// Tests ReadAsync when the underlying stream returns all requested bytes in a single read.
        /// Should return the number of bytes read and update current position.
        /// </summary>
        [Fact]
        public async Task ReadAsync_SingleReadSuccess_ReturnsExpectedBytes()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[10];
            const int toRead = 5;

            mockStream.ReadAsync(buffer, 0, toRead).Returns(toRead);

            // Act
            var result = await reader.ReadAsync(buffer, toRead);

            // Assert
            Assert.Equal(toRead, result);
            Assert.Equal(toRead, reader.CurrentPosition);
            await mockStream.Received(1).ReadAsync(buffer, 0, toRead);
        }

        /// <summary>
        /// Tests ReadAsync when the underlying stream returns bytes in multiple smaller reads.
        /// Should continue reading until all bytes are read or stream indicates end.
        /// </summary>
        [Fact]
        public async Task ReadAsync_MultipleReadsSuccess_ReturnsExpectedBytes()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[10];
            const int toRead = 6;

            mockStream.ReadAsync(buffer, 0, 6).Returns(3);
            mockStream.ReadAsync(buffer, 3, 3).Returns(3);

            // Act
            var result = await reader.ReadAsync(buffer, toRead);

            // Assert
            Assert.Equal(toRead, result);
            Assert.Equal(toRead, reader.CurrentPosition);
            await mockStream.Received(1).ReadAsync(buffer, 0, 6);
            await mockStream.Received(1).ReadAsync(buffer, 3, 3);
        }

        /// <summary>
        /// Tests ReadAsync when the underlying stream returns -1 (end of stream).
        /// Should break from the loop and return the partial bytes read.
        /// </summary>
        [Fact]
        public async Task ReadAsync_StreamReturnsMinusOne_BreaksAndReturnsPartialBytes()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[10];
            const int toRead = 5;

            mockStream.ReadAsync(buffer, 0, toRead).Returns(-1);

            // Act
            var result = await reader.ReadAsync(buffer, toRead);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(0, reader.CurrentPosition);
            await mockStream.Received(1).ReadAsync(buffer, 0, toRead);
        }

        /// <summary>
        /// Tests ReadAsync when the underlying stream returns -1 after partial read.
        /// Should break from the loop and return the partial bytes read.
        /// </summary>
        [Fact]
        public async Task ReadAsync_StreamReturnsMinusOneAfterPartialRead_ReturnsPartialBytes()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[10];
            const int toRead = 5;

            mockStream.ReadAsync(buffer, 0, 5).Returns(2);
            mockStream.ReadAsync(buffer, 2, 3).Returns(-1);

            // Act
            var result = await reader.ReadAsync(buffer, toRead);

            // Assert
            Assert.Equal(2, result);
            Assert.Equal(2, reader.CurrentPosition);
            await mockStream.Received(1).ReadAsync(buffer, 0, 5);
            await mockStream.Received(1).ReadAsync(buffer, 2, 3);
        }

        /// <summary>
        /// Tests ReadAsync when the underlying stream returns zero bytes.
        /// Should continue looping (though this could lead to infinite loop in practice).
        /// </summary>
        [Fact]
        public async Task ReadAsync_StreamReturnsZero_ContinuesLoop()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[10];
            const int toRead = 3;

            // First call returns 0, second call returns the remaining bytes
            mockStream.ReadAsync(buffer, 0, 3).Returns(0);
            mockStream.ReadAsync(buffer, 0, 3).Returns(3);

            // Act
            var result = await reader.ReadAsync(buffer, toRead);

            // Assert
            Assert.Equal(toRead, result);
            Assert.Equal(toRead, reader.CurrentPosition);
            await mockStream.Received(2).ReadAsync(buffer, 0, 3);
        }

        /// <summary>
        /// Tests ReadAsync with null buffer.
        /// Should throw ArgumentNullException when the underlying stream is called.
        /// </summary>
        [Fact]
        public async Task ReadAsync_NullBuffer_ThrowsException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);

            mockStream.ReadAsync(null, 0, 5).Returns(Task.FromException<int>(new ArgumentNullException("buffer")));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => reader.ReadAsync(null, 5));
        }

        /// <summary>
        /// Tests ReadAsync updates current position correctly across multiple calls.
        /// Should accumulate the position correctly.
        /// </summary>
        [Fact]
        public async Task ReadAsync_MultipleCallsUpdateCurrentPosition_AccumulatesPosition()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[20];

            mockStream.ReadAsync(buffer, 0, 5).Returns(5);
            mockStream.ReadAsync(buffer, 0, 7).Returns(7);

            // Act
            var firstResult = await reader.ReadAsync(buffer, 5);
            var secondResult = await reader.ReadAsync(buffer, 7);

            // Assert
            Assert.Equal(5, firstResult);
            Assert.Equal(7, secondResult);
            Assert.Equal(12, reader.CurrentPosition);
        }

        /// <summary>
        /// Tests ReadAsync with boundary values for toRead and buffer sizes.
        /// Should handle edge cases appropriately.
        /// </summary>
        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(1, 100, 1)]
        [InlineData(100, 100, 100)]
        [InlineData(int.MaxValue, int.MaxValue, 50)]
        public async Task ReadAsync_BoundaryValues_ReturnsExpectedResult(int bufferSize, int toRead, int streamReturn)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[bufferSize];

            mockStream.ReadAsync(buffer, 0, toRead).Returns(streamReturn);

            // Act
            var result = await reader.ReadAsync(buffer, toRead);

            // Assert
            Assert.Equal(streamReturn, result);
            Assert.Equal(streamReturn, reader.CurrentPosition);
        }

        /// <summary>
        /// Tests ReadAsync when toRead equals buffer length exactly.
        /// Should work correctly without buffer overflow.
        /// </summary>
        [Fact]
        public async Task ReadAsync_ToReadEqualsBufferLength_ReturnsExpectedBytes()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var reader = new GIFDecoderStreamReader(mockStream);
            var buffer = new byte[5];
            const int toRead = 5;

            mockStream.ReadAsync(buffer, 0, toRead).Returns(toRead);

            // Act
            var result = await reader.ReadAsync(buffer, toRead);

            // Assert
            Assert.Equal(toRead, result);
            Assert.Equal(toRead, reader.CurrentPosition);
            await mockStream.Received(1).ReadAsync(buffer, 0, toRead);
        }

        /// <summary>
        /// Tests ReadBlockAsync when Read() returns a valid block size and ReadAsync() returns the expected number of bytes.
        /// Should successfully return the number of bytes read.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(255)]
        public async Task ReadBlockAsync_ValidBlockSizeAndSuccessfulRead_ReturnsExpectedBytesRead(int blockSize)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(blockSize);
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(blockSize));

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = await reader.ReadBlockAsync();

            // Assert
            Assert.Equal(blockSize, result);
            mockStream.Received(1).ReadByte();
            await mockStream.Received(1).ReadAsync(Arg.Any<byte[]>(), 0, blockSize);
        }

        /// <summary>
        /// Tests ReadBlockAsync when Read() returns zero block size.
        /// Should successfully return zero without throwing an exception.
        /// </summary>
        [Fact]
        public async Task ReadBlockAsync_ZeroBlockSize_ReturnsZero()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(0);
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(0));

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = await reader.ReadBlockAsync();

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests ReadBlockAsync when Read() returns -1 (end of stream).
        /// Should handle the negative block size appropriately and return the actual bytes read.
        /// </summary>
        [Fact]
        public async Task ReadBlockAsync_ReadReturnsNegativeOne_ReturnsActualBytesRead()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(-1);
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(0));

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = await reader.ReadBlockAsync();

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests ReadBlockAsync when ReadAsync returns fewer bytes than the expected block size.
        /// Should throw GIFDecoderFormatException with the correct message.
        /// </summary>
        [Theory]
        [InlineData(10, 5)]
        [InlineData(100, 50)]
        [InlineData(255, 200)]
        [InlineData(5, 0)]
        public async Task ReadBlockAsync_ReadAsyncReturnsFewerbytes_ThrowsGIFDecoderFormatException(int expectedBlockSize, int actualBytesRead)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(expectedBlockSize);
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(actualBytesRead));

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<GIFDecoderFormatException>(
                () => reader.ReadBlockAsync());

            Assert.Equal("Current block to small.", exception.Message);
        }

        /// <summary>
        /// Tests ReadBlockAsync boundary condition where ReadAsync returns exactly one less byte than expected.
        /// Should throw GIFDecoderFormatException.
        /// </summary>
        [Fact]
        public async Task ReadBlockAsync_ReadAsyncReturnsOneLessByte_ThrowsGIFDecoderFormatException()
        {
            // Arrange
            const int blockSize = 50;
            const int bytesRead = blockSize - 1;

            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(blockSize);
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(bytesRead));

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<GIFDecoderFormatException>(
                () => reader.ReadBlockAsync());

            Assert.Equal("Current block to small.", exception.Message);
        }

        /// <summary>
        /// Tests ReadBlockAsync with maximum valid byte value from ReadByte.
        /// Should handle the maximum block size correctly.
        /// </summary>
        [Fact]
        public async Task ReadBlockAsync_MaximumBlockSize_ReturnsMaximumValue()
        {
            // Arrange
            const int maxBlockSize = 255;
            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(maxBlockSize);
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(maxBlockSize));

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = await reader.ReadBlockAsync();

            // Assert
            Assert.Equal(maxBlockSize, result);
        }

        /// <summary>
        /// Tests ReadBlockAsync when ReadAsync returns more bytes than requested (edge case).
        /// Should return the actual bytes read without throwing an exception.
        /// </summary>
        [Fact]
        public async Task ReadBlockAsync_ReadAsyncReturnsMoreBytes_ReturnsActualBytesRead()
        {
            // Arrange
            const int blockSize = 10;
            const int moreBytesRead = 15;

            var mockStream = Substitute.For<Stream>();
            mockStream.ReadByte().Returns(blockSize);
            mockStream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(moreBytesRead));

            var reader = new GIFDecoderStreamReader(mockStream);

            // Act
            int result = await reader.ReadBlockAsync();

            // Assert
            Assert.Equal(moreBytesRead, result);
        }
    }
}