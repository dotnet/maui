using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PointerEventArgsTests
    {
        /// <summary>
        /// Tests that the internal constructor properly initializes all properties with default parameter values.
        /// Input: null getPosition, default args (null), default button (Primary).
        /// Expected: _getPosition is null, PlatformArgs is null, Button is Primary.
        /// </summary>
        [Fact]
        public void Constructor_WithDefaultParameters_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var pointerEventArgs = new PointerEventArgs(getPosition: null);

            // Assert
            Assert.Null(pointerEventArgs.PlatformArgs);
            Assert.Equal(ButtonsMask.Primary, pointerEventArgs.Button);
        }

        /// <summary>
        /// Tests that the internal constructor properly initializes all properties with explicit null values.
        /// Input: null getPosition, null args, Primary button.
        /// Expected: _getPosition is null, PlatformArgs is null, Button is Primary.
        /// </summary>
        [Fact]
        public void Constructor_WithExplicitNullValues_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var pointerEventArgs = new PointerEventArgs(getPosition: null, args: null, button: ButtonsMask.Primary);

            // Assert
            Assert.Null(pointerEventArgs.PlatformArgs);
            Assert.Equal(ButtonsMask.Primary, pointerEventArgs.Button);
        }

        /// <summary>
        /// Tests that the internal constructor properly initializes properties with valid getPosition function.
        /// Input: valid getPosition function, null args, Primary button.
        /// Expected: getPosition function is stored, PlatformArgs is null, Button is Primary.
        /// </summary>
        [Fact]
        public void Constructor_WithValidGetPositionFunction_SetsPropertiesCorrectly()
        {
            // Arrange
            Func<IElement, Point?> getPosition = element => new Point(10, 20);

            // Act
            var pointerEventArgs = new PointerEventArgs(getPosition, args: null, button: ButtonsMask.Primary);

            // Assert
            Assert.Null(pointerEventArgs.PlatformArgs);
            Assert.Equal(ButtonsMask.Primary, pointerEventArgs.Button);
        }

        /// <summary>
        /// Tests that the internal constructor properly sets the Button property with different ButtonsMask values.
        /// Input: various button mask values including Primary, Secondary, and combined flags.
        /// Expected: Button property is set to the specified value.
        /// </summary>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Secondary)]
        [InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
        public void Constructor_WithDifferentButtonMasks_SetsButtonPropertyCorrectly(ButtonsMask button)
        {
            // Arrange & Act
            var pointerEventArgs = new PointerEventArgs(getPosition: null, args: null, button: button);

            // Assert
            Assert.Equal(button, pointerEventArgs.Button);
            Assert.Null(pointerEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that the internal constructor properly handles all parameter combinations.
        /// Input: combinations of null/valid getPosition, null args, and different button values.
        /// Expected: All properties are set according to the input parameters.
        /// </summary>
        [Theory]
        [InlineData(true, ButtonsMask.Primary)]
        [InlineData(true, ButtonsMask.Secondary)]
        [InlineData(false, ButtonsMask.Primary)]
        [InlineData(false, ButtonsMask.Secondary)]
        public void Constructor_WithParameterCombinations_SetsPropertiesCorrectly(bool hasGetPosition, ButtonsMask button)
        {
            // Arrange
            Func<IElement, Point?>? getPosition = hasGetPosition ? (element => new Point(5, 15)) : null;

            // Act
            var pointerEventArgs = new PointerEventArgs(getPosition, args: null, button: button);

            // Assert
            Assert.Equal(button, pointerEventArgs.Button);
            Assert.Null(pointerEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that GetPosition returns null when _getPosition field is null (using parameterless constructor).
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFieldIsNull_ReturnsNull()
        {
            // Arrange
            var pointerEventArgs = new PointerEventArgs();
            var mockElement = Substitute.For<Element>();

            // Act
            var result = pointerEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition returns null when _getPosition field is null and relativeTo is null.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFieldIsNullAndRelativeToIsNull_ReturnsNull()
        {
            // Arrange
            var pointerEventArgs = new PointerEventArgs();

            // Act
            var result = pointerEventArgs.GetPosition(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition invokes the delegate and returns null when the delegate returns null.
        /// </summary>
        [Fact]
        public void GetPosition_WhenDelegateReturnsNull_ReturnsNull()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var getPositionFunc = Substitute.For<Func<IElement, Point?>>();
            getPositionFunc.Invoke(mockElement).Returns((Point?)null);

            var pointerEventArgs = new PointerEventArgs(getPositionFunc);

            // Act
            var result = pointerEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
            getPositionFunc.Received(1).Invoke(mockElement);
        }

        /// <summary>
        /// Tests that GetPosition invokes the delegate and returns a valid Point when the delegate returns a Point.
        /// </summary>
        [Fact]
        public void GetPosition_WhenDelegateReturnsValidPoint_ReturnsPoint()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var expectedPoint = new Point(10.5, 20.7);
            var getPositionFunc = Substitute.For<Func<IElement, Point?>>();
            getPositionFunc.Invoke(mockElement).Returns(expectedPoint);

            var pointerEventArgs = new PointerEventArgs(getPositionFunc);

            // Act
            var result = pointerEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
            getPositionFunc.Received(1).Invoke(mockElement);
        }

        /// <summary>
        /// Tests that GetPosition passes null relativeTo parameter correctly to the delegate.
        /// </summary>
        [Fact]
        public void GetPosition_WhenRelativeToIsNull_PassesNullToDelegate()
        {
            // Arrange
            var expectedPoint = new Point(5.0, 15.0);
            var getPositionFunc = Substitute.For<Func<IElement, Point?>>();
            getPositionFunc.Invoke(Arg.Any<IElement>()).Returns(expectedPoint);

            var pointerEventArgs = new PointerEventArgs(getPositionFunc);

            // Act
            var result = pointerEventArgs.GetPosition(null);

            // Assert
            Assert.Equal(expectedPoint, result);
            getPositionFunc.Received(1).Invoke(null);
        }

        /// <summary>
        /// Tests that GetPosition handles boundary values for Point coordinates correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(0.0, 0.0)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void GetPosition_WithBoundaryPointValues_ReturnsCorrectPoint(double x, double y)
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var expectedPoint = new Point(x, y);
            var getPositionFunc = Substitute.For<Func<IElement, Point?>>();
            getPositionFunc.Invoke(mockElement).Returns(expectedPoint);

            var pointerEventArgs = new PointerEventArgs(getPositionFunc);

            // Act
            var result = pointerEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        /// <summary>
        /// Tests that GetPosition propagates exceptions thrown by the delegate.
        /// </summary>
        [Fact]
        public void GetPosition_WhenDelegateThrowsException_PropagatesException()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var expectedException = new InvalidOperationException("Test exception");
            var getPositionFunc = Substitute.For<Func<IElement, Point?>>();
            getPositionFunc.When(x => x.Invoke(mockElement)).Do(x => throw expectedException);

            var pointerEventArgs = new PointerEventArgs(getPositionFunc);

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() => pointerEventArgs.GetPosition(mockElement));
            Assert.Equal(expectedException.Message, actualException.Message);
        }

        /// <summary>
        /// Tests that GetPosition works correctly when constructed with internal constructor with all parameters.
        /// </summary>
        [Fact]
        public void GetPosition_WithInternalConstructorAllParameters_WorksCorrectly()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var expectedPoint = new Point(42.0, 24.0);
            var getPositionFunc = Substitute.For<Func<IElement, Point?>>();
            getPositionFunc.Invoke(mockElement).Returns(expectedPoint);
            var mockPlatformArgs = Substitute.For<PlatformPointerEventArgs>();

            var pointerEventArgs = new PointerEventArgs(getPositionFunc, mockPlatformArgs, ButtonsMask.Secondary);

            // Act
            var result = pointerEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Equal(mockPlatformArgs, pointerEventArgs.PlatformArgs);
            Assert.Equal(ButtonsMask.Secondary, pointerEventArgs.Button);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid PointerEventArgs instance.
        /// Verifies that the instance is properly initialized and inherits from EventArgs.
        /// Expected result: A non-null PointerEventArgs instance that is an EventArgs.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Act
            var pointerEventArgs = new PointerEventArgs();

            // Assert
            Assert.NotNull(pointerEventArgs);
            Assert.IsAssignableFrom<EventArgs>(pointerEventArgs);
        }

        /// <summary>
        /// Tests that the parameterless constructor sets the Button property to Primary.
        /// Verifies that the default button mask is correctly initialized.
        /// Expected result: Button property equals ButtonsMask.Primary.
        /// </summary>
        [Fact]
        public void Constructor_Default_SetsButtonToPrimary()
        {
            // Act
            var pointerEventArgs = new PointerEventArgs();

            // Assert
            Assert.Equal(ButtonsMask.Primary, pointerEventArgs.Button);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes PlatformArgs to null.
        /// Verifies that platform-specific arguments are not set by default.
        /// Expected result: PlatformArgs property is null.
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesPlatformArgsToNull()
        {
            // Act
            var pointerEventArgs = new PointerEventArgs();

            // Assert
            Assert.Null(pointerEventArgs.PlatformArgs);
        }

        /// <summary>
        /// Tests that multiple instances created by the parameterless constructor have consistent default values.
        /// Verifies that the constructor behavior is deterministic and repeatable.
        /// Expected result: All instances have identical default property values.
        /// </summary>
        [Fact]
        public void Constructor_Default_ConsistentInitializationAcrossInstances()
        {
            // Act
            var instance1 = new PointerEventArgs();
            var instance2 = new PointerEventArgs();

            // Assert
            Assert.Equal(instance1.Button, instance2.Button);
            Assert.Equal(instance1.PlatformArgs, instance2.PlatformArgs);
            Assert.Equal(ButtonsMask.Primary, instance1.Button);
            Assert.Equal(ButtonsMask.Primary, instance2.Button);
        }
    }
}