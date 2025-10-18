using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class DragStartingEventArgsTests
    {
        /// <summary>
        /// Tests that GetPosition returns null when _getPosition field is null (default parameterless constructor case).
        /// This should test the null-conditional operator behavior when the delegate is null.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFieldIsNull_ReturnsNull()
        {
            // Arrange
            var eventArgs = new DragStartingEventArgs();
            var mockElement = Substitute.For<Element>();

            // Act
            var result = eventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition returns null when _getPosition field is null and relativeTo parameter is null.
        /// This ensures proper null handling for both the delegate and parameter.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFieldIsNullAndRelativeToIsNull_ReturnsNull()
        {
            // Arrange
            var eventArgs = new DragStartingEventArgs();

            // Act
            var result = eventArgs.GetPosition(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition invokes the _getPosition delegate and returns its result when delegate returns a valid Point.
        /// This tests the successful execution path with a non-null delegate that returns a valid Point.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionReturnsValidPoint_ReturnsPoint()
        {
            // Arrange
            var expectedPoint = new Point(10.5, 20.7);
            Func<IElement, Point?> getPositionFunc = element => expectedPoint;
            var eventArgs = new DragStartingEventArgs(getPositionFunc, null);
            var mockElement = Substitute.For<Element>();

            // Act
            var result = eventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        /// <summary>
        /// Tests that GetPosition invokes the _getPosition delegate and returns null when delegate returns null.
        /// This tests the case where the delegate is present but returns null.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionDelegateReturnsNull_ReturnsNull()
        {
            // Arrange
            Func<IElement, Point?> getPositionFunc = element => null;
            var eventArgs = new DragStartingEventArgs(getPositionFunc, null);
            var mockElement = Substitute.For<Element>();

            // Act
            var result = eventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition correctly passes null relativeTo parameter to the delegate.
        /// This ensures the parameter is properly passed through when it's null.
        /// </summary>
        [Fact]
        public void GetPosition_WithNullRelativeTo_PassesNullToDelegate()
        {
            // Arrange
            var expectedPoint = new Point(5.0, 15.0);
            IElement passedElement = null;
            Func<IElement, Point?> getPositionFunc = element =>
            {
                passedElement = element;
                return expectedPoint;
            };
            var eventArgs = new DragStartingEventArgs(getPositionFunc, null);

            // Act
            var result = eventArgs.GetPosition(null);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Null(passedElement);
        }

        /// <summary>
        /// Tests that GetPosition correctly passes the Element parameter to the delegate.
        /// This verifies that the relativeTo parameter is properly forwarded to the delegate.
        /// </summary>
        [Fact]
        public void GetPosition_WithValidElement_PassesElementToDelegate()
        {
            // Arrange
            var expectedPoint = new Point(100.0, 200.0);
            var mockElement = Substitute.For<Element>();
            IElement passedElement = null;
            Func<IElement, Point?> getPositionFunc = element =>
            {
                passedElement = element;
                return expectedPoint;
            };
            var eventArgs = new DragStartingEventArgs(getPositionFunc, null);

            // Act
            var result = eventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Same(mockElement, passedElement);
        }

        /// <summary>
        /// Tests GetPosition with extreme coordinate values to ensure proper handling of boundary cases.
        /// This tests that the method can handle Points with extreme double values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(0.0, 0.0)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void GetPosition_WithExtremeCoordinateValues_ReturnsCorrectPoint(double x, double y)
        {
            // Arrange
            var expectedPoint = new Point(x, y);
            Func<IElement, Point?> getPositionFunc = element => expectedPoint;
            var eventArgs = new DragStartingEventArgs(getPositionFunc, null);
            var mockElement = Substitute.For<Element>();

            // Act
            var result = eventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
        }
    }
}
