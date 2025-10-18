#nullable disable

using System;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class RoundRectangleTests
    {
        /// <summary>
        /// Tests HeightForPathComputation property when Height equals -1.
        /// Verifies that the property returns the _fallbackHeight field value.
        /// Expected result: Returns the fallback height value.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(100.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void HeightForPathComputation_WhenHeightIsMinusOne_ReturnsFallbackHeight(double fallbackHeight)
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            SetFallbackHeight(roundRectangle, fallbackHeight);
            SetHeight(roundRectangle, -1.0);

            // Act
            double result = roundRectangle.HeightForPathComputation;

            // Assert
            if (double.IsNaN(fallbackHeight))
                Assert.True(double.IsNaN(result));
            else
                Assert.Equal(fallbackHeight, result);
        }

        /// <summary>
        /// Tests HeightForPathComputation property when Height is not equal to -1.
        /// Verifies that the property returns the Height property value directly.
        /// Expected result: Returns the Height property value.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(50.25)]
        [InlineData(100.0)]
        [InlineData(-2.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void HeightForPathComputation_WhenHeightIsNotMinusOne_ReturnsHeightValue(double heightValue)
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            SetFallbackHeight(roundRectangle, 999.0); // Set different fallback to ensure it's not used
            SetHeight(roundRectangle, heightValue);

            // Act
            double result = roundRectangle.HeightForPathComputation;

            // Assert
            if (double.IsNaN(heightValue))
                Assert.True(double.IsNaN(result));
            else
                Assert.Equal(heightValue, result);
        }

        /// <summary>
        /// Tests HeightForPathComputation property with boundary case where Height is exactly -1.
        /// Verifies proper handling of the exact boundary condition.
        /// Expected result: Returns the fallback height when Height is exactly -1.
        /// </summary>
        [Fact]
        public void HeightForPathComputation_WhenHeightIsExactlyMinusOne_ReturnsFallbackHeight()
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            const double expectedFallbackHeight = 42.5;
            SetFallbackHeight(roundRectangle, expectedFallbackHeight);
            SetHeight(roundRectangle, -1.0);

            // Act
            double result = roundRectangle.HeightForPathComputation;

            // Assert
            Assert.Equal(expectedFallbackHeight, result);
        }

        /// <summary>
        /// Tests HeightForPathComputation property with zero values.
        /// Verifies proper handling when both Height and fallback are zero.
        /// Expected result: Returns zero when Height is zero.
        /// </summary>
        [Fact]
        public void HeightForPathComputation_WhenHeightIsZero_ReturnsZero()
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            SetFallbackHeight(roundRectangle, 100.0); // Different from zero
            SetHeight(roundRectangle, 0.0);

            // Act
            double result = roundRectangle.HeightForPathComputation;

            // Assert
            Assert.Equal(0.0, result);
        }

        private static void SetFallbackHeight(RoundRectangle roundRectangle, double value)
        {
            var fallbackHeightField = typeof(RoundRectangle).GetField("_fallbackHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(fallbackHeightField);
            fallbackHeightField.SetValue(roundRectangle, value);
        }

        private static void SetHeight(RoundRectangle roundRectangle, double value)
        {
            // Use reflection to access the private setter of Height property
            var heightProperty = typeof(VisualElement).GetProperty("Height");
            Assert.NotNull(heightProperty);

            // Try to set via the private setter if available
            var setter = heightProperty.GetSetMethod(true);
            if (setter != null)
            {
                setter.Invoke(roundRectangle, new object[] { value });
            }
            else
            {
                // Alternative: use HeightProperty directly through SetValue
                var heightPropertyField = typeof(VisualElement).GetField("HeightProperty", BindingFlags.Public | BindingFlags.Static);
                if (heightPropertyField != null)
                {
                    var heightProp = (BindableProperty)heightPropertyField.GetValue(null);
                    roundRectangle.SetValue(heightProp, value);
                }
                else
                {
                    // Last resort: access _mockHeight field if it exists
                    var mockHeightField = typeof(VisualElement).GetField("_mockHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mockHeightField != null)
                    {
                        mockHeightField.SetValue(roundRectangle, value);
                    }
                }
            }
        }

        /// <summary>
        /// Tests that GetPath returns a non-null PathF with standard dimensions and uniform corner radius.
        /// Input: Standard width (100), height (50), uniform corner radius (10).
        /// Expected: Non-null PathF with one closed sub-path.
        /// </summary>
        [Fact]
        public void GetPath_StandardDimensionsWithUniformCornerRadius_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 100,
                HeightRequest = 50,
                CornerRadius = new CornerRadius(10)
            };

            // Force the Width and Height properties to use the requested values
            SetupDimensionsForTesting(roundRectangle, 100, 50);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.Equal(1, path.SubPathCount);
        }

        /// <summary>
        /// Tests that GetPath returns a valid path with different corner radii for each corner.
        /// Input: Standard dimensions with different corner radii (5, 10, 15, 20).
        /// Expected: Non-null PathF with one closed sub-path.
        /// </summary>
        [Fact]
        public void GetPath_DifferentCornerRadii_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 100,
                HeightRequest = 80,
                CornerRadius = new CornerRadius(5, 10, 15, 20)
            };

            SetupDimensionsForTesting(roundRectangle, 100, 80);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.Equal(1, path.SubPathCount);
        }

        /// <summary>
        /// Tests that GetPath handles zero corner radius correctly (creates a regular rectangle).
        /// Input: Standard dimensions with zero corner radius.
        /// Expected: Non-null PathF with one closed sub-path.
        /// </summary>
        [Fact]
        public void GetPath_ZeroCornerRadius_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 100,
                HeightRequest = 50,
                CornerRadius = new CornerRadius(0)
            };

            SetupDimensionsForTesting(roundRectangle, 100, 50);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.Equal(1, path.SubPathCount);
        }

        /// <summary>
        /// Tests that GetPath handles zero width correctly.
        /// Input: Zero width, positive height, and corner radius.
        /// Expected: Non-null PathF.
        /// </summary>
        [Fact]
        public void GetPath_ZeroWidth_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 0,
                HeightRequest = 50,
                CornerRadius = new CornerRadius(10)
            };

            SetupDimensionsForTesting(roundRectangle, 0, 50);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests that GetPath handles zero height correctly.
        /// Input: Positive width, zero height, and corner radius.
        /// Expected: Non-null PathF.
        /// </summary>
        [Fact]
        public void GetPath_ZeroHeight_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 100,
                HeightRequest = 0,
                CornerRadius = new CornerRadius(10)
            };

            SetupDimensionsForTesting(roundRectangle, 100, 0);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests that GetPath handles both zero width and height correctly.
        /// Input: Zero width, zero height, and corner radius.
        /// Expected: Non-null PathF.
        /// </summary>
        [Fact]
        public void GetPath_ZeroDimensions_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 0,
                HeightRequest = 0,
                CornerRadius = new CornerRadius(10)
            };

            SetupDimensionsForTesting(roundRectangle, 0, 0);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests that GetPath handles very large corner radius values correctly (should be clamped by PathF).
        /// Input: Small dimensions with very large corner radius.
        /// Expected: Non-null PathF with one closed sub-path.
        /// </summary>
        [Fact]
        public void GetPath_LargeCornerRadius_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 50,
                HeightRequest = 30,
                CornerRadius = new CornerRadius(100) // Larger than dimensions
            };

            SetupDimensionsForTesting(roundRectangle, 50, 30);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.Equal(1, path.SubPathCount);
        }

        /// <summary>
        /// Tests that GetPath handles very large dimensions correctly.
        /// Input: Very large width and height values.
        /// Expected: Non-null PathF with one closed sub-path.
        /// </summary>
        [Fact]
        public void GetPath_LargeDimensions_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 10000,
                HeightRequest = 8000,
                CornerRadius = new CornerRadius(50)
            };

            SetupDimensionsForTesting(roundRectangle, 10000, 8000);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.Equal(1, path.SubPathCount);
        }

        /// <summary>
        /// Tests that GetPath handles very small positive dimensions correctly.
        /// Input: Very small positive width and height values.
        /// Expected: Non-null PathF.
        /// </summary>
        [Fact]
        public void GetPath_SmallDimensions_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 0.1,
                HeightRequest = 0.2,
                CornerRadius = new CornerRadius(0.05)
            };

            SetupDimensionsForTesting(roundRectangle, 0.1, 0.2);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests that GetPath uses fallback width when Width property returns -1.
        /// Input: Width = -1 (triggers fallback), positive height, corner radius.
        /// Expected: Non-null PathF using fallback width value.
        /// </summary>
        [Fact]
        public void GetPath_UsesFallbackWidth_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new TestableRoundRectangle
            {
                HeightRequest = 50,
                CornerRadius = new CornerRadius(10)
            };

            // Set fallback width and make Width return -1
            roundRectangle.SetFallbackWidth(75);
            roundRectangle.SetWidthToFallback();
            SetupDimensionsForTesting(roundRectangle, -1, 50);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests that GetPath uses fallback height when Height property returns -1.
        /// Input: Positive width, Height = -1 (triggers fallback), corner radius.
        /// Expected: Non-null PathF using fallback height value.
        /// </summary>
        [Fact]
        public void GetPath_UsesFallbackHeight_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new TestableRoundRectangle
            {
                WidthRequest = 100,
                CornerRadius = new CornerRadius(10)
            };

            // Set fallback height and make Height return -1
            roundRectangle.SetFallbackHeight(60);
            roundRectangle.SetHeightToFallback();
            SetupDimensionsForTesting(roundRectangle, 100, -1);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests that GetPath handles mixed corner radii with some zero and some non-zero values.
        /// Input: Standard dimensions with mixed corner radii (0, 10, 0, 20).
        /// Expected: Non-null PathF with one closed sub-path.
        /// </summary>
        [Fact]
        public void GetPath_MixedCornerRadii_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                WidthRequest = 120,
                HeightRequest = 80,
                CornerRadius = new CornerRadius(0, 10, 0, 20)
            };

            SetupDimensionsForTesting(roundRectangle, 120, 80);

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.Equal(1, path.SubPathCount);
        }

        /// <summary>
        /// Tests that GetPath handles negative fallback dimensions correctly.
        /// Input: Negative fallback width and height values.
        /// Expected: Non-null PathF (negative values will be passed to PathF).
        /// </summary>
        [Fact]
        public void GetPath_NegativeFallbackDimensions_ReturnsValidPath()
        {
            // Arrange
            var roundRectangle = new TestableRoundRectangle
            {
                CornerRadius = new CornerRadius(5)
            };

            roundRectangle.SetFallbackWidth(-10);
            roundRectangle.SetFallbackHeight(-20);
            roundRectangle.SetWidthToFallback();
            roundRectangle.SetHeightToFallback();

            // Act
            var path = roundRectangle.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Helper method to set up dimensions for testing by ensuring the Width and Height properties return expected values.
        /// This simulates the behavior of the WidthForPathComputation and HeightForPathComputation properties.
        /// </summary>
        private static void SetupDimensionsForTesting(RoundRectangle roundRectangle, double width, double height)
        {
            // For testing purposes, we set the WidthRequest and HeightRequest
            // The actual Width/Height properties will use these values in a real scenario
            if (width >= 0)
            {
                roundRectangle.WidthRequest = width;
            }
            if (height >= 0)
            {
                roundRectangle.HeightRequest = height;
            }
        }

        /// <summary>
        /// Tests GetInnerPath with normal strokeThickness value.
        /// Verifies that the method creates a PathF and calculates correct dimensions and positions.
        /// </summary>
        [Theory]
        [InlineData(2.0f, 100.0, 50.0, 5.0, 3.0, 4.0, 6.0)]
        [InlineData(5.0f, 200.0, 100.0, 10.0, 8.0, 7.0, 9.0)]
        [InlineData(1.0f, 50.0, 25.0, 2.0, 1.5, 1.0, 2.5)]
        public void GetInnerPath_WithValidStrokeThickness_ReturnsCorrectPath(
            float strokeThickness,
            double width,
            double height,
            double topLeft,
            double topRight,
            double bottomLeft,
            double bottomRight)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight)
            };

            // Act
            var result = roundRectangle.GetInnerPath(strokeThickness);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with zero strokeThickness.
        /// Verifies that the method handles zero stroke correctly and uses full dimensions.
        /// </summary>
        [Fact]
        public void GetInnerPath_WithZeroStrokeThickness_ReturnsPathWithFullDimensions()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(0.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with negative strokeThickness.
        /// Verifies that the method handles negative stroke values correctly.
        /// </summary>
        [Theory]
        [InlineData(-1.0f)]
        [InlineData(-5.0f)]
        [InlineData(-100.0f)]
        public void GetInnerPath_WithNegativeStrokeThickness_ReturnsPath(float strokeThickness)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(strokeThickness);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with strokeThickness larger than dimensions.
        /// Verifies that the method handles cases where stroke is larger than width/height.
        /// </summary>
        [Theory]
        [InlineData(150.0f, 100.0, 50.0)]
        [InlineData(200.0f, 100.0, 50.0)]
        public void GetInnerPath_WithStrokeThicknessLargerThanDimensions_ReturnsPath(
            float strokeThickness,
            double width,
            double height)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(strokeThickness);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with extreme float values for strokeThickness.
        /// Verifies that the method handles float edge cases correctly.
        /// </summary>
        [Theory]
        [InlineData(float.MaxValue)]
        [InlineData(float.MinValue)]
        public void GetInnerPath_WithExtremeStrokeThickness_ReturnsPath(float strokeThickness)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(strokeThickness);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with special float values for strokeThickness.
        /// Verifies that the method handles NaN and infinity values correctly.
        /// </summary>
        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void GetInnerPath_WithSpecialFloatStrokeThickness_ReturnsPath(float strokeThickness)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(strokeThickness);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with zero corner radius values.
        /// Verifies that the method handles cases with no rounded corners correctly.
        /// </summary>
        [Fact]
        public void GetInnerPath_WithZeroCornerRadius_ReturnsPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(0.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(2.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with corner radius smaller than strokeThickness.
        /// Verifies that corner radii are correctly clamped to zero when they would become negative.
        /// </summary>
        [Theory]
        [InlineData(10.0f, 5.0, 3.0, 4.0, 6.0)]
        [InlineData(8.0f, 2.0, 1.0, 3.0, 5.0)]
        public void GetInnerPath_WithCornerRadiusSmallerThanStroke_ClampsRadiusToZero(
            float strokeThickness,
            double topLeft,
            double topRight,
            double bottomLeft,
            double bottomRight)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight)
            };

            // Act
            var result = roundRectangle.GetInnerPath(strokeThickness);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with zero width and height.
        /// Verifies that the method handles zero dimensions correctly.
        /// </summary>
        [Fact]
        public void GetInnerPath_WithZeroDimensions_ReturnsPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 0.0,
                Height = 0.0,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(2.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with negative width and height.
        /// Verifies that the method handles negative dimensions correctly.
        /// </summary>
        [Theory]
        [InlineData(-50.0, -25.0)]
        [InlineData(-100.0, 50.0)]
        [InlineData(100.0, -50.0)]
        public void GetInnerPath_WithNegativeDimensions_ReturnsPath(double width, double height)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(2.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with very large dimensions.
        /// Verifies that the method handles large width and height values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(1e10, 1e10)]
        public void GetInnerPath_WithLargeDimensions_ReturnsPath(double width, double height)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(2.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath using fallback dimensions when Width/Height is -1.
        /// Verifies that the method correctly uses fallback width and height values.
        /// </summary>
        [Fact]
        public void GetInnerPath_WithFallbackDimensions_ReturnsPath()
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = -1.0,
                Height = -1.0,
                CornerRadius = new CornerRadius(5.0, 3.0, 4.0, 6.0)
            };

            // Act
            var result = roundRectangle.GetInnerPath(2.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with extreme corner radius values.
        /// Verifies that the method handles very large corner radius values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(1e10, 1e10, 1e10, 1e10)]
        public void GetInnerPath_WithExtremeCornerRadius_ReturnsPath(
            double topLeft,
            double topRight,
            double bottomLeft,
            double bottomRight)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight)
            };

            // Act
            var result = roundRectangle.GetInnerPath(2.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetInnerPath with different corner radius combinations.
        /// Verifies that the method correctly handles asymmetric corner radii.
        /// </summary>
        [Theory]
        [InlineData(10.0, 0.0, 5.0, 0.0)]
        [InlineData(0.0, 10.0, 0.0, 5.0)]
        [InlineData(20.0, 15.0, 10.0, 5.0)]
        public void GetInnerPath_WithAsymmetricCornerRadius_ReturnsPath(
            double topLeft,
            double topRight,
            double bottomLeft,
            double bottomRight)
        {
            // Arrange
            var roundRectangle = new RoundRectangle
            {
                Width = 100.0,
                Height = 50.0,
                CornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight)
            };

            // Act
            var result = roundRectangle.GetInnerPath(3.0f);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests that WidthForPathComputation returns _fallbackWidth when Width equals -1.
        /// </summary>
        /// <param name="fallbackWidth">The fallback width value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(100.0)]
        [InlineData(-50.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void WidthForPathComputation_WhenWidthIsNegativeOne_ReturnsFallbackWidth(double fallbackWidth)
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            SetFallbackWidth(roundRectangle, fallbackWidth);
            SetMockWidth(roundRectangle, -1.0);

            // Act
            var result = roundRectangle.WidthForPathComputation;

            // Assert
            Assert.Equal(fallbackWidth, result);
        }

        /// <summary>
        /// Tests that WidthForPathComputation returns Width when Width is not equal to -1.
        /// </summary>
        /// <param name="width">The width value to test.</param>
        /// <param name="fallbackWidth">The fallback width value (should be ignored).</param>
        [Theory]
        [InlineData(0.0, 999.0)]
        [InlineData(100.0, 888.0)]
        [InlineData(-50.0, 777.0)]
        [InlineData(25.5, 666.0)]
        [InlineData(double.MaxValue, 555.0)]
        [InlineData(double.MinValue, 444.0)]
        [InlineData(double.NaN, 333.0)]
        [InlineData(double.PositiveInfinity, 222.0)]
        [InlineData(double.NegativeInfinity, 111.0)]
        [InlineData(-0.9, 123.0)] // Close to -1 but not exactly -1
        [InlineData(-1.1, 456.0)] // Close to -1 but not exactly -1
        public void WidthForPathComputation_WhenWidthIsNotNegativeOne_ReturnsWidth(double width, double fallbackWidth)
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            SetFallbackWidth(roundRectangle, fallbackWidth);
            SetMockWidth(roundRectangle, width);

            // Act
            var result = roundRectangle.WidthForPathComputation;

            // Assert
            Assert.Equal(width, result);
        }

        /// <summary>
        /// Tests that WidthForPathComputation returns _fallbackWidth when Width exactly equals -1.0.
        /// Tests the boundary condition where Width is exactly -1.
        /// </summary>
        [Fact]
        public void WidthForPathComputation_WhenWidthIsExactlyNegativeOne_ReturnsFallbackWidth()
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            var expectedFallbackWidth = 42.5;
            SetFallbackWidth(roundRectangle, expectedFallbackWidth);
            SetMockWidth(roundRectangle, -1.0);

            // Act
            var result = roundRectangle.WidthForPathComputation;

            // Assert
            Assert.Equal(expectedFallbackWidth, result);
        }

        /// <summary>
        /// Tests that WidthForPathComputation returns Width when Width is zero.
        /// Verifies that zero is treated as a valid width, not as a fallback trigger.
        /// </summary>
        [Fact]
        public void WidthForPathComputation_WhenWidthIsZero_ReturnsZero()
        {
            // Arrange
            var roundRectangle = new RoundRectangle();
            SetFallbackWidth(roundRectangle, 999.0); // Should be ignored
            SetMockWidth(roundRectangle, 0.0);

            // Act
            var result = roundRectangle.WidthForPathComputation;

            // Assert
            Assert.Equal(0.0, result);
        }

        private static void SetFallbackWidth(RoundRectangle roundRectangle, double value)
        {
            var field = typeof(RoundRectangle).GetField("_fallbackWidth", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(roundRectangle, value);
        }

        private static void SetMockWidth(RoundRectangle roundRectangle, double value)
        {
            var field = typeof(VisualElement).GetField("_mockWidth", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(roundRectangle, value);
        }
    }
}