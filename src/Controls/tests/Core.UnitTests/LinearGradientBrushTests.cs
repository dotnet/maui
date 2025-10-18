#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class LinearGradientBrushTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();

            Assert.Equal(1.0d, linearGradientBrush.EndPoint.X);
            Assert.Equal(1.0d, linearGradientBrush.EndPoint.Y);
        }

        [Fact]
        public void TestConstructorUsingGradientStopCollection()
        {
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.1f },
                new GradientStop { Color = Colors.Orange, Offset = 0.8f }
            };

            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientStops, new Point(0, 0), new Point(0, 1));

            Assert.NotEmpty(linearGradientBrush.GradientStops);
            Assert.Equal(0.0d, linearGradientBrush.EndPoint.X);
            Assert.Equal(1.0d, linearGradientBrush.EndPoint.Y);
        }

        [Fact]
        public void TestEmptyLinearGradientBrush()
        {
            LinearGradientBrush nullLinearGradientBrush = new LinearGradientBrush();
            Assert.True(nullLinearGradientBrush.IsEmpty);

            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Orange, Offset = 0.1f },
                    new GradientStop { Color = Colors.Red, Offset = 0.8f }
                }
            };

            Assert.False(linearGradientBrush.IsEmpty);
        }

        [Fact]
        public void TestNullOrEmptyLinearGradientBrush()
        {
            LinearGradientBrush nullLinearGradientBrush = null;
            Assert.True(Brush.IsNullOrEmpty(nullLinearGradientBrush));

            LinearGradientBrush emptyLinearGradientBrush = new LinearGradientBrush();
            Assert.True(Brush.IsNullOrEmpty(emptyLinearGradientBrush));

            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Orange, Offset = 0.1f },
                    new GradientStop { Color = Colors.Red, Offset = 0.8f }
                }
            };

            Assert.False(Brush.IsNullOrEmpty(linearGradientBrush));
        }

        [Fact]
        public void TestNullOrEmptyLinearGradientPaintWithEmptyGradientStop()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(),
                    new GradientStop()
                }
            };

            Paint linearGradientPaint = linearGradientBrush;

            Assert.True(linearGradientPaint.IsNullOrEmpty());
        }

        [Fact]
        public void TestNullOrEmptyLinearGradientPaintWithNullGradientStop()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    null,
                    null
                }
            };

            Paint linearGradientPaint = linearGradientBrush;

            Assert.True(linearGradientPaint.IsNullOrEmpty());
        }

        [Fact]
        public void TestNullGradientStopLinearGradientPaint()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Red, Offset = 0.1f },
                    null,
                    new GradientStop { Color = Colors.Blue, Offset = 1.0f }
                }
            };

            Paint linearGradientPaint = linearGradientBrush;

            Assert.False(linearGradientPaint.IsNullOrEmpty());
        }

        [Fact]
        public void TestLinearGradientBrushPoints()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            Assert.Equal(0, linearGradientBrush.StartPoint.X);
            Assert.Equal(0, linearGradientBrush.StartPoint.Y);

            Assert.Equal(1, linearGradientBrush.EndPoint.X);
            Assert.Equal(0, linearGradientBrush.EndPoint.Y);
        }

        [Fact]
        public void TestLinearGradientBrushOnlyOneGradientStop()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Red, }
                },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            Assert.NotNull(linearGradientBrush);
        }

        [Fact]
        public void TestLinearGradientBrushGradientStops()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Colors.Red, Offset = 0.1f },
                    new GradientStop { Color = Colors.Blue, Offset = 1.0f }
                },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            Assert.Equal(2, linearGradientBrush.GradientStops.Count);
        }

        /// <summary>
        /// Tests that the LinearGradientBrush constructor correctly sets the GradientStops property
        /// when provided with a valid GradientStopCollection containing gradient stops.
        /// </summary>
        [Fact]
        public void Constructor_WithValidGradientStopCollection_SetsGradientStopsProperty()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.0f },
                new GradientStop { Color = Colors.Blue, Offset = 1.0f }
            };

            // Act
            var linearGradientBrush = new LinearGradientBrush(gradientStops);

            // Assert
            Assert.Same(gradientStops, linearGradientBrush.GradientStops);
            Assert.Equal(2, linearGradientBrush.GradientStops.Count);
        }

        /// <summary>
        /// Tests that the LinearGradientBrush constructor correctly sets the GradientStops property
        /// when provided with an empty GradientStopCollection.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyGradientStopCollection_SetsGradientStopsProperty()
        {
            // Arrange
            var gradientStops = new GradientStopCollection();

            // Act
            var linearGradientBrush = new LinearGradientBrush(gradientStops);

            // Assert
            Assert.Same(gradientStops, linearGradientBrush.GradientStops);
            Assert.Empty(linearGradientBrush.GradientStops);
        }

        /// <summary>
        /// Tests that the LinearGradientBrush constructor correctly handles a null GradientStopCollection
        /// by setting the GradientStops property to null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullGradientStopCollection_SetsGradientStopsToNull()
        {
            // Arrange
            GradientStopCollection gradientStops = null;

            // Act
            var linearGradientBrush = new LinearGradientBrush(gradientStops);

            // Assert
            Assert.Null(linearGradientBrush.GradientStops);
        }

        /// <summary>
        /// Tests that the LinearGradientBrush constructor correctly sets the GradientStops property
        /// when provided with a GradientStopCollection containing a single gradient stop.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleGradientStop_SetsGradientStopsProperty()
        {
            // Arrange
            var gradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Green, Offset = 0.5f }
            };

            // Act
            var linearGradientBrush = new LinearGradientBrush(gradientStops);

            // Assert
            Assert.Same(gradientStops, linearGradientBrush.GradientStops);
            Assert.Single(linearGradientBrush.GradientStops);
            Assert.Equal(Colors.Green, linearGradientBrush.GradientStops[0].Color);
            Assert.Equal(0.5f, linearGradientBrush.GradientStops[0].Offset);
        }
    }
}