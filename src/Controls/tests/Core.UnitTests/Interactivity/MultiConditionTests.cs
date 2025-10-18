#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class MultiConditionTests
    {
        /// <summary>
        /// Tests that GetState returns true when the bindable object's aggregated state property value is true.
        /// Input condition: Valid BindableObject with aggregated state property set to true.
        /// Expected result: Returns true.
        /// </summary>
        [Fact]
        public void GetState_ValidBindableWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var multiCondition = new MultiCondition();
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(Arg.Any<BindableProperty>()).Returns(true);

            // Act
            var result = multiCondition.GetState(bindable);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetState returns false when the bindable object's aggregated state property value is false.
        /// Input condition: Valid BindableObject with aggregated state property set to false.
        /// Expected result: Returns false.
        /// </summary>
        [Fact]
        public void GetState_ValidBindableWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var multiCondition = new MultiCondition();
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(Arg.Any<BindableProperty>()).Returns(false);

            // Act
            var result = multiCondition.GetState(bindable);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetState throws ArgumentNullException when the bindable parameter is null.
        /// Input condition: Null bindable parameter.
        /// Expected result: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void GetState_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            var multiCondition = new MultiCondition();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => multiCondition.GetState(null));
        }

        /// <summary>
        /// Tests that GetState throws InvalidCastException when GetValue returns null and cannot be cast to bool.
        /// Input condition: BindableObject.GetValue returns null.
        /// Expected result: Throws InvalidCastException or NullReferenceException.
        /// </summary>
        [Fact]
        public void GetState_GetValueReturnsNull_ThrowsException()
        {
            // Arrange
            var multiCondition = new MultiCondition();
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(Arg.Any<BindableProperty>()).Returns((object)null);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => multiCondition.GetState(bindable));
        }

        /// <summary>
        /// Tests that GetState throws InvalidCastException when GetValue returns a non-bool value.
        /// Input condition: BindableObject.GetValue returns a string value that cannot be cast to bool.
        /// Expected result: Throws InvalidCastException.
        /// </summary>
        [Fact]
        public void GetState_GetValueReturnsNonBoolValue_ThrowsInvalidCastException()
        {
            // Arrange
            var multiCondition = new MultiCondition();
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(Arg.Any<BindableProperty>()).Returns("not a bool");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => multiCondition.GetState(bindable));
        }

        /// <summary>
        /// Tests that GetState throws InvalidCastException when GetValue returns an integer value.
        /// Input condition: BindableObject.GetValue returns an integer that cannot be cast to bool.
        /// Expected result: Throws InvalidCastException.
        /// </summary>
        [Fact]
        public void GetState_GetValueReturnsInteger_ThrowsInvalidCastException()
        {
            // Arrange
            var multiCondition = new MultiCondition();
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(Arg.Any<BindableProperty>()).Returns(42);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => multiCondition.GetState(bindable));
        }
    }
}
