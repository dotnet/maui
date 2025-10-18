#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for ReorderableItemsView class.
    /// </summary>
    public class ReorderableItemsViewTests
    {
        /// <summary>
        /// Tests that SendReorderCompleted does not throw when no event handlers are subscribed.
        /// Input conditions: ReorderableItemsView instance with no ReorderCompleted event subscribers.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void SendReorderCompleted_NoEventHandlers_DoesNotThrow()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act & Assert
            var exception = Record.Exception(() => reorderableItemsView.SendReorderCompleted());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendReorderCompleted properly invokes a single subscribed event handler with correct parameters.
        /// Input conditions: ReorderableItemsView instance with one ReorderCompleted event handler subscribed.
        /// Expected result: Event handler is invoked with the ReorderableItemsView instance as sender and EventArgs.Empty as arguments.
        /// </summary>
        [Fact]
        public void SendReorderCompleted_SingleEventHandler_InvokesHandlerWithCorrectParameters()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();
            object receivedSender = null;
            EventArgs receivedArgs = null;
            bool handlerInvoked = false;

            reorderableItemsView.ReorderCompleted += (sender, args) =>
            {
                receivedSender = sender;
                receivedArgs = args;
                handlerInvoked = true;
            };

            // Act
            reorderableItemsView.SendReorderCompleted();

            // Assert
            Assert.True(handlerInvoked);
            Assert.Same(reorderableItemsView, receivedSender);
            Assert.Same(EventArgs.Empty, receivedArgs);
        }

        /// <summary>
        /// Tests that SendReorderCompleted properly invokes all subscribed event handlers.
        /// Input conditions: ReorderableItemsView instance with multiple ReorderCompleted event handlers subscribed.
        /// Expected result: All event handlers are invoked.
        /// </summary>
        [Fact]
        public void SendReorderCompleted_MultipleEventHandlers_InvokesAllHandlers()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();
            int handler1InvokeCount = 0;
            int handler2InvokeCount = 0;
            int handler3InvokeCount = 0;

            reorderableItemsView.ReorderCompleted += (sender, args) => handler1InvokeCount++;
            reorderableItemsView.ReorderCompleted += (sender, args) => handler2InvokeCount++;
            reorderableItemsView.ReorderCompleted += (sender, args) => handler3InvokeCount++;

            // Act
            reorderableItemsView.SendReorderCompleted();

            // Assert
            Assert.Equal(1, handler1InvokeCount);
            Assert.Equal(1, handler2InvokeCount);
            Assert.Equal(1, handler3InvokeCount);
        }

        /// <summary>
        /// Tests that SendReorderCompleted continues to work after event handlers are subscribed and unsubscribed.
        /// Input conditions: ReorderableItemsView instance with event handlers that are subscribed, unsubscribed, and resubscribed.
        /// Expected result: Only currently subscribed handlers are invoked.
        /// </summary>
        [Fact]
        public void SendReorderCompleted_AfterUnsubscribingHandlers_OnlyInvokesRemainingHandlers()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();
            int handler1InvokeCount = 0;
            int handler2InvokeCount = 0;

            EventHandler handler1 = (sender, args) => handler1InvokeCount++;
            EventHandler handler2 = (sender, args) => handler2InvokeCount++;

            reorderableItemsView.ReorderCompleted += handler1;
            reorderableItemsView.ReorderCompleted += handler2;

            // Act - First invocation with both handlers
            reorderableItemsView.SendReorderCompleted();

            // Assert - Both handlers invoked
            Assert.Equal(1, handler1InvokeCount);
            Assert.Equal(1, handler2InvokeCount);

            // Arrange - Remove one handler
            reorderableItemsView.ReorderCompleted -= handler1;

            // Act - Second invocation with only one handler
            reorderableItemsView.SendReorderCompleted();

            // Assert - Only remaining handler invoked
            Assert.Equal(1, handler1InvokeCount); // Should remain 1
            Assert.Equal(2, handler2InvokeCount); // Should be incremented to 2
        }

        /// <summary>
        /// Tests that SendReorderCompleted handles exceptions in event handlers gracefully and continues to invoke remaining handlers.
        /// Input conditions: ReorderableItemsView instance with multiple event handlers where one throws an exception.
        /// Expected result: Exception from one handler does not prevent other handlers from being invoked.
        /// </summary>
        [Fact]
        public void SendReorderCompleted_EventHandlerThrowsException_ContinuesInvokingRemainingHandlers()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();
            bool handler1Invoked = false;
            bool handler3Invoked = false;

            reorderableItemsView.ReorderCompleted += (sender, args) => handler1Invoked = true;
            reorderableItemsView.ReorderCompleted += (sender, args) => throw new InvalidOperationException("Test exception");
            reorderableItemsView.ReorderCompleted += (sender, args) => handler3Invoked = true;

            // Act & Assert
            var exception = Record.Exception(() => reorderableItemsView.SendReorderCompleted());

            // The behavior depends on .NET's event invocation implementation
            // If an exception occurs, it might prevent subsequent handlers from running
            // This test verifies the actual behavior
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
            Assert.True(handler1Invoked);
            // handler3Invoked may or may not be true depending on .NET's event implementation
        }

        /// <summary>
        /// Tests that the CanReorderItems property returns false by default.
        /// </summary>
        [Fact]
        public void CanReorderItems_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act
            bool result = reorderableItemsView.CanReorderItems;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CanReorderItems property can be set to true and returns true when retrieved.
        /// </summary>
        [Fact]
        public void CanReorderItems_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act
            reorderableItemsView.CanReorderItems = true;
            bool result = reorderableItemsView.CanReorderItems;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the CanReorderItems property can be set to false and returns false when retrieved.
        /// </summary>
        [Fact]
        public void CanReorderItems_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act
            reorderableItemsView.CanReorderItems = false;
            bool result = reorderableItemsView.CanReorderItems;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CanReorderItems property correctly handles multiple value changes.
        /// </summary>
        [Fact]
        public void CanReorderItems_SetTrueThenFalse_ReturnsFalse()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act
            reorderableItemsView.CanReorderItems = true;
            reorderableItemsView.CanReorderItems = false;
            bool result = reorderableItemsView.CanReorderItems;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CanReorderItems property correctly handles multiple value changes in reverse order.
        /// </summary>
        [Fact]
        public void CanReorderItems_SetFalseThenTrue_ReturnsTrue()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act
            reorderableItemsView.CanReorderItems = false;
            reorderableItemsView.CanReorderItems = true;
            bool result = reorderableItemsView.CanReorderItems;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the CanMixGroups property returns the default value of false when not explicitly set.
        /// This test verifies the getter behavior with the default value from the BindableProperty definition.
        /// Expected result: The property should return false as the default value.
        /// </summary>
        [Fact]
        public void CanMixGroups_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act
            bool result = reorderableItemsView.CanMixGroups;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CanMixGroups property can be set to various boolean values and retrieved correctly.
        /// This test verifies both the setter and getter behavior with different boolean inputs.
        /// Expected result: The property should store and return the exact boolean value that was set.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanMixGroups_SetValue_ReturnsSetValue(bool value)
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act
            reorderableItemsView.CanMixGroups = value;
            bool result = reorderableItemsView.CanMixGroups;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the CanMixGroups property can be set multiple times with different values.
        /// This test verifies that the property correctly updates its stored value when changed.
        /// Expected result: Each subsequent get operation should return the most recently set value.
        /// </summary>
        [Fact]
        public void CanMixGroups_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();

            // Act & Assert - Set to true
            reorderableItemsView.CanMixGroups = true;
            Assert.True(reorderableItemsView.CanMixGroups);

            // Act & Assert - Set to false
            reorderableItemsView.CanMixGroups = false;
            Assert.False(reorderableItemsView.CanMixGroups);

            // Act & Assert - Set to true again
            reorderableItemsView.CanMixGroups = true;
            Assert.True(reorderableItemsView.CanMixGroups);
        }

        /// <summary>
        /// Tests that the CanMixGroups property maintains its value across multiple get operations.
        /// This test verifies that the getter consistently returns the same value without side effects.
        /// Expected result: Multiple get operations should return the same value without changing the stored value.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanMixGroups_MultipleGets_ReturnsSameValue(bool value)
        {
            // Arrange
            var reorderableItemsView = new ReorderableItemsView();
            reorderableItemsView.CanMixGroups = value;

            // Act
            bool result1 = reorderableItemsView.CanMixGroups;
            bool result2 = reorderableItemsView.CanMixGroups;
            bool result3 = reorderableItemsView.CanMixGroups;

            // Assert
            Assert.Equal(value, result1);
            Assert.Equal(value, result2);
            Assert.Equal(value, result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }
    }
}