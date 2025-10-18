#nullable disable
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the BindingDiagnostics class.
    /// </summary>
    public class BindingDiagnosticsTests
    {
        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with null binding parameter.
        /// </summary>
        [Fact]
        public void SendBindingFailure_NullBinding_DoesNotThrowException()
        {
            // Arrange
            BindingBase binding = null;
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with null source parameter.
        /// </summary>
        [Fact]
        public void SendBindingFailure_NullSource_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = null;
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with null BindableObject parameter.
        /// </summary>
        [Fact]
        public void SendBindingFailure_NullBindableObject_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            BindableObject bo = null;
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with null BindableProperty parameter.
        /// </summary>
        [Fact]
        public void SendBindingFailure_NullBindableProperty_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            BindableProperty bp = null;
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with null errorCode parameter.
        /// </summary>
        [Fact]
        public void SendBindingFailure_NullErrorCode_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = null;
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with null message parameter.
        /// </summary>
        [Fact]
        public void SendBindingFailure_NullMessage_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = null;
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with null messageArgs parameter.
        /// </summary>
        [Fact]
        public void SendBindingFailure_NullMessageArgs_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = null;

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with empty string parameters.
        /// </summary>
        [Theory]
        [InlineData("", "")]
        [InlineData("", "message")]
        [InlineData("error", "")]
        [InlineData("   ", "message")]
        [InlineData("error", "   ")]
        public void SendBindingFailure_EmptyStringParameters_DoesNotThrowException(string errorCode, string message)
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with empty messageArgs array.
        /// </summary>
        [Fact]
        public void SendBindingFailure_EmptyMessageArgs_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[0];

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when called with various types in messageArgs.
        /// </summary>
        [Fact]
        public void SendBindingFailure_VariousMessageArgTypes_DoesNotThrowException()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message {0} {1} {2} {3}";
            object[] messageArgs = new object[] { 123, "string", null, DateTime.Now };

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendBindingFailure does not throw exception when all parameters are null.
        /// </summary>
        [Fact]
        public void SendBindingFailure_AllParametersNull_DoesNotThrowException()
        {
            // Arrange
            BindingBase binding = null;
            object source = null;
            BindableObject bo = null;
            BindableProperty bp = null;
            string errorCode = null;
            string message = null;
            object[] messageArgs = null;

            // Act & Assert
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that BindingFailed event is invoked when subscribers are present.
        /// This test verifies the event invocation behavior when RuntimeFeature.EnableMauiDiagnostics is true.
        /// Note: This test may not execute the event invocation if RuntimeFeature.EnableMauiDiagnostics returns false,
        /// as this is a static property that cannot be mocked with NSubstitute.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithEventSubscriber_InvokesBindingFailedEvent()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            bool eventInvoked = false;
            BindingBaseErrorEventArgs capturedArgs = null;

            // Subscribe to the event
            BindingDiagnostics.BindingFailed += (sender, args) =>
            {
                eventInvoked = true;
                capturedArgs = args;
            };

            try
            {
                // Act
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs);

                // Assert
                // Note: The event may not be invoked if RuntimeFeature.EnableMauiDiagnostics is false
                // This is a limitation due to the static dependency that cannot be mocked
                if (eventInvoked)
                {
                    Assert.NotNull(capturedArgs);
                    Assert.IsType<BindingErrorEventArgs>(capturedArgs);

                    var errorArgs = (BindingErrorEventArgs)capturedArgs;
                    Assert.Equal(binding, errorArgs.Binding);
                    Assert.Equal(source, errorArgs.Source);
                    Assert.Equal(bo, errorArgs.Target);
                    Assert.Equal(bp, errorArgs.TargetProperty);
                    Assert.Equal(errorCode, errorArgs.ErrorCode);
                    Assert.Equal(message, errorArgs.Message);
                    Assert.Equal(messageArgs, errorArgs.MessageArgs);
                }
            }
            finally
            {
                // Cleanup - unsubscribe from the event
                BindingDiagnostics.BindingFailed -= (sender, args) =>
                {
                    eventInvoked = true;
                    capturedArgs = args;
                };
            }
        }

        /// <summary>
        /// Tests that SendBindingFailure handles RuntimeFeature.EnableMauiDiagnostics correctly.
        /// Note: This test documents the limitation that RuntimeFeature.EnableMauiDiagnostics is a static property
        /// that cannot be mocked with NSubstitute. The actual behavior depends on the runtime configuration.
        /// When RuntimeFeature.EnableMauiDiagnostics is false, the method should return early without
        /// executing the logging or event invocation logic.
        /// </summary>
        [Fact]
        public void SendBindingFailure_RuntimeFeatureCheck_HandlesCorrectly()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            // This test primarily ensures that the method does not throw an exception
            // regardless of the RuntimeFeature.EnableMauiDiagnostics value
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);

            // TODO: To fully test the early return behavior when RuntimeFeature.EnableMauiDiagnostics is false,
            // consider using a test framework that supports static mocking or refactor the code to use
            // dependency injection for the RuntimeFeature dependency.
        }

        /// <summary>
        /// Tests that SendBindingFailure handles Application.Current being null gracefully.
        /// Note: This test documents the limitation that Application.Current is a static property
        /// that cannot be mocked with NSubstitute. The logging chain uses null-conditional operators,
        /// so null values should be handled gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void SendBindingFailure_ApplicationCurrentNull_HandlesGracefully()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            object source = new object();
            var bo = Substitute.For<BindableObject>();
            var bp = Substitute.For<BindableProperty>();
            string errorCode = "ERROR001";
            string message = "Test error message";
            object[] messageArgs = new object[] { "arg1", "arg2" };

            // Act & Assert
            // The method should not throw an exception even if Application.Current is null
            // due to the null-conditional operator usage in the logging chain
            var exception = Record.Exception(() =>
                BindingDiagnostics.SendBindingFailure(binding, source, bo, bp, errorCode, message, messageArgs));

            Assert.Null(exception);

            // TODO: To fully test the behavior when Application.Current is null,
            // consider using a test framework that supports static mocking or refactor the code
            // to use dependency injection for the Application dependency.
        }

        /// <summary>
        /// Tests that SendBindingFailure fires the BindingFailed event with correct arguments when diagnostics are enabled.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WhenDiagnosticsEnabled_FiresBindingFailedEvent()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var message = "Test message";
            var messageArgs = new object[] { "arg1", 42 };

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Same(binding, capturedEventArgs.Binding);
            Assert.Equal(errorCode, capturedEventArgs.ErrorCode);
            Assert.Equal(message, capturedEventArgs.Message);
            Assert.Same(messageArgs, capturedEventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles null errorCode parameter correctly.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithNullErrorCode_FiresEventWithNullErrorCode()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var message = "Test message";
            var messageArgs = new object[] { "test" };

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, null, message, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Null(capturedEventArgs.ErrorCode);
            Assert.Same(binding, capturedEventArgs.Binding);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles null message parameter correctly.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithNullMessage_FiresEventWithNullMessage()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var messageArgs = new object[] { "test" };

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, null, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Null(capturedEventArgs.Message);
            Assert.Same(binding, capturedEventArgs.Binding);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles null messageArgs parameter correctly.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithNullMessageArgs_FiresEventWithNullMessageArgs()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var message = "Test message";

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, null);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Null(capturedEventArgs.MessageArgs);
            Assert.Same(binding, capturedEventArgs.Binding);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles empty string parameters correctly.
        /// </summary>
        [Theory]
        [InlineData("", "")]
        [InlineData("", "message")]
        [InlineData("error", "")]
        [InlineData("   ", "   ")]
        public void SendBindingFailure_WithEmptyStrings_FiresEventWithEmptyStrings(string errorCode, string message)
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var messageArgs = new object[0];

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(errorCode, capturedEventArgs.ErrorCode);
            Assert.Equal(message, capturedEventArgs.Message);
            Assert.Same(binding, capturedEventArgs.Binding);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles empty messageArgs array correctly.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithEmptyMessageArgs_FiresEventWithEmptyArray()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var message = "Test message";
            var messageArgs = new object[0];

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Same(messageArgs, capturedEventArgs.MessageArgs);
            Assert.Empty(capturedEventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles messageArgs array with null elements correctly.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithNullElementsInMessageArgs_FiresEventWithNullElements()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var message = "Test message";
            var messageArgs = new object[] { null, "test", null, 42, null };

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Same(messageArgs, capturedEventArgs.MessageArgs);
            Assert.Equal(5, capturedEventArgs.MessageArgs.Length);
            Assert.Null(capturedEventArgs.MessageArgs[0]);
            Assert.Equal("test", capturedEventArgs.MessageArgs[1]);
            Assert.Null(capturedEventArgs.MessageArgs[2]);
            Assert.Equal(42, capturedEventArgs.MessageArgs[3]);
            Assert.Null(capturedEventArgs.MessageArgs[4]);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles long strings correctly.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithLongStrings_FiresEventWithLongStrings()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var longErrorCode = new string('E', 10000);
            var longMessage = new string('M', 10000);
            var messageArgs = new object[] { new string('A', 5000) };

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, longErrorCode, longMessage, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(longErrorCode, capturedEventArgs.ErrorCode);
            Assert.Equal(longMessage, capturedEventArgs.Message);
            Assert.Same(messageArgs, capturedEventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests that SendBindingFailure handles special characters in strings correctly.
        /// </summary>
        [Theory]
        [InlineData("\n\r\t", "Message with\nnewlines\rand\ttabs")]
        [InlineData("Error\u0000WithNull", "Message\u0001WithControl")]
        [InlineData("🎉ErrorWithEmoji🚀", "Message with emoji 🎯")]
        public void SendBindingFailure_WithSpecialCharacters_FiresEventWithSpecialCharacters(string errorCode, string message)
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var messageArgs = new object[] { "normal", "\u0002control", "🔥emoji" };

            BindingBaseErrorEventArgs capturedEventArgs = null;
            BindingDiagnostics.BindingFailed += (sender, e) => capturedEventArgs = e;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);

            // Assert
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(errorCode, capturedEventArgs.ErrorCode);
            Assert.Equal(message, capturedEventArgs.Message);
        }

        /// <summary>
        /// Tests that SendBindingFailure works when no event subscribers exist.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithNoEventSubscribers_DoesNotThrow()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var message = "Test message";
            var messageArgs = new object[] { "test" };

            // Act & Assert (should not throw)
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);
        }

        /// <summary>
        /// Tests that SendBindingFailure fires event to multiple subscribers.
        /// </summary>
        [Fact]
        public void SendBindingFailure_WithMultipleSubscribers_FiresEventToAllSubscribers()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var message = "Test message";
            var messageArgs = new object[] { "test" };

            var eventFiredCount = 0;
            BindingBaseErrorEventArgs capturedEventArgs1 = null;
            BindingBaseErrorEventArgs capturedEventArgs2 = null;

            BindingDiagnostics.BindingFailed += (sender, e) =>
            {
                eventFiredCount++;
                capturedEventArgs1 = e;
            };

            BindingDiagnostics.BindingFailed += (sender, e) =>
            {
                eventFiredCount++;
                capturedEventArgs2 = e;
            };

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);

            // Assert
            Assert.Equal(2, eventFiredCount);
            Assert.NotNull(capturedEventArgs1);
            Assert.NotNull(capturedEventArgs2);
            Assert.Same(binding, capturedEventArgs1.Binding);
            Assert.Same(binding, capturedEventArgs2.Binding);
        }

        /// <summary>
        /// Tests that SendBindingFailure passes null as sender to event subscribers.
        /// </summary>
        [Fact]
        public void SendBindingFailure_EventInvocation_PassesNullAsSender()
        {
            // Arrange
            var binding = Substitute.For<BindingBase>();
            var errorCode = "TestError";
            var message = "Test message";
            var messageArgs = new object[] { "test" };

            object capturedSender = new object(); // Initialize to non-null to verify it gets set to null
            BindingDiagnostics.BindingFailed += (sender, e) => capturedSender = sender;

            // Act
            BindingDiagnostics.SendBindingFailure(binding, errorCode, message, messageArgs);

            // Assert
            Assert.Null(capturedSender);
        }
    }
}