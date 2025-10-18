#nullable disable
//
// TweenerTests.cs
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
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class TweenerAnimationTests
    {
        /// <summary>
        /// Tests that ForceFinish sets HasFinished to true and invokes the step function with long.MaxValue
        /// when the animation has not yet finished.
        /// </summary>
        [Fact]
        public void ForceFinish_WhenNotFinished_SetsHasFinishedToTrueAndInvokesStepWithLongMaxValue()
        {
            // Arrange
            var mockStep = Substitute.For<Func<long, bool>>();
            mockStep.Invoke(Arg.Any<long>()).Returns(true);
            var tweenerAnimation = new TweenerAnimation(mockStep);

            // Act
            tweenerAnimation.ForceFinish();

            // Assert
            Assert.True(tweenerAnimation.HasFinished);
            mockStep.Received(1).Invoke(long.MaxValue);
        }

        /// <summary>
        /// Tests that ForceFinish returns early without invoking the step function
        /// when the animation has already finished.
        /// </summary>
        [Fact]
        public void ForceFinish_WhenAlreadyFinished_DoesNotInvokeStepAgain()
        {
            // Arrange
            var mockStep = Substitute.For<Func<long, bool>>();
            mockStep.Invoke(Arg.Any<long>()).Returns(true);
            var tweenerAnimation = new TweenerAnimation(mockStep);

            // First call to set HasFinished to true
            tweenerAnimation.ForceFinish();
            mockStep.ClearReceivedCalls();

            // Act - Second call when already finished
            tweenerAnimation.ForceFinish();

            // Assert
            Assert.True(tweenerAnimation.HasFinished);
            mockStep.DidNotReceive().Invoke(Arg.Any<long>());
        }

        /// <summary>
        /// Tests that ForceFinish works correctly when the step function returns false.
        /// </summary>
        [Fact]
        public void ForceFinish_WhenStepReturnsFalse_StillSetsHasFinishedToTrue()
        {
            // Arrange
            var mockStep = Substitute.For<Func<long, bool>>();
            mockStep.Invoke(Arg.Any<long>()).Returns(false);
            var tweenerAnimation = new TweenerAnimation(mockStep);

            // Act
            tweenerAnimation.ForceFinish();

            // Assert
            Assert.True(tweenerAnimation.HasFinished);
            mockStep.Received(1).Invoke(long.MaxValue);
        }

        /// <summary>
        /// Tests that ForceFinish handles null step function gracefully.
        /// </summary>
        [Fact]
        public void ForceFinish_WithNullStep_ThrowsNullReferenceException()
        {
            // Arrange
            var tweenerAnimation = new TweenerAnimation(null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => tweenerAnimation.ForceFinish());
        }
    }
}
