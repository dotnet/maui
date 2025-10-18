#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the SwipeItemView class.
    /// </summary>
    public partial class SwipeItemViewTests
    {
        /// <summary>
        /// Tests that the CommandParameter property correctly stores and retrieves various types of values.
        /// Verifies the getter calls GetValue with the correct BindableProperty and returns the expected value.
        /// </summary>
        /// <param name="value">The value to set and retrieve from the CommandParameter property.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("test")]
        [InlineData("   ")]
        [InlineData("very long string that contains many characters to test edge cases")]
        [InlineData(42)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(3.14)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(true)]
        [InlineData(false)]
        public void CommandParameter_SetAndGet_ReturnsCorrectValue(object value)
        {
            // Arrange
            var swipeItemView = new SwipeItemView();

            // Act
            swipeItemView.CommandParameter = value;
            var result = swipeItemView.CommandParameter;

            // Assert
            if (value is double doubleValue && double.IsNaN(doubleValue))
            {
                Assert.True(double.IsNaN((double)result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that the CommandParameter property correctly handles complex objects.
        /// Verifies that custom objects can be stored and retrieved properly.
        /// </summary>
        [Fact]
        public void CommandParameter_SetComplexObject_ReturnsCorrectObject()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var complexObject = new { Name = "Test", Value = 123 };

            // Act
            swipeItemView.CommandParameter = complexObject;
            var result = swipeItemView.CommandParameter;

            // Assert
            Assert.Same(complexObject, result);
        }

        /// <summary>
        /// Tests that the CommandParameter property correctly handles collections.
        /// Verifies that arrays and lists can be stored and retrieved properly.
        /// </summary>
        [Fact]
        public void CommandParameter_SetCollection_ReturnsCorrectCollection()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var collection = new List<string> { "item1", "item2", "item3" };

            // Act
            swipeItemView.CommandParameter = collection;
            var result = swipeItemView.CommandParameter;

            // Assert
            Assert.Same(collection, result);
        }

        /// <summary>
        /// Tests that the CommandParameter property correctly handles empty collections.
        /// Verifies that empty arrays and lists can be stored and retrieved properly.
        /// </summary>
        [Fact]
        public void CommandParameter_SetEmptyCollection_ReturnsCorrectCollection()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var emptyCollection = new List<object>();

            // Act
            swipeItemView.CommandParameter = emptyCollection;
            var result = swipeItemView.CommandParameter;

            // Assert
            Assert.Same(emptyCollection, result);
        }

        /// <summary>
        /// Tests that the CommandParameter property getter returns the default value when not set.
        /// Verifies that the initial state of the property is null.
        /// </summary>
        [Fact]
        public void CommandParameter_GetWithoutSet_ReturnsDefaultValue()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();

            // Act
            var result = swipeItemView.CommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the CommandParameter property can be set multiple times with different values.
        /// Verifies that subsequent sets overwrite previous values correctly.
        /// </summary>
        [Fact]
        public void CommandParameter_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var firstValue = "first";
            var secondValue = 42;
            var thirdValue = new { Test = "object" };

            // Act & Assert
            swipeItemView.CommandParameter = firstValue;
            Assert.Equal(firstValue, swipeItemView.CommandParameter);

            swipeItemView.CommandParameter = secondValue;
            Assert.Equal(secondValue, swipeItemView.CommandParameter);

            swipeItemView.CommandParameter = thirdValue;
            Assert.Same(thirdValue, swipeItemView.CommandParameter);
        }

        /// <summary>
        /// Tests that the CommandParameter property correctly handles special string values.
        /// Verifies that strings with special characters, control characters, and Unicode are handled properly.
        /// </summary>
        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("\t")]
        [InlineData("🚀")]
        [InlineData("Special chars: !@#$%^&*()")]
        [InlineData("Unicode: αβγδε")]
        public void CommandParameter_SetSpecialStrings_ReturnsCorrectValue(string value)
        {
            // Arrange
            var swipeItemView = new SwipeItemView();

            // Act
            swipeItemView.CommandParameter = value;
            var result = swipeItemView.CommandParameter;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that OnInvoked does not execute command when Command is null but still invokes the Invoked event.
        /// </summary>
        [Fact]
        public void OnInvoked_CommandIsNull_DoesNotExecuteCommandButInvokesEvent()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = null;
            swipeItemView.CommandParameter = "test";

            var eventInvoked = false;
            swipeItemView.Invoked += (sender, args) => eventInvoked = true;

            // Act
            swipeItemView.OnInvoked();

            // Assert
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests that OnInvoked does not execute command when Command.CanExecute returns false but still invokes the Invoked event.
        /// </summary>
        [Fact]
        public void OnInvoked_CommandCanExecuteReturnsFalse_DoesNotExecuteCommandButInvokesEvent()
        {
            // Arrange
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(false);

            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = mockCommand;
            swipeItemView.CommandParameter = "test";

            var eventInvoked = false;
            swipeItemView.Invoked += (sender, args) => eventInvoked = true;

            // Act
            swipeItemView.OnInvoked();

            // Assert
            mockCommand.Received(1).CanExecute("test");
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests that OnInvoked executes command when Command.CanExecute returns true and invokes the Invoked event.
        /// </summary>
        [Fact]
        public void OnInvoked_CommandCanExecuteReturnsTrue_ExecutesCommandAndInvokesEvent()
        {
            // Arrange
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(true);

            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = mockCommand;
            swipeItemView.CommandParameter = "test";

            var eventInvoked = false;
            swipeItemView.Invoked += (sender, args) => eventInvoked = true;

            // Act
            swipeItemView.OnInvoked();

            // Assert
            mockCommand.Received(1).CanExecute("test");
            mockCommand.Received(1).Execute("test");
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests that OnInvoked works correctly when CommandParameter is null.
        /// </summary>
        [Fact]
        public void OnInvoked_CommandParameterIsNull_ExecutesCommandWithNullParameter()
        {
            // Arrange
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(null).Returns(true);

            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = mockCommand;
            swipeItemView.CommandParameter = null;

            var eventInvoked = false;
            swipeItemView.Invoked += (sender, args) => eventInvoked = true;

            // Act
            swipeItemView.OnInvoked();

            // Assert
            mockCommand.Received(1).CanExecute(null);
            mockCommand.Received(1).Execute(null);
            Assert.True(eventInvoked);
        }

        /// <summary>
        /// Tests that OnInvoked invokes the event with correct sender and EventArgs.Empty.
        /// </summary>
        [Fact]
        public void OnInvoked_InvokedEvent_PassesCorrectSenderAndEventArgs()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = null;

            object actualSender = null;
            EventArgs actualArgs = null;
            swipeItemView.Invoked += (sender, args) =>
            {
                actualSender = sender;
                actualArgs = args;
            };

            // Act
            swipeItemView.OnInvoked();

            // Assert
            Assert.Same(swipeItemView, actualSender);
            Assert.Same(EventArgs.Empty, actualArgs);
        }

        /// <summary>
        /// Tests that OnInvoked handles multiple event subscribers correctly.
        /// </summary>
        [Fact]
        public void OnInvoked_MultipleEventSubscribers_InvokesAllSubscribers()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = null;

            var firstSubscriberInvoked = false;
            var secondSubscriberInvoked = false;

            swipeItemView.Invoked += (sender, args) => firstSubscriberInvoked = true;
            swipeItemView.Invoked += (sender, args) => secondSubscriberInvoked = true;

            // Act
            swipeItemView.OnInvoked();

            // Assert
            Assert.True(firstSubscriberInvoked);
            Assert.True(secondSubscriberInvoked);
        }

        /// <summary>
        /// Tests that OnInvoked does not throw when no event subscribers are present.
        /// </summary>
        [Fact]
        public void OnInvoked_NoEventSubscribers_DoesNotThrow()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = null;

            // Act & Assert
            var exception = Record.Exception(() => swipeItemView.OnInvoked());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnInvoked passes the exact CommandParameter value to CanExecute and Execute methods.
        /// </summary>
        [Theory]
        [InlineData("string parameter")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(null)]
        public void OnInvoked_VariousCommandParameterTypes_PassesCorrectParameterToCommand(object commandParameter)
        {
            // Arrange
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(commandParameter).Returns(true);

            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = mockCommand;
            swipeItemView.CommandParameter = commandParameter;

            // Act
            swipeItemView.OnInvoked();

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.Received(1).Execute(commandParameter);
        }

        /// <summary>
        /// Tests that OnInvoked checks CanExecute with the current CommandParameter value.
        /// </summary>
        [Fact]
        public void OnInvoked_CommandParameterChanged_UsesCurrentParameterValue()
        {
            // Arrange
            var mockCommand = Substitute.For<ICommand>();
            mockCommand.CanExecute(Arg.Any<object>()).Returns(true);

            var swipeItemView = new SwipeItemView();
            swipeItemView.Command = mockCommand;
            swipeItemView.CommandParameter = "initial";

            // Change the parameter before invoking
            swipeItemView.CommandParameter = "changed";

            // Act
            swipeItemView.OnInvoked();

            // Assert
            mockCommand.Received(1).CanExecute("changed");
            mockCommand.Received(1).Execute("changed");
            mockCommand.DidNotReceive().CanExecute("initial");
            mockCommand.DidNotReceive().Execute("initial");
        }

        /// <summary>
        /// Tests that the Command property getter returns null when no command has been set.
        /// Verifies the initial state and default behavior of the Command property.
        /// Expected result: Command property returns null.
        /// </summary>
        [Fact]
        public void Command_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();

            // Act
            var result = swipeItemView.Command;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Command property getter returns the correct ICommand instance after setting it.
        /// Verifies that the property correctly stores and retrieves the assigned command.
        /// Expected result: Command property returns the same ICommand instance that was set.
        /// </summary>
        [Fact]
        public void Command_WhenSet_ReturnsCorrectValue()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            swipeItemView.Command = mockCommand;
            var result = swipeItemView.Command;

            // Assert
            Assert.Same(mockCommand, result);
        }

        /// <summary>
        /// Tests that the Command property can be set to null and returns null when retrieved.
        /// Verifies that the property correctly handles null assignment and retrieval.
        /// Expected result: Command property returns null after being set to null.
        /// </summary>
        [Fact]
        public void Command_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var mockCommand = Substitute.For<ICommand>();
            swipeItemView.Command = mockCommand;

            // Act
            swipeItemView.Command = null;
            var result = swipeItemView.Command;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Command property can be changed multiple times and always returns the most recent value.
        /// Verifies that subsequent assignments properly overwrite previous values.
        /// Expected result: Command property returns the last assigned ICommand instance.
        /// </summary>
        [Fact]
        public void Command_WhenChangedMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var firstCommand = Substitute.For<ICommand>();
            var secondCommand = Substitute.For<ICommand>();

            // Act
            swipeItemView.Command = firstCommand;
            swipeItemView.Command = secondCommand;
            var result = swipeItemView.Command;

            // Assert
            Assert.Same(secondCommand, result);
            Assert.NotSame(firstCommand, result);
        }

        /// <summary>
        /// Tests that the Command property getter correctly casts the underlying value to ICommand.
        /// Verifies the type casting behavior in the getter implementation.
        /// Expected result: Command property returns an ICommand instance with correct type.
        /// </summary>
        [Fact]
        public void Command_WhenRetrieved_ReturnsCorrectType()
        {
            // Arrange
            var swipeItemView = new SwipeItemView();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            swipeItemView.Command = mockCommand;
            var result = swipeItemView.Command;

            // Assert
            Assert.IsAssignableFrom<ICommand>(result);
            Assert.IsType<Castle.Proxies.ICommandProxy>(result);
        }
    }
}