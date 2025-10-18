#nullable disable

using System;
using System.ComponentModel;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class SolidColorBrushTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush();
            Assert.Null(solidColorBrush.Color);
        }

        [Fact]
        public void TestConstructorUsingColor()
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Red);
            Assert.Equal(solidColorBrush.Color, Colors.Red);
        }

        [Fact]
        public void TestEmptySolidColorBrush()
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush();
            Assert.True(solidColorBrush.IsEmpty);

            SolidColorBrush red = Brush.Red;
            Assert.False(red.IsEmpty);
        }

        [Fact]
        public void TestNullOrEmptySolidColorBrush()
        {
            SolidColorBrush nullSolidColorBrush = null;
            Assert.True(Brush.IsNullOrEmpty(nullSolidColorBrush));

            SolidColorBrush emptySolidColorBrush = new SolidColorBrush();
            Assert.True(Brush.IsNullOrEmpty(emptySolidColorBrush));

            SolidColorBrush solidColorBrush = Brush.Yellow;
            Assert.False(Brush.IsNullOrEmpty(solidColorBrush));
        }

        [Fact]
        public void TestDefaultBrushes()
        {
            SolidColorBrush black = Brush.Black;
            Assert.NotNull(black.Color);
            Assert.Equal(black.Color, Colors.Black);

            SolidColorBrush white = Brush.White;
            Assert.NotNull(white.Color);
            Assert.Equal(white.Color, Colors.White);
        }

        /// <summary>
        /// Tests that GetHashCode throws NullReferenceException when Color property is null (default constructor).
        /// This verifies the expected behavior when accessing Color.GetHashCode() on a null Color.
        /// </summary>
        [Fact]
        public void GetHashCode_DefaultBrush_ThrowsNullReferenceException()
        {
            // Arrange
            var brush = new SolidColorBrush();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => brush.GetHashCode());
        }

        /// <summary>
        /// Tests that GetHashCode returns the expected calculated value for known colors.
        /// Verifies the mathematical formula: -1234567890 + Color.GetHashCode().
        /// </summary>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green  
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        public void GetHashCode_WithValidColor_ReturnsExpectedValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var color = new Color(red, green, blue, alpha);
            var brush = new SolidColorBrush(color);
            var expectedHashCode = -1234567890 + color.GetHashCode();

            // Act
            var actualHashCode = brush.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent results when called multiple times on the same instance.
        /// This verifies the GetHashCode contract requirement for consistency.
        /// </summary>
        [Fact]
        public void GetHashCode_ConsistentResults_ReturnsSameHashCodeOnMultipleCalls()
        {
            // Arrange
            var color = Colors.Red;
            var brush = new SolidColorBrush(color);

            // Act
            var hashCode1 = brush.GetHashCode();
            var hashCode2 = brush.GetHashCode();
            var hashCode3 = brush.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
            Assert.Equal(hashCode2, hashCode3);
        }

        /// <summary>
        /// Tests that two SolidColorBrush instances with the same Color produce the same hash code.
        /// This verifies the GetHashCode contract requirement for equality.
        /// </summary>
        [Fact]
        public void GetHashCode_SameColors_ReturnsSameHashCode()
        {
            // Arrange
            var color = new Color(0.5f, 0.3f, 0.7f, 0.8f);
            var brush1 = new SolidColorBrush(color);
            var brush2 = new SolidColorBrush(color);

            // Act
            var hashCode1 = brush1.GetHashCode();
            var hashCode2 = brush2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that SolidColorBrush instances with different Colors produce different hash codes.
        /// While hash codes can collide, they should be different for obviously different colors.
        /// </summary>
        [Fact]
        public void GetHashCode_DifferentColors_ReturnsDifferentHashCodes()
        {
            // Arrange
            var redBrush = new SolidColorBrush(Colors.Red);
            var blueBrush = new SolidColorBrush(Colors.Blue);
            var greenBrush = new SolidColorBrush(Colors.Green);

            // Act
            var redHashCode = redBrush.GetHashCode();
            var blueHashCode = blueBrush.GetHashCode();
            var greenHashCode = greenBrush.GetHashCode();

            // Assert
            Assert.NotEqual(redHashCode, blueHashCode);
            Assert.NotEqual(redHashCode, greenHashCode);
            Assert.NotEqual(blueHashCode, greenHashCode);
        }

        /// <summary>
        /// Tests GetHashCode with extreme color component values to ensure it handles edge cases properly.
        /// Verifies behavior with minimum and maximum float values for color components.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // All minimum
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // All maximum
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Mixed values
        [InlineData(1.0f, 0.0f, 1.0f, 0.0f)] // Mixed values
        public void GetHashCode_ExtremeColorValues_ReturnsValidHashCode(float red, float green, float blue, float alpha)
        {
            // Arrange
            var color = new Color(red, green, blue, alpha);
            var brush = new SolidColorBrush(color);

            // Act
            var hashCode = brush.GetHashCode();

            // Assert
            // Should not throw and should return a valid integer
            Assert.IsType<int>(hashCode);
            Assert.Equal(-1234567890 + color.GetHashCode(), hashCode);
        }
    }

    public partial class SolidColorBrushEqualsTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Equals returns false when comparing with null object.
        /// </summary>
        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var solidColorBrush = new SolidColorBrush(Colors.Red);

            // Act
            bool result = solidColorBrush.Equals(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing with objects of different types.
        /// This tests the type check condition that is not covered by existing tests.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        public void Equals_DifferentTypeObject_ReturnsFalse(object obj)
        {
            // Arrange
            var solidColorBrush = new SolidColorBrush(Colors.Red);

            // Act
            bool result = solidColorBrush.Equals(obj);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing a SolidColorBrush instance with itself.
        /// </summary>
        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var solidColorBrush = new SolidColorBrush(Colors.Blue);

            // Act
            bool result = solidColorBrush.Equals(solidColorBrush);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing SolidColorBrush instances with the same Color.
        /// </summary>
        [Theory]
        [InlineData("Red")]
        [InlineData("Blue")]
        [InlineData("Green")]
        [InlineData("Transparent")]
        public void Equals_SameColor_ReturnsTrue(string colorName)
        {
            // Arrange
            var color = GetColorByName(colorName);
            var solidColorBrush1 = new SolidColorBrush(color);
            var solidColorBrush2 = new SolidColorBrush(color);

            // Act
            bool result = solidColorBrush1.Equals(solidColorBrush2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing SolidColorBrush instances with different Colors.
        /// </summary>
        [Fact]
        public void Equals_DifferentColors_ReturnsFalse()
        {
            // Arrange
            var solidColorBrush1 = new SolidColorBrush(Colors.Red);
            var solidColorBrush2 = new SolidColorBrush(Colors.Blue);

            // Act
            bool result = solidColorBrush1.Equals(solidColorBrush2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing default constructed SolidColorBrush instances.
        /// Both have null Color properties.
        /// </summary>
        [Fact]
        public void Equals_BothDefaultConstructed_ReturnsTrue()
        {
            // Arrange
            var solidColorBrush1 = new SolidColorBrush();
            var solidColorBrush2 = new SolidColorBrush();

            // Act
            bool result = solidColorBrush1.Equals(solidColorBrush2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns false when one SolidColorBrush has null Color and another has a Color.
        /// </summary>
        [Fact]
        public void Equals_OneNullColorOneSetColor_ReturnsFalse()
        {
            // Arrange
            var solidColorBrush1 = new SolidColorBrush();
            var solidColorBrush2 = new SolidColorBrush(Colors.Red);

            // Act
            bool result1 = solidColorBrush1.Equals(solidColorBrush2);
            bool result2 = solidColorBrush2.Equals(solidColorBrush1);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
        }

        private Color GetColorByName(string colorName)
        {
            return colorName switch
            {
                "Red" => Colors.Red,
                "Blue" => Colors.Blue,
                "Green" => Colors.Green,
                "Transparent" => Colors.Transparent,
                _ => Colors.Black
            };
        }
    }
}