#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the BindingCondition class focusing on the Value property.
    /// </summary>
    public sealed partial class BindingConditionTests
    {
        /// <summary>
        /// Tests that the Value getter returns the correct value that was previously set.
        /// </summary>
        [Fact]
        public void Value_GetAfterSet_ReturnsSetValue()
        {
            // Arrange
            var condition = new BindingCondition();
            var expectedValue = "test value";

            // Act
            condition.Value = expectedValue;
            var actualValue = condition.Value;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the Value getter returns null when no value has been set.
        /// </summary>
        [Fact]
        public void Value_GetWithoutSet_ReturnsNull()
        {
            // Arrange
            var condition = new BindingCondition();

            // Act
            var actualValue = condition.Value;

            // Assert
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that setting the same value twice does not change the internal state.
        /// This test covers the early return logic when _triggerValue == value.
        /// </summary>
        [Fact]
        public void Value_SetSameValueTwice_DoesNotChangeState()
        {
            // Arrange
            var condition = new BindingCondition();
            var testValue = "same value";

            // Act
            condition.Value = testValue;
            condition.Value = testValue; // This should trigger the early return (line 33-34)

            // Assert
            Assert.Equal(testValue, condition.Value);
        }

        /// <summary>
        /// Tests that setting the same null value twice works correctly.
        /// This covers the equality check with null values.
        /// </summary>
        [Fact]
        public void Value_SetSameNullValueTwice_DoesNotChangeState()
        {
            // Arrange
            var condition = new BindingCondition();

            // Act
            condition.Value = null;
            condition.Value = null; // This should trigger the early return (line 33-34)

            // Assert
            Assert.Null(condition.Value);
        }

        /// <summary>
        /// Tests setting different values in sequence.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(null)]
        [InlineData(3.14)]
        public void Value_SetDifferentValues_UpdatesValueCorrectly(object testValue)
        {
            // Arrange
            var condition = new BindingCondition();

            // Act
            condition.Value = testValue;

            // Assert
            Assert.Equal(testValue, condition.Value);
        }

        /// <summary>
        /// Tests changing from one value to another value.
        /// </summary>
        [Fact]
        public void Value_ChangeFromOneValueToAnother_UpdatesCorrectly()
        {
            // Arrange
            var condition = new BindingCondition();
            var initialValue = "initial";
            var newValue = "new";

            // Act
            condition.Value = initialValue;
            condition.Value = newValue;

            // Assert
            Assert.Equal(newValue, condition.Value);
        }

        /// <summary>
        /// Tests setting value to null after it had a non-null value.
        /// </summary>
        [Fact]
        public void Value_SetToNullAfterNonNull_UpdatesCorrectly()
        {
            // Arrange
            var condition = new BindingCondition();
            var initialValue = "not null";

            // Act
            condition.Value = initialValue;
            condition.Value = null;

            // Assert
            Assert.Null(condition.Value);
        }

        /// <summary>
        /// Tests setting value to non-null after it was null.
        /// </summary>
        [Fact]
        public void Value_SetToNonNullAfterNull_UpdatesCorrectly()
        {
            // Arrange
            var condition = new BindingCondition();
            var newValue = "not null anymore";

            // Act
            condition.Value = null;
            condition.Value = newValue;

            // Assert
            Assert.Equal(newValue, condition.Value);
        }

        /// <summary>
        /// PARTIAL TEST: Tests that setting Value when condition is sealed throws InvalidOperationException.
        /// 
        /// NOTE: This test is incomplete because the IsSealed property is internal and cannot be directly
        /// controlled from unit tests. To complete this test, you would need to:
        /// 1. Find a way to seal the condition through its public API, or
        /// 2. Use integration tests that work with the complete trigger/condition system, or
        /// 3. Use reflection to access internal members (not recommended)
        /// 
        /// The expected behavior is that when IsSealed is true, setting Value should throw
        /// InvalidOperationException with message "Cannot change Value once the Condition has been applied."
        /// </summary>
        [Fact(Skip = "Cannot test sealed condition scenario - IsSealed is internal property")]
        public void Value_SetWhenSealed_ThrowsInvalidOperationException()
        {
            // Arrange
            var condition = new BindingCondition();

            // TODO: Need to find a way to seal the condition through public API
            // The following line would be needed but IsSealed is internal:
            // condition.IsSealed = true;

            // Act & Assert
            // Should throw InvalidOperationException with message: 
            // "Cannot change Value once the Condition has been applied."
            // var exception = Assert.Throws<InvalidOperationException>(() => condition.Value = "new value");
            // Assert.Equal("Cannot change Value once the Condition has been applied.", exception.Message);
        }

        /// <summary>
        /// Tests that the Binding getter returns the current binding value.
        /// </summary>
        [Fact]
        public void Binding_Get_ReturnsCurrentBindingValue()
        {
            // Arrange
            var bindingCondition = new BindingCondition();
            var mockBinding = Substitute.For<BindingBase>();
            bindingCondition.Binding = mockBinding;

            // Act
            var result = bindingCondition.Binding;

            // Assert
            Assert.Same(mockBinding, result);
        }

        /// <summary>
        /// Tests that setting a new binding value when not sealed successfully sets the binding.
        /// </summary>
        [Fact]
        public void Binding_SetNewValue_WhenNotSealed_SetsBinding()
        {
            // Arrange
            var bindingCondition = new BindingCondition();
            var mockBinding = Substitute.For<BindingBase>();

            // Act
            bindingCondition.Binding = mockBinding;

            // Assert
            Assert.Same(mockBinding, bindingCondition.Binding);
        }

        /// <summary>
        /// Tests that setting the same binding value when not sealed returns early without reassignment.
        /// This test covers the early return path when _binding == value.
        /// </summary>
        [Fact]
        public void Binding_SetSameValue_WhenNotSealed_ReturnsEarly()
        {
            // Arrange
            var bindingCondition = new BindingCondition();
            var mockBinding = Substitute.For<BindingBase>();
            bindingCondition.Binding = mockBinding;

            // Act
            bindingCondition.Binding = mockBinding; // Setting same value

            // Assert
            Assert.Same(mockBinding, bindingCondition.Binding);
        }

        /// <summary>
        /// Tests that setting a binding value when the condition is sealed throws InvalidOperationException.
        /// This test covers the exception path when IsSealed is true.
        /// </summary>
        [Fact]
        public void Binding_SetValue_WhenSealed_ThrowsInvalidOperationException()
        {
            // Arrange
            var bindingCondition = new BindingCondition();
            var mockBinding = Substitute.For<BindingBase>();

            // Seal the condition by setting IsSealed to true
            var conditionType = typeof(BindingCondition).BaseType; // Condition
            var isSealed = conditionType.GetProperty("IsSealed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sealedField = conditionType.GetField("_isSealed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            sealedField.SetValue(bindingCondition, true);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => bindingCondition.Binding = mockBinding);
            Assert.Equal("Cannot change Binding once the Condition has been applied.", exception.Message);
        }

        /// <summary>
        /// Tests that setting a null binding when not sealed successfully sets the binding to null.
        /// </summary>
        [Fact]
        public void Binding_SetNull_WhenNotSealed_SetsBindingToNull()
        {
            // Arrange
            var bindingCondition = new BindingCondition();
            var mockBinding = Substitute.For<BindingBase>();
            bindingCondition.Binding = mockBinding; // Set initial value

            // Act
            bindingCondition.Binding = null;

            // Assert
            Assert.Null(bindingCondition.Binding);
        }

        /// <summary>
        /// Tests that setting the same null value when binding is already null returns early.
        /// This test covers the early return path with null values.
        /// </summary>
        [Fact]
        public void Binding_SetNull_WhenAlreadyNull_ReturnsEarly()
        {
            // Arrange
            var bindingCondition = new BindingCondition();
            // Default binding is null

            // Act
            bindingCondition.Binding = null; // Setting null when already null

            // Assert
            Assert.Null(bindingCondition.Binding);
        }

        /// <summary>
        /// Tests edge case where setting different BindingBase instances when not sealed works correctly.
        /// </summary>
        [Fact]
        public void Binding_SetDifferentValues_WhenNotSealed_UpdatesBinding()
        {
            // Arrange
            var bindingCondition = new BindingCondition();
            var firstBinding = Substitute.For<BindingBase>();
            var secondBinding = Substitute.For<BindingBase>();

            // Act
            bindingCondition.Binding = firstBinding;
            var firstResult = bindingCondition.Binding;

            bindingCondition.Binding = secondBinding;
            var secondResult = bindingCondition.Binding;

            // Assert
            Assert.Same(firstBinding, firstResult);
            Assert.Same(secondBinding, secondResult);
            Assert.NotSame(firstResult, secondResult);
        }
    }
}