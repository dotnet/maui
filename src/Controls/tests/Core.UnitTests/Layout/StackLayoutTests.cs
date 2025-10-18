#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class StackLayoutTests
    {
        /// <summary>
        /// Tests that ComputeConstraintForView returns VerticallyFixed when orientation is horizontal,
        /// constraint has VerticallyFixed flag, and view's vertical alignment is Fill.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_HorizontalOrientationWithVerticallyFixedAndFillAlignment_ReturnsVerticallyFixed()
        {
            // Arrange
            var testStackLayout = new TestableStackLayout
            {
                TestOrientation = StackOrientation.Horizontal,
                TestConstraint = LayoutConstraint.VerticallyFixed
            };
            var view = new View
            {
                VerticalOptions = new LayoutOptions(LayoutAlignment.Fill, false)
            };

            // Act
            var result = testStackLayout.CallComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.VerticallyFixed, result);
        }

        /// <summary>
        /// Tests that ComputeConstraintForView returns HorizontallyFixed when orientation is vertical,
        /// constraint has HorizontallyFixed flag, and view's horizontal alignment is Fill.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_VerticalOrientationWithHorizontallyFixedAndFillAlignment_ReturnsHorizontallyFixed()
        {
            // Arrange
            var testStackLayout = new TestableStackLayout
            {
                TestOrientation = StackOrientation.Vertical,
                TestConstraint = LayoutConstraint.HorizontallyFixed
            };
            var view = new View
            {
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, false)
            };

            // Act
            var result = testStackLayout.CallComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.HorizontallyFixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView with various combinations of orientation, constraint, and alignment
        /// to ensure None is returned when conditions for specific constraints are not met.
        /// </summary>
        [Theory]
        [InlineData(StackOrientation.Horizontal, LayoutConstraint.VerticallyFixed, LayoutAlignment.Start, LayoutConstraint.None)]
        [InlineData(StackOrientation.Horizontal, LayoutConstraint.VerticallyFixed, LayoutAlignment.Center, LayoutConstraint.None)]
        [InlineData(StackOrientation.Horizontal, LayoutConstraint.VerticallyFixed, LayoutAlignment.End, LayoutConstraint.None)]
        [InlineData(StackOrientation.Horizontal, LayoutConstraint.None, LayoutAlignment.Fill, LayoutConstraint.None)]
        [InlineData(StackOrientation.Horizontal, LayoutConstraint.HorizontallyFixed, LayoutAlignment.Fill, LayoutConstraint.None)]
        [InlineData(StackOrientation.Vertical, LayoutConstraint.HorizontallyFixed, LayoutAlignment.Start, LayoutConstraint.None)]
        [InlineData(StackOrientation.Vertical, LayoutConstraint.HorizontallyFixed, LayoutAlignment.Center, LayoutConstraint.None)]
        [InlineData(StackOrientation.Vertical, LayoutConstraint.HorizontallyFixed, LayoutAlignment.End, LayoutConstraint.None)]
        [InlineData(StackOrientation.Vertical, LayoutConstraint.None, LayoutAlignment.Fill, LayoutConstraint.None)]
        [InlineData(StackOrientation.Vertical, LayoutConstraint.VerticallyFixed, LayoutAlignment.Fill, LayoutConstraint.None)]
        public void ComputeConstraintForView_VariousCombinations_ReturnsExpectedResult(
            StackOrientation orientation,
            LayoutConstraint constraint,
            LayoutAlignment alignment,
            LayoutConstraint expected)
        {
            // Arrange
            var testStackLayout = new TestableStackLayout
            {
                TestOrientation = orientation,
                TestConstraint = constraint
            };
            var view = new View();

            if (orientation == StackOrientation.Horizontal)
            {
                view.VerticalOptions = new LayoutOptions(alignment, false);
            }
            else
            {
                view.HorizontalOptions = new LayoutOptions(alignment, false);
            }

            // Act
            var result = testStackLayout.CallComputeConstraintForView(view);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView with combined constraint flags to ensure correct bitwise operations.
        /// </summary>
        [Theory]
        [InlineData(StackOrientation.Horizontal, LayoutConstraint.Fixed, LayoutAlignment.Fill, LayoutConstraint.VerticallyFixed)]
        [InlineData(StackOrientation.Vertical, LayoutConstraint.Fixed, LayoutAlignment.Fill, LayoutConstraint.HorizontallyFixed)]
        public void ComputeConstraintForView_CombinedConstraintFlags_ReturnsExpectedResult(
            StackOrientation orientation,
            LayoutConstraint constraint,
            LayoutAlignment alignment,
            LayoutConstraint expected)
        {
            // Arrange
            var testStackLayout = new TestableStackLayout
            {
                TestOrientation = orientation,
                TestConstraint = constraint
            };
            var view = new View();

            if (orientation == StackOrientation.Horizontal)
            {
                view.VerticalOptions = new LayoutOptions(alignment, false);
            }
            else
            {
                view.HorizontalOptions = new LayoutOptions(alignment, false);
            }

            // Act
            var result = testStackLayout.CallComputeConstraintForView(view);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Helper class to expose the protected ComputeConstraintForView method and allow controlling
        /// the Orientation and Constraint properties for testing purposes.
        /// </summary>
        private class TestableStackLayout : StackLayout
        {
            public StackOrientation TestOrientation { get; set; } = StackOrientation.Vertical;
            public LayoutConstraint TestConstraint { get; set; } = LayoutConstraint.None;

            public new StackOrientation Orientation
            {
                get => TestOrientation;
                set => TestOrientation = value;
            }

            public new LayoutConstraint Constraint => TestConstraint;

            public LayoutConstraint CallComputeConstraintForView(View view)
            {
                return ComputeConstraintForView(view);
            }
        }
    }
}
