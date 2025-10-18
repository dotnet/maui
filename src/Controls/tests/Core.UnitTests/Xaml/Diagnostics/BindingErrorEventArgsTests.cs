#nullable disable
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class BindingBaseErrorEventArgsTests
    {
        /// <summary>
        /// Tests that the BindingBaseErrorEventArgs constructor properly initializes all properties
        /// with valid input parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithValidParameters_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 10, 5);
            var binding = Substitute.For<BindingBase>();
            var errorCode = "BINDING_ERROR_001";
            var message = "Test error message";
            var messageArgs = new object[] { "arg1", 42, true };

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, errorCode, message, messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, eventArgs.XamlSourceInfo);
            Assert.Same(binding, eventArgs.Binding);
            Assert.Equal(errorCode, eventArgs.ErrorCode);
            Assert.Equal(message, eventArgs.Message);
            Assert.Same(messageArgs, eventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests that the constructor handles null binding parameter correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithNullBinding_SetsBindingToNull()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 1, 1);
            BindingBase binding = null;
            var errorCode = "ERROR";
            var message = "Message";
            var messageArgs = new object[0];

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, errorCode, message, messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, eventArgs.XamlSourceInfo);
            Assert.Null(eventArgs.Binding);
            Assert.Equal(errorCode, eventArgs.ErrorCode);
            Assert.Equal(message, eventArgs.Message);
            Assert.Same(messageArgs, eventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor behavior with various string parameter edge cases including null, empty, and whitespace values.
        /// </summary>
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("   \t\n\r   ", "   \t\n\r   ")]
        [InlineData(null, "Valid message")]
        [InlineData("VALID_CODE", null)]
        [InlineData("", "Valid message")]
        [InlineData("VALID_CODE", "")]
        public void Constructor_WithStringEdgeCases_HandlesAllStringValues(string errorCode, string message)
        {
            // Arrange
            var sourceUri = new Uri("https://example.com/test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 0, 0);
            var binding = Substitute.For<BindingBase>();
            var messageArgs = new object[0];

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, errorCode, message, messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, eventArgs.XamlSourceInfo);
            Assert.Same(binding, eventArgs.Binding);
            Assert.Equal(errorCode, eventArgs.ErrorCode);
            Assert.Equal(message, eventArgs.Message);
            Assert.Same(messageArgs, eventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor behavior with various messageArgs array edge cases including null and empty arrays.
        /// </summary>
        [Fact]
        public void Constructor_WithNullMessageArgs_SetsMessageArgsToNull()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 5, 10);
            var binding = Substitute.For<BindingBase>();
            var errorCode = "ERROR_CODE";
            var message = "Error message";
            object[] messageArgs = null;

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, errorCode, message, messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, eventArgs.XamlSourceInfo);
            Assert.Same(binding, eventArgs.Binding);
            Assert.Equal(errorCode, eventArgs.ErrorCode);
            Assert.Equal(message, eventArgs.Message);
            Assert.Null(eventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor with various types of objects in the messageArgs array.
        /// </summary>
        [Fact]
        public void Constructor_WithVariousMessageArgTypes_HandlesAllObjectTypes()
        {
            // Arrange
            var sourceUri = new Uri("https://test.com/file.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, int.MaxValue, int.MinValue);
            var binding = Substitute.For<BindingBase>();
            var errorCode = "COMPLEX_ERROR";
            var message = "Complex error with various args";
            var messageArgs = new object[]
            {
                "string",
                42,
                3.14159,
                true,
                DateTime.Now,
                null,
                new object(),
                new int[] { 1, 2, 3 }
            };

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, errorCode, message, messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, eventArgs.XamlSourceInfo);
            Assert.Same(binding, eventArgs.Binding);
            Assert.Equal(errorCode, eventArgs.ErrorCode);
            Assert.Equal(message, eventArgs.Message);
            Assert.Same(messageArgs, eventArgs.MessageArgs);
            Assert.Equal(8, eventArgs.MessageArgs.Length);
        }

        /// <summary>
        /// Tests constructor with extreme boundary values for SourceInfo parameters.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        [InlineData(1000000, 1000000)]
        public void Constructor_WithExtremeBoundaryValues_HandlesAllIntegerBoundaries(int lineNumber, int linePosition)
        {
            // Arrange
            var sourceUri = new Uri("file:///boundary-test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, lineNumber, linePosition);
            var binding = Substitute.For<BindingBase>();
            var errorCode = "BOUNDARY_TEST";
            var message = "Testing boundary values";
            var messageArgs = new object[0];

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, errorCode, message, messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, eventArgs.XamlSourceInfo);
            Assert.Equal(lineNumber, eventArgs.XamlSourceInfo.LineNumber);
            Assert.Equal(linePosition, eventArgs.XamlSourceInfo.LinePosition);
            Assert.Same(binding, eventArgs.Binding);
            Assert.Equal(errorCode, eventArgs.ErrorCode);
            Assert.Equal(message, eventArgs.Message);
            Assert.Same(messageArgs, eventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor with very long strings to verify no length limitations cause issues.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongStrings_HandlesLargeStringInputs()
        {
            // Arrange
            var sourceUri = new Uri("file:///long-string-test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 1, 1);
            var binding = Substitute.For<BindingBase>();
            var longErrorCode = new string('E', 10000);
            var longMessage = new string('M', 10000);
            var messageArgs = new object[] { new string('A', 5000) };

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, longErrorCode, longMessage, messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, eventArgs.XamlSourceInfo);
            Assert.Same(binding, eventArgs.Binding);
            Assert.Equal(longErrorCode, eventArgs.ErrorCode);
            Assert.Equal(longMessage, eventArgs.Message);
            Assert.Same(messageArgs, eventArgs.MessageArgs);
            Assert.Equal(10000, eventArgs.ErrorCode.Length);
            Assert.Equal(10000, eventArgs.Message.Length);
        }

        /// <summary>
        /// Tests that the constructor properly inherits from EventArgs.
        /// </summary>
        [Fact]
        public void Constructor_CreateInstance_InheritsFromEventArgs()
        {
            // Arrange
            var sourceUri = new Uri("file:///inheritance-test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 1, 1);
            var binding = Substitute.For<BindingBase>();
            var errorCode = "INHERITANCE_TEST";
            var message = "Testing inheritance";
            var messageArgs = new object[0];

            // Act
            var eventArgs = new BindingBaseErrorEventArgs(xamlSourceInfo, binding, errorCode, message, messageArgs);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests.Xaml.Diagnostics
{
    /// <summary>
    /// Unit tests for the BindingErrorEventArgs class constructor.
    /// Tests parameter validation, property initialization, and base class integration.
    /// </summary>
    public class BindingErrorEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor correctly initializes all properties with valid parameters.
        /// Verifies that all constructor parameters are properly assigned to their corresponding properties
        /// and that the base class constructor is called with the correct arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_InitializesAllProperties()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 10, 5);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = new object();
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BindableObject), "default");
            var errorCode = "BE001";
            var message = "Binding error occurred";
            var messageArgs = new object[] { "arg1", 42 };

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, bindingErrorEventArgs.XamlSourceInfo);
            Assert.Same(binding, bindingErrorEventArgs.Binding);
            Assert.Same(bindingSource, bindingErrorEventArgs.Source);
            Assert.Same(target, bindingErrorEventArgs.Target);
            Assert.Same(property, bindingErrorEventArgs.TargetProperty);
            Assert.Equal(errorCode, bindingErrorEventArgs.ErrorCode);
            Assert.Equal(message, bindingErrorEventArgs.Message);
            Assert.Same(messageArgs, bindingErrorEventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor behavior with null binding source parameter.
        /// Verifies that null binding sources are handled correctly since object type allows null values.
        /// </summary>
        [Fact]
        public void Constructor_NullBindingSource_InitializesSourceAsNull()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 1, 1);
            var binding = Substitute.For<BindingBase>();
            object bindingSource = null;
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BindableObject));
            var errorCode = "BE002";
            var message = "Test message";
            var messageArgs = new object[0];

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Null(bindingErrorEventArgs.Source);
            Assert.Same(target, bindingErrorEventArgs.Target);
            Assert.Same(property, bindingErrorEventArgs.TargetProperty);
        }

        /// <summary>
        /// Tests constructor with null error code parameter.
        /// Verifies that null error codes are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_NullErrorCode_InitializesErrorCodeAsNull()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 5, 10);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = "test source";
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(int), typeof(BindableObject), 0);
            string errorCode = null;
            var message = "Error message";
            var messageArgs = new object[] { "test" };

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Null(bindingErrorEventArgs.ErrorCode);
            Assert.Equal(message, bindingErrorEventArgs.Message);
            Assert.Same(bindingSource, bindingErrorEventArgs.Source);
        }

        /// <summary>
        /// Tests constructor with null message parameter.
        /// Verifies that null messages are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_NullMessage_InitializesMessageAsNull()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 15, 20);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = new { Name = "TestSource" };
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(bool), typeof(BindableObject), false);
            var errorCode = "BE003";
            string message = null;
            var messageArgs = new object[] { 1, 2, 3 };

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Null(bindingErrorEventArgs.Message);
            Assert.Equal(errorCode, bindingErrorEventArgs.ErrorCode);
            Assert.Same(messageArgs, bindingErrorEventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor with null message arguments parameter.
        /// Verifies that null message argument arrays are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_NullMessageArgs_InitializesMessageArgsAsNull()
        {
            // Arrange
            var sourceUri = new Uri("file:///test.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 25, 30);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = 12345;
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(double), typeof(BindableObject), 0.0);
            var errorCode = "BE004";
            var message = "Test error message";
            object[] messageArgs = null;

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Null(bindingErrorEventArgs.MessageArgs);
            Assert.Same(bindingSource, bindingErrorEventArgs.Source);
            Assert.Equal(message, bindingErrorEventArgs.Message);
        }

        /// <summary>
        /// Tests constructor with empty string parameters for error code and message.
        /// Verifies that empty strings are preserved and handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_EmptyStrings_InitializesWithEmptyStrings()
        {
            // Arrange
            var sourceUri = new Uri("file:///empty.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 1, 1);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = "source";
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BindableObject), string.Empty);
            var errorCode = string.Empty;
            var message = string.Empty;
            var messageArgs = new object[0];

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Equal(string.Empty, bindingErrorEventArgs.ErrorCode);
            Assert.Equal(string.Empty, bindingErrorEventArgs.Message);
            Assert.Empty(bindingErrorEventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor with whitespace-only string parameters.
        /// Verifies that whitespace strings are preserved correctly.
        /// </summary>
        [Fact]
        public void Constructor_WhitespaceStrings_InitializesWithWhitespace()
        {
            // Arrange
            var sourceUri = new Uri("file:///whitespace.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 2, 4);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = true;
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(object), typeof(BindableObject));
            var errorCode = "   ";
            var message = "\t\n  ";
            var messageArgs = new object[] { null, "", "  " };

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Equal("   ", bindingErrorEventArgs.ErrorCode);
            Assert.Equal("\t\n  ", bindingErrorEventArgs.Message);
            Assert.Equal(3, bindingErrorEventArgs.MessageArgs.Length);
            Assert.Null(bindingErrorEventArgs.MessageArgs[0]);
            Assert.Equal("", bindingErrorEventArgs.MessageArgs[1]);
            Assert.Equal("  ", bindingErrorEventArgs.MessageArgs[2]);
        }

        /// <summary>
        /// Tests constructor with empty message arguments array.
        /// Verifies that empty arrays are handled correctly and preserved.
        /// </summary>
        [Fact]
        public void Constructor_EmptyMessageArgsArray_InitializesWithEmptyArray()
        {
            // Arrange
            var sourceUri = new Uri("file:///empty-args.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 100, 200);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = DateTime.Now;
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(DateTime), typeof(BindableObject), DateTime.MinValue);
            var errorCode = "BE005";
            var message = "Empty args test";
            var messageArgs = new object[0];

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.NotNull(bindingErrorEventArgs.MessageArgs);
            Assert.Empty(bindingErrorEventArgs.MessageArgs);
        }

        /// <summary>
        /// Tests constructor with message arguments array containing null elements.
        /// Verifies that arrays with null values are preserved correctly.
        /// </summary>
        [Fact]
        public void Constructor_MessageArgsWithNullElements_InitializesWithNullElements()
        {
            // Arrange
            var sourceUri = new Uri("file:///null-elements.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 50, 75);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = Guid.NewGuid();
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(Guid), typeof(BindableObject), Guid.Empty);
            var errorCode = "BE006";
            var message = "Null elements test";
            var messageArgs = new object[] { null, "valid", null, 42, null };

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Equal(5, bindingErrorEventArgs.MessageArgs.Length);
            Assert.Null(bindingErrorEventArgs.MessageArgs[0]);
            Assert.Equal("valid", bindingErrorEventArgs.MessageArgs[1]);
            Assert.Null(bindingErrorEventArgs.MessageArgs[2]);
            Assert.Equal(42, bindingErrorEventArgs.MessageArgs[3]);
            Assert.Null(bindingErrorEventArgs.MessageArgs[4]);
        }

        /// <summary>
        /// Tests constructor with extreme SourceInfo values.
        /// Verifies that boundary values for line numbers and positions are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_ExtremeSourceInfoValues_InitializesCorrectly()
        {
            // Arrange
            var sourceUri = new Uri("file:///extreme.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, int.MaxValue, int.MaxValue);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = double.NaN;
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(double), typeof(BindableObject), 0.0);
            var errorCode = "BE007";
            var message = "Extreme values test";
            var messageArgs = new object[] { int.MinValue, int.MaxValue, double.PositiveInfinity, double.NegativeInfinity };

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                errorCode,
                message,
                messageArgs);

            // Assert
            Assert.Same(xamlSourceInfo, bindingErrorEventArgs.XamlSourceInfo);
            Assert.Equal(int.MaxValue, bindingErrorEventArgs.XamlSourceInfo.LineNumber);
            Assert.Equal(int.MaxValue, bindingErrorEventArgs.XamlSourceInfo.LinePosition);
            Assert.True(double.IsNaN((double)bindingErrorEventArgs.Source));
        }

        /// <summary>
        /// Tests constructor with very long string parameters.
        /// Verifies that long strings are handled correctly without truncation.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongStrings_InitializesCorrectly()
        {
            // Arrange
            var sourceUri = new Uri("file:///long-strings.xaml");
            var xamlSourceInfo = new SourceInfo(sourceUri, 1, 1);
            var binding = Substitute.For<BindingBase>();
            var bindingSource = "test";
            var target = Substitute.For<BindableObject>();
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BindableObject));
            var longErrorCode = new string('E', 10000);
            var longMessage = new string('M', 10000);
            var messageArgs = new object[] { new string('A', 5000) };

            // Act
            var bindingErrorEventArgs = new BindingErrorEventArgs(
                xamlSourceInfo,
                binding,
                bindingSource,
                target,
                property,
                longErrorCode,
                longMessage,
                messageArgs);

            // Assert
            Assert.Equal(10000, bindingErrorEventArgs.ErrorCode.Length);
            Assert.Equal(10000, bindingErrorEventArgs.Message.Length);
            Assert.True(bindingErrorEventArgs.ErrorCode.All(c => c == 'E'));
            Assert.True(bindingErrorEventArgs.Message.All(c => c == 'M'));
            Assert.Equal(5000, ((string)bindingErrorEventArgs.MessageArgs[0]).Length);
        }
    }
}