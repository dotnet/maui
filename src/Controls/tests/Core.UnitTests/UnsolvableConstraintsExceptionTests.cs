#nullable disable

using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class UnsolvableConstraintsExceptionTests
    {
        /// <summary>
        /// Tests the UnsolvableConstraintsException constructor with valid message and inner exception.
        /// Verifies that both the message and inner exception are properly set.
        /// </summary>
        [Fact]
        public void Constructor_ValidMessageAndInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test constraint error";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new UnsolvableConstraintsException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests the UnsolvableConstraintsException constructor with null message and valid inner exception.
        /// Verifies that null message is handled correctly and inner exception is set.
        /// </summary>
        [Fact]
        public void Constructor_NullMessageAndValidInnerException_SetsInnerExceptionCorrectly()
        {
            // Arrange
            string message = null;
            var innerException = new ArgumentException("Inner error");

            // Act
            var exception = new UnsolvableConstraintsException(message, innerException);

            // Assert
            Assert.Equal(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests the UnsolvableConstraintsException constructor with valid message and null inner exception.
        /// Verifies that the message is set correctly and null inner exception is handled.
        /// </summary>
        [Fact]
        public void Constructor_ValidMessageAndNullInnerException_SetsMessageCorrectly()
        {
            // Arrange
            var message = "Test constraint error";
            Exception innerException = null;

            // Act
            var exception = new UnsolvableConstraintsException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests the UnsolvableConstraintsException constructor with both null parameters.
        /// Verifies that both null values are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNull_HandlesNullValuesCorrectly()
        {
            // Arrange
            string message = null;
            Exception innerException = null;

            // Act
            var exception = new UnsolvableConstraintsException(message, innerException);

            // Assert
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests the UnsolvableConstraintsException constructor with various string edge cases.
        /// Verifies that empty strings, whitespace, and special characters are handled correctly.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        [InlineData("Special chars: !@#$%^&*()")]
        [InlineData("Very long message that contains many characters to test the behavior with lengthy error messages that might be encountered in real-world scenarios")]
        public void Constructor_VariousMessageFormats_SetsMessageCorrectly(string message)
        {
            // Arrange
            var innerException = new Exception("Inner error");

            // Act
            var exception = new UnsolvableConstraintsException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        /// <summary>
        /// Tests the UnsolvableConstraintsException constructor with different types of inner exceptions.
        /// Verifies that various exception types can be used as inner exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(UnsolvableConstraintsException))]
        public void Constructor_DifferentInnerExceptionTypes_SetsInnerExceptionCorrectly(Type exceptionType)
        {
            // Arrange
            var message = "Test message";
            var innerException = (Exception)Activator.CreateInstance(exceptionType, "Inner error message");

            // Act
            var exception = new UnsolvableConstraintsException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
            Assert.IsType(exceptionType, exception.InnerException);
        }

        /// <summary>
        /// Tests that the UnsolvableConstraintsException constructor creates an instance that is serializable.
        /// Verifies that the SerializableAttribute is properly applied and the exception can be created.
        /// </summary>
        [Fact]
        public void Constructor_CreatesSerializableException_InstanceCreatedSuccessfully()
        {
            // Arrange
            var message = "Serializable test message";
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new UnsolvableConstraintsException(message, innerException);

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<Exception>(exception);
            Assert.True(exception.GetType().IsSerializable);
        }

#if !NETSTANDARD
        /// <summary>
        /// Tests that the serialization constructor properly initializes the exception with valid SerializationInfo and StreamingContext.
        /// This test verifies that the protected constructor correctly calls the base Exception constructor.
        /// Expected result: Exception is created without throwing and base constructor is called.
        /// </summary>
        [Fact]
        public void SerializationConstructor_ValidParameters_InitializesException()
        {
            // Arrange
            var info = new SerializationInfo(typeof(UnsolvableConstraintsException), new FormatterConverter());
            var context = new StreamingContext(StreamingContextStates.All);
            info.AddValue("ClassName", typeof(UnsolvableConstraintsException).FullName);
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

            // Act & Assert - No exception should be thrown
            var exception = new TestableUnsolvableConstraintsException(info, context);

            // Verify the exception was created successfully
            Assert.NotNull(exception);
        }

        /// <summary>
        /// Tests that the serialization constructor throws ArgumentNullException when SerializationInfo is null.
        /// This test verifies proper parameter validation in the constructor.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SerializationConstructor_NullSerializationInfo_ThrowsArgumentNullException()
        {
            // Arrange
            SerializationInfo info = null;
            var context = new StreamingContext(StreamingContextStates.All);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestableUnsolvableConstraintsException(info, context));
        }

        /// <summary>
        /// Helper class to expose the protected serialization constructor for testing.
        /// </summary>
        private class TestableUnsolvableConstraintsException : UnsolvableConstraintsException
        {
            public TestableUnsolvableConstraintsException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }
#endif

        /// <summary>
        /// Tests that the parameterless constructor creates a valid UnsolvableConstraintsException instance
        /// with default properties set correctly.
        /// Input: No parameters.
        /// Expected: Valid exception instance with default message and null inner exception.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CreatesValidExceptionInstance()
        {
            // Act
            var exception = new UnsolvableConstraintsException();

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<UnsolvableConstraintsException>(exception);
            Assert.IsAssignableFrom<Exception>(exception);
            Assert.Null(exception.InnerException);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an exception that can be thrown and caught correctly.
        /// Input: No parameters.
        /// Expected: Exception can be thrown and caught as both UnsolvableConstraintsException and Exception.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_ExceptionCanBeThrownAndCaught()
        {
            // Arrange & Act & Assert
            var thrownException = Assert.Throws<UnsolvableConstraintsException>(() => throw new UnsolvableConstraintsException());

            Assert.NotNull(thrownException);
            Assert.IsType<UnsolvableConstraintsException>(thrownException);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an exception that can be caught as base Exception type.
        /// Input: No parameters.
        /// Expected: Exception can be caught as System.Exception.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_ExceptionCanBeCaughtAsBaseException()
        {
            // Arrange & Act & Assert
            var thrownException = Assert.Throws<Exception>(() => throw new UnsolvableConstraintsException());

            Assert.NotNull(thrownException);
            Assert.IsType<UnsolvableConstraintsException>(thrownException);
        }
    }
}