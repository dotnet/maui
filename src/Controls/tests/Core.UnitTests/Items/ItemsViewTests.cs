#nullable disable

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ItemsViewTests
    {
        /// <summary>
        /// Tests that OnRemainingItemsThresholdReached can be called without throwing exceptions.
        /// The method is currently empty but should not throw when invoked.
        /// </summary>
        [Fact]
        public void OnRemainingItemsThresholdReached_Called_DoesNotThrow()
        {
            // Arrange
            var itemsView = new TestItemsView();

            // Act & Assert
            var exception = Record.Exception(() => itemsView.CallOnRemainingItemsThresholdReached());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnRemainingItemsThresholdReached can be overridden and the overridden behavior is executed.
        /// Verifies that the virtual method pattern works correctly for derived classes.
        /// </summary>
        [Fact]
        public void OnRemainingItemsThresholdReached_WhenOverridden_ExecutesOverriddenBehavior()
        {
            // Arrange
            var itemsView = new TestItemsViewWithOverride();

            // Act
            itemsView.CallOnRemainingItemsThresholdReached();

            // Assert
            Assert.True(itemsView.WasCalled);
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached calls OnRemainingItemsThresholdReached.
        /// Verifies the method is invoked as part of the threshold reached workflow.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_Called_InvokesOnRemainingItemsThresholdReached()
        {
            // Arrange
            var itemsView = new TestItemsViewWithOverride();

            // Act
            itemsView.SendRemainingItemsThresholdReached();

            // Assert
            Assert.True(itemsView.WasCalled);
        }

        private class TestItemsView : ItemsView
        {
            public void CallOnRemainingItemsThresholdReached()
            {
                OnRemainingItemsThresholdReached();
            }
        }

        private class TestItemsViewWithOverride : ItemsView
        {
            public bool WasCalled { get; private set; }

            public void CallOnRemainingItemsThresholdReached()
            {
                OnRemainingItemsThresholdReached();
            }

            protected override void OnRemainingItemsThresholdReached()
            {
                WasCalled = true;
                base.OnRemainingItemsThresholdReached();
            }
        }

        /// <summary>
        /// Tests that OnScrolled can be called with valid ItemsViewScrolledEventArgs without throwing exceptions.
        /// Input: Valid ItemsViewScrolledEventArgs with typical values.
        /// Expected: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnScrolled_ValidEventArgs_DoesNotThrow()
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = 10.5,
                VerticalDelta = 20.3,
                HorizontalOffset = 100.0,
                VerticalOffset = 200.0,
                FirstVisibleItemIndex = 0,
                CenterItemIndex = 5,
                LastVisibleItemIndex = 10
            };

            // Act & Assert
            var exception = Record.Exception(() => testItemsView.CallOnScrolled(eventArgs));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnScrolled can be called with null ItemsViewScrolledEventArgs parameter.
        /// Input: null ItemsViewScrolledEventArgs.
        /// Expected: Method executes without throwing exceptions (depends on implementation).
        /// </summary>
        [Fact]
        public void OnScrolled_NullEventArgs_DoesNotThrow()
        {
            // Arrange
            var testItemsView = new TestableItemsView();

            // Act & Assert
            var exception = Record.Exception(() => testItemsView.CallOnScrolled(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnScrolled with edge case double values including NaN, PositiveInfinity, and NegativeInfinity.
        /// Input: ItemsViewScrolledEventArgs with extreme double values.
        /// Expected: Method executes without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(-100.5, -200.3, -50.7, -150.9)]
        public void OnScrolled_EdgeCaseDoubleValues_DoesNotThrow(double horizontalDelta, double verticalDelta, double horizontalOffset, double verticalOffset)
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = horizontalDelta,
                VerticalDelta = verticalDelta,
                HorizontalOffset = horizontalOffset,
                VerticalOffset = verticalOffset,
                FirstVisibleItemIndex = 0,
                CenterItemIndex = 1,
                LastVisibleItemIndex = 2
            };

            // Act & Assert
            var exception = Record.Exception(() => testItemsView.CallOnScrolled(eventArgs));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnScrolled with edge case integer values including minimum, maximum, zero, and negative values.
        /// Input: ItemsViewScrolledEventArgs with extreme integer values.
        /// Expected: Method executes without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue)]
        [InlineData(0, 0, 0)]
        [InlineData(-1, -1, -1)]
        [InlineData(-100, -200, -300)]
        public void OnScrolled_EdgeCaseIntegerValues_DoesNotThrow(int firstVisibleItemIndex, int centerItemIndex, int lastVisibleItemIndex)
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = 0.0,
                VerticalDelta = 0.0,
                HorizontalOffset = 0.0,
                VerticalOffset = 0.0,
                FirstVisibleItemIndex = firstVisibleItemIndex,
                CenterItemIndex = centerItemIndex,
                LastVisibleItemIndex = lastVisibleItemIndex
            };

            // Act & Assert
            var exception = Record.Exception(() => testItemsView.CallOnScrolled(eventArgs));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnScrolled can be overridden properly and the override is called.
        /// Input: Valid ItemsViewScrolledEventArgs.
        /// Expected: Override method is called successfully.
        /// </summary>
        [Fact]
        public void OnScrolled_OverriddenMethod_IsCalled()
        {
            // Arrange
            var overridableItemsView = new OverridableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = 15.5,
                VerticalDelta = 25.3,
                HorizontalOffset = 150.0,
                VerticalOffset = 250.0,
                FirstVisibleItemIndex = 2,
                CenterItemIndex = 7,
                LastVisibleItemIndex = 12
            };

            // Act
            overridableItemsView.CallOnScrolled(eventArgs);

            // Assert
            Assert.True(overridableItemsView.OnScrolledWasCalled);
            Assert.Same(eventArgs, overridableItemsView.ReceivedEventArgs);
        }

        /// <summary>
        /// Tests OnScrolled with mixed valid and edge case combinations.
        /// Input: ItemsViewScrolledEventArgs with mixed typical and edge case values.
        /// Expected: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnScrolled_MixedValidAndEdgeValues_DoesNotThrow()
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = double.PositiveInfinity,
                VerticalDelta = -50.5,
                HorizontalOffset = 0.0,
                VerticalOffset = double.NaN,
                FirstVisibleItemIndex = int.MaxValue,
                CenterItemIndex = 0,
                LastVisibleItemIndex = -1
            };

            // Act & Assert
            var exception = Record.Exception(() => testItemsView.CallOnScrolled(eventArgs));
            Assert.Null(exception);
        }

        /// <summary>
        /// Testable implementation of ItemsView that exposes the protected OnScrolled method.
        /// </summary>
        private class TestableItemsView : ItemsView
        {
            public void CallOnScrolled(ItemsViewScrolledEventArgs e)
            {
                OnScrolled(e);
            }
        }

        /// <summary>
        /// Overridable implementation of ItemsView that tracks calls to OnScrolled method.
        /// </summary>
        private class OverridableItemsView : ItemsView
        {
            public bool OnScrolledWasCalled { get; private set; }
            public ItemsViewScrolledEventArgs ReceivedEventArgs { get; private set; }

            public void CallOnScrolled(ItemsViewScrolledEventArgs e)
            {
                OnScrolled(e);
            }

            protected override void OnScrolled(ItemsViewScrolledEventArgs e)
            {
                OnScrolledWasCalled = true;
                ReceivedEventArgs = e;
                base.OnScrolled(e);
            }
        }

        /// <summary>
        /// Tests ScrollTo method with default parameters (only item provided).
        /// Verifies that OnScrollToRequested is called with correct default values.
        /// </summary>
        [Fact]
        public void ScrollTo_WithItemOnly_CallsOnScrollToRequestedWithDefaults()
        {
            // Arrange
            var testItemsView = new TestItemsView();
            var item = "test item";
            ScrollToRequestEventArgs capturedArgs = null;

            testItemsView.ScrollToRequested += (sender, args) => capturedArgs = args;

            // Act
            testItemsView.ScrollTo(item);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(item, capturedArgs.Item);
            Assert.Null(capturedArgs.Group);
            Assert.Equal(ScrollToPosition.MakeVisible, capturedArgs.ScrollToPosition);
            Assert.True(capturedArgs.IsAnimated);
            Assert.Equal(ScrollToMode.Element, capturedArgs.Mode);
        }

        /// <summary>
        /// Tests ScrollTo method with all parameters explicitly provided.
        /// Verifies that OnScrollToRequested is called with all specified values.
        /// </summary>
        [Theory]
        [InlineData(ScrollToPosition.MakeVisible, true)]
        [InlineData(ScrollToPosition.Start, false)]
        [InlineData(ScrollToPosition.Center, true)]
        [InlineData(ScrollToPosition.End, false)]
        public void ScrollTo_WithAllParameters_CallsOnScrollToRequestedWithSpecifiedValues(ScrollToPosition position, bool animate)
        {
            // Arrange
            var testItemsView = new TestItemsView();
            var item = "test item";
            var group = "test group";
            ScrollToRequestEventArgs capturedArgs = null;

            testItemsView.ScrollToRequested += (sender, args) => capturedArgs = args;

            // Act
            testItemsView.ScrollTo(item, group, position, animate);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(item, capturedArgs.Item);
            Assert.Equal(group, capturedArgs.Group);
            Assert.Equal(position, capturedArgs.ScrollToPosition);
            Assert.Equal(animate, capturedArgs.IsAnimated);
            Assert.Equal(ScrollToMode.Element, capturedArgs.Mode);
        }

        /// <summary>
        /// Tests ScrollTo method with null item parameter.
        /// Verifies that null items are accepted and passed through correctly.
        /// </summary>
        [Fact]
        public void ScrollTo_WithNullItem_CallsOnScrollToRequestedWithNullItem()
        {
            // Arrange
            var testItemsView = new TestItemsView();
            object item = null;
            ScrollToRequestEventArgs capturedArgs = null;

            testItemsView.ScrollToRequested += (sender, args) => capturedArgs = args;

            // Act
            testItemsView.ScrollTo(item);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Null(capturedArgs.Item);
            Assert.Null(capturedArgs.Group);
            Assert.Equal(ScrollToPosition.MakeVisible, capturedArgs.ScrollToPosition);
            Assert.True(capturedArgs.IsAnimated);
        }

        /// <summary>
        /// Tests ScrollTo method with different object types for item and group parameters.
        /// Verifies that various object types are handled correctly.
        /// </summary>
        [Theory]
        [InlineData("string item", "string group")]
        [InlineData(42, 99)]
        [InlineData("mixed item", 123)]
        public void ScrollTo_WithDifferentObjectTypes_CallsOnScrollToRequestedWithCorrectTypes(object item, object group)
        {
            // Arrange
            var testItemsView = new TestItemsView();
            ScrollToRequestEventArgs capturedArgs = null;

            testItemsView.ScrollToRequested += (sender, args) => capturedArgs = args;

            // Act
            testItemsView.ScrollTo(item, group);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(item, capturedArgs.Item);
            Assert.Equal(group, capturedArgs.Group);
        }

        /// <summary>
        /// Tests ScrollTo method with explicit null group parameter.
        /// Verifies that null groups are handled correctly.
        /// </summary>
        [Fact]
        public void ScrollTo_WithNullGroup_CallsOnScrollToRequestedWithNullGroup()
        {
            // Arrange
            var testItemsView = new TestItemsView();
            var item = "test item";
            ScrollToRequestEventArgs capturedArgs = null;

            testItemsView.ScrollToRequested += (sender, args) => capturedArgs = args;

            // Act
            testItemsView.ScrollTo(item, null, ScrollToPosition.Center, false);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(item, capturedArgs.Item);
            Assert.Null(capturedArgs.Group);
            Assert.Equal(ScrollToPosition.Center, capturedArgs.ScrollToPosition);
            Assert.False(capturedArgs.IsAnimated);
        }

        /// <summary>
        /// Tests ScrollTo method with invalid ScrollToPosition enum values.
        /// Verifies that out-of-range enum values are handled without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData((ScrollToPosition)(-1))]
        [InlineData((ScrollToPosition)999)]
        [InlineData((ScrollToPosition)int.MaxValue)]
        public void ScrollTo_WithInvalidScrollToPosition_CallsOnScrollToRequestedWithInvalidEnum(ScrollToPosition invalidPosition)
        {
            // Arrange
            var testItemsView = new TestItemsView();
            var item = "test item";
            ScrollToRequestEventArgs capturedArgs = null;

            testItemsView.ScrollToRequested += (sender, args) => capturedArgs = args;

            // Act
            testItemsView.ScrollTo(item, null, invalidPosition, true);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(item, capturedArgs.Item);
            Assert.Equal(invalidPosition, capturedArgs.ScrollToPosition);
        }

        /// <summary>
        /// Tests ScrollTo method with complex objects as item and group parameters.
        /// Verifies that complex object references are preserved correctly.
        /// </summary>
        [Fact]
        public void ScrollTo_WithComplexObjects_CallsOnScrollToRequestedWithSameReferences()
        {
            // Arrange
            var testItemsView = new TestItemsView();
            var complexItem = new { Name = "Test", Value = 42 };
            var complexGroup = new { Id = 1, Title = "Group1" };
            ScrollToRequestEventArgs capturedArgs = null;

            testItemsView.ScrollToRequested += (sender, args) => capturedArgs = args;

            // Act
            testItemsView.ScrollTo(complexItem, complexGroup, ScrollToPosition.End, false);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Same(complexItem, capturedArgs.Item);
            Assert.Same(complexGroup, capturedArgs.Group);
            Assert.Equal(ScrollToPosition.End, capturedArgs.ScrollToPosition);
            Assert.False(capturedArgs.IsAnimated);
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached invokes the RemainingItemsThresholdReached event with correct arguments.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_InvokesEvent_WithCorrectArguments()
        {
            // Arrange
            var itemsView = new TestItemsView();
            EventArgs receivedEventArgs = null;
            object receivedSender = null;

            itemsView.RemainingItemsThresholdReached += (sender, e) =>
            {
                receivedSender = sender;
                receivedEventArgs = e;
            };

            // Act
            itemsView.SendRemainingItemsThresholdReached();

            // Assert
            Assert.Same(itemsView, receivedSender);
            Assert.Same(EventArgs.Empty, receivedEventArgs);
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached executes command when CanExecute returns true.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_ExecutesCommand_WhenCanExecuteReturnsTrue()
        {
            // Arrange
            var itemsView = new TestItemsView();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();

            mockCommand.CanExecute(commandParameter).Returns(true);

            itemsView.RemainingItemsThresholdReachedCommand = mockCommand;
            itemsView.RemainingItemsThresholdReachedCommandParameter = commandParameter;

            // Act
            itemsView.SendRemainingItemsThresholdReached();

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.Received(1).Execute(commandParameter);
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached does not execute command when CanExecute returns false.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_DoesNotExecuteCommand_WhenCanExecuteReturnsFalse()
        {
            // Arrange
            var itemsView = new TestItemsView();
            var mockCommand = Substitute.For<ICommand>();
            var commandParameter = new object();

            mockCommand.CanExecute(commandParameter).Returns(false);

            itemsView.RemainingItemsThresholdReachedCommand = mockCommand;
            itemsView.RemainingItemsThresholdReachedCommandParameter = commandParameter;

            // Act
            itemsView.SendRemainingItemsThresholdReached();

            // Assert
            mockCommand.Received(1).CanExecute(commandParameter);
            mockCommand.DidNotReceive().Execute(Arg.Any<object>());
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached handles null command gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_HandlesNullCommand_WithoutException()
        {
            // Arrange
            var itemsView = new TestItemsView();
            itemsView.RemainingItemsThresholdReachedCommand = null;

            // Act & Assert
            var exception = Record.Exception(() => itemsView.SendRemainingItemsThresholdReached());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached handles null command parameter correctly.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_HandlesNullCommandParameter_Correctly()
        {
            // Arrange
            var itemsView = new TestItemsView();
            var mockCommand = Substitute.For<ICommand>();

            mockCommand.CanExecute(null).Returns(true);

            itemsView.RemainingItemsThresholdReachedCommand = mockCommand;
            itemsView.RemainingItemsThresholdReachedCommandParameter = null;

            // Act
            itemsView.SendRemainingItemsThresholdReached();

            // Assert
            mockCommand.Received(1).CanExecute(null);
            mockCommand.Received(1).Execute(null);
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached calls the virtual OnRemainingItemsThresholdReached method.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_CallsVirtualMethod_OnRemainingItemsThresholdReached()
        {
            // Arrange
            var itemsView = new TestItemsView();

            // Act
            itemsView.SendRemainingItemsThresholdReached();

            // Assert
            Assert.True(itemsView.OnRemainingItemsThresholdReachedCalled);
        }

        /// <summary>
        /// Tests that SendRemainingItemsThresholdReached works when event has no subscribers.
        /// </summary>
        [Fact]
        public void SendRemainingItemsThresholdReached_WorksWithoutEventSubscribers_WithoutException()
        {
            // Arrange
            var itemsView = new TestItemsView();

            // Act & Assert
            var exception = Record.Exception(() => itemsView.SendRemainingItemsThresholdReached());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendScrolled properly invokes the Scrolled event and calls OnScrolled with valid event arguments.
        /// Input conditions: Valid ItemsViewScrolledEventArgs with typical values.
        /// Expected result: Scrolled event is invoked and OnScrolled is called with the same parameter.
        /// </summary>
        [Fact]
        public void SendScrolled_ValidEventArgs_InvokesScrolledEventAndOnScrolled()
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = 10.5,
                VerticalDelta = 20.3,
                HorizontalOffset = 100.0,
                VerticalOffset = 200.0,
                FirstVisibleItemIndex = 5,
                CenterItemIndex = 10,
                LastVisibleItemIndex = 15
            };

            bool scrolledEventInvoked = false;
            ItemsViewScrolledEventArgs receivedEventArgs = null;
            testItemsView.Scrolled += (sender, e) =>
            {
                scrolledEventInvoked = true;
                receivedEventArgs = e;
            };

            // Act
            testItemsView.SendScrolled(eventArgs);

            // Assert
            Assert.True(scrolledEventInvoked);
            Assert.Same(eventArgs, receivedEventArgs);
            Assert.True(testItemsView.OnScrolledCalled);
            Assert.Same(eventArgs, testItemsView.OnScrolledParameter);
        }

        /// <summary>
        /// Tests that SendScrolled invokes the Scrolled event when there are multiple subscribers.
        /// Input conditions: Valid ItemsViewScrolledEventArgs with multiple event subscribers.
        /// Expected result: All event subscribers are notified.
        /// </summary>
        [Fact]
        public void SendScrolled_WithMultipleEventSubscribers_InvokesAllSubscribers()
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs { HorizontalDelta = 5.0 };

            int invocationCount = 0;
            testItemsView.Scrolled += (sender, e) => invocationCount++;
            testItemsView.Scrolled += (sender, e) => invocationCount++;

            // Act
            testItemsView.SendScrolled(eventArgs);

            // Assert
            Assert.Equal(2, invocationCount);
            Assert.True(testItemsView.OnScrolledCalled);
        }

        /// <summary>
        /// Tests that SendScrolled works correctly when there are no event subscribers.
        /// Input conditions: Valid ItemsViewScrolledEventArgs with no event subscribers.
        /// Expected result: OnScrolled is called without throwing exceptions.
        /// </summary>
        [Fact]
        public void SendScrolled_WithoutEventSubscribers_CallsOnScrolledWithoutError()
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs { VerticalDelta = 15.7 };

            // Act & Assert (should not throw)
            testItemsView.SendScrolled(eventArgs);

            Assert.True(testItemsView.OnScrolledCalled);
            Assert.Same(eventArgs, testItemsView.OnScrolledParameter);
        }

        /// <summary>
        /// Tests that SendScrolled handles boundary values correctly in ItemsViewScrolledEventArgs.
        /// Input conditions: ItemsViewScrolledEventArgs with extreme numeric values.
        /// Expected result: Method executes without errors and preserves all values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue, int.MinValue, int.MaxValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity, 0, -1)]
        [InlineData(double.NaN, 0.0, int.MaxValue, int.MinValue)]
        public void SendScrolled_BoundaryValues_ExecutesSuccessfully(double horizontalDelta, double verticalDelta, int firstIndex, int lastIndex)
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalDelta = horizontalDelta,
                VerticalDelta = verticalDelta,
                FirstVisibleItemIndex = firstIndex,
                LastVisibleItemIndex = lastIndex
            };

            bool eventInvoked = false;
            testItemsView.Scrolled += (sender, e) => eventInvoked = true;

            // Act
            testItemsView.SendScrolled(eventArgs);

            // Assert
            Assert.True(eventInvoked);
            Assert.True(testItemsView.OnScrolledCalled);
            Assert.Same(eventArgs, testItemsView.OnScrolledParameter);
        }

        /// <summary>
        /// Tests that SendScrolled throws ArgumentNullException when passed null event arguments.
        /// Input conditions: null ItemsViewScrolledEventArgs parameter.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SendScrolled_NullEventArgs_ThrowsArgumentNullException()
        {
            // Arrange
            var testItemsView = new TestableItemsView();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => testItemsView.SendScrolled(null));
        }

        /// <summary>
        /// Tests that SendScrolled passes the exact same parameter instance to OnScrolled.
        /// Input conditions: Valid ItemsViewScrolledEventArgs instance.
        /// Expected result: The same object reference is passed to OnScrolled.
        /// </summary>
        [Fact]
        public void SendScrolled_ValidEventArgs_PassesSameInstanceToOnScrolled()
        {
            // Arrange
            var testItemsView = new TestableItemsView();
            var eventArgs = new ItemsViewScrolledEventArgs
            {
                HorizontalOffset = 42.0,
                VerticalOffset = 84.0,
                CenterItemIndex = 7
            };

            // Act
            testItemsView.SendScrolled(eventArgs);

            // Assert
            Assert.True(ReferenceEquals(eventArgs, testItemsView.OnScrolledParameter));
        }

    }
}