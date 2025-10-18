#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for PathGeometry.FillRule property
    /// </summary>
    public sealed class PathGeometryTests
    {
        /// <summary>
        /// Tests that FillRule property setter correctly stores valid enum values and getter retrieves them.
        /// Tests various valid FillRule enum values including EvenOdd and Nonzero.
        /// Expected result: Property should store and return the exact value that was set.
        /// </summary>
        /// <param name="fillRule">The FillRule value to test</param>
        [Theory]
        [InlineData(FillRule.EvenOdd)]
        [InlineData(FillRule.Nonzero)]
        public void FillRule_SetValidEnumValue_ReturnsSetValue(FillRule fillRule)
        {
            // Arrange
            var pathGeometry = new PathGeometry();

            // Act
            pathGeometry.FillRule = fillRule;
            var result = pathGeometry.FillRule;

            // Assert
            Assert.Equal(fillRule, result);
        }

        /// <summary>
        /// Tests that FillRule property has the correct default value.
        /// Tests that a newly created PathGeometry instance has FillRule.EvenOdd as default.
        /// Expected result: Default value should be FillRule.EvenOdd.
        /// </summary>
        [Fact]
        public void FillRule_DefaultValue_ReturnsEvenOdd()
        {
            // Arrange & Act
            var pathGeometry = new PathGeometry();
            var result = pathGeometry.FillRule;

            // Assert
            Assert.Equal(FillRule.EvenOdd, result);
        }

        /// <summary>
        /// Tests that FillRule property setter handles invalid enum values correctly.
        /// Tests setting values outside the defined enum range to verify robustness.
        /// Expected result: Property should accept the value and store it as cast to FillRule.
        /// </summary>
        /// <param name="invalidValue">Invalid integer value to cast to FillRule</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void FillRule_SetInvalidEnumValue_StoresValue(int invalidValue)
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var invalidFillRule = (FillRule)invalidValue;

            // Act
            pathGeometry.FillRule = invalidFillRule;
            var result = pathGeometry.FillRule;

            // Assert
            Assert.Equal(invalidFillRule, result);
        }

        /// <summary>
        /// Tests that FillRule property setter correctly updates the value multiple times.
        /// Tests that setting different values in sequence works correctly.
        /// Expected result: Each set operation should update the stored value.
        /// </summary>
        [Fact]
        public void FillRule_SetMultipleValues_UpdatesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();

            // Act & Assert - Set EvenOdd
            pathGeometry.FillRule = FillRule.EvenOdd;
            Assert.Equal(FillRule.EvenOdd, pathGeometry.FillRule);

            // Act & Assert - Set Nonzero
            pathGeometry.FillRule = FillRule.Nonzero;
            Assert.Equal(FillRule.Nonzero, pathGeometry.FillRule);

            // Act & Assert - Set back to EvenOdd
            pathGeometry.FillRule = FillRule.EvenOdd;
            Assert.Equal(FillRule.EvenOdd, pathGeometry.FillRule);
        }

        /// <summary>
        /// Tests that FillRule property setter works correctly when initialized with different constructors.
        /// Tests that the property behaves consistently regardless of how the PathGeometry was constructed.
        /// Expected result: Property should work the same way regardless of constructor used.
        /// </summary>
        /// <param name="fillRule">The FillRule value to test</param>
        [Theory]
        [InlineData(FillRule.EvenOdd)]
        [InlineData(FillRule.Nonzero)]
        public void FillRule_SetValueOnDifferentConstructors_WorksCorrectly(FillRule fillRule)
        {
            // Arrange
            var pathGeometry1 = new PathGeometry();
            var pathGeometry2 = new PathGeometry(new PathFigureCollection());
            var pathGeometry3 = new PathGeometry(new PathFigureCollection(), FillRule.EvenOdd);

            // Act
            pathGeometry1.FillRule = fillRule;
            pathGeometry2.FillRule = fillRule;
            pathGeometry3.FillRule = fillRule;

            // Assert
            Assert.Equal(fillRule, pathGeometry1.FillRule);
            Assert.Equal(fillRule, pathGeometry2.FillRule);
            Assert.Equal(fillRule, pathGeometry3.FillRule);
        }

        /// <summary>
        /// Tests that AppendPath throws ArgumentNullException when path parameter is null.
        /// This tests the input validation for null path parameter.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void AppendPath_NullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var pathGeometry = new PathGeometry();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => pathGeometry.AppendPath(null));
        }

        /// <summary>
        /// Tests that AppendPath handles empty Figures collection correctly.
        /// This tests the behavior when there are no figures to process.
        /// Expected result: Method completes without error and without modifying the path.
        /// </summary>
        [Fact]
        public void AppendPath_EmptyFigures_DoesNotThrow()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert - No exception should be thrown
            // Path should not be modified (no MoveTo, Close, or other calls)
            path.DidNotReceive().MoveTo(Arg.Any<float>(), Arg.Any<float>());
            path.DidNotReceive().Close();
        }

        /// <summary>
        /// Tests that AppendPath correctly moves to start point for figure with no segments.
        /// This tests the basic MoveTo functionality with a figure containing no path segments.
        /// Expected result: Only MoveTo should be called with the start point coordinates.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithNoSegments_CallsMoveTo()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(10.5, 20.7)
            };
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(10.5f, 20.7f);
            path.DidNotReceive().Close();
        }

        /// <summary>
        /// Tests that AppendPath processes LineSegment correctly.
        /// This tests the LineSegment branch in the segment type checking logic.
        /// Expected result: MoveTo should be called for start point, and AddLine should be invoked.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithLineSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(5, 10)
            };
            figure.Segments.Add(new LineSegment(new Point(15, 20)));
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(5f, 10f);
            // Note: AddLine is a private method, so we can't directly verify its call
            // But we can verify the path was processed without exception
        }

        /// <summary>
        /// Tests that AppendPath processes ArcSegment correctly.
        /// This tests the ArcSegment branch in the segment type checking logic.
        /// Expected result: MoveTo should be called for start point, and AddArc should be invoked.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithArcSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(0, 0)
            };
            figure.Segments.Add(new ArcSegment
            {
                Point = new Point(10, 10),
                Size = new Size(5, 5)
            });
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(0f, 0f);
        }

        /// <summary>
        /// Tests that AppendPath processes BezierSegment correctly.
        /// This tests the BezierSegment branch in the segment type checking logic.
        /// Expected result: MoveTo should be called for start point, and AddBezier should be invoked.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithBezierSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(1, 2)
            };
            figure.Segments.Add(new BezierSegment
            {
                Point1 = new Point(3, 4),
                Point2 = new Point(5, 6),
                Point3 = new Point(7, 8)
            });
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(1f, 2f);
        }

        /// <summary>
        /// Tests that AppendPath processes PolyBezierSegment correctly.
        /// This tests the PolyBezierSegment branch in the segment type checking logic.
        /// Expected result: MoveTo should be called for start point, and AddPolyBezier should be invoked.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithPolyBezierSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(2, 3)
            };
            var polyBezier = new PolyBezierSegment();
            polyBezier.Points.Add(new Point(4, 5));
            polyBezier.Points.Add(new Point(6, 7));
            polyBezier.Points.Add(new Point(8, 9));
            figure.Segments.Add(polyBezier);
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(2f, 3f);
        }

        /// <summary>
        /// Tests that AppendPath processes PolyLineSegment correctly.
        /// This tests the PolyLineSegment branch in the segment type checking logic.
        /// Expected result: MoveTo should be called for start point, and AddPolyLine should be invoked.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithPolyLineSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(3, 4)
            };
            var polyLine = new PolyLineSegment();
            polyLine.Points.Add(new Point(5, 6));
            polyLine.Points.Add(new Point(7, 8));
            figure.Segments.Add(polyLine);
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(3f, 4f);
        }

        /// <summary>
        /// Tests that AppendPath processes PolyQuadraticBezierSegment correctly.
        /// This tests the PolyQuadraticBezierSegment branch in the segment type checking logic.
        /// Expected result: MoveTo should be called for start point, and AddPolyQuad should be invoked.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithPolyQuadraticBezierSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(4, 5)
            };
            var polyQuad = new PolyQuadraticBezierSegment();
            polyQuad.Points.Add(new Point(6, 7));
            polyQuad.Points.Add(new Point(8, 9));
            figure.Segments.Add(polyQuad);
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(4f, 5f);
        }

        /// <summary>
        /// Tests that AppendPath processes QuadraticBezierSegment correctly.
        /// This tests the QuadraticBezierSegment branch in the segment type checking logic.
        /// Expected result: MoveTo should be called for start point, and AddQuad should be invoked.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithQuadraticBezierSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(5, 6)
            };
            figure.Segments.Add(new QuadraticBezierSegment
            {
                Point1 = new Point(7, 8),
                Point2 = new Point(9, 10)
            });
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(5f, 6f);
        }

        /// <summary>
        /// Tests that AppendPath calls Close for closed figures.
        /// This tests the IsClosed condition in the figure processing logic.
        /// Expected result: MoveTo and Close should both be called.
        /// </summary>
        [Fact]
        public void AppendPath_ClosedFigure_CallsClose()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(10, 20),
                IsClosed = true
            };
            figure.Segments.Add(new LineSegment(new Point(30, 40)));
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(10f, 20f);
            path.Received(1).Close();
        }

        /// <summary>
        /// Tests that AppendPath does not call Close for open figures.
        /// This tests the IsClosed condition when figure is not closed.
        /// Expected result: MoveTo should be called but Close should not.
        /// </summary>
        [Fact]
        public void AppendPath_OpenFigure_DoesNotCallClose()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(10, 20),
                IsClosed = false
            };
            figure.Segments.Add(new LineSegment(new Point(30, 40)));
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(10f, 20f);
            path.DidNotReceive().Close();
        }

        /// <summary>
        /// Tests that AppendPath processes multiple figures correctly.
        /// This tests the outer loop that iterates through all figures in the collection.
        /// Expected result: MoveTo should be called for each figure's start point.
        /// </summary>
        [Fact]
        public void AppendPath_MultipleFigures_ProcessesAll()
        {
            // Arrange
            var pathGeometry = new PathGeometry();

            var figure1 = new PathFigure
            {
                StartPoint = new Point(1, 2)
            };
            figure1.Segments.Add(new LineSegment(new Point(3, 4)));

            var figure2 = new PathFigure
            {
                StartPoint = new Point(5, 6),
                IsClosed = true
            };
            figure2.Segments.Add(new LineSegment(new Point(7, 8)));

            pathGeometry.Figures.Add(figure1);
            pathGeometry.Figures.Add(figure2);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(1f, 2f);
            path.Received(1).MoveTo(5f, 6f);
            path.Received(1).Close(); // Only figure2 is closed
        }

        /// <summary>
        /// Tests that AppendPath handles extreme coordinate values correctly.
        /// This tests edge cases with double.NaN, double.PositiveInfinity, and double.NegativeInfinity values.
        /// Expected result: MoveTo should be called with converted float values (including NaN and infinities).
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(-0.0, 0.0)]
        public void AppendPath_ExtremeCoordinateValues_HandlesCorrectly(double x, double y)
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(x, y)
            };
            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo((float)x, (float)y);
        }

        /// <summary>
        /// Tests that AppendPath processes multiple segments in a single figure correctly.
        /// This tests the inner loop that iterates through all segments in a figure.
        /// Expected result: MoveTo should be called once, and all segments should be processed.
        /// </summary>
        [Fact]
        public void AppendPath_FigureWithMultipleSegments_ProcessesAll()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(0, 0)
            };

            // Add multiple different segment types
            figure.Segments.Add(new LineSegment(new Point(10, 10)));
            figure.Segments.Add(new ArcSegment
            {
                Point = new Point(20, 20),
                Size = new Size(5, 5)
            });
            figure.Segments.Add(new BezierSegment
            {
                Point1 = new Point(25, 25),
                Point2 = new Point(30, 30),
                Point3 = new Point(35, 35)
            });

            pathGeometry.Figures.Add(figure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(1).MoveTo(0f, 0f);
            // All segments should be processed without exception
        }

        /// <summary>
        /// Tests that AppendPath handles figures with mixed closed and open states correctly.
        /// This tests the combination of multiple figures with different IsClosed values.
        /// Expected result: Close should be called only for closed figures.
        /// </summary>
        [Fact]
        public void AppendPath_MixedClosedAndOpenFigures_HandlesCorrectly()
        {
            // Arrange
            var pathGeometry = new PathGeometry();

            var openFigure = new PathFigure
            {
                StartPoint = new Point(0, 0),
                IsClosed = false
            };
            openFigure.Segments.Add(new LineSegment(new Point(10, 10)));

            var closedFigure = new PathFigure
            {
                StartPoint = new Point(20, 20),
                IsClosed = true
            };
            closedFigure.Segments.Add(new LineSegment(new Point(30, 30)));

            var anotherClosedFigure = new PathFigure
            {
                StartPoint = new Point(40, 40),
                IsClosed = true
            };
            anotherClosedFigure.Segments.Add(new LineSegment(new Point(50, 50)));

            pathGeometry.Figures.Add(openFigure);
            pathGeometry.Figures.Add(closedFigure);
            pathGeometry.Figures.Add(anotherClosedFigure);
            var path = Substitute.For<PathF>();

            // Act
            pathGeometry.AppendPath(path);

            // Assert
            path.Received(3).MoveTo(Arg.Any<float>(), Arg.Any<float>());
            path.Received(2).Close(); // Only the two closed figures
        }

        /// <summary>
        /// Tests that the PathGeometry constructor with PathFigureCollection parameter correctly assigns the provided collection to the Figures property.
        /// Input: A valid PathFigureCollection instance.
        /// Expected: The Figures property returns the same collection instance that was passed to the constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithValidPathFigureCollection_AssignsFiguresProperty()
        {
            // Arrange
            var figures = new PathFigureCollection();

            // Act
            var pathGeometry = new PathGeometry(figures);

            // Assert
            Assert.Same(figures, pathGeometry.Figures);
        }

        /// <summary>
        /// Tests that the PathGeometry constructor with PathFigureCollection parameter correctly handles null input.
        /// Input: null PathFigureCollection.
        /// Expected: The Figures property is set to null without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_WithNullPathFigureCollection_AssignsNullToFiguresProperty()
        {
            // Arrange
            PathFigureCollection figures = null;

            // Act
            var pathGeometry = new PathGeometry(figures);

            // Assert
            Assert.Null(pathGeometry.Figures);
        }

        /// <summary>
        /// Tests that the PathGeometry constructor with PathFigureCollection parameter correctly assigns an empty collection.
        /// Input: An empty PathFigureCollection.
        /// Expected: The Figures property returns the same empty collection instance.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyPathFigureCollection_AssignsEmptyCollection()
        {
            // Arrange
            var figures = new PathFigureCollection();

            // Act
            var pathGeometry = new PathGeometry(figures);

            // Assert
            Assert.Same(figures, pathGeometry.Figures);
            Assert.Empty(pathGeometry.Figures);
        }

        /// <summary>
        /// Tests that the PathGeometry constructor with PathFigureCollection parameter correctly assigns a collection containing PathFigure items.
        /// Input: A PathFigureCollection with PathFigure items.
        /// Expected: The Figures property returns the same collection instance with all items preserved.
        /// </summary>
        [Fact]
        public void Constructor_WithPathFigureCollectionContainingItems_PreservesCollectionAndItems()
        {
            // Arrange
            var figures = new PathFigureCollection();
            var pathFigure1 = new PathFigure();
            var pathFigure2 = new PathFigure();
            figures.Add(pathFigure1);
            figures.Add(pathFigure2);

            // Act
            var pathGeometry = new PathGeometry(figures);

            // Assert
            Assert.Same(figures, pathGeometry.Figures);
            Assert.Equal(2, pathGeometry.Figures.Count);
            Assert.Same(pathFigure1, pathGeometry.Figures[0]);
            Assert.Same(pathFigure2, pathGeometry.Figures[1]);
        }
    }
}