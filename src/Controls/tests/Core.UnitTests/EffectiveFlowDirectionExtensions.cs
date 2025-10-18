#nullable disable

using System;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class EffectiveFlowDirectionExtensions : BaseTestFixture
    {
        [Fact]
        public void LeftToRightImplicit()
        {
            var target = FlowDirection.LeftToRight.ToEffectiveFlowDirection();

            Assert.True(target.IsLeftToRight());
            Assert.True(target.IsImplicit());

            Assert.False(target.IsRightToLeft());
            Assert.False(target.IsExplicit());
        }

        [Fact]
        public void LeftToRightExplicit()
        {
            var target = FlowDirection.LeftToRight.ToEffectiveFlowDirection(isExplicit: true);

            Assert.True(target.IsLeftToRight());
            Assert.True(target.IsExplicit());

            Assert.False(target.IsRightToLeft());
            Assert.False(target.IsImplicit());
        }

        [Fact]
        public void RightToLeftImplicit()
        {
            var target = FlowDirection.RightToLeft.ToEffectiveFlowDirection();

            Assert.True(target.IsRightToLeft());
            Assert.True(target.IsImplicit());

            Assert.False(target.IsLeftToRight());
            Assert.False(target.IsExplicit());
        }

        [Fact]
        public void RightToLeftExplicit()
        {
            var target = FlowDirection.RightToLeft.ToEffectiveFlowDirection(isExplicit: true);

            Assert.True(target.IsRightToLeft());
            Assert.True(target.IsExplicit());

            Assert.False(target.IsLeftToRight());
            Assert.False(target.IsImplicit());
        }

        /// <summary>
        /// Tests that MatchParent with implicit setting returns default EffectiveFlowDirection.
        /// Input: FlowDirection.MatchParent with isExplicit = false
        /// Expected: Returns default(EffectiveFlowDirection) which is 0
        /// </summary>
        [Fact]
        public void ToEffectiveFlowDirection_MatchParentImplicit_ReturnsDefault()
        {
            // Arrange
            var flowDirection = FlowDirection.MatchParent;

            // Act
            var result = flowDirection.ToEffectiveFlowDirection(isExplicit: false);

            // Assert
            Assert.Equal(default(EffectiveFlowDirection), result);
            Assert.True(result.IsLeftToRight());
            Assert.True(result.IsImplicit());
            Assert.False(result.IsRightToLeft());
            Assert.False(result.IsExplicit());
        }

        /// <summary>
        /// Tests that MatchParent with explicit setting still returns default EffectiveFlowDirection.
        /// Input: FlowDirection.MatchParent with isExplicit = true
        /// Expected: Returns default(EffectiveFlowDirection) which is 0, ignoring isExplicit flag
        /// </summary>
        [Fact]
        public void ToEffectiveFlowDirection_MatchParentExplicit_ReturnsDefault()
        {
            // Arrange
            var flowDirection = FlowDirection.MatchParent;

            // Act
            var result = flowDirection.ToEffectiveFlowDirection(isExplicit: true);

            // Assert
            Assert.Equal(default(EffectiveFlowDirection), result);
            Assert.True(result.IsLeftToRight());
            Assert.True(result.IsImplicit());
            Assert.False(result.IsRightToLeft());
            Assert.False(result.IsExplicit());
        }

        /// <summary>
        /// Tests that an invalid FlowDirection value throws InvalidOperationException.
        /// Input: Invalid FlowDirection value (999)
        /// Expected: Throws InvalidOperationException with descriptive message
        /// </summary>
        [Fact]
        public void ToEffectiveFlowDirection_InvalidFlowDirection_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidFlowDirection = (FlowDirection)999;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                invalidFlowDirection.ToEffectiveFlowDirection());

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("999", exception.Message);
            Assert.Contains(nameof(EffectiveFlowDirection), exception.Message);
        }

        /// <summary>
        /// Tests that another invalid FlowDirection value throws InvalidOperationException.
        /// Input: Invalid negative FlowDirection value (-1)
        /// Expected: Throws InvalidOperationException with descriptive message
        /// </summary>
        [Fact]
        public void ToEffectiveFlowDirection_NegativeInvalidFlowDirection_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidFlowDirection = (FlowDirection)(-1);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                invalidFlowDirection.ToEffectiveFlowDirection(isExplicit: true));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("-1", exception.Message);
            Assert.Contains(nameof(EffectiveFlowDirection), exception.Message);
        }

        /// <summary>
        /// Tests various combinations of valid FlowDirection values with both isExplicit settings using parameterized test.
        /// Input: Various combinations of valid FlowDirection values and isExplicit flags
        /// Expected: Returns correct EffectiveFlowDirection values according to the conversion logic
        /// </summary>
        [Theory]
        [InlineData(FlowDirection.MatchParent, false, EffectiveFlowDirection.RightToLeft & ~EffectiveFlowDirection.RightToLeft)]
        [InlineData(FlowDirection.MatchParent, true, EffectiveFlowDirection.RightToLeft & ~EffectiveFlowDirection.RightToLeft)]
        [InlineData(FlowDirection.LeftToRight, false, EffectiveFlowDirection.RightToLeft & ~EffectiveFlowDirection.RightToLeft)]
        [InlineData(FlowDirection.LeftToRight, true, EffectiveFlowDirection.Explicit)]
        [InlineData(FlowDirection.RightToLeft, false, EffectiveFlowDirection.RightToLeft)]
        [InlineData(FlowDirection.RightToLeft, true, EffectiveFlowDirection.RightToLeft | EffectiveFlowDirection.Explicit)]
        public void ToEffectiveFlowDirection_ValidInputs_ReturnsExpectedResult(FlowDirection input, bool isExplicit, EffectiveFlowDirection expected)
        {
            // Act
            var result = input.ToEffectiveFlowDirection(isExplicit);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}