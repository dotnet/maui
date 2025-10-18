using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ShapeExtensionsTests
    {
        /// <summary>
        /// Tests that GetPathWindingMode returns NonZero when drawable is null.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_DrawableIsNull_ReturnsNonZero()
        {
            // Arrange
            IDrawable drawable = null;
            var shapeView = Substitute.For<IShapeView>();

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.NonZero, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns NonZero when drawable is not a ShapeDrawable.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_DrawableIsNotShapeDrawable_ReturnsNonZero()
        {
            // Arrange
            var drawable = Substitute.For<IDrawable>();
            var shapeView = Substitute.For<IShapeView>();

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.NonZero, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns NonZero when shapeView is null.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_ShapeViewIsNull_ReturnsNonZero()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            IShapeView shapeView = null;

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.NonZero, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns NonZero when shapeView.Shape is null.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_ShapeViewShapeIsNull_ReturnsNonZero()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            shapeView.Shape.Returns((IShape)null);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.NonZero, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns NonZero when shapeView.Shape is not a Path.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_ShapeViewShapeIsNotPath_ReturnsNonZero()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            var shape = Substitute.For<IShape>(); // Not a Path
            shapeView.Shape.Returns(shape);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.NonZero, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns EvenOdd when Path.Data is null (uses default FillRule.EvenOdd).
        /// </summary>
        [Fact]
        public void GetPathWindingMode_PathDataIsNull_ReturnsEvenOdd()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            var path = new Path { Data = null };
            shapeView.Shape.Returns(path);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.EvenOdd, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns EvenOdd when GeometryGroup has EvenOdd FillRule.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_GeometryGroupWithEvenOddFillRule_ReturnsEvenOdd()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            var geometryGroup = new GeometryGroup { FillRule = FillRule.EvenOdd };
            var path = new Path { Data = geometryGroup };
            shapeView.Shape.Returns(path);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.EvenOdd, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns NonZero when GeometryGroup has Nonzero FillRule.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_GeometryGroupWithNonzeroFillRule_ReturnsNonZero()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            var geometryGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
            var path = new Path { Data = geometryGroup };
            shapeView.Shape.Returns(path);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.NonZero, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns EvenOdd when PathGeometry has EvenOdd FillRule.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_PathGeometryWithEvenOddFillRule_ReturnsEvenOdd()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            var pathGeometry = new PathGeometry { FillRule = FillRule.EvenOdd };
            var path = new Path { Data = pathGeometry };
            shapeView.Shape.Returns(path);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.EvenOdd, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns NonZero when PathGeometry has Nonzero FillRule.
        /// </summary>
        [Fact]
        public void GetPathWindingMode_PathGeometryWithNonzeroFillRule_ReturnsNonZero()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            var pathGeometry = new PathGeometry { FillRule = FillRule.Nonzero };
            var path = new Path { Data = pathGeometry };
            shapeView.Shape.Returns(path);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.NonZero, result);
        }

        /// <summary>
        /// Tests that GetPathWindingMode returns EvenOdd when Geometry is neither GeometryGroup nor PathGeometry (uses default FillRule.EvenOdd).
        /// </summary>
        [Fact]
        public void GetPathWindingMode_GeometryIsOtherType_ReturnsEvenOdd()
        {
            // Arrange
            var drawable = new ShapeDrawable();
            var shapeView = Substitute.For<IShapeView>();
            var geometry = new TestGeometry();
            var path = new Path { Data = geometry };
            shapeView.Shape.Returns(path);

            // Act
            var result = drawable.GetPathWindingMode(shapeView);

            // Assert
            Assert.Equal(WindingMode.EvenOdd, result);
        }

    }
}
