#nullable disable
//
// Tweener.cs
//
// Author:
//       Jason Smith <jason.smith@xamarin.com>
//
// Copyright (c) 2012 Microsoft.Maui.Controls Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for AnimationExtensions class.
    /// </summary>
    public class AnimationExtensionsTests
    {
        /// <summary>
        /// Tests that TweenersCounter property returns a non-negative integer value.
        /// This verifies the basic functionality and type safety of the property.
        /// Expected result: Returns an integer value that is greater than or equal to zero.
        /// </summary>
        [Fact]
        public void TweenersCounter_AccessProperty_ReturnsNonNegativeInteger()
        {
            // Act
            int result = AnimationExtensions.TweenersCounter;

            // Assert
            Assert.True(result >= 0, "TweenersCounter should return a non-negative value");
            Assert.IsType<int>(result);
        }

        /// <summary>
        /// Tests that TweenersCounter property can be accessed multiple times consistently.
        /// This verifies the property is stable and doesn't throw exceptions on repeated access.
        /// Expected result: Multiple calls return consistent integer values without exceptions.
        /// </summary>
        [Fact]
        public void TweenersCounter_MultipleAccess_ReturnsConsistentValues()
        {
            // Act
            int firstCall = AnimationExtensions.TweenersCounter;
            int secondCall = AnimationExtensions.TweenersCounter;

            // Assert
            Assert.Equal(firstCall, secondCall);
            Assert.True(firstCall >= 0);
            Assert.True(secondCall >= 0);
        }

        /// <summary>
        /// Tests that TweenersCounter property returns the correct type.
        /// This verifies the property contract and ensures type safety.
        /// Expected result: Returns exactly an int type.
        /// </summary>
        [Fact]
        public void TweenersCounter_TypeVerification_ReturnsIntType()
        {
            // Act
            var result = AnimationExtensions.TweenersCounter;

            // Assert
            Assert.IsType<int>(result);
        }

        /// <summary>
        /// Tests that AnimationIsRunning throws ArgumentNullException when self parameter is null.
        /// Validates that the method properly delegates parameter validation to AnimatableKey constructor.
        /// </summary>
        [Fact]
        public void AnimationIsRunning_NullSelf_ThrowsArgumentNullException()
        {
            // Arrange
            IAnimatable animatable = null;
            string handle = "testHandle";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AnimationExtensions.AnimationIsRunning(animatable, handle));
        }

        /// <summary>
        /// Tests that AnimationIsRunning throws ArgumentException when handle parameter is null.
        /// Validates that the method properly delegates parameter validation to AnimatableKey constructor.
        /// </summary>
        [Fact]
        public void AnimationIsRunning_NullHandle_ThrowsArgumentException()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => AnimationExtensions.AnimationIsRunning(animatable, handle));
        }

        /// <summary>
        /// Tests that AnimationIsRunning throws ArgumentException when handle parameter is empty string.
        /// Validates that the method properly delegates parameter validation to AnimatableKey constructor.
        /// </summary>
        [Fact]
        public void AnimationIsRunning_EmptyHandle_ThrowsArgumentException()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => AnimationExtensions.AnimationIsRunning(animatable, handle));
        }

        /// <summary>
        /// Tests that AnimationIsRunning returns false when no animation is running for the specified handle.
        /// Validates the normal operation when the animation key is not present in the static animations dictionary.
        /// </summary>
        [Theory]
        [InlineData("testHandle")]
        [InlineData("animation1")]
        [InlineData("very_long_handle_name_with_special_characters_123_!@#$%^&*()")]
        [InlineData("   whitespace   ")]
        [InlineData("🎬🎭🎨")] // Unicode characters
        public void AnimationIsRunning_NoAnimationRunning_ReturnsFalse(string handle)
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();

            // Act
            bool result = AnimationExtensions.AnimationIsRunning(animatable, handle);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that AnimationIsRunning returns false for different animatable instances with the same handle.
        /// Validates that the method correctly distinguishes between different animatable objects.
        /// </summary>
        [Fact]
        public void AnimationIsRunning_DifferentAnimatablesSameHandle_ReturnsFalse()
        {
            // Arrange
            var animatable1 = Substitute.For<IAnimatable>();
            var animatable2 = Substitute.For<IAnimatable>();
            string handle = "sameHandle";

            // Act
            bool result1 = AnimationExtensions.AnimationIsRunning(animatable1, handle);
            bool result2 = AnimationExtensions.AnimationIsRunning(animatable2, handle);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
        }

        /// <summary>
        /// Tests that AnimationIsRunning returns false for same animatable instance with different handles.
        /// Validates that the method correctly handles multiple handle queries for the same animatable object.
        /// </summary>
        [Fact]
        public void AnimationIsRunning_SameAnimatableDifferentHandles_ReturnsFalse()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle1 = "handle1";
            string handle2 = "handle2";

            // Act
            bool result1 = AnimationExtensions.AnimationIsRunning(animatable, handle1);
            bool result2 = AnimationExtensions.AnimationIsRunning(animatable, handle2);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
        }

        /// <summary>
        /// Tests that AnimationIsRunning handles boundary values for string handles correctly.
        /// Validates the method behavior with edge case string values that are valid but unusual.
        /// </summary>
        [Theory]
        [InlineData("a")] // Single character
        [InlineData("1234567890")] // Numeric string
        [InlineData("Handle With Spaces")]
        [InlineData("Handle\tWith\nControl\rCharacters")]
        [InlineData("Handle_With_Underscores_And_Numbers_123")]
        public void AnimationIsRunning_BoundaryStringValues_ReturnsFalse(string handle)
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();

            // Act
            bool result = AnimationExtensions.AnimationIsRunning(animatable, handle);

            // Assert
            Assert.False(result);
        }
    }
}