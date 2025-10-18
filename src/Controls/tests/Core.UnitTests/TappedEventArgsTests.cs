using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class TappedEventArgsTests
    {
        /// <summary>
        /// Tests that GetPosition returns null when _getPosition field is null (using public constructor).
        /// Input: relativeTo parameter can be null or valid Element.
        /// Expected: Method returns null regardless of relativeTo value.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPosition_WhenGetPositionFunctionIsNull_ReturnsNull(bool useNullRelativeTo)
        {
            // Arrange
            var tappedEventArgs = new TappedEventArgs(parameter: "test");
            var relativeTo = useNullRelativeTo ? null : Substitute.For<Element>();

            // Act
            var result = tappedEventArgs.GetPosition(relativeTo);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition invokes the _getPosition function and returns its result when function is not null.
        /// Input: Valid _getPosition function that returns null.
        /// Expected: Method returns null as returned by the function.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFunctionReturnsNull_ReturnsNull()
        {
            // Arrange
            var relativeTo = Substitute.For<Element>();
            Func<IElement, Point?> getPositionFunc = (element) => null;
            var tappedEventArgs = new TappedEventArgs(parameter: "test", getPositionFunc, ButtonsMask.Primary);

            // Act
            var result = tappedEventArgs.GetPosition(relativeTo);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPosition invokes the _getPosition function and returns its result when function returns a Point.
        /// Input: Valid _getPosition function that returns a Point value, with non-null relativeTo.
        /// Expected: Method returns the Point value returned by the function.
        /// </summary>
        [Fact]
        public void GetPosition_WhenGetPositionFunctionReturnsPoint_ReturnsPoint()
        {
            // Arrange
            var relativeTo = Substitute.For<Element>();
            var expectedPoint = new Point(10.5, 20.3);
            Func<IElement, Point?> getPositionFunc = (element) => expectedPoint;
            var tappedEventArgs = new TappedEventArgs(parameter: "test", getPositionFunc, ButtonsMask.Primary);

            // Act
            var result = tappedEventArgs.GetPosition(relativeTo);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        /// <summary>
        /// Tests that GetPosition properly passes null relativeTo parameter to the _getPosition function.
        /// Input: Valid _getPosition function and null relativeTo parameter.
        /// Expected: Function is called with null and returns expected result.
        /// </summary>
        [Fact]
        public void GetPosition_WithNullRelativeTo_PassesNullToFunction()
        {
            // Arrange
            var expectedPoint = new Point(5.0, 15.0);
            IElement receivedElement = null;
            Func<IElement, Point?> getPositionFunc = (element) =>
            {
                receivedElement = element;
                return expectedPoint;
            };
            var tappedEventArgs = new TappedEventArgs(parameter: "test", getPositionFunc, ButtonsMask.Primary);

            // Act
            var result = tappedEventArgs.GetPosition(null);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Null(receivedElement);
        }

        /// <summary>
        /// Tests that GetPosition properly passes the relativeTo parameter to the _getPosition function.
        /// Input: Valid _getPosition function and valid Element as relativeTo parameter.
        /// Expected: Function receives the correct Element parameter and returns expected result.
        /// </summary>
        [Fact]
        public void GetPosition_WithValidRelativeTo_PassesElementToFunction()
        {
            // Arrange
            var relativeTo = Substitute.For<Element>();
            var expectedPoint = new Point(100.0, 200.0);
            IElement receivedElement = null;
            Func<IElement, Point?> getPositionFunc = (element) =>
            {
                receivedElement = element;
                return expectedPoint;
            };
            var tappedEventArgs = new TappedEventArgs(parameter: "test", getPositionFunc, ButtonsMask.Primary);

            // Act
            var result = tappedEventArgs.GetPosition(relativeTo);

            // Assert
            Assert.Equal(expectedPoint, result);
            Assert.Same(relativeTo, receivedElement);
        }

        /// <summary>
        /// Tests GetPosition behavior with extreme Point coordinate values.
        /// Input: _getPosition function that returns Point with extreme double values.
        /// Expected: Method returns the extreme Point values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(0.0, 0.0)]
        public void GetPosition_WithExtremePointValues_ReturnsCorrectValues(double x, double y)
        {
            // Arrange
            var relativeTo = Substitute.For<Element>();
            var expectedPoint = new Point(x, y);
            Func<IElement, Point?> getPositionFunc = (element) => expectedPoint;
            var tappedEventArgs = new TappedEventArgs(parameter: "test", getPositionFunc, ButtonsMask.Primary);

            // Act
            var result = tappedEventArgs.GetPosition(relativeTo);

            // Assert
            Assert.Equal(expectedPoint, result);
            if (double.IsNaN(x))
                Assert.True(double.IsNaN(result.Value.X));
            else
                Assert.Equal(x, result.Value.X);

            if (double.IsNaN(y))
                Assert.True(double.IsNaN(result.Value.Y));
            else
                Assert.Equal(y, result.Value.Y);
        }

        /// <summary>
        /// Tests that the internal constructor correctly sets the Parameter property for various parameter values.
        /// </summary>
        /// <param name="parameter">The parameter value to test</param>
        [Theory]
        [InlineData(null)]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        public void Constructor_WithVariousParameters_SetsParameterCorrectly(object parameter)
        {
            // Arrange
            Func<IElement, Point?> getPosition = null;
            var buttons = ButtonsMask.Primary;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter, getPosition, buttons);

            // Assert
            Assert.Equal(parameter, tappedEventArgs.Parameter);
        }

        /// <summary>
        /// Tests that the internal constructor correctly sets the Buttons property for various ButtonsMask values.
        /// </summary>
        /// <param name="buttons">The ButtonsMask value to test</param>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Secondary)]
        [InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
        public void Constructor_WithVariousButtonsMask_SetsButtonsCorrectly(ButtonsMask buttons)
        {
            // Arrange
            object parameter = "test";
            Func<IElement, Point?> getPosition = null;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter, getPosition, buttons);

            // Assert
            Assert.Equal(buttons, tappedEventArgs.Buttons);
        }

        /// <summary>
        /// Tests that when getPosition parameter is null, the GetPosition method returns null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullGetPosition_GetPositionReturnsNull()
        {
            // Arrange
            object parameter = "test";
            Func<IElement, Point?> getPosition = null;
            var buttons = ButtonsMask.Primary;
            var mockElement = Substitute.For<Element>();

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter, getPosition, buttons);
            var result = tappedEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that when getPosition parameter is provided, the GetPosition method correctly invokes the function.
        /// </summary>
        [Fact]
        public void Constructor_WithGetPositionFunction_GetPositionInvokesFunction()
        {
            // Arrange
            object parameter = "test";
            var expectedPoint = new Point(10, 20);
            var mockElement = Substitute.For<Element>();
            var mockIElement = Substitute.For<IElement>();
            Func<IElement, Point?> getPosition = (element) => expectedPoint;
            var buttons = ButtonsMask.Secondary;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter, getPosition, buttons);
            var result = tappedEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        /// <summary>
        /// Tests that when getPosition function returns null, the GetPosition method returns null.
        /// </summary>
        [Fact]
        public void Constructor_WithGetPositionReturningNull_GetPositionReturnsNull()
        {
            // Arrange
            object parameter = "test";
            var mockElement = Substitute.For<Element>();
            Func<IElement, Point?> getPosition = (element) => null;
            var buttons = ButtonsMask.Primary;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter, getPosition, buttons);
            var result = tappedEventArgs.GetPosition(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the internal constructor correctly handles invalid ButtonsMask enum values.
        /// </summary>
        [Fact]
        public void Constructor_WithInvalidButtonsMaskValue_SetsButtonsCorrectly()
        {
            // Arrange
            object parameter = "test";
            Func<IElement, Point?> getPosition = null;
            var invalidButtons = (ButtonsMask)99;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter, getPosition, invalidButtons);

            // Assert
            Assert.Equal(invalidButtons, tappedEventArgs.Buttons);
        }

        /// <summary>
        /// Tests that the internal constructor correctly handles all parameters being edge case values.
        /// </summary>
        [Fact]
        public void Constructor_WithAllEdgeCaseValues_SetsPropertiesCorrectly()
        {
            // Arrange
            object parameter = null;
            Func<IElement, Point?> getPosition = null;
            var buttons = (ButtonsMask)0;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter, getPosition, buttons);

            // Assert
            Assert.Null(tappedEventArgs.Parameter);
            Assert.Equal(buttons, tappedEventArgs.Buttons);
            Assert.Null(tappedEventArgs.GetPosition(null));
        }

        /// <summary>
        /// Verifies that the TappedEventArgs constructor properly initializes with a null parameter.
        /// Tests the constructor with null input to ensure it handles nullable parameters correctly.
        /// Expected result: Parameter property should be null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullParameter_SetsParameterToNull()
        {
            // Arrange
            object parameter = null;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter);

            // Assert
            Assert.Null(tappedEventArgs.Parameter);
        }

        /// <summary>
        /// Verifies that the TappedEventArgs constructor properly initializes with various parameter types.
        /// Tests the constructor with different object types to ensure proper parameter assignment.
        /// Expected result: Parameter property should match the input parameter value and type.
        /// </summary>
        [Theory]
        [InlineData("string parameter")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        [InlineData('c')]
        public void Constructor_WithVariousParameterTypes_SetsParameterCorrectly(object parameter)
        {
            // Act
            var tappedEventArgs = new TappedEventArgs(parameter);

            // Assert
            Assert.Equal(parameter, tappedEventArgs.Parameter);
        }

        /// <summary>
        /// Verifies that the TappedEventArgs constructor properly initializes with an empty string parameter.
        /// Tests the constructor with empty string to ensure proper handling of edge case string values.
        /// Expected result: Parameter property should be an empty string.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyString_SetsParameterToEmptyString()
        {
            // Arrange
            var parameter = string.Empty;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter);

            // Assert
            Assert.Equal(string.Empty, tappedEventArgs.Parameter);
        }

        /// <summary>
        /// Verifies that the TappedEventArgs constructor properly initializes with a whitespace string parameter.
        /// Tests the constructor with whitespace-only string to ensure proper handling of special string values.
        /// Expected result: Parameter property should be the whitespace string.
        /// </summary>
        [Fact]
        public void Constructor_WithWhitespaceString_SetsParameterToWhitespace()
        {
            // Arrange
            var parameter = "   ";

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter);

            // Assert
            Assert.Equal("   ", tappedEventArgs.Parameter);
        }

        /// <summary>
        /// Verifies that the TappedEventArgs constructor properly initializes with a custom object parameter.
        /// Tests the constructor with a complex object to ensure proper reference assignment.
        /// Expected result: Parameter property should reference the same object instance.
        /// </summary>
        [Fact]
        public void Constructor_WithCustomObject_SetsParameterToSameReference()
        {
            // Arrange
            var customObject = new { Name = "Test", Value = 123 };

            // Act
            var tappedEventArgs = new TappedEventArgs(customObject);

            // Assert
            Assert.Same(customObject, tappedEventArgs.Parameter);
        }

        /// <summary>
        /// Verifies that the TappedEventArgs constructor properly initializes with DBNull parameter.
        /// Tests the constructor with DBNull to ensure proper handling of special null-like values.
        /// Expected result: Parameter property should be DBNull.Value.
        /// </summary>
        [Fact]
        public void Constructor_WithDbNull_SetsParameterToDbNull()
        {
            // Arrange
            var parameter = DBNull.Value;

            // Act
            var tappedEventArgs = new TappedEventArgs(parameter);

            // Assert
            Assert.Equal(DBNull.Value, tappedEventArgs.Parameter);
        }

        /// <summary>
        /// Verifies that the TappedEventArgs constructor creates an instance that inherits from EventArgs.
        /// Tests that the constructor properly calls the base EventArgs constructor.
        /// Expected result: Created instance should be assignable to EventArgs.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceInheritingFromEventArgs()
        {
            // Act
            var tappedEventArgs = new TappedEventArgs("test");

            // Assert
            Assert.IsAssignableFrom<EventArgs>(tappedEventArgs);
        }
    }
}