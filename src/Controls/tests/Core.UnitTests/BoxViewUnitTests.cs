#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class BoxViewUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            var box = new BoxView
            {
                Color = new Color(0.2f, 0.3f, 0.4f),
                WidthRequest = 20,
                HeightRequest = 30,
                IsPlatformEnabled = true,
            };

            Assert.Equal(new Color(0.2f, 0.3f, 0.4f), box.Color);
            var request = box.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
            Assert.Equal(20, request.Width);
            Assert.Equal(30, request.Height);
        }

        [Fact]
        public void DefaultSize()
        {
            var box = new BoxView
            {
                IsPlatformEnabled = true,
            };

            var request = box.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
            Assert.Equal(40, request.Width);
            Assert.Equal(40, request.Height);
        }
    }

    public partial class BoxViewTests
    {
        /// <summary>
        /// Test platform configuration type for testing On method.
        /// </summary>
        private class TestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another test platform configuration type for testing multiple platform scenarios.
        /// </summary>
        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that On method returns a valid platform configuration for a valid platform type.
        /// Verifies that the method properly initializes and returns IPlatformElementConfiguration.
        /// </summary>
        [Fact]
        public void On_ValidPlatformType_ReturnsValidConfiguration()
        {
            // Arrange
            var boxView = new BoxView();

            // Act
            var result = boxView.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, BoxView>>(result);
        }

        /// <summary>
        /// Tests that On method returns the same configuration instance when called multiple times with the same platform type.
        /// Verifies that the platform configuration registry properly caches configurations.
        /// </summary>
        [Fact]
        public void On_CalledTwiceWithSamePlatformType_ReturnsSameInstance()
        {
            // Arrange
            var boxView = new BoxView();

            // Act
            var result1 = boxView.On<TestPlatform>();
            var result2 = boxView.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that On method returns different configuration instances for different platform types.
        /// Verifies that the platform configuration registry creates separate configurations per platform.
        /// </summary>
        [Fact]
        public void On_CalledWithDifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var boxView = new BoxView();

            // Act
            var result1 = boxView.On<TestPlatform>();
            var result2 = boxView.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that On method works correctly with multiple BoxView instances.
        /// Verifies that each BoxView maintains its own platform configuration registry.
        /// </summary>
        [Fact]
        public void On_MultipleBoxViewInstances_ReturnsSeparateConfigurations()
        {
            // Arrange
            var boxView1 = new BoxView();
            var boxView2 = new BoxView();

            // Act
            var result1 = boxView1.On<TestPlatform>();
            var result2 = boxView2.On<TestPlatform>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that On method properly initializes the lazy platform configuration registry on first access.
        /// Verifies that the method works immediately after BoxView construction.
        /// </summary>
        [Fact]
        public void On_FirstCallAfterConstruction_InitializesRegistryAndReturnsConfiguration()
        {
            // Arrange
            var boxView = new BoxView();

            // Act & Assert - Should not throw and should return valid configuration
            var result = boxView.On<TestPlatform>();

            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, BoxView>>(result);
        }

        /// <summary>
        /// Tests that the CornerRadius property getter returns the default value when no value has been set.
        /// Validates the property returns default(CornerRadius) which has all corners set to 0.
        /// </summary>
        [Fact]
        public void CornerRadius_DefaultValue_ReturnsDefaultCornerRadius()
        {
            // Arrange
            var boxView = new BoxView();

            // Act
            var result = boxView.CornerRadius;

            // Assert
            Assert.Equal(default(CornerRadius), result);
            Assert.Equal(0, result.TopLeft);
            Assert.Equal(0, result.TopRight);
            Assert.Equal(0, result.BottomLeft);
            Assert.Equal(0, result.BottomRight);
        }

        /// <summary>
        /// Tests that the CornerRadius property getter returns the correct value after setting uniform corner radius.
        /// Validates property roundtrip behavior with uniform radius values including edge cases.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10.5)]
        [InlineData(-5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void CornerRadius_UniformRadius_ReturnsSetValue(double uniformRadius)
        {
            // Arrange
            var boxView = new BoxView();
            var expectedCornerRadius = new CornerRadius(uniformRadius);

            // Act
            boxView.CornerRadius = expectedCornerRadius;
            var result = boxView.CornerRadius;

            // Assert
            Assert.Equal(expectedCornerRadius, result);
            Assert.Equal(uniformRadius, result.TopLeft);
            Assert.Equal(uniformRadius, result.TopRight);
            Assert.Equal(uniformRadius, result.BottomLeft);
            Assert.Equal(uniformRadius, result.BottomRight);
        }

        /// <summary>
        /// Tests that the CornerRadius property getter returns the correct value after setting individual corner radii.
        /// Validates property roundtrip behavior with different values for each corner.
        /// </summary>
        [Theory]
        [InlineData(1, 2, 3, 4)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(-1, -2, -3, -4)]
        [InlineData(10.5, 20.7, 30.9, 40.1)]
        [InlineData(double.MaxValue, double.MinValue, 0, 1)]
        public void CornerRadius_IndividualCorners_ReturnsSetValue(double topLeft, double topRight, double bottomLeft, double bottomRight)
        {
            // Arrange
            var boxView = new BoxView();
            var expectedCornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight);

            // Act
            boxView.CornerRadius = expectedCornerRadius;
            var result = boxView.CornerRadius;

            // Assert
            Assert.Equal(expectedCornerRadius, result);
            Assert.Equal(topLeft, result.TopLeft);
            Assert.Equal(topRight, result.TopRight);
            Assert.Equal(bottomLeft, result.BottomLeft);
            Assert.Equal(bottomRight, result.BottomRight);
        }

        /// <summary>
        /// Tests that the CornerRadius property getter handles special double values correctly.
        /// Validates behavior with NaN, PositiveInfinity, and NegativeInfinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CornerRadius_SpecialDoubleValues_ReturnsSetValue(double specialValue)
        {
            // Arrange
            var boxView = new BoxView();
            var expectedCornerRadius = new CornerRadius(specialValue);

            // Act
            boxView.CornerRadius = expectedCornerRadius;
            var result = boxView.CornerRadius;

            // Assert
            Assert.Equal(expectedCornerRadius, result);
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result.TopLeft));
                Assert.True(double.IsNaN(result.TopRight));
                Assert.True(double.IsNaN(result.BottomLeft));
                Assert.True(double.IsNaN(result.BottomRight));
            }
            else
            {
                Assert.Equal(specialValue, result.TopLeft);
                Assert.Equal(specialValue, result.TopRight);
                Assert.Equal(specialValue, result.BottomLeft);
                Assert.Equal(specialValue, result.BottomRight);
            }
        }

        /// <summary>
        /// Tests that the CornerRadius property getter works correctly with implicit conversion from double.
        /// Validates the property handles implicit conversion from double to CornerRadius.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(5.5)]
        [InlineData(-2.3)]
        [InlineData(100)]
        public void CornerRadius_ImplicitConversionFromDouble_ReturnsCorrectValue(double value)
        {
            // Arrange
            var boxView = new BoxView();

            // Act
            boxView.CornerRadius = value; // Implicit conversion from double to CornerRadius
            var result = boxView.CornerRadius;

            // Assert
            Assert.Equal(value, result.TopLeft);
            Assert.Equal(value, result.TopRight);
            Assert.Equal(value, result.BottomLeft);
            Assert.Equal(value, result.BottomRight);
        }

        /// <summary>
        /// Tests that the CornerRadius property getter returns correct values after multiple property assignments.
        /// Validates that subsequent property assignments work correctly and override previous values.
        /// </summary>
        [Fact]
        public void CornerRadius_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var boxView = new BoxView();
            var firstCornerRadius = new CornerRadius(5);
            var secondCornerRadius = new CornerRadius(1, 2, 3, 4);
            var thirdCornerRadius = new CornerRadius(10.5);

            // Act & Assert - First assignment
            boxView.CornerRadius = firstCornerRadius;
            var firstResult = boxView.CornerRadius;
            Assert.Equal(firstCornerRadius, firstResult);

            // Act & Assert - Second assignment
            boxView.CornerRadius = secondCornerRadius;
            var secondResult = boxView.CornerRadius;
            Assert.Equal(secondCornerRadius, secondResult);

            // Act & Assert - Third assignment
            boxView.CornerRadius = thirdCornerRadius;
            var thirdResult = boxView.CornerRadius;
            Assert.Equal(thirdCornerRadius, thirdResult);
        }
    }
}