#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the StateTriggerBase class.
    /// </summary>
    public class StateTriggerBaseTests
    {
        /// <summary>
        /// Tests that SendDetached returns early without calling OnDetached when IsAttached is false.
        /// This test ensures the early return path is executed when the trigger is not attached.
        /// Expected result: OnDetached should not be called and IsAttached should remain false.
        /// </summary>
        [Fact]
        public void SendDetached_WhenIsAttachedIsFalse_ReturnsEarlyWithoutCallingOnDetached()
        {
            // Arrange
            var stateTrigger = Substitute.For<StateTriggerBase>();
            // IsAttached is false by default (private setter, starts as false)

            // Act
            stateTrigger.SendDetached();

            // Assert
            stateTrigger.DidNotReceive().OnDetached();
            Assert.False(stateTrigger.IsAttached);
        }

        /// <summary>
        /// Tests that SendDetached calls OnDetached and sets IsAttached to false when IsAttached is true.
        /// This test verifies the main execution path when the trigger is attached.
        /// Expected result: OnDetached should be called once and IsAttached should be set to false.
        /// </summary>
        [Fact]
        public void SendDetached_WhenIsAttachedIsTrue_CallsOnDetachedAndSetsIsAttachedToFalse()
        {
            // Arrange
            var stateTrigger = Substitute.For<StateTriggerBase>();
            // First attach to set IsAttached to true
            stateTrigger.SendAttached();
            Assert.True(stateTrigger.IsAttached); // Verify it's attached

            // Act
            stateTrigger.SendDetached();

            // Assert
            stateTrigger.Received(1).OnDetached();
            Assert.False(stateTrigger.IsAttached);
        }

        /// <summary>
        /// Tests that SendDetached can be called multiple times when IsAttached is false without side effects.
        /// This test ensures calling SendDetached multiple times when not attached is safe.
        /// Expected result: OnDetached should never be called and IsAttached should remain false.
        /// </summary>
        [Fact]
        public void SendDetached_WhenCalledMultipleTimesWhileNotAttached_DoesNotCallOnDetached()
        {
            // Arrange
            var stateTrigger = Substitute.For<StateTriggerBase>();
            Assert.False(stateTrigger.IsAttached); // Verify initial state

            // Act
            stateTrigger.SendDetached();
            stateTrigger.SendDetached();
            stateTrigger.SendDetached();

            // Assert
            stateTrigger.DidNotReceive().OnDetached();
            Assert.False(stateTrigger.IsAttached);
        }

        /// <summary>
        /// Tests that SendDetached can be called multiple times after detachment without calling OnDetached again.
        /// This test verifies that once detached, subsequent calls to SendDetached have no effect.
        /// Expected result: OnDetached should be called only once during the first detachment.
        /// </summary>
        [Fact]
        public void SendDetached_WhenCalledMultipleTimesAfterDetachment_CallsOnDetachedOnlyOnce()
        {
            // Arrange
            var stateTrigger = Substitute.For<StateTriggerBase>();
            stateTrigger.SendAttached(); // Attach first
            Assert.True(stateTrigger.IsAttached);

            // Act
            stateTrigger.SendDetached(); // First detachment
            stateTrigger.SendDetached(); // Should return early
            stateTrigger.SendDetached(); // Should return early

            // Assert
            stateTrigger.Received(1).OnDetached(); // Called only once
            Assert.False(stateTrigger.IsAttached);
        }

        /// <summary>
        /// Tests that SendAttached calls OnAttached and sets IsAttached to true when the trigger is not already attached.
        /// This verifies the normal flow when SendAttached is called for the first time.
        /// </summary>
        [Fact]
        public void SendAttached_WhenNotAttached_CallsOnAttachedAndSetsIsAttachedTrue()
        {
            // Arrange
            var stateTrigger = Substitute.For<StateTriggerBase>();

            // Act
            stateTrigger.SendAttached();

            // Assert
            stateTrigger.Received(1).OnAttached();
            Assert.True(stateTrigger.IsAttached);
        }

        /// <summary>
        /// Tests that SendAttached returns early without calling OnAttached when the trigger is already attached.
        /// This verifies the guard clause behavior and ensures OnAttached is not called multiple times.
        /// Expected behavior: OnAttached should not be called and IsAttached should remain true.
        /// </summary>
        [Fact]
        public void SendAttached_WhenAlreadyAttached_DoesNotCallOnAttachedAgain()
        {
            // Arrange
            var stateTrigger = Substitute.For<StateTriggerBase>();
            stateTrigger.SendAttached(); // First call to set IsAttached = true
            stateTrigger.ClearReceivedCalls(); // Clear call history

            // Act
            stateTrigger.SendAttached(); // Second call should return early

            // Assert
            stateTrigger.DidNotReceive().OnAttached();
            Assert.True(stateTrigger.IsAttached);
        }

        /// <summary>
        /// Tests multiple consecutive calls to SendAttached to ensure OnAttached is only called once.
        /// This verifies that the guard clause prevents multiple invocations of OnAttached.
        /// Expected behavior: OnAttached should only be called on the first invocation.
        /// </summary>
        [Fact]
        public void SendAttached_MultipleConsecutiveCalls_OnlyCallsOnAttachedOnce()
        {
            // Arrange
            var stateTrigger = Substitute.For<StateTriggerBase>();

            // Act
            stateTrigger.SendAttached();
            stateTrigger.SendAttached();
            stateTrigger.SendAttached();

            // Assert
            stateTrigger.Received(1).OnAttached();
            Assert.True(stateTrigger.IsAttached);
        }
    }
}