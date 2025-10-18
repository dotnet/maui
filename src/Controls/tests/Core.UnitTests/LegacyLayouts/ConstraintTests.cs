#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ConstraintTests
    {
        /// <summary>
        /// Tests that FromExpression throws ArgumentNullException when expression parameter is null.
        /// </summary>
        [Fact]
        public void FromExpression_NullExpression_ThrowsArgumentNullException()
        {
            // Arrange
            Expression<Func<double>> expression = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Constraint.FromExpression(expression));
        }

        /// <summary>
        /// Tests that FromExpression creates a constraint with correct measure function for simple constant expression.
        /// </summary>
        [Fact]
        public void FromExpression_SimpleConstantExpression_CreatesConstraintWithCorrectMeasureFunction()
        {
            // Arrange
            var mockExpressionSearch = Substitute.For<IExpressionSearch>();
            mockExpressionSearch.FindObjects<View>(Arg.Any<Expression>()).Returns(new List<View>());
            var originalExpressionSearch = ExpressionSearch.Default;
            ExpressionSearch.Default = mockExpressionSearch;

            try
            {
                Expression<Func<double>> expression = () => 42.5;
                var mockLayout = Substitute.For<RelativeLayout>();

                // Act
                var result = Constraint.FromExpression(expression);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(42.5, result.Compute(mockLayout));
                mockExpressionSearch.Received(1).FindObjects<View>(expression);
            }
            finally
            {
                ExpressionSearch.Default = originalExpressionSearch;
            }
        }

        /// <summary>
        /// Tests that FromExpression creates a constraint with correct measure function for expression with calculations.
        /// </summary>
        [Fact]
        public void FromExpression_ExpressionWithCalculations_CreatesConstraintWithCorrectMeasureFunction()
        {
            // Arrange
            var mockExpressionSearch = Substitute.For<IExpressionSearch>();
            mockExpressionSearch.FindObjects<View>(Arg.Any<Expression>()).Returns(new List<View>());
            var originalExpressionSearch = ExpressionSearch.Default;
            ExpressionSearch.Default = mockExpressionSearch;

            try
            {
                var baseValue = 10.0;
                Expression<Func<double>> expression = () => baseValue * 2 + 5;
                var mockLayout = Substitute.For<RelativeLayout>();

                // Act
                var result = Constraint.FromExpression(expression);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(25.0, result.Compute(mockLayout));
                mockExpressionSearch.Received(1).FindObjects<View>(expression);
            }
            finally
            {
                ExpressionSearch.Default = originalExpressionSearch;
            }
        }

        /// <summary>
        /// Tests that FromExpression sets RelativeTo property from ExpressionSearch results.
        /// </summary>
        [Fact]
        public void FromExpression_WithViewReferences_SetsRelativeToProperty()
        {
            // Arrange
            var mockExpressionSearch = Substitute.For<IExpressionSearch>();
            var mockView1 = Substitute.For<View>();
            var mockView2 = Substitute.For<View>();
            var viewList = new List<View> { mockView1, mockView2 };
            mockExpressionSearch.FindObjects<View>(Arg.Any<Expression>()).Returns(viewList);
            var originalExpressionSearch = ExpressionSearch.Default;
            ExpressionSearch.Default = mockExpressionSearch;

            try
            {
                Expression<Func<double>> expression = () => 100.0;

                // Act
                var result = Constraint.FromExpression(expression);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.RelativeTo);
                var relativeToArray = result.RelativeTo.ToArray();
                Assert.Equal(2, relativeToArray.Length);
                Assert.Contains(mockView1, relativeToArray);
                Assert.Contains(mockView2, relativeToArray);
                mockExpressionSearch.Received(1).FindObjects<View>(expression);
            }
            finally
            {
                ExpressionSearch.Default = originalExpressionSearch;
            }
        }

        /// <summary>
        /// Tests that FromExpression creates constraint with empty RelativeTo when no views found.
        /// </summary>
        [Fact]
        public void FromExpression_NoViewReferences_SetsEmptyRelativeToProperty()
        {
            // Arrange
            var mockExpressionSearch = Substitute.For<IExpressionSearch>();
            mockExpressionSearch.FindObjects<View>(Arg.Any<Expression>()).Returns(new List<View>());
            var originalExpressionSearch = ExpressionSearch.Default;
            ExpressionSearch.Default = mockExpressionSearch;

            try
            {
                Expression<Func<double>> expression = () => 75.0;

                // Act
                var result = Constraint.FromExpression(expression);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.RelativeTo);
                Assert.Empty(result.RelativeTo);
                mockExpressionSearch.Received(1).FindObjects<View>(expression);
            }
            finally
            {
                ExpressionSearch.Default = originalExpressionSearch;
            }
        }

        /// <summary>
        /// Tests that FromExpression handles expressions with special double values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        [InlineData(0.0)]
        [InlineData(-0.0)]
        public void FromExpression_SpecialDoubleValues_HandlesCorrectly(double expectedValue)
        {
            // Arrange
            var mockExpressionSearch = Substitute.For<IExpressionSearch>();
            mockExpressionSearch.FindObjects<View>(Arg.Any<Expression>()).Returns(new List<View>());
            var originalExpressionSearch = ExpressionSearch.Default;
            ExpressionSearch.Default = mockExpressionSearch;

            try
            {
                Expression<Func<double>> expression = () => expectedValue;
                var mockLayout = Substitute.For<RelativeLayout>();

                // Act
                var result = Constraint.FromExpression(expression);

                // Assert
                Assert.NotNull(result);
                var actualValue = result.Compute(mockLayout);

                if (double.IsNaN(expectedValue))
                {
                    Assert.True(double.IsNaN(actualValue));
                }
                else
                {
                    Assert.Equal(expectedValue, actualValue);
                }
            }
            finally
            {
                ExpressionSearch.Default = originalExpressionSearch;
            }
        }

        /// <summary>
        /// Tests that FromExpression creates a new constraint instance each time called.
        /// </summary>
        [Fact]
        public void FromExpression_MultipleCalls_CreatesDistinctInstances()
        {
            // Arrange
            var mockExpressionSearch = Substitute.For<IExpressionSearch>();
            mockExpressionSearch.FindObjects<View>(Arg.Any<Expression>()).Returns(new List<View>());
            var originalExpressionSearch = ExpressionSearch.Default;
            ExpressionSearch.Default = mockExpressionSearch;

            try
            {
                Expression<Func<double>> expression = () => 50.0;

                // Act
                var result1 = Constraint.FromExpression(expression);
                var result2 = Constraint.FromExpression(expression);

                // Assert
                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.NotSame(result1, result2);
            }
            finally
            {
                ExpressionSearch.Default = originalExpressionSearch;
            }
        }
    }
}
