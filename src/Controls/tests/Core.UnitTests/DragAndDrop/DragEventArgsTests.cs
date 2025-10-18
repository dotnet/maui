using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class DragEventArgsTests
    {
        /// <summary>
        /// Tests GetPosition method when _getPosition is null (using public constructor).
        /// Should return null when no position function is provided.
        /// </summary>
        [Fact]
        public void GetPosition_WithNullPositionFunction_ReturnsNull()
        {
            // Arrange
            var dataPackage = Substitute.For<DataPackage>();
            var dragEventArgs = new DragEventArgs(dataPackage);
            var element = Substitute.For<Element>();

            // Act
            var result = dragEventArgs.GetPosition(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetPosition method when _getPosition is null and relativeTo is null.
        /// Should return null when no position function is provided and relativeTo is null.
        /// </summary>
        [Fact]
        public void GetPosition_WithNullPositionFunctionAndNullElement_ReturnsNull()
        {
            // Arrange
            var dataPackage = Substitute.For<DataPackage>();
            var dragEventArgs = new DragEventArgs(dataPackage);

            // Act
            var result = dragEventArgs.GetPosition(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetPosition method when _getPosition is provided and returns a valid point.
        /// Should invoke the position function and return the result.
        /// </summary>
        [Fact]
        public void GetPosition_WithValidPositionFunction_InvokesFunctionAndReturnsResult()
        {
            // Arrange
            var dataPackage = Substitute.For<DataPackage>();
            var platformArgs = Substitute.For<PlatformDragEventArgs>();
            var element = Substitute.For<Element>();
            var expectedPoint = new Point(10.5, 20.5);

            Func<IElement, Point?> positionFunction = Substitute.For<Func<IElement, Point?>>();
            positionFunction.Invoke(element).Returns(expectedPoint);

            var dragEventArgs = new DragEventArgs(dataPackage, positionFunction, platformArgs);

            // Act
            var result = dragEventArgs.GetPosition(element);

            // Assert
            Assert.Equal(expectedPoint, result);
            positionFunction.Received(1).Invoke(element);
        }

        /// <summary>
        /// Tests GetPosition method when _getPosition is provided but returns null.
        /// Should invoke the position function and return null result.
        /// </summary>
        [Fact]
        public void GetPosition_WithPositionFunctionReturningNull_InvokesFunctionAndReturnsNull()
        {
            // Arrange
            var dataPackage = Substitute.For<DataPackage>();
            var platformArgs = Substitute.For<PlatformDragEventArgs>();
            var element = Substitute.For<Element>();

            Func<IElement, Point?> positionFunction = Substitute.For<Func<IElement, Point?>>();
            positionFunction.Invoke(element).Returns((Point?)null);

            var dragEventArgs = new DragEventArgs(dataPackage, positionFunction, platformArgs);

            // Act
            var result = dragEventArgs.GetPosition(element);

            // Assert
            Assert.Null(result);
            positionFunction.Received(1).Invoke(element);
        }

        /// <summary>
        /// Tests GetPosition method when _getPosition is provided and relativeTo is null.
        /// Should invoke the position function with null parameter and return the result.
        /// </summary>
        [Fact]
        public void GetPosition_WithPositionFunctionAndNullElement_InvokesFunctionWithNull()
        {
            // Arrange
            var dataPackage = Substitute.For<DataPackage>();
            var platformArgs = Substitute.For<PlatformDragEventArgs>();
            var expectedPoint = new Point(100.0, 200.0);

            Func<IElement, Point?> positionFunction = Substitute.For<Func<IElement, Point?>>();
            positionFunction.Invoke(null).Returns(expectedPoint);

            var dragEventArgs = new DragEventArgs(dataPackage, positionFunction, platformArgs);

            // Act
            var result = dragEventArgs.GetPosition(null);

            // Assert
            Assert.Equal(expectedPoint, result);
            positionFunction.Received(1).Invoke(null);
        }

        /// <summary>
        /// Tests GetPosition method with boundary values for Point coordinates.
        /// Should handle extreme coordinate values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(0.0, 0.0)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void GetPosition_WithBoundaryPointValues_HandlesExtremeValues(double x, double y)
        {
            // Arrange
            var dataPackage = Substitute.For<DataPackage>();
            var platformArgs = Substitute.For<PlatformDragEventArgs>();
            var element = Substitute.For<Element>();
            var expectedPoint = new Point(x, y);

            Func<IElement, Point?> positionFunction = Substitute.For<Func<IElement, Point?>>();
            positionFunction.Invoke(element).Returns(expectedPoint);

            var dragEventArgs = new DragEventArgs(dataPackage, positionFunction, platformArgs);

            // Act
            var result = dragEventArgs.GetPosition(element);

            // Assert
            Assert.Equal(expectedPoint, result);
            positionFunction.Received(1).Invoke(element);
        }

        /// <summary>
        /// Tests that the internal constructor correctly sets all properties when provided with valid parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var dataPackage = new DataPackage();
            var mockElement = Substitute.For<IElement>();
            var expectedPoint = new Point(10, 20);
            Func<IElement, Point?> getPosition = element => expectedPoint;
            var platformArgs = new PlatformDragEventArgs();

            // Act
            var dragEventArgs = new DragEventArgs(dataPackage, getPosition, platformArgs);

            // Assert
            Assert.Same(dataPackage, dragEventArgs.Data);
            Assert.Same(platformArgs, dragEventArgs.PlatformArgs);
            Assert.Equal(expectedPoint, dragEventArgs.GetPosition(mockElement));
        }

        /// <summary>
        /// Tests that the internal constructor correctly handles null getPosition parameter.
        /// </summary>
        [Fact]
        public void Constructor_WithNullGetPosition_SetsPropertiesCorrectly()
        {
            // Arrange
            var dataPackage = new DataPackage();
            Func<IElement, Point?> getPosition = null;
            var platformArgs = new PlatformDragEventArgs();
            var mockElement = Substitute.For<IElement>();

            // Act
            var dragEventArgs = new DragEventArgs(dataPackage, getPosition, platformArgs);

            // Assert
            Assert.Same(dataPackage, dragEventArgs.Data);
            Assert.Same(platformArgs, dragEventArgs.PlatformArgs);
            Assert.Null(dragEventArgs.GetPosition(mockElement));
        }

        /// <summary>
        /// Tests that the internal constructor throws ArgumentNullException when dataPackage is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullDataPackage_ThrowsArgumentNullException()
        {
            // Arrange
            DataPackage dataPackage = null;
            Func<IElement, Point?> getPosition = element => new Point(10, 20);
            var platformArgs = new PlatformDragEventArgs();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DragEventArgs(dataPackage, getPosition, platformArgs));
        }

        /// <summary>
        /// Tests that the internal constructor throws ArgumentNullException when platformArgs is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullPlatformArgs_ThrowsArgumentNullException()
        {
            // Arrange
            var dataPackage = new DataPackage();
            Func<IElement, Point?> getPosition = element => new Point(10, 20);
            PlatformDragEventArgs platformArgs = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DragEventArgs(dataPackage, getPosition, platformArgs));
        }

        /// <summary>
        /// Tests that the GetPosition method correctly invokes the delegate with the provided element.
        /// </summary>
        [Fact]
        public void Constructor_GetPositionDelegateInvoked_PassesCorrectElement()
        {
            // Arrange
            var dataPackage = new DataPackage();
            var platformArgs = new PlatformDragEventArgs();
            var mockElement = Substitute.For<IElement>();
            var expectedPoint = new Point(50, 75);
            IElement capturedElement = null;

            Func<IElement, Point?> getPosition = element =>
            {
                capturedElement = element;
                return expectedPoint;
            };

            var dragEventArgs = new DragEventArgs(dataPackage, getPosition, platformArgs);

            // Act
            var result = dragEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Same(mockElement, capturedElement);
        }

        /// <summary>
        /// Tests that the GetPosition method returns null when getPosition delegate returns null.
        /// </summary>
        [Fact]
        public void Constructor_GetPositionDelegateReturnsNull_ReturnsNull()
        {
            // Arrange
            var dataPackage = new DataPackage();
            var platformArgs = new PlatformDragEventArgs();
            var mockElement = Substitute.For<IElement>();

            Func<IElement, Point?> getPosition = element => null;

            var dragEventArgs = new DragEventArgs(dataPackage, getPosition, platformArgs);

            // Act
            var result = dragEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }
    }
}