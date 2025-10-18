#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class GraphicsViewTests
    {
        /// <summary>
        /// Tests that calling Invalidate when Handler is null does not throw an exception.
        /// This verifies the null-conditional operator works correctly.
        /// </summary>
        [Fact]
        public void Invalidate_WhenHandlerIsNull_DoesNotThrowException()
        {
            // Arrange
            var graphicsView = new GraphicsView();

            // Ensure Handler is null (default state)
            Assert.Null(graphicsView.Handler);

            // Act & Assert
            var exception = Record.Exception(() => graphicsView.Invalidate());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that calling Invalidate when Handler is not null invokes the Handler's Invoke method
        /// with the correct command parameter ("Invalidate") and null arguments.
        /// </summary>
        [Fact]
        public void Invalidate_WhenHandlerIsNotNull_CallsInvokeWithCorrectParameters()
        {
            // Arrange
            var graphicsView = new GraphicsView();
            var mockHandler = Substitute.For<IViewHandler>();
            graphicsView.Handler = mockHandler;

            // Act
            graphicsView.Invalidate();

            // Assert
            mockHandler.Received(1).Invoke("Invalidate", null);
        }

        /// <summary>
        /// Tests that setting a valid IDrawable instance to the Drawable property stores the value correctly.
        /// </summary>
        [Fact]
        public void Drawable_SetValidValue_StoresValueCorrectly()
        {
            // Arrange
            var graphicsView = new GraphicsView();
            var drawable = Substitute.For<IDrawable>();

            // Act
            graphicsView.Drawable = drawable;

            // Assert
            Assert.Same(drawable, graphicsView.Drawable);
        }

        /// <summary>
        /// Tests that setting null to the Drawable property stores null correctly.
        /// </summary>
        [Fact]
        public void Drawable_SetNullValue_StoresNullCorrectly()
        {
            // Arrange
            var graphicsView = new GraphicsView();
            var drawable = Substitute.For<IDrawable>();
            graphicsView.Drawable = drawable; // First set a non-null value

            // Act
            graphicsView.Drawable = null;

            // Assert
            Assert.Null(graphicsView.Drawable);
        }

        /// <summary>
        /// Tests that getting the Drawable property when no value has been set returns null (default value).
        /// </summary>
        [Fact]
        public void Drawable_GetDefaultValue_ReturnsNull()
        {
            // Arrange
            var graphicsView = new GraphicsView();

            // Act
            var result = graphicsView.Drawable;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that multiple set and get operations maintain correct values for the Drawable property.
        /// </summary>
        [Fact]
        public void Drawable_MultipleSetGetOperations_MaintainsCorrectValues()
        {
            // Arrange
            var graphicsView = new GraphicsView();
            var drawable1 = Substitute.For<IDrawable>();
            var drawable2 = Substitute.For<IDrawable>();

            // Act & Assert - First drawable
            graphicsView.Drawable = drawable1;
            Assert.Same(drawable1, graphicsView.Drawable);

            // Act & Assert - Second drawable
            graphicsView.Drawable = drawable2;
            Assert.Same(drawable2, graphicsView.Drawable);

            // Act & Assert - Back to null
            graphicsView.Drawable = null;
            Assert.Null(graphicsView.Drawable);

            // Act & Assert - Back to first drawable
            graphicsView.Drawable = drawable1;
            Assert.Same(drawable1, graphicsView.Drawable);
        }

        /// <summary>
        /// Tests that the Drawable property uses the correct BindableProperty for its implementation.
        /// </summary>
        [Fact]
        public void Drawable_PropertyImplementation_UsesCorrectBindableProperty()
        {
            // Arrange
            var graphicsView = new GraphicsView();
            var drawable = Substitute.For<IDrawable>();

            // Act
            graphicsView.SetValue(GraphicsView.DrawableProperty, drawable);
            var getResult = graphicsView.Drawable;

            graphicsView.Drawable = drawable;
            var getValueResult = (IDrawable)graphicsView.GetValue(GraphicsView.DrawableProperty);

            // Assert
            Assert.Same(drawable, getResult);
            Assert.Same(drawable, getValueResult);
        }
    }
}

namespace Microsoft.Maui.Controls.UnitTests
{
    public class TouchEventArgsTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates an instance with default property values.
        /// Verifies that IsInsideBounds is false and Touches is null by default.
        /// </summary>
        [Fact]
        public void Constructor_WhenCalledWithoutParameters_CreatesInstanceWithDefaultValues()
        {
            // Arrange & Act
            var touchEventArgs = new TouchEventArgs();

            // Assert
            Assert.NotNull(touchEventArgs);
            Assert.False(touchEventArgs.IsInsideBounds);
            Assert.Null(touchEventArgs.Touches);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with valid points array and isInsideBounds true.
        /// Verifies that properties are correctly assigned.
        /// </summary>
        [Fact]
        public void Constructor_ValidPointsArrayAndTrueIsInsideBounds_SetsPropertiesCorrectly()
        {
            // Arrange
            var points = new PointF[] { new PointF(1.0f, 2.0f), new PointF(3.0f, 4.0f) };
            var isInsideBounds = true;

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Same(points, touchEventArgs.Touches);
            Assert.True(touchEventArgs.IsInsideBounds);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with valid points array and isInsideBounds false.
        /// Verifies that properties are correctly assigned.
        /// </summary>
        [Fact]
        public void Constructor_ValidPointsArrayAndFalseIsInsideBounds_SetsPropertiesCorrectly()
        {
            // Arrange
            var points = new PointF[] { new PointF(5.0f, 6.0f) };
            var isInsideBounds = false;

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Same(points, touchEventArgs.Touches);
            Assert.False(touchEventArgs.IsInsideBounds);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with null points array.
        /// Verifies that null is accepted and assigned to Touches property.
        /// </summary>
        [Fact]
        public void Constructor_NullPointsArray_AcceptsNullAndSetsProperty()
        {
            // Arrange
            PointF[] points = null;
            var isInsideBounds = true;

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Null(touchEventArgs.Touches);
            Assert.True(touchEventArgs.IsInsideBounds);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with empty points array.
        /// Verifies that empty array is accepted and assigned correctly.
        /// </summary>
        [Fact]
        public void Constructor_EmptyPointsArray_SetsPropertiesCorrectly()
        {
            // Arrange
            var points = new PointF[0];
            var isInsideBounds = false;

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Same(points, touchEventArgs.Touches);
            Assert.Empty(touchEventArgs.Touches);
            Assert.False(touchEventArgs.IsInsideBounds);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with single point array.
        /// Verifies that single-element array is handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_SinglePointArray_SetsPropertiesCorrectly()
        {
            // Arrange
            var points = new PointF[] { new PointF(10.5f, 20.5f) };
            var isInsideBounds = true;

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Same(points, touchEventArgs.Touches);
            Assert.Single(touchEventArgs.Touches);
            Assert.Equal(10.5f, touchEventArgs.Touches[0].X);
            Assert.Equal(20.5f, touchEventArgs.Touches[0].Y);
            Assert.True(touchEventArgs.IsInsideBounds);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with points containing extreme float values.
        /// Verifies that extreme values like infinity and NaN are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(float.MaxValue, float.MinValue, true)]
        [InlineData(float.PositiveInfinity, float.NegativeInfinity, false)]
        [InlineData(float.NaN, 0.0f, true)]
        [InlineData(0.0f, float.NaN, false)]
        public void Constructor_PointsWithExtremeValues_SetsPropertiesCorrectly(float x, float y, bool isInsideBounds)
        {
            // Arrange
            var points = new PointF[] { new PointF(x, y) };

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Same(points, touchEventArgs.Touches);
            Assert.Equal(x, touchEventArgs.Touches[0].X);
            Assert.Equal(y, touchEventArgs.Touches[0].Y);
            Assert.Equal(isInsideBounds, touchEventArgs.IsInsideBounds);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with large points array.
        /// Verifies that arrays with many points are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_LargePointsArray_SetsPropertiesCorrectly()
        {
            // Arrange
            var points = new PointF[1000];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new PointF(i, i * 2.0f);
            }
            var isInsideBounds = true;

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Same(points, touchEventArgs.Touches);
            Assert.Equal(1000, touchEventArgs.Touches.Length);
            Assert.Equal(0.0f, touchEventArgs.Touches[0].X);
            Assert.Equal(999.0f, touchEventArgs.Touches[999].X);
            Assert.Equal(1998.0f, touchEventArgs.Touches[999].Y);
            Assert.True(touchEventArgs.IsInsideBounds);
        }

        /// <summary>
        /// Tests the TouchEventArgs constructor with zero points (PointF.Zero).
        /// Verifies that zero coordinates are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_ZeroPoints_SetsPropertiesCorrectly()
        {
            // Arrange
            var points = new PointF[] { PointF.Zero, PointF.Zero };
            var isInsideBounds = false;

            // Act
            var touchEventArgs = new TouchEventArgs(points, isInsideBounds);

            // Assert
            Assert.Same(points, touchEventArgs.Touches);
            Assert.Equal(2, touchEventArgs.Touches.Length);
            Assert.Equal(0.0f, touchEventArgs.Touches[0].X);
            Assert.Equal(0.0f, touchEventArgs.Touches[0].Y);
            Assert.Equal(0.0f, touchEventArgs.Touches[1].X);
            Assert.Equal(0.0f, touchEventArgs.Touches[1].Y);
            Assert.False(touchEventArgs.IsInsideBounds);
        }
    }
}