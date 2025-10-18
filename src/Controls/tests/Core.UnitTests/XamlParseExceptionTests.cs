#nullable disable

using System;
using System.Runtime.Serialization;
using System.Xml;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
    public class XamlParseExceptionTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates a valid XamlParseException instance
        /// without throwing any exceptions and initializes the object correctly.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CreatesValidInstance()
        {
            // Arrange & Act
            var exception = new XamlParseException();

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<XamlParseException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        /// <summary>
        /// Tests that the parameterless constructor sets default property values correctly,
        /// including null XmlInfo, null InnerException, and proper UnformattedMessage behavior.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_SetsDefaultPropertyValues()
        {
            // Arrange & Act
            var exception = new XamlParseException();

            // Assert
            Assert.Null(exception.XmlInfo);
            Assert.Null(exception.InnerException);
            Assert.Equal(exception.Message, exception.UnformattedMessage);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an exception with an empty or null message,
        /// matching the behavior of the base Exception class parameterless constructor.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_HasDefaultMessage()
        {
            // Arrange & Act
            var exception = new XamlParseException();
            var baseException = new Exception();

            // Assert
            Assert.Equal(baseException.Message, exception.Message);
        }

        /// <summary>
        /// Tests that UnformattedMessage property returns the base Message when the internal
        /// _unformattedMessage field is null (as it would be with the parameterless constructor).
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_UnformattedMessageReturnsBaseMessage()
        {
            // Arrange & Act
            var exception = new XamlParseException();

            // Assert
            Assert.Equal(exception.Message, exception.UnformattedMessage);
        }

        /// <summary>
        /// Tests that the exception created with the parameterless constructor can be used
        /// in exception handling scenarios and behaves like a standard Exception.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CanBeThrown()
        {
            // Arrange & Act & Assert
            var thrownException = Assert.Throws<XamlParseException>(() => throw new XamlParseException());

            Assert.NotNull(thrownException);
            Assert.IsType<XamlParseException>(thrownException);
        }

        /// <summary>
        /// Tests that the constructor with message parameter creates a valid exception instance
        /// with the provided message for various string inputs including edge cases.
        /// </summary>
        /// <param name="message">The message parameter to test.</param>
        /// <param name="expectedMessage">The expected message value (null becomes empty string in Exception base class).</param>
        [Theory]
        [InlineData("Test message", "Test message")]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData("Message with special chars: !@#$%^&*()", "Message with special chars: !@#$%^&*()")]
        [InlineData("Message with\nnewline\tand\rtab", "Message with\nnewline\tand\rtab")]
        [InlineData("Very long message that exceeds typical length boundaries to test handling of large string inputs in exception constructor", "Very long message that exceeds typical length boundaries to test handling of large string inputs in exception constructor")]
        public void Constructor_WithMessage_CreatesExceptionWithCorrectMessage(string message, string expectedMessage)
        {
            // Act
            var exception = new XamlParseException(message);

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<XamlParseException>(exception);
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor with null message parameter creates a valid exception instance
        /// where the base Exception class handles null by converting it to empty string.
        /// </summary>
        [Fact]
        public void Constructor_WithNullMessage_CreatesExceptionWithEmptyMessage()
        {
            // Arrange
            string message = null;

            // Act
            var exception = new XamlParseException(message);

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<XamlParseException>(exception);
            Assert.Equal(string.Empty, exception.Message);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that the constructor properly inherits from Exception class
        /// and can be caught as both XamlParseException and Exception types.
        /// </summary>
        [Fact]
        public void Constructor_WithMessage_InheritsFromException()
        {
            // Arrange
            const string testMessage = "Test inheritance";

            // Act
            var exception = new XamlParseException(testMessage);

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
            Assert.IsType<XamlParseException>(exception);
        }

#if !NETSTANDARD
        /// <summary>
        /// Helper class to expose the protected serialization constructor for testing
        /// </summary>
        private class TestableXamlParseException : XamlParseException
        {
            public TestableXamlParseException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        /// <summary>
        /// Tests that the serialization constructor properly handles valid SerializationInfo and StreamingContext parameters.
        /// Verifies that the constructor can be instantiated without throwing exceptions when provided with valid serialization data.
        /// Expected result: Constructor completes successfully and creates a valid XamlParseException instance.
        /// </summary>
        [Fact]
        public void SerializationConstructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var info = new SerializationInfo(typeof(XamlParseException), new FormatterConverter());
            info.AddValue("ClassName", typeof(XamlParseException).FullName);
            info.AddValue("Message", "Test message");
            info.AddValue("Data", null);
            info.AddValue("InnerException", null);
            info.AddValue("HelpURL", null);
            info.AddValue("StackTraceString", null);
            info.AddValue("RemoteStackTraceString", null);
            info.AddValue("RemoteStackIndex", 0);
            info.AddValue("ExceptionMethod", null);
            info.AddValue("HResult", -2146233088);
            info.AddValue("Source", null);
            var context = new StreamingContext(StreamingContextStates.All);

            // Act & Assert
            var exception = new TestableXamlParseException(info, context);
            Assert.NotNull(exception);
        }

        /// <summary>
        /// Tests that the serialization constructor throws ArgumentNullException when SerializationInfo is null.
        /// Verifies proper validation of the required SerializationInfo parameter.
        /// Expected result: ArgumentNullException is thrown with appropriate parameter name.
        /// </summary>
        [Fact]
        public void SerializationConstructor_NullSerializationInfo_ThrowsArgumentNullException()
        {
            // Arrange
            SerializationInfo info = null;
            var context = new StreamingContext(StreamingContextStates.All);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new TestableXamlParseException(info, context));
            Assert.Equal("info", exception.ParamName);
        }

        /// <summary>
        /// Tests that the serialization constructor handles empty SerializationInfo correctly.
        /// Verifies behavior when SerializationInfo contains minimal required data.
        /// Expected result: Constructor may throw or complete depending on base Exception requirements.
        /// </summary>
        [Fact]
        public void SerializationConstructor_EmptySerializationInfo_HandledByBaseClass()
        {
            // Arrange
            var info = new SerializationInfo(typeof(XamlParseException), new FormatterConverter());
            var context = new StreamingContext(StreamingContextStates.All);

            // Act & Assert - The base Exception class will handle validation of required serialization data
            // This may throw SerializationException if required data is missing
            try
            {
                var exception = new TestableXamlParseException(info, context);
                Assert.NotNull(exception);
            }
            catch (SerializationException)
            {
                // Expected behavior if base class requires specific serialization data
            }
        }

        /// <summary>
        /// Tests that the serialization constructor preserves message data from SerializationInfo.
        /// Verifies that the message passed through serialization info is properly handled by the base constructor.
        /// Expected result: Exception message should be preserved from serialization data.
        /// </summary>
        [Fact]
        public void SerializationConstructor_WithMessageInSerializationInfo_PreservesMessage()
        {
            // Arrange
            const string testMessage = "Test XAML parse error message";
            var info = new SerializationInfo(typeof(XamlParseException), new FormatterConverter());
            info.AddValue("ClassName", typeof(XamlParseException).FullName);
            info.AddValue("Message", testMessage);
            info.AddValue("Data", null);
            info.AddValue("InnerException", null);
            info.AddValue("HelpURL", null);
            info.AddValue("StackTraceString", null);
            info.AddValue("RemoteStackTraceString", null);
            info.AddValue("RemoteStackIndex", 0);
            info.AddValue("ExceptionMethod", null);
            info.AddValue("HResult", -2146233088);
            info.AddValue("Source", null);
            var context = new StreamingContext(StreamingContextStates.All);

            // Act
            var exception = new TestableXamlParseException(info, context);

            // Assert
            Assert.Equal(testMessage, exception.Message);
        }
#endif

        /// <summary>
        /// Tests that the internal constructor properly delegates to the main constructor with GetLineInfo result when serviceProvider provides IXmlLineInfoProvider.
        /// Input: Valid message, serviceProvider with IXmlLineInfoProvider, null innerException.
        /// Expected: Constructor succeeds and creates exception with formatted message including line info.
        /// </summary>
        [Fact]
        public void Constructor_ValidServiceProviderWithLineInfo_CreatesExceptionWithFormattedMessage()
        {
            // Arrange
            var message = "Test error message";
            var serviceProvider = Substitute.For<IServiceProvider>();
            var lineInfoProvider = Substitute.For<IXmlLineInfoProvider>();
            var xmlLineInfo = Substitute.For<IXmlLineInfo>();

            xmlLineInfo.HasLineInfo().Returns(true);
            xmlLineInfo.LineNumber.Returns(10);
            xmlLineInfo.LinePosition.Returns(5);
            lineInfoProvider.XmlLineInfo.Returns(xmlLineInfo);
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(lineInfoProvider);

            // Act
            var exception = new XamlParseException(message, serviceProvider);

            // Assert
            Assert.Equal("Position 10:5. Test error message", exception.Message);
            Assert.Equal(xmlLineInfo, exception.XmlInfo);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that the internal constructor properly delegates when serviceProvider does not provide IXmlLineInfoProvider.
        /// Input: Valid message, serviceProvider returning null for IXmlLineInfoProvider, null innerException.
        /// Expected: Constructor succeeds and creates exception with original message (no formatting).
        /// </summary>
        [Fact]
        public void Constructor_ServiceProviderWithoutLineInfo_CreatesExceptionWithOriginalMessage()
        {
            // Arrange
            var message = "Test error message";
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(null);

            // Act
            var exception = new XamlParseException(message, serviceProvider);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.NotNull(exception.XmlInfo); // Should be new XmlLineInfo()
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that the internal constructor throws NullReferenceException when serviceProvider is null.
        /// Input: Valid message, null serviceProvider, null innerException.
        /// Expected: NullReferenceException is thrown when GetLineInfo tries to call GetService on null.
        /// </summary>
        [Fact]
        public void Constructor_NullServiceProvider_ThrowsNullReferenceException()
        {
            // Arrange
            var message = "Test error message";
            IServiceProvider serviceProvider = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new XamlParseException(message, serviceProvider));
        }

        /// <summary>
        /// Tests that the internal constructor properly handles innerException parameter.
        /// Input: Valid message, valid serviceProvider, valid innerException.
        /// Expected: Constructor succeeds and creates exception with specified innerException.
        /// </summary>
        [Fact]
        public void Constructor_WithInnerException_CreatesExceptionWithInnerException()
        {
            // Arrange
            var message = "Test error message";
            var serviceProvider = Substitute.For<IServiceProvider>();
            var innerException = new InvalidOperationException("Inner error");
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(null);

            // Act
            var exception = new XamlParseException(message, serviceProvider, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the internal constructor handles various message edge cases.
        /// Input: Edge case messages (null, empty, whitespace), valid serviceProvider, null innerException.
        /// Expected: Constructor succeeds with all message types.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Normal message")]
        [InlineData("Message with special chars: !@#$%^&*()")]
        public void Constructor_VariousMessages_CreatesExceptionWithExpectedMessage(string message)
        {
            // Arrange
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(null);

            // Act
            var exception = new XamlParseException(message, serviceProvider);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Tests that the internal constructor handles very long messages correctly.
        /// Input: Very long message, valid serviceProvider, null innerException.
        /// Expected: Constructor succeeds and preserves the long message.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongMessage_CreatesExceptionWithLongMessage()
        {
            // Arrange
            var longMessage = new string('A', 10000);
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(null);

            // Act
            var exception = new XamlParseException(longMessage, serviceProvider);

            // Assert
            Assert.Equal(longMessage, exception.Message);
        }

        /// <summary>
        /// Tests that the internal constructor properly handles line info without position information.
        /// Input: Valid message, serviceProvider with IXmlLineInfoProvider that has no line info, null innerException.
        /// Expected: Constructor succeeds and creates exception with original message (no formatting).
        /// </summary>
        [Fact]
        public void Constructor_LineInfoWithoutPosition_CreatesExceptionWithOriginalMessage()
        {
            // Arrange
            var message = "Test error message";
            var serviceProvider = Substitute.For<IServiceProvider>();
            var lineInfoProvider = Substitute.For<IXmlLineInfoProvider>();
            var xmlLineInfo = Substitute.For<IXmlLineInfo>();

            xmlLineInfo.HasLineInfo().Returns(false);
            lineInfoProvider.XmlLineInfo.Returns(xmlLineInfo);
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(lineInfoProvider);

            // Act
            var exception = new XamlParseException(message, serviceProvider);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(xmlLineInfo, exception.XmlInfo);
        }

        /// <summary>
        /// Tests that the internal constructor properly handles boundary values for line and position numbers.
        /// Input: Valid message, serviceProvider with IXmlLineInfoProvider having boundary line/position values, null innerException.
        /// Expected: Constructor succeeds and creates exception with formatted message using boundary values.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(-1, -1)]
        public void Constructor_BoundaryLinePositionValues_CreatesExceptionWithFormattedMessage(int lineNumber, int linePosition)
        {
            // Arrange
            var message = "Test error message";
            var serviceProvider = Substitute.For<IServiceProvider>();
            var lineInfoProvider = Substitute.For<IXmlLineInfoProvider>();
            var xmlLineInfo = Substitute.For<IXmlLineInfo>();

            xmlLineInfo.HasLineInfo().Returns(true);
            xmlLineInfo.LineNumber.Returns(lineNumber);
            xmlLineInfo.LinePosition.Returns(linePosition);
            lineInfoProvider.XmlLineInfo.Returns(xmlLineInfo);
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(lineInfoProvider);

            // Act
            var exception = new XamlParseException(message, serviceProvider);

            // Assert
            var expectedMessage = $"Position {lineNumber}:{linePosition}. {message}";
            Assert.Equal(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Tests XamlParseException constructor with valid message, xmlInfo with line info, and no inner exception.
        /// Verifies that the message is formatted with position information and properties are set correctly.
        /// </summary>
        [Fact]
        public void Constructor_ValidMessageAndXmlInfoWithLineInfo_FormatsMessageAndSetsProperties()
        {
            // Arrange
            var message = "Test error message";
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(10);
            xmlInfo.LinePosition.Returns(5);

            // Act
            var exception = new XamlParseException(message, xmlInfo);

            // Assert
            Assert.Equal("Position 10:5. Test error message", exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests XamlParseException constructor with valid message, xmlInfo with line info, and inner exception.
        /// Verifies that all parameters are handled correctly including the inner exception.
        /// </summary>
        [Fact]
        public void Constructor_ValidMessageXmlInfoAndInnerException_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var message = "Test error";
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(25);
            xmlInfo.LinePosition.Returns(15);
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new XamlParseException(message, xmlInfo, innerException);

            // Assert
            Assert.Equal("Position 25:15. Test error", exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
            Assert.Same(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests XamlParseException constructor with null xmlInfo parameter.
        /// Verifies that the original message is preserved and XmlInfo is set to null.
        /// </summary>
        [Fact]
        public void Constructor_NullXmlInfo_PreservesOriginalMessageAndSetsXmlInfoToNull()
        {
            // Arrange
            var message = "Error without position info";

            // Act
            var exception = new XamlParseException(message, null);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Null(exception.XmlInfo);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests XamlParseException constructor with xmlInfo that has no line info.
        /// Verifies that the original message is preserved when HasLineInfo returns false.
        /// </summary>
        [Fact]
        public void Constructor_XmlInfoWithNoLineInfo_PreservesOriginalMessage()
        {
            // Arrange
            var message = "Error without position data";
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(false);

            // Act
            var exception = new XamlParseException(message, xmlInfo);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests XamlParseException constructor with null message parameter.
        /// Verifies that null message is handled correctly in both formatted and unformatted cases.
        /// </summary>
        [Theory]
        [InlineData(true, 10, 5)]
        [InlineData(false, 0, 0)]
        public void Constructor_NullMessage_HandlesNullMessageCorrectly(bool hasLineInfo, int lineNumber, int linePosition)
        {
            // Arrange
            string message = null;
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(hasLineInfo);
            xmlInfo.LineNumber.Returns(lineNumber);
            xmlInfo.LinePosition.Returns(linePosition);

            // Act
            var exception = new XamlParseException(message, xmlInfo);

            // Assert
            if (hasLineInfo)
            {
                Assert.Equal($"Position {lineNumber}:{linePosition}. ", exception.Message);
            }
            else
            {
                Assert.Null(exception.Message);
            }
            Assert.Null(exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
        }

        /// <summary>
        /// Tests XamlParseException constructor with empty and whitespace message parameters.
        /// Verifies that empty strings and whitespace are handled correctly in message formatting.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("   \t\n   ")]
        public void Constructor_EmptyOrWhitespaceMessage_HandlesCorrectly(string message)
        {
            // Arrange
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(1);
            xmlInfo.LinePosition.Returns(1);

            // Act
            var exception = new XamlParseException(message, xmlInfo);

            // Assert
            Assert.Equal($"Position 1:1. {message}", exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
        }

        /// <summary>
        /// Tests XamlParseException constructor with special characters in message.
        /// Verifies that special characters are preserved in both formatted and unformatted messages.
        /// </summary>
        [Theory]
        [InlineData("Message with \"quotes\"")]
        [InlineData("Message with 'single quotes'")]
        [InlineData("Message with \n newline")]
        [InlineData("Message with \t tab")]
        [InlineData("Message with unicode: ñáéíóú")]
        [InlineData("Message with symbols: !@#$%^&*()")]
        public void Constructor_MessageWithSpecialCharacters_PreservesCharacters(string message)
        {
            // Arrange
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(5);
            xmlInfo.LinePosition.Returns(10);

            // Act
            var exception = new XamlParseException(message, xmlInfo);

            // Assert
            Assert.Equal($"Position 5:10. {message}", exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
        }

        /// <summary>
        /// Tests XamlParseException constructor with very long message parameter.
        /// Verifies that long messages are handled correctly without truncation.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongMessage_HandlesCorrectly()
        {
            // Arrange
            var message = new string('x', 10000);
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(100);
            xmlInfo.LinePosition.Returns(200);

            // Act
            var exception = new XamlParseException(message, xmlInfo);

            // Assert
            Assert.Equal($"Position 100:200. {message}", exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
        }

        /// <summary>
        /// Tests XamlParseException constructor with boundary values for line numbers and positions.
        /// Verifies that extreme line number and position values are formatted correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(-1, -1)]
        public void Constructor_BoundaryLineNumbersAndPositions_FormatsCorrectly(int lineNumber, int linePosition)
        {
            // Arrange
            var message = "Test message";
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(lineNumber);
            xmlInfo.LinePosition.Returns(linePosition);

            // Act
            var exception = new XamlParseException(message, xmlInfo);

            // Assert
            Assert.Equal($"Position {lineNumber}:{linePosition}. {message}", exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
        }

        /// <summary>
        /// Tests XamlParseException constructor with different types of inner exceptions.
        /// Verifies that various exception types are properly stored as inner exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(FormatException))]
        public void Constructor_DifferentInnerExceptionTypes_StoresCorrectly(Type exceptionType)
        {
            // Arrange
            var message = "Outer exception";
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(false);
            var innerException = (Exception)Activator.CreateInstance(exceptionType, "Inner exception message");

            // Act
            var exception = new XamlParseException(message, xmlInfo, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
            Assert.Same(innerException, exception.InnerException);
            Assert.IsType(exceptionType, exception.InnerException);
        }

        /// <summary>
        /// Tests XamlParseException constructor with null inner exception explicitly passed.
        /// Verifies that explicitly passing null for inner exception works the same as omitting it.
        /// </summary>
        [Fact]
        public void Constructor_ExplicitNullInnerException_HandlesCorrectly()
        {
            // Arrange
            var message = "Test message";
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(42);
            xmlInfo.LinePosition.Returns(24);

            // Act
            var exception = new XamlParseException(message, xmlInfo, null);

            // Assert
            Assert.Equal("Position 42:24. Test message", exception.Message);
            Assert.Equal(message, exception.UnformattedMessage);
            Assert.Same(xmlInfo, exception.XmlInfo);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that UnformattedMessage returns Message when _unformattedMessage is null (default constructor).
        /// Input: Default constructor with no parameters.
        /// Expected: UnformattedMessage returns the inherited Message property value (empty string).
        /// </summary>
        [Fact]
        public void UnformattedMessage_WhenUnformattedMessageIsNull_DefaultConstructor_ReturnsMessage()
        {
            // Arrange
            var exception = new XamlParseException();

            // Act
            var result = exception.UnformattedMessage;

            // Assert
            Assert.Equal(exception.Message, result);
        }

        /// <summary>
        /// Tests that UnformattedMessage returns Message when _unformattedMessage is null (message constructor).
        /// Input: Constructor with message parameter.
        /// Expected: UnformattedMessage returns the provided message.
        /// </summary>
        [Theory]
        [InlineData("Test message")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void UnformattedMessage_WhenUnformattedMessageIsNull_MessageConstructor_ReturnsMessage(string message)
        {
            // Arrange
            var exception = new XamlParseException(message);

            // Act
            var result = exception.UnformattedMessage;

            // Assert
            Assert.Equal(exception.Message, result);
        }

        /// <summary>
        /// Tests that UnformattedMessage returns Message when _unformattedMessage is null (message + inner exception constructor).
        /// Input: Constructor with message and inner exception parameters.
        /// Expected: UnformattedMessage returns the provided message.
        /// </summary>
        [Theory]
        [InlineData("Test message")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void UnformattedMessage_WhenUnformattedMessageIsNull_MessageAndInnerExceptionConstructor_ReturnsMessage(string message)
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner exception");
            var exception = new XamlParseException(message, innerException);

            // Act
            var result = exception.UnformattedMessage;

            // Assert
            Assert.Equal(exception.Message, result);
        }

#if !NETSTANDARD
        /// <summary>
        /// Tests that UnformattedMessage returns Message when _unformattedMessage is null (serialization constructor).
        /// Input: Serialization constructor.
        /// Expected: UnformattedMessage returns the Message property value.
        /// </summary>
        [Fact]
        public void UnformattedMessage_WhenUnformattedMessageIsNull_SerializationConstructor_ReturnsMessage()
        {
            // Arrange
            var info = Substitute.For<SerializationInfo>(typeof(XamlParseException), Substitute.For<IFormatterConverter>());
            var context = new StreamingContext();
            info.AddValue("ClassName", typeof(XamlParseException).FullName);
            info.AddValue("Message", "Serialized message");
            info.AddValue("Data", null);
            info.AddValue("InnerException", null);
            info.AddValue("HelpURL", null);
            info.AddValue("StackTraceString", null);
            info.AddValue("RemoteStackTraceString", null);
            info.AddValue("RemoteStackIndex", 0);
            info.AddValue("ExceptionMethod", null);
            info.AddValue("HResult", -2146233088);
            info.AddValue("Source", null);

            var exception = new TestableXamlParseException(info, context);

            // Act
            var result = exception.UnformattedMessage;

            // Assert
            Assert.Equal(exception.Message, result);
        }
#endif

        /// <summary>
        /// Tests that UnformattedMessage returns _unformattedMessage when it has a value.
        /// Input: Constructor with IXmlLineInfo parameter setting _unformattedMessage.
        /// Expected: UnformattedMessage returns the original unformatted message.
        /// </summary>
        [Theory]
        [InlineData("Original message")]
        [InlineData("")]
        [InlineData("   ")]
        public void UnformattedMessage_WhenUnformattedMessageHasValue_ReturnsUnformattedMessage(string originalMessage)
        {
            // Arrange
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(true);
            xmlInfo.LineNumber.Returns(10);
            xmlInfo.LinePosition.Returns(5);

            var exception = new XamlParseException(originalMessage, xmlInfo);

            // Act
            var result = exception.UnformattedMessage;

            // Assert
            Assert.Equal(originalMessage, result);
            // Verify that Message property contains the formatted version
            Assert.Contains("Position 10:5", exception.Message);
        }

        /// <summary>
        /// Tests that UnformattedMessage returns _unformattedMessage when it's null and constructor with IXmlLineInfo is used.
        /// Input: Constructor with null message and IXmlLineInfo parameter.
        /// Expected: UnformattedMessage returns null (the original unformatted message).
        /// </summary>
        [Fact]
        public void UnformattedMessage_WhenUnformattedMessageIsNullFromXmlInfoConstructor_ReturnsNull()
        {
            // Arrange
            var xmlInfo = Substitute.For<IXmlLineInfo>();
            xmlInfo.HasLineInfo().Returns(false);

            var exception = new XamlParseException(null, xmlInfo);

            // Act
            var result = exception.UnformattedMessage;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that UnformattedMessage correctly handles the null-coalescing behavior with different scenarios.
        /// Input: Various combinations of _unformattedMessage and Message values.
        /// Expected: UnformattedMessage returns _unformattedMessage when not null, otherwise Message.
        /// </summary>
        [Fact]
        public void UnformattedMessage_NullCoalescingBehavior_ReturnsCorrectValue()
        {
            // Arrange & Act & Assert

            // Case 1: _unformattedMessage has value, should return it regardless of Message
            var xmlInfo1 = Substitute.For<IXmlLineInfo>();
            xmlInfo1.HasLineInfo().Returns(true);
            xmlInfo1.LineNumber.Returns(1);
            xmlInfo1.LinePosition.Returns(1);
            var exception1 = new XamlParseException("unformatted", xmlInfo1);

            Assert.Equal("unformatted", exception1.UnformattedMessage);

            // Case 2: _unformattedMessage is null, should return Message
            var exception2 = new XamlParseException("formatted message");

            Assert.Equal("formatted message", exception2.UnformattedMessage);
            Assert.Equal(exception2.Message, exception2.UnformattedMessage);
        }

#if !NETSTANDARD
#endif

        /// <summary>
        /// Tests the XamlParseException constructor with message and innerException parameters.
        /// Verifies that the constructor properly initializes the exception with the provided parameters
        /// and that the Message and InnerException properties are set correctly.
        /// </summary>
        /// <param name="message">The message parameter to test</param>
        /// <param name="hasInnerException">Whether to include an inner exception</param>
        [Theory]
        [InlineData(null, false)]
        [InlineData(null, true)]
        [InlineData("", false)]
        [InlineData("", true)]
        [InlineData("   ", false)]
        [InlineData("   ", true)]
        [InlineData("Test error message", false)]
        [InlineData("Test error message", true)]
        [InlineData("Very long message with special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?", false)]
        [InlineData("Very long message with special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?", true)]
        public void Constructor_WithMessageAndInnerException_InitializesPropertiesCorrectly(string message, bool hasInnerException)
        {
            // Arrange
            Exception innerException = hasInnerException ? new InvalidOperationException("Inner exception") : null;

            // Act
            var exception = new XamlParseException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests that the XamlParseException constructor with message and innerException can create
        /// an exception that can be properly thrown and caught.
        /// Verifies the exception behavior in a throw/catch scenario.
        /// </summary>
        [Fact]
        public void Constructor_WithMessageAndInnerException_CanBeThrown()
        {
            // Arrange
            string expectedMessage = "Test XAML parse error";
            var expectedInnerException = new ArgumentException("Invalid argument");

            // Act & Assert
            var thrownException = Assert.Throws<XamlParseException>(() =>
            {
                throw new XamlParseException(expectedMessage, expectedInnerException);
            });

            Assert.Equal(expectedMessage, thrownException.Message);
            Assert.Equal(expectedInnerException, thrownException.InnerException);
        }

        /// <summary>
        /// Tests that the XamlParseException constructor with message and innerException
        /// properly handles nested exception scenarios where the inner exception also has an inner exception.
        /// Verifies that the exception chain is preserved correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithMessageAndNestedInnerException_PreservesExceptionChain()
        {
            // Arrange
            var rootException = new InvalidOperationException("Root cause");
            var middleException = new ArgumentException("Middle exception", rootException);
            string message = "XAML parse error with nested exceptions";

            // Act
            var exception = new XamlParseException(message, middleException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(middleException, exception.InnerException);
            Assert.Equal(rootException, exception.InnerException.InnerException);
        }

        /// <summary>
        /// Tests that the XamlParseException constructor with message and innerException
        /// handles various exception types as inner exceptions correctly.
        /// Verifies that different exception types can be used as inner exceptions.
        /// </summary>
        /// <param name="innerExceptionType">The type of inner exception to test</param>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(FormatException))]
        [InlineData(typeof(NullReferenceException))]
        public void Constructor_WithMessageAndDifferentInnerExceptionTypes_HandlesAllTypes(Type innerExceptionType)
        {
            // Arrange
            string message = "XAML parse error";
            var innerException = (Exception)Activator.CreateInstance(innerExceptionType, "Inner exception message");

            // Act
            var exception = new XamlParseException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
            Assert.IsType(innerExceptionType, exception.InnerException);
        }
    }
}