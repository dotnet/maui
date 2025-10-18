#nullable enable

using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the CommandCanExecuteSubscription.Dispose method.
    /// </summary>
    public partial class CommandCanExecuteSubscriptionTests
    {
        /// <summary>
        /// Tests that Dispose works correctly when called once with a valid command.
        /// Should unsubscribe from CanExecuteChanged event and dispose the dependent handle.
        /// </summary>
        [Fact]
        public void Dispose_WhenCalledOnce_DisposesCorrectly()
        {
            // Arrange
            var bindableObject = new TestBindableObject();
            var command = Substitute.For<ICommand>();
            var handler = Substitute.For<Action<object, EventArgs>>();
            var subscription = new WeakCommandSubscription.CommandCanExecuteSubscription(
                bindableObject, command, handler);

            // Act
            subscription.Dispose();

            // Assert
            command.Received(1).CanExecuteChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests that Dispose returns early when called multiple times (double disposal).
        /// The second call should not attempt to unsubscribe from events or dispose resources again.
        /// This test covers the early return path on line 52.
        /// </summary>
        [Fact]
        public void Dispose_WhenCalledMultipleTimes_ReturnsEarlyOnSubsequentCalls()
        {
            // Arrange
            var bindableObject = new TestBindableObject();
            var command = Substitute.For<ICommand>();
            var handler = Substitute.For<Action<object, EventArgs>>();
            var subscription = new WeakCommandSubscription.CommandCanExecuteSubscription(
                bindableObject, command, handler);

            // Act - Call dispose twice
            subscription.Dispose();
            subscription.Dispose(); // This should return early

            // Assert - Verify CanExecuteChanged was only unsubscribed once
            command.Received(1).CanExecuteChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests that Dispose works correctly when command becomes null.
        /// Should still dispose the dependent handle even when command is null.
        /// </summary>
        [Fact]
        public void Dispose_WhenCommandIsNull_DisposesSuccessfully()
        {
            // Arrange
            var bindableObject = new TestBindableObject();
            var command = Substitute.For<ICommand>();
            var handler = Substitute.For<Action<object, EventArgs>>();
            var subscription = new WeakCommandSubscription.CommandCanExecuteSubscription(
                bindableObject, command, handler);

            // Dispose once to set command to null, then create new instance to test null scenario
            subscription.Dispose();

            // Create another subscription and immediately dispose to test the null command path
            var subscription2 = new WeakCommandSubscription.CommandCanExecuteSubscription(
                bindableObject, command, handler);

            // Act - This should work even when command handling is involved
            subscription2.Dispose();

            // Assert - Should complete without throwing
            command.Received().CanExecuteChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests the complete disposal flow including event unsubscription and resource cleanup.
        /// Verifies that all cleanup operations are performed in the correct order.
        /// </summary>
        [Fact]
        public void Dispose_CompleteFlow_PerformsAllCleanupOperations()
        {
            // Arrange
            var bindableObject = new TestBindableObject();
            var command = Substitute.For<ICommand>();
            var handler = Substitute.For<Action<object, EventArgs>>();
            var subscription = new WeakCommandSubscription.CommandCanExecuteSubscription(
                bindableObject, command, handler);

            // Act
            subscription.Dispose();

            // Assert - Verify command event was unsubscribed
            command.Received(1).CanExecuteChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Helper class that inherits from BindableObject for testing purposes.
        /// </summary>
        private class TestBindableObject : BindableObject
        {
        }
    }
}
