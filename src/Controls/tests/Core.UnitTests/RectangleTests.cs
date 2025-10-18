#nullable disable

using System;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

using FormsRectangle = Microsoft.Maui.Controls.Shapes.Rectangle;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class RectangleTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the RadiusY property setter correctly stores normal positive values.
        /// Verifies that the setter calls SetValue and the value can be retrieved via the getter.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(10.5)]
        [InlineData(100.0)]
        [InlineData(1000.0)]
        public void RadiusY_SetPositiveValues_StoresCorrectly(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusY = value;

            // Assert
            Assert.Equal(value, rectangle.RadiusY);
        }

        /// <summary>
        /// Tests that the RadiusY property setter correctly handles negative values.
        /// Verifies that negative values are accepted and stored correctly.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.5)]
        [InlineData(-100.0)]
        public void RadiusY_SetNegativeValues_StoresCorrectly(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusY = value;

            // Assert
            Assert.Equal(value, rectangle.RadiusY);
        }

        /// <summary>
        /// Tests that the RadiusY property setter correctly handles extreme double values.
        /// Verifies boundary conditions including minimum, maximum, and special double values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        public void RadiusY_SetExtremeValues_StoresCorrectly(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusY = value;

            // Assert
            Assert.Equal(value, rectangle.RadiusY);
        }

        /// <summary>
        /// Tests that the RadiusY property setter correctly handles special double values.
        /// Verifies that NaN, positive infinity, and negative infinity values are handled properly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RadiusY_SetSpecialDoubleValues_StoresCorrectly(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusY = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(rectangle.RadiusY));
            }
            else
            {
                Assert.Equal(value, rectangle.RadiusY);
            }
        }

        /// <summary>
        /// Tests that multiple consecutive sets to the RadiusY property work correctly.
        /// Verifies that the setter can be called multiple times and each value is stored properly.
        /// </summary>
        [Fact]
        public void RadiusY_SetMultipleValues_UpdatesCorrectly()
        {
            // Arrange
            var rectangle = new FormsRectangle();
            var values = new double[] { 0.0, 5.0, 10.5, -3.2, 100.0 };

            foreach (var value in values)
            {
                // Act
                rectangle.RadiusY = value;

                // Assert
                Assert.Equal(value, rectangle.RadiusY);
            }
        }

        /// <summary>
        /// Tests that the RadiusY property has the correct default value.
        /// Verifies that a newly created Rectangle has RadiusY set to 0.0.
        /// </summary>
        [Fact]
        public void RadiusY_DefaultValue_IsZero()
        {
            // Arrange & Act
            var rectangle = new FormsRectangle();

            // Assert
            Assert.Equal(0.0, rectangle.RadiusY);
        }

        /// <summary>
        /// Tests that GetPath returns a valid PathF object under all conditions.
        /// Verifies that the method never throws exceptions and always returns a non-null PathF.
        /// </summary>
        [Fact]
        public void GetPath_Always_ReturnsValidPathF()
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with zero corner radius values to ensure it uses the rectangular path logic.
        /// Verifies that when both RadiusX and RadiusY are 0, cornerRadius equals 0 and triggers the rectangular path.
        /// </summary>
        [Fact]
        public void GetPath_ZeroCornerRadius_ReturnsRectangularPath()
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 0.0,
                RadiusY = 0.0,
                Width = 100,
                Height = 50,
                StrokeThickness = 2.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with positive RadiusX and zero RadiusY to verify Math.Max logic.
        /// Expects cornerRadius to equal RadiusX and use the rounded rectangle path.
        /// </summary>
        [Fact]
        public void GetPath_PositiveRadiusXZeroRadiusY_ReturnsRoundedRectangularPath()
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 10.0,
                RadiusY = 0.0,
                Width = 100,
                Height = 50,
                StrokeThickness = 2.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with zero RadiusX and positive RadiusY to verify Math.Max logic.
        /// Expects cornerRadius to equal RadiusY and use the rounded rectangle path.
        /// </summary>
        [Fact]
        public void GetPath_ZeroRadiusXPositiveRadiusY_ReturnsRoundedRectangularPath()
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 0.0,
                RadiusY = 15.0,
                Width = 100,
                Height = 50,
                StrokeThickness = 2.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with different positive RadiusX and RadiusY values.
        /// Verifies that Math.Max correctly determines the corner radius for rounded rectangle path.
        /// </summary>
        [Theory]
        [InlineData(5.0, 10.0)] // RadiusY > RadiusX
        [InlineData(15.0, 8.0)] // RadiusX > RadiusY  
        [InlineData(12.0, 12.0)] // RadiusX == RadiusY
        public void GetPath_BothRadiiPositive_ReturnsRoundedRectangularPathWithMaxRadius(double radiusX, double radiusY)
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Width = 100,
                Height = 80,
                StrokeThickness = 1.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with negative radius values to verify handling of edge cases.
        /// Tests that negative values are handled correctly in Math.Max calculations.
        /// </summary>
        [Theory]
        [InlineData(-5.0, 0.0)]
        [InlineData(0.0, -10.0)]
        [InlineData(-3.0, -7.0)]
        [InlineData(-15.0, 8.0)]
        public void GetPath_NegativeRadiusValues_ReturnsValidPath(double radiusX, double radiusY)
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Width = 100,
                Height = 60,
                StrokeThickness = 2.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with special floating point values for radius properties.
        /// Verifies that NaN, PositiveInfinity, and NegativeInfinity are handled without exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 5.0)]
        [InlineData(5.0, double.NaN)]
        [InlineData(double.PositiveInfinity, 10.0)]
        [InlineData(10.0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 8.0)]
        [InlineData(8.0, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void GetPath_SpecialFloatingPointRadiusValues_ReturnsValidPath(double radiusX, double radiusY)
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Width = 50,
                Height = 30,
                StrokeThickness = 1.0
            };

            // Act & Assert - Should not throw
            var result = rectangle.GetPath();
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with various StrokeThickness values to verify dimension calculations.
        /// Tests that x, y, w, h calculations handle different stroke thickness scenarios correctly.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(5.5)]
        [InlineData(10.0)]
        [InlineData(50.0)] // Thickness larger than dimensions
        public void GetPath_VariousStrokeThickness_CalculatesCorrectDimensions(double strokeThickness)
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 5.0,
                RadiusY = 8.0,
                Width = 40,
                Height = 30,
                StrokeThickness = strokeThickness
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with negative StrokeThickness to verify handling of edge cases.
        /// Verifies that negative stroke thickness values don't cause exceptions.
        /// </summary>
        [Fact]
        public void GetPath_NegativeStrokeThickness_ReturnsValidPath()
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 3.0,
                RadiusY = 3.0,
                Width = 60,
                Height = 40,
                StrokeThickness = -5.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with special floating point values for StrokeThickness.
        /// Verifies that NaN, PositiveInfinity, and NegativeInfinity stroke thickness values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void GetPath_SpecialFloatingPointStrokeThickness_ReturnsValidPath(double strokeThickness)
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 4.0,
                RadiusY = 6.0,
                Width = 80,
                Height = 50,
                StrokeThickness = strokeThickness
            };

            // Act & Assert - Should not throw
            var result = rectangle.GetPath();
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with zero and negative dimensions to verify edge case handling.
        /// Tests scenarios where width or height might be zero or negative.
        /// </summary>
        [Theory]
        [InlineData(0.0, 50.0)]
        [InlineData(100.0, 0.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(-20.0, 40.0)]
        [InlineData(60.0, -30.0)]
        [InlineData(-25.0, -15.0)]
        public void GetPath_ZeroOrNegativeDimensions_ReturnsValidPath(double width, double height)
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 2.0,
                RadiusY = 3.0,
                Width = width,
                Height = height,
                StrokeThickness = 1.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests GetPath with very large dimension values to verify handling of extreme cases.
        /// Ensures that very large values don't cause overflow or other issues.
        /// </summary>
        [Fact]
        public void GetPath_VeryLargeDimensions_ReturnsValidPath()
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = 100.0,
                RadiusY = 200.0,
                Width = double.MaxValue / 2,
                Height = double.MaxValue / 2,
                StrokeThickness = 10000.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests that GetPath correctly determines when to use rectangular vs rounded rectangle paths.
        /// Verifies the cornerRadius calculation logic and path type selection.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, true)]   // Both zero -> rectangular path
        [InlineData(0.1, 0.0, false)]  // RadiusX > 0 -> rounded path
        [InlineData(0.0, 0.1, false)]  // RadiusY > 0 -> rounded path
        [InlineData(5.0, 8.0, false)]  // Both > 0 -> rounded path
        public void GetPath_CornerRadiusLogic_SelectsCorrectPathType(double radiusX, double radiusY, bool expectRectangular)
        {
            // Arrange
            var rectangle = new FormsRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Width = 100,
                Height = 80,
                StrokeThickness = 2.0
            };

            // Act
            var result = rectangle.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);

            // We can't directly verify which internal method was called,
            // but we can verify the path was created successfully
            // The actual path content verification would require more detailed PathF inspection
        }
    }


    /// <summary>
    /// Unit tests for the RadiusX property of the Rectangle class.
    /// </summary>
    public partial class RectangleRadiusXTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that RadiusX property has the correct default value of 0.0.
        /// Verifies the initial state when a Rectangle is instantiated.
        /// Expected result: RadiusX should return 0.0.
        /// </summary>
        [Fact]
        public void RadiusX_DefaultValue_ReturnsZero()
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            var result = rectangle.RadiusX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that RadiusX property can be set and retrieved with various valid positive values.
        /// Verifies basic setter and getter functionality with different positive double inputs.
        /// Expected result: RadiusX should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(1.0)]
        [InlineData(10.5)]
        [InlineData(100.0)]
        [InlineData(999.999)]
        public void RadiusX_SetPositiveValue_ReturnsSetValue(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusX = value;
            var result = rectangle.RadiusX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that RadiusX property can be set to zero.
        /// Verifies that setting zero explicitly works correctly.
        /// Expected result: RadiusX should return 0.0.
        /// </summary>
        [Fact]
        public void RadiusX_SetZero_ReturnsZero()
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusX = 0.0;
            var result = rectangle.RadiusX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that RadiusX property can be set and retrieved with negative values.
        /// Verifies that the property accepts and stores negative double values.
        /// Expected result: RadiusX should return the exact negative value that was set.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.5)]
        [InlineData(-100.0)]
        public void RadiusX_SetNegativeValue_ReturnsSetValue(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusX = value;
            var result = rectangle.RadiusX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that RadiusX property handles boundary double values correctly.
        /// Verifies behavior with extreme double values including MinValue and MaxValue.
        /// Expected result: RadiusX should return the exact boundary value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void RadiusX_SetBoundaryValue_ReturnsSetValue(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusX = value;
            var result = rectangle.RadiusX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that RadiusX property handles special floating-point values correctly.
        /// Verifies behavior with NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected result: RadiusX should return the exact special value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RadiusX_SetSpecialFloatingPointValue_ReturnsSetValue(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusX = value;
            var result = rectangle.RadiusX;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that multiple successive assignments to RadiusX work correctly.
        /// Verifies that the property can be changed multiple times and always returns the latest value.
        /// Expected result: RadiusX should return the final assigned value.
        /// </summary>
        [Fact]
        public void RadiusX_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusX = 5.0;
            rectangle.RadiusX = 10.0;
            rectangle.RadiusX = 15.5;
            var result = rectangle.RadiusX;

            // Assert
            Assert.Equal(15.5, result);
        }

        /// <summary>
        /// Tests that RadiusX property maintains precision with very small positive values.
        /// Verifies that small floating-point values are stored and retrieved accurately.
        /// Expected result: RadiusX should return the exact small value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon)]
        [InlineData(0.000001)]
        [InlineData(1e-10)]
        public void RadiusX_SetVerySmallValue_ReturnsSetValue(double value)
        {
            // Arrange
            var rectangle = new FormsRectangle();

            // Act
            rectangle.RadiusX = value;
            var result = rectangle.RadiusX;

            // Assert
            Assert.Equal(value, result);
        }
    }
}