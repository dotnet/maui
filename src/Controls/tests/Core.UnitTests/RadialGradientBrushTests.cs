#nullable disable

using System;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class RadialGradientBrushTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush();

            int gradientStops = radialGradientBrush.GradientStops.Count;

            Assert.Equal(0, gradientStops);
        }

        [Fact]
        public void TestNullOrEmptyRadialGradientPaintWithEmptyGradientStop()
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush
            {
                Center = new Point(0, 0),
                Radius = 10,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(),
                    new GradientStop()
                }
            };

            Paint radialGradientPaint = radialGradientBrush;

            Assert.True(radialGradientPaint.IsNullOrEmpty());
        }

        [Fact]
        public void TestNullOrEmptyRadialGradientPaintWithNullGradientStop()
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush
            {
                Center = new Point(0, 0),
                Radius = 10,
                GradientStops = new GradientStopCollection
                {
                    null,
                    null
                }
            };

            Paint radialGradientPaint = radialGradientBrush;

            Assert.True(radialGradientPaint.IsNullOrEmpty());
        }

        [Fact]
        public void TestConstructorUsingGradientStopCollection()
        {
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.1f },
                new GradientStop { Color = Colors.Orange, Offset = 0.8f }
            };

            RadialGradientBrush radialGradientBrush = new RadialGradientBrush(gradientStops, new Point(0, 0), 10);

            Assert.NotEmpty(radialGradientBrush.GradientStops);
            Assert.Equal(0, radialGradientBrush.Center.X);
            Assert.Equal(0, radialGradientBrush.Center.Y);
            Assert.Equal(10, radialGradientBrush.Radius);
        }

        [Fact]
        public void TestEmptyRadialGradientBrush()
        {
            RadialGradientBrush nullRadialGradientBrush = new RadialGradientBrush();
            Assert.True(nullRadialGradientBrush.IsEmpty);

            RadialGradientBrush radialGradientBrush = new RadialGradientBrush
            {
                Center = new Point(0, 0),
                Radius = 10,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Orange, Offset = 0.1f },
                    new GradientStop { Color = Colors.Red, Offset = 0.8f }
                }
            };

            Assert.False(radialGradientBrush.IsEmpty);
        }

        [Fact]
        public void TestNullOrEmptyRadialGradientBrush()
        {
            RadialGradientBrush nullRadialGradientBrush = null;
            Assert.True(Brush.IsNullOrEmpty(nullRadialGradientBrush));

            RadialGradientBrush emptyRadialGradientBrush = new RadialGradientBrush();
            Assert.True(Brush.IsNullOrEmpty(emptyRadialGradientBrush));

            RadialGradientBrush radialGradientBrush = new RadialGradientBrush
            {
                Center = new Point(0, 0),
                Radius = 10,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Orange, Offset = 0.1f },
                    new GradientStop { Color = Colors.Red, Offset = 0.8f }
                }
            };

            Assert.False(Brush.IsNullOrEmpty(radialGradientBrush));
        }

        [Fact]
        public void TestRadialGradientBrushRadius()
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush();
            radialGradientBrush.Radius = 20;

            Assert.Equal(20, radialGradientBrush.Radius);
        }

        [Fact]
        public void TestRadialGradientBrushOnlyOneGradientStop()
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Red, }
                },
                Radius = 20
            };

            Assert.NotNull(radialGradientBrush);
        }

        [Fact]
        public void TestRadialGradientBrushGradientStops()
        {
            RadialGradientBrush radialGradientBrush = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Red, Offset = 0.1f },
                    new GradientStop { Color = Colors.Blue, Offset = 1.0f }
                },
                Radius = 20
            };

            Assert.Equal(2, radialGradientBrush.GradientStops.Count);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with valid GradientStopCollection and positive radius.
        /// Verifies that both GradientStops and Radius properties are correctly set.
        /// </summary>
        [Fact]
        public void RadialGradientBrush_ValidGradientStopsAndPositiveRadius_SetsPropertiesCorrectly()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.0f },
                new GradientStop { Color = Colors.Blue, Offset = 1.0f }
            };
            double radius = 0.75;

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
            Assert.Equal(2, brush.GradientStops.Count);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with empty GradientStopCollection and valid radius.
        /// Verifies that empty collection is accepted and radius is set correctly.
        /// </summary>
        [Fact]
        public void RadialGradientBrush_EmptyGradientStopsAndValidRadius_SetsPropertiesCorrectly()
        {
            // Arrange
            var gradientStops = new GradientStopCollection();
            double radius = 1.0;

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
            Assert.Empty(brush.GradientStops);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with null GradientStopCollection.
        /// Verifies that null collection is handled and radius is set correctly.
        /// </summary>
        [Fact]
        public void RadialGradientBrush_NullGradientStopsAndValidRadius_SetsPropertiesCorrectly()
        {
            // Arrange
            GradientStopCollection gradientStops = null;
            double radius = 0.5;

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with zero radius.
        /// Verifies that zero radius is accepted and properties are set correctly.
        /// </summary>
        [Fact]
        public void RadialGradientBrush_ValidGradientStopsAndZeroRadius_SetsPropertiesCorrectly()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Green, Offset = 0.5f }
            };
            double radius = 0.0;

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with negative radius.
        /// Verifies that negative radius is handled and properties are set correctly.
        /// </summary>
        [Fact]
        public void RadialGradientBrush_ValidGradientStopsAndNegativeRadius_SetsPropertiesCorrectly()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Yellow, Offset = 0.2f }
            };
            double radius = -1.5;

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with extreme double values for radius.
        /// Verifies that extreme values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void RadialGradientBrush_ValidGradientStopsAndExtremeRadius_SetsPropertiesCorrectly(double radius)
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Purple, Offset = 0.0f }
            };

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with single gradient stop and various radius values.
        /// Verifies that single-item collections work correctly with different radius values.
        /// </summary>
        [Theory]
        [InlineData(0.1)]
        [InlineData(1.0)]
        [InlineData(10.0)]
        [InlineData(100.0)]
        public void RadialGradientBrush_SingleGradientStopAndVariousRadius_SetsPropertiesCorrectly(double radius)
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Black, Offset = 1.0f }
            };

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
            Assert.Single(brush.GradientStops);
        }

        /// <summary>
        /// Tests the RadialGradientBrush constructor with multiple gradient stops and standard radius.
        /// Verifies that multi-item collections are preserved correctly.
        /// </summary>
        [Fact]
        public void RadialGradientBrush_MultipleGradientStopsAndStandardRadius_SetsPropertiesCorrectly()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.0f },
                new GradientStop { Color = Colors.Green, Offset = 0.33f },
                new GradientStop { Color = Colors.Blue, Offset = 0.66f },
                new GradientStop { Color = Colors.White, Offset = 1.0f }
            };
            double radius = 2.5;

            // Act
            var brush = new RadialGradientBrush(gradientStops, radius);

            // Assert
            Assert.Equal(gradientStops, brush.GradientStops);
            Assert.Equal(radius, brush.Radius);
            Assert.Equal(4, brush.GradientStops.Count);
        }

        /// <summary>
        /// Tests that the RadialGradientBrush constructor properly assigns a valid GradientStopCollection with multiple stops.
        /// Verifies that the GradientStops property is correctly set and contains the expected stops.
        /// </summary>
        [Fact]
        public void Constructor_WithValidGradientStopCollection_AssignsGradientStops()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.0f },
                new GradientStop { Color = Colors.Blue, Offset = 1.0f }
            };

            // Act
            var radialGradientBrush = new RadialGradientBrush(gradientStops);

            // Assert
            Assert.NotNull(radialGradientBrush.GradientStops);
            Assert.Equal(2, radialGradientBrush.GradientStops.Count);
            Assert.Equal(Colors.Red, radialGradientBrush.GradientStops[0].Color);
            Assert.Equal(0.0f, radialGradientBrush.GradientStops[0].Offset);
            Assert.Equal(Colors.Blue, radialGradientBrush.GradientStops[1].Color);
            Assert.Equal(1.0f, radialGradientBrush.GradientStops[1].Offset);
        }

        /// <summary>
        /// Tests that the RadialGradientBrush constructor properly assigns an empty GradientStopCollection.
        /// Verifies that the GradientStops property is correctly set to the empty collection.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyGradientStopCollection_AssignsEmptyGradientStops()
        {
            // Arrange
            var gradientStops = new GradientStopCollection();

            // Act
            var radialGradientBrush = new RadialGradientBrush(gradientStops);

            // Assert
            Assert.NotNull(radialGradientBrush.GradientStops);
            Assert.Empty(radialGradientBrush.GradientStops);
            Assert.Same(gradientStops, radialGradientBrush.GradientStops);
        }

        /// <summary>
        /// Tests that the RadialGradientBrush constructor handles null GradientStopCollection parameter.
        /// Verifies the behavior when null is passed as the gradientStops parameter.
        /// </summary>
        [Fact]
        public void Constructor_WithNullGradientStopCollection_AssignsNull()
        {
            // Arrange
            GradientStopCollection gradientStops = null;

            // Act
            var radialGradientBrush = new RadialGradientBrush(gradientStops);

            // Assert
            Assert.Null(radialGradientBrush.GradientStops);
        }

        /// <summary>
        /// Tests that the RadialGradientBrush constructor properly assigns a single GradientStop.
        /// Verifies that the GradientStops property contains exactly one stop with correct values.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleGradientStop_AssignsSingleStop()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Green, Offset = 0.5f }
            };

            // Act
            var radialGradientBrush = new RadialGradientBrush(gradientStops);

            // Assert
            Assert.NotNull(radialGradientBrush.GradientStops);
            Assert.Single(radialGradientBrush.GradientStops);
            Assert.Equal(Colors.Green, radialGradientBrush.GradientStops[0].Color);
            Assert.Equal(0.5f, radialGradientBrush.GradientStops[0].Offset);
        }
    }
}