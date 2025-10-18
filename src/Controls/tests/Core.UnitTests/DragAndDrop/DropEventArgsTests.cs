using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class DropEventArgsTests
    {
        /// <summary>
        /// Tests that GetPosition returns null when _getPosition field is null (using public constructor).
        /// This verifies the null-conditional operator behavior when the position function is not set.
        /// Expected result: null should be returned regardless of the relativeTo parameter.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFunctionIsNull_ReturnsNull()
        {
            // Arrange
            var dataPackageView = new DataPackageView(new DataPackage());
            var dropEventArgs = new DropEventArgs(dataPackageView);
            var mockElement = Substitute.For<Element>();

            // Act
            var result = dropEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition returns null when _getPosition field is null and relativeTo is null.
        /// This verifies the null-conditional operator behavior with null parameters.
        /// Expected result: null should be returned.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFunctionIsNullAndRelativeToIsNull_ReturnsNull()
        {
            // Arrange
            var dataPackageView = new DataPackageView(new DataPackage());
            var dropEventArgs = new DropEventArgs(dataPackageView);

            // Act
            var result = dropEventArgs.GetPosition(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition invokes the _getPosition function and returns its result when function is not null.
        /// This verifies that the function is properly called with the correct parameter.
        /// Expected result: The Point returned by the mocked function should be returned.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFunctionIsNotNull_InvokesFunctionAndReturnsResult()
        {
            // Arrange
            var expectedPoint = new Point(10.5, 20.7);
            var mockElement = Substitute.For<Element>();
            Func<IElement, Point?> getPositionFunc = element => expectedPoint;
            var dropEventArgs = new DropEventArgs(null, getPositionFunc, null);

            // Act
            var result = dropEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        /// <summary>
        /// Tests that GetPosition passes null parameter correctly to the _getPosition function.
        /// This verifies that null relativeTo parameters are properly handled by the function invocation.
        /// Expected result: The function should be called with null and return the expected result.
        /// </summary>
        [Fact]
        public void GetPosition_WhenRelativeToIsNullAndGetPositionFunctionIsNotNull_PassesNullToFunction()
        {
            // Arrange
            var expectedPoint = new Point(5.0, 15.0);
            IElement receivedElement = null;
            Func<IElement, Point?> getPositionFunc = element =>
            {
                receivedElement = element;
                return expectedPoint;
            };
            var dropEventArgs = new DropEventArgs(null, getPositionFunc, null);

            // Act
            var result = dropEventArgs.GetPosition(null);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Null(receivedElement);
        }

        /// <summary>
        /// Tests that GetPosition returns null when the _getPosition function returns null.
        /// This verifies that null return values from the function are properly propagated.
        /// Expected result: null should be returned when the function returns null.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFunctionReturnsNull_ReturnsNull()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            Func<IElement, Point?> getPositionFunc = element => null;
            var dropEventArgs = new DropEventArgs(null, getPositionFunc, null);

            // Act
            var result = dropEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition handles Point with zero coordinates correctly.
        /// This verifies edge case behavior with Point.Zero values.
        /// Expected result: Point.Zero should be returned correctly.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFunctionReturnsZeroPoint_ReturnsZeroPoint()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            Func<IElement, Point?> getPositionFunc = element => Point.Zero;
            var dropEventArgs = new DropEventArgs(null, getPositionFunc, null);

            // Act
            var result = dropEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(Point.Zero, result);
        }

        /// <summary>
        /// Tests that GetPosition handles extreme coordinate values correctly.
        /// This verifies boundary value behavior with maximum and minimum double values.
        /// Expected result: Extreme coordinate values should be handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void GetPosition_WhenGetPositionFunctionReturnsExtremeValues_ReturnsExtremeValues(double x, double y)
        {
            // Arrange
            var expectedPoint = new Point(x, y);
            var mockElement = Substitute.For<Element>();
            Func<IElement, Point?> getPositionFunc = element => expectedPoint;
            var dropEventArgs = new DropEventArgs(null, getPositionFunc, null);

            // Act
            var result = dropEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        /// <summary>
        /// Tests that GetPosition correctly passes the Element parameter to the _getPosition function.
        /// This verifies that the function receives the exact same Element instance that was passed.
        /// Expected result: The same Element instance should be passed to the function.
        /// </summary>
        [Fact]
        public void GetPosition_WhenCalledWithElement_PassesCorrectElementToFunction()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var expectedPoint = new Point(1.0, 2.0);
            IElement receivedElement = null;
            Func<IElement, Point?> getPositionFunc = element =>
            {
                receivedElement = element;
                return expectedPoint;
            };
            var dropEventArgs = new DropEventArgs(null, getPositionFunc, null);

            // Act
            var result = dropEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Same(mockElement, receivedElement);
        }

        /// <summary>
        /// Tests that the internal constructor creates a new DataPackageView when view parameter is null.
        /// Tests the specific input condition where view is null.
        /// Expected result: Data property should contain a new DataPackageView with new DataPackage.
        /// </summary>
        [Fact]
        public void InternalConstructor_NullView_CreatesNewDataPackageView()
        {
            // Arrange
            DataPackageView view = null;
            Func<IElement, Point?> getPosition = Substitute.For<Func<IElement, Point?>>();
            var platformArgs = Substitute.For<PlatformDropEventArgs>();

            // Act
            var dropEventArgs = new DropEventArgs(view, getPosition, platformArgs);

            // Assert
            Assert.NotNull(dropEventArgs.Data);
            Assert.Equal(getPosition, GetPrivateField<Func<IElement, Point?>>(dropEventArgs, "_getPosition"));
            Assert.Equal(platformArgs, dropEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor uses the provided view when view parameter is not null.
        /// Tests the specific input condition where view is a valid DataPackageView.
        /// Expected result: Data property should be the provided view instance.
        /// </summary>
        [Fact]
        public void InternalConstructor_ValidView_UsesProvidedView()
        {
            // Arrange
            var dataPackage = new DataPackage();
            var view = new DataPackageView(dataPackage);
            Func<IElement, Point?> getPosition = Substitute.For<Func<IElement, Point?>>();
            var platformArgs = Substitute.For<PlatformDropEventArgs>();

            // Act
            var dropEventArgs = new DropEventArgs(view, getPosition, platformArgs);

            // Assert
            Assert.Same(view, dropEventArgs.Data);
            Assert.Equal(getPosition, GetPrivateField<Func<IElement, Point?>>(dropEventArgs, "_getPosition"));
            Assert.Equal(platformArgs, dropEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor handles null getPosition parameter correctly.
        /// Tests the specific input condition where getPosition is null.
        /// Expected result: _getPosition field should be null.
        /// </summary>
        [Fact]
        public void InternalConstructor_NullGetPosition_AssignsNullToGetPositionField()
        {
            // Arrange
            var dataPackage = new DataPackage();
            var view = new DataPackageView(dataPackage);
            Func<IElement, Point?> getPosition = null;
            var platformArgs = Substitute.For<PlatformDropEventArgs>();

            // Act
            var dropEventArgs = new DropEventArgs(view, getPosition, platformArgs);

            // Assert
            Assert.Same(view, dropEventArgs.Data);
            Assert.Null(GetPrivateField<Func<IElement, Point?>>(dropEventArgs, "_getPosition"));
            Assert.Equal(platformArgs, dropEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor handles null platformArgs parameter correctly.
        /// Tests the specific input condition where platformArgs is null.
        /// Expected result: PlatformArgs property should be null.
        /// </summary>
        [Fact]
        public void InternalConstructor_NullPlatformArgs_AssignsNullToPlatformArgs()
        {
            // Arrange
            var dataPackage = new DataPackage();
            var view = new DataPackageView(dataPackage);
            Func<IElement, Point?> getPosition = Substitute.For<Func<IElement, Point?>>();
            PlatformDropEventArgs platformArgs = null;

            // Act
            var dropEventArgs = new DropEventArgs(view, getPosition, platformArgs);

            // Assert
            Assert.Same(view, dropEventArgs.Data);
            Assert.Equal(getPosition, GetPrivateField<Func<IElement, Point?>>(dropEventArgs, "_getPosition"));
            Assert.Null(dropEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor handles all null parameters except platformArgs correctly.
        /// Tests the specific input condition where view and getPosition are null.
        /// Expected result: Data should be new DataPackageView, _getPosition should be null, PlatformArgs assigned.
        /// </summary>
        [Fact]
        public void InternalConstructor_ViewAndGetPositionNull_CreatesNewDataAndAssignsCorrectly()
        {
            // Arrange
            DataPackageView view = null;
            Func<IElement, Point?> getPosition = null;
            var platformArgs = Substitute.For<PlatformDropEventArgs>();

            // Act
            var dropEventArgs = new DropEventArgs(view, getPosition, platformArgs);

            // Assert
            Assert.NotNull(dropEventArgs.Data);
            Assert.Null(GetPrivateField<Func<IElement, Point?>>(dropEventArgs, "_getPosition"));
            Assert.Equal(platformArgs, dropEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor handles all valid parameters correctly.
        /// Tests the specific input condition where all parameters are valid non-null values.
        /// Expected result: All properties should be assigned to the provided values.
        /// </summary>
        [Fact]
        public void InternalConstructor_AllValidParameters_AssignsAllPropertiesCorrectly()
        {
            // Arrange
            var dataPackage = new DataPackage();
            var view = new DataPackageView(dataPackage);
            Func<IElement, Point?> getPosition = Substitute.For<Func<IElement, Point?>>();
            var platformArgs = Substitute.For<PlatformDropEventArgs>();

            // Act
            var dropEventArgs = new DropEventArgs(view, getPosition, platformArgs);

            // Assert
            Assert.Same(view, dropEventArgs.Data);
            Assert.Equal(getPosition, GetPrivateField<Func<IElement, Point?>>(dropEventArgs, "_getPosition"));
            Assert.Equal(platformArgs, dropEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Helper method to access private fields using reflection.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="obj">The object containing the field.</param>
        /// <param name="fieldName">The name of the private field.</param>
        /// <returns>The value of the private field.</returns>
        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = typeof(DropEventArgs).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }
    }
}