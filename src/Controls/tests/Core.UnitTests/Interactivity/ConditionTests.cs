#nullable disable

using System;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ConditionTests
    {
        /// <summary>
        /// Tests that the IsSealed property returns false by default when a new Condition instance is created.
        /// </summary>
        [Fact]
        public void IsSealed_InitialValue_ReturnsFalse()
        {
            // Arrange
            var condition = Substitute.For<Condition>();

            // Act
            var result = condition.IsSealed;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that setting IsSealed to true when it's false sets the value and calls OnSealed.
        /// </summary>
        [Fact]
        public void IsSealed_SetToTrue_SetsValueAndCallsOnSealed()
        {
            // Arrange
            var condition = Substitute.For<Condition>();

            // Act
            condition.IsSealed = true;

            // Assert
            Assert.True(condition.IsSealed);
            condition.Received(1).OnSealed();
        }

        /// <summary>
        /// Tests that setting IsSealed to true when it's already true does not call OnSealed (early return scenario).
        /// This test covers the uncovered line 27 where _isSealed == value.
        /// </summary>
        [Fact]
        public void IsSealed_SetToTrueWhenAlreadyTrue_DoesNotCallOnSealed()
        {
            // Arrange
            var condition = Substitute.For<Condition>();
            condition.IsSealed = true;
            condition.ClearReceivedCalls(); // Clear the OnSealed call from the setup

            // Act
            condition.IsSealed = true;

            // Assert
            Assert.True(condition.IsSealed);
            condition.DidNotReceive().OnSealed();
        }

        /// <summary>
        /// Tests that setting IsSealed to false when it's already false does not call OnSealed (early return scenario).
        /// This test covers the uncovered line 27 where _isSealed == value.
        /// </summary>
        [Fact]
        public void IsSealed_SetToFalseWhenAlreadyFalse_DoesNotCallOnSealed()
        {
            // Arrange
            var condition = Substitute.For<Condition>();
            // IsSealed is false by default

            // Act
            condition.IsSealed = false;

            // Assert
            Assert.False(condition.IsSealed);
            condition.DidNotReceive().OnSealed();
        }

        /// <summary>
        /// Tests that setting IsSealed to false when it's true throws InvalidOperationException with the correct message.
        /// This test covers the uncovered line 29 where !value is true (attempting to unseal).
        /// </summary>
        [Fact]
        public void IsSealed_SetToFalseWhenTrue_ThrowsInvalidOperationException()
        {
            // Arrange
            var condition = Substitute.For<Condition>();
            condition.IsSealed = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => condition.IsSealed = false);
            Assert.Equal("What is sealed cannot be unsealed.", exception.Message);
        }

        /// <summary>
        /// Tests that getting ConditionChanged returns the current value of the internal field.
        /// </summary>
        [Fact]
        public void ConditionChanged_Get_ReturnsCurrentValue()
        {
            // Arrange
            var condition = new TestableCondition();
            Action<BindableObject, bool, bool> expectedAction = (bindable, arg1, arg2) => { };

            // Act & Assert - Initial state should be null
            Assert.Null(condition.ConditionChanged);

            // Arrange - Set a value
            condition.ConditionChanged = expectedAction;

            // Act & Assert - Should return the set value
            Assert.Equal(expectedAction, condition.ConditionChanged);
        }

        /// <summary>
        /// Tests that setting ConditionChanged to the same value returns early without throwing an exception.
        /// This tests the early return condition on line 22-23.
        /// </summary>
        [Fact]
        public void ConditionChanged_SetSameValue_ReturnsEarlyWithoutException()
        {
            // Arrange
            var condition = new TestableCondition();
            Action<BindableObject, bool, bool> action = (bindable, arg1, arg2) => { };
            condition.ConditionChanged = action;

            // Act & Assert - Setting the same value should not throw
            condition.ConditionChanged = action;
            Assert.Equal(action, condition.ConditionChanged);
        }

        /// <summary>
        /// Tests that setting ConditionChanged to null when it's already null succeeds.
        /// </summary>
        [Fact]
        public void ConditionChanged_SetNullWhenNull_Succeeds()
        {
            // Arrange
            var condition = new TestableCondition();

            // Act & Assert - Setting null when already null should succeed
            condition.ConditionChanged = null;
            Assert.Null(condition.ConditionChanged);
        }

        /// <summary>
        /// Tests that setting ConditionChanged to null when it's already null (setting same value) returns early.
        /// This tests the early return condition on line 22-23 with null values.
        /// </summary>
        [Fact]
        public void ConditionChanged_SetNullWhenAlreadyNull_ReturnsEarly()
        {
            // Arrange
            var condition = new TestableCondition();
            condition.ConditionChanged = null; // Explicitly set to null first

            // Act & Assert - Setting null again should return early without exception
            condition.ConditionChanged = null;
            Assert.Null(condition.ConditionChanged);
        }

        /// <summary>
        /// Tests that setting ConditionChanged to a valid action when it's currently null succeeds.
        /// </summary>
        [Fact]
        public void ConditionChanged_SetValidActionWhenNull_Succeeds()
        {
            // Arrange
            var condition = new TestableCondition();
            Action<BindableObject, bool, bool> action = (bindable, arg1, arg2) => { };

            // Act
            condition.ConditionChanged = action;

            // Assert
            Assert.Equal(action, condition.ConditionChanged);
        }

        /// <summary>
        /// Tests that setting ConditionChanged to a different non-null value when it's already set throws InvalidOperationException.
        /// This tests the exception condition on line 24-25.
        /// </summary>
        [Fact]
        public void ConditionChanged_SetDifferentValueWhenAlreadySet_ThrowsInvalidOperationException()
        {
            // Arrange
            var condition = new TestableCondition();
            Action<BindableObject, bool, bool> firstAction = (bindable, arg1, arg2) => { };
            Action<BindableObject, bool, bool> secondAction = (bindable, arg1, arg2) => { };
            condition.ConditionChanged = firstAction;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => condition.ConditionChanged = secondAction);
            Assert.Equal("The same condition instance cannot be reused", exception.Message);
            Assert.Equal(firstAction, condition.ConditionChanged); // Original value should remain
        }

        /// <summary>
        /// Tests that setting ConditionChanged to null when it's already set to a non-null value throws InvalidOperationException.
        /// This tests the exception condition on line 24-25 when trying to set null after a non-null value.
        /// </summary>
        [Fact]
        public void ConditionChanged_SetNullWhenAlreadySetToNonNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var condition = new TestableCondition();
            Action<BindableObject, bool, bool> action = (bindable, arg1, arg2) => { };
            condition.ConditionChanged = action;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => condition.ConditionChanged = null);
            Assert.Equal("The same condition instance cannot be reused", exception.Message);
            Assert.Equal(action, condition.ConditionChanged); // Original value should remain
        }

        /// <summary>
        /// Concrete implementation of Condition for testing purposes.
        /// </summary>
        private class TestableCondition : Condition
        {
            internal override bool GetState(BindableObject bindable)
            {
                return false;
            }

            internal override void SetUp(BindableObject bindable)
            {
            }

            internal override void TearDown(BindableObject bindable)
            {
            }
        }
    }
}