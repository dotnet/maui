#nullable disable

using System;
using System.Runtime.Serialization;

using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class InvalidNavigationExceptionTests
    {
        /// <summary>
        /// Tests that the InvalidNavigationException constructor with message and innerException parameters
        /// correctly initializes the exception with the provided message and inner exception.
        /// </summary>
        /// <param name="message">The error message to test</param>
        /// <param name="innerException">The inner exception to test</param>
        [Theory]
        [InlineData("Test message", null)]
        [InlineData("Test message", "ArgumentException")]
        [InlineData("", null)]
        [InlineData("", "InvalidOperationException")]
        [InlineData(null, null)]
        [InlineData(null, "Exception")]
        [InlineData("   ", null)]
        [InlineData("Very long message that contains many characters to test boundary conditions and ensure the constructor handles long strings properly without any issues", "ArgumentNullException")]
        [InlineData("Message with special characters: !@#$%^&*()_+-=[]{}|;:'\",.<>?/~`", null)]
        public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly(string message, string innerExceptionType)
        {
            // Arrange
            Exception innerException = innerExceptionType switch
            {
                "ArgumentException" => new ArgumentException("Inner argument exception"),
                "InvalidOperationException" => new InvalidOperationException("Inner invalid operation exception"),
                "Exception" => new Exception("Inner generic exception"),
                "ArgumentNullException" => new ArgumentNullException("paramName", "Inner argument null exception"),
                _ => null
            };

            // Act
            var exception = new InvalidNavigationException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
            Assert.IsType<InvalidNavigationException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        /// <summary>
        /// Tests that the InvalidNavigationException constructor with message and innerException parameters
        /// properly handles nested inner exceptions (exception with its own inner exception).
        /// </summary>
        [Fact]
        public void Constructor_WithMessageAndNestedInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            var rootException = new Exception("Root exception");
            var nestedInnerException = new ArgumentException("Nested inner exception", rootException);
            var message = "Test message with nested inner exception";

            // Act
            var exception = new InvalidNavigationException(message, nestedInnerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(nestedInnerException, exception.InnerException);
            Assert.Equal(rootException, exception.InnerException.InnerException);
        }

        /// <summary>
        /// Tests that the InvalidNavigationException constructor with message and innerException parameters
        /// handles extremely long messages without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongMessage_HandlesCorrectly()
        {
            // Arrange
            var longMessage = new string('A', 10000);
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new InvalidNavigationException(longMessage, innerException);

            // Assert
            Assert.Equal(longMessage, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the InvalidNavigationException constructor with message and innerException parameters
        /// properly handles whitespace-only messages.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("   \t\n\r   ")]
        public void Constructor_WithWhitespaceMessage_SetsMessageCorrectly(string whitespaceMessage)
        {
            // Arrange
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new InvalidNavigationException(whitespaceMessage, innerException);

            // Assert
            Assert.Equal(whitespaceMessage, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

#if !NETSTANDARD
        /// <summary>
        /// Tests that the serialization constructor creates a valid InvalidNavigationException instance
        /// when provided with valid SerializationInfo and StreamingContext parameters.
        /// Verifies that the protected constructor properly initializes the exception.
        /// </summary>
        [Fact]
        public void SerializationConstructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var info = new SerializationInfo(typeof(InvalidNavigationException), new FormatterConverter());
            var context = new StreamingContext();
            info.AddValue("Message", "Test message");
            info.AddValue("InnerException", (Exception)null);
            info.AddValue("HelpURL", (string)null);
            info.AddValue("StackTraceString", (string)null);
            info.AddValue("RemoteStackTraceString", (string)null);
            info.AddValue("RemoteStackIndex", 0);
            info.AddValue("ExceptionMethod", (string)null);
            info.AddValue("HResult", -2146233088);
            info.AddValue("Source", (string)null);

            // Act
            var exception = new TestableInvalidNavigationException(info, context);

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<TestableInvalidNavigationException>(exception);
        }

        /// <summary>
        /// Tests that the serialization constructor throws ArgumentNullException
        /// when SerializationInfo parameter is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void SerializationConstructor_NullSerializationInfo_ThrowsArgumentNullException()
        {
            // Arrange
            SerializationInfo info = null;
            var context = new StreamingContext();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestableInvalidNavigationException(info, context));
        }

        /// <summary>
        /// Helper class to expose the protected serialization constructor for testing purposes.
        /// Inherits from InvalidNavigationException to access the protected constructor.
        /// </summary>
        private class TestableInvalidNavigationException : InvalidNavigationException
        {
            public TestableInvalidNavigationException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }
#endif

        /// <summary>
        /// Tests that the parameterless constructor creates a valid InvalidNavigationException instance
        /// with default Exception properties and can be properly instantiated.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Act
            var exception = new InvalidNavigationException();

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<InvalidNavigationException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an exception with null inner exception
        /// as expected from the default Exception behavior.
        /// </summary>
        [Fact]
        public void Constructor_Default_InnerExceptionIsNull()
        {
            // Act
            var exception = new InvalidNavigationException();

            // Assert
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that the InvalidNavigationException created with parameterless constructor
        /// can be thrown and caught properly in exception handling scenarios.
        /// </summary>
        [Fact]
        public void Constructor_Default_CanBeThrown()
        {
            // Act & Assert
            var thrownException = Assert.Throws<InvalidNavigationException>(() => throw new InvalidNavigationException());

            Assert.NotNull(thrownException);
            Assert.IsType<InvalidNavigationException>(thrownException);
        }

        /// <summary>
        /// Tests that the InvalidNavigationException created with parameterless constructor
        /// can be caught as its base Exception type.
        /// </summary>
        [Fact]
        public void Constructor_Default_CanBeCaughtAsException()
        {
            // Act & Assert
            var thrownException = Assert.Throws<Exception>(() => throw new InvalidNavigationException());

            Assert.NotNull(thrownException);
            Assert.IsType<InvalidNavigationException>(thrownException);
        }
    }
}