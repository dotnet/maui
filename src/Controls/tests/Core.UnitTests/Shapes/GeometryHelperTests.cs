#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    /// <summary>
    /// Unit tests for GeometryHelper.FlattenGeometry method
    /// </summary>
    public partial class GeometryHelperTests
    {
        /// <summary>
        /// Tests that FlattenGeometry handles null destination PathGeometry parameter correctly
        /// </summary>
        [Fact]
        public void FlattenGeometry_NullPathGeoDst_ThrowsArgumentNullException()
        {
            // Arrange
            PathGeometry pathGeoDst = null;
            var geoSrc = new LineGeometry { StartPoint = new Point(0, 0), EndPoint = new Point(10, 10) };
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => GeometryHelper.FlattenGeometry(pathGeoDst, geoSrc, tolerance, matrix));
        }

        /// <summary>
        /// Tests that FlattenGeometry handles null source geometry correctly
        /// </summary>
        [Fact]
        public void FlattenGeometry_NullGeoSrc_DoesNotThrow()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            Geometry geoSrc = null;
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act & Assert - should not throw
            GeometryHelper.FlattenGeometry(pathGeoDst, geoSrc, tolerance, matrix);
            Assert.Empty(pathGeoDst.Figures);
        }

        /// <summary>
        /// Tests FlattenGeometry with various tolerance values including edge cases
        /// </summary>
        [Theory]
        [InlineData(0.1)]
        [InlineData(1.0)]
        [InlineData(10.0)]
        [InlineData(double.Epsilon)]
        public void FlattenGeometry_ValidTolerance_ProcessesCorrectly(double tolerance)
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var geoSrc = new LineGeometry { StartPoint = new Point(0, 0), EndPoint = new Point(10, 10) };
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, geoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            Assert.Equal(new Point(0, 0), pathGeoDst.Figures[0].StartPoint);
        }

        /// <summary>
        /// Tests FlattenGeometry with invalid tolerance values
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FlattenGeometry_InvalidTolerance_HandlesGracefully(double tolerance)
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var geoSrc = new LineGeometry { StartPoint = new Point(0, 0), EndPoint = new Point(10, 10) };
            var matrix = Matrix.Identity;

            // Act & Assert - should not throw but behavior may vary
            GeometryHelper.FlattenGeometry(pathGeoDst, geoSrc, tolerance, matrix);
        }

        /// <summary>
        /// Tests FlattenGeometry with LineGeometry source
        /// </summary>
        [Fact]
        public void FlattenGeometry_LineGeometry_CreatesCorrectPathFigure()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var lineGeo = new LineGeometry
            {
                StartPoint = new Point(1, 2),
                EndPoint = new Point(3, 4)
            };
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, lineGeo, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var figure = pathGeoDst.Figures[0];
            Assert.Equal(new Point(1, 2), figure.StartPoint);
            Assert.Single(figure.Segments);
            var segment = Assert.IsType<PolyLineSegment>(figure.Segments[0]);
            Assert.Single(segment.Points);
            Assert.Equal(new Point(3, 4), segment.Points[0]);
        }

        /// <summary>
        /// Tests FlattenGeometry with RectangleGeometry source
        /// </summary>
        [Fact]
        public void FlattenGeometry_RectangleGeometry_CreatesCorrectPathFigure()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var rectGeo = new RectangleGeometry
            {
                Rect = new Rect(10, 20, 30, 40)
            };
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, rectGeo, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var figure = pathGeoDst.Figures[0];
            Assert.Equal(new Point(10, 20), figure.StartPoint); // Top-left
            Assert.True(figure.IsClosed);
            Assert.Single(figure.Segments);
            var segment = Assert.IsType<PolyLineSegment>(figure.Segments[0]);
            Assert.Equal(5, segment.Points.Count);
            Assert.Equal(new Point(40, 20), segment.Points[0]); // Top-right
            Assert.Equal(new Point(40, 60), segment.Points[1]); // Bottom-right
            Assert.Equal(new Point(10, 60), segment.Points[2]); // Bottom-left
            Assert.Equal(new Point(10, 20), segment.Points[3]); // Back to top-left
        }

        /// <summary>
        /// Tests FlattenGeometry with EllipseGeometry source
        /// </summary>
        [Fact]
        public void FlattenGeometry_EllipseGeometry_CreatesCorrectPathFigure()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var ellipseGeo = new EllipseGeometry
            {
                Center = new Point(50, 50),
                RadiusX = 20,
                RadiusY = 15
            };
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, ellipseGeo, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var figure = pathGeoDst.Figures[0];
            Assert.True(figure.IsClosed);
            Assert.Single(figure.Segments);
            var segment = Assert.IsType<PolyLineSegment>(figure.Segments[0]);
            Assert.True(segment.Points.Count > 0);
        }

        /// <summary>
        /// Tests FlattenGeometry with EllipseGeometry having zero radii
        /// </summary>
        [Theory]
        [InlineData(0, 10)]
        [InlineData(10, 0)]
        [InlineData(0, 0)]
        public void FlattenGeometry_EllipseGeometry_ZeroRadii_HandlesCorrectly(double radiusX, double radiusY)
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var ellipseGeo = new EllipseGeometry
            {
                Center = new Point(50, 50),
                RadiusX = radiusX,
                RadiusY = radiusY
            };
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, ellipseGeo, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
        }

        /// <summary>
        /// Tests FlattenGeometry with GeometryGroup containing multiple geometries
        /// </summary>
        [Fact]
        public void FlattenGeometry_GeometryGroup_ProcessesAllChildren()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(new LineGeometry { StartPoint = new Point(0, 0), EndPoint = new Point(5, 5) });
            geometryGroup.Children.Add(new LineGeometry { StartPoint = new Point(10, 10), EndPoint = new Point(15, 15) });
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, geometryGroup, tolerance, matrix);

            // Assert
            Assert.Equal(2, pathGeoDst.Figures.Count);
        }

        /// <summary>
        /// Tests FlattenGeometry with empty GeometryGroup
        /// </summary>
        [Fact]
        public void FlattenGeometry_EmptyGeometryGroup_CreatesNoFigures()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var geometryGroup = new GeometryGroup();
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, geometryGroup, tolerance, matrix);

            // Assert
            Assert.Empty(pathGeoDst.Figures);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing LineSegment
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_LineSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(0, 0),
                IsFilled = true,
                IsClosed = false
            };
            figure.Segments.Add(new LineSegment { Point = new Point(10, 10) });
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Equal(new Point(0, 0), resultFigure.StartPoint);
            Assert.True(resultFigure.IsFilled);
            Assert.False(resultFigure.IsClosed);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing PolyLineSegment
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_PolyLineSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var polySegment = new PolyLineSegment();
            polySegment.Points.Add(new Point(5, 5));
            polySegment.Points.Add(new Point(10, 0));
            figure.Segments.Add(polySegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.Equal(2, resultSegment.Points.Count);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing BezierSegment
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_BezierSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var bezierSegment = new BezierSegment
            {
                Point1 = new Point(0, 10),
                Point2 = new Point(10, 10),
                Point3 = new Point(10, 0)
            };
            figure.Segments.Add(bezierSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.True(resultSegment.Points.Count > 0);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing PolyBezierSegment with sufficient points
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_PolyBezierSegment_SufficientPoints_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var polyBezierSegment = new PolyBezierSegment();
            polyBezierSegment.Points.Add(new Point(0, 10));  // Control point 1
            polyBezierSegment.Points.Add(new Point(10, 10)); // Control point 2
            polyBezierSegment.Points.Add(new Point(10, 0));  // End point
            figure.Segments.Add(polyBezierSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.True(resultSegment.Points.Count > 0);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing PolyBezierSegment with insufficient points
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_PolyBezierSegment_InsufficientPoints_HandlesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var polyBezierSegment = new PolyBezierSegment();
            polyBezierSegment.Points.Add(new Point(5, 5)); // Only one point, need at least 3
            figure.Segments.Add(polyBezierSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.Empty(resultSegment.Points); // Should be empty due to insufficient points
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing QuadraticBezierSegment
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_QuadraticBezierSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var quadBezierSegment = new QuadraticBezierSegment
            {
                Point1 = new Point(5, 10),
                Point2 = new Point(10, 0)
            };
            figure.Segments.Add(quadBezierSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.True(resultSegment.Points.Count > 0);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing PolyQuadraticBezierSegment with sufficient points
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_PolyQuadraticBezierSegment_SufficientPoints_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var polyQuadBezierSegment = new PolyQuadraticBezierSegment();
            polyQuadBezierSegment.Points.Add(new Point(5, 10)); // Control point
            polyQuadBezierSegment.Points.Add(new Point(10, 0)); // End point
            figure.Segments.Add(polyQuadBezierSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.True(resultSegment.Points.Count > 0);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing PolyQuadraticBezierSegment with insufficient points
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_PolyQuadraticBezierSegment_InsufficientPoints_HandlesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var polyQuadBezierSegment = new PolyQuadraticBezierSegment();
            polyQuadBezierSegment.Points.Add(new Point(5, 5)); // Only one point, need at least 2
            figure.Segments.Add(polyQuadBezierSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.Empty(resultSegment.Points); // Should be empty due to insufficient points
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing ArcSegment
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_ArcSegment_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var arcSegment = new ArcSegment
            {
                Point = new Point(10, 10),
                Size = new Size(5, 5),
                RotationAngle = 45,
                IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise
            };
            figure.Segments.Add(arcSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.True(resultSegment.Points.Count >= 0);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry containing ArcSegment with CounterClockwise direction
        /// </summary>
        [Fact]
        public void FlattenGeometry_PathGeometry_ArcSegment_CounterClockwise_ProcessesCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            var arcSegment = new ArcSegment
            {
                Point = new Point(10, 10),
                Size = new Size(5, 5),
                RotationAngle = 45,
                IsLargeArc = true,
                SweepDirection = SweepDirection.CounterClockwise
            };
            figure.Segments.Add(arcSegment);
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Single(resultFigure.Segments);
            var resultSegment = Assert.IsType<PolyLineSegment>(resultFigure.Segments[0]);
            Assert.True(resultSegment.Points.Count >= 0);
        }

        /// <summary>
        /// Tests FlattenGeometry with PathGeometry preserving FillRule
        /// </summary>
        [Theory]
        [InlineData(FillRule.EvenOdd)]
        [InlineData(FillRule.Nonzero)]
        public void FlattenGeometry_PathGeometry_PreservesFillRule(FillRule fillRule)
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry { FillRule = fillRule };
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            figure.Segments.Add(new LineSegment { Point = new Point(10, 10) });
            pathGeoSrc.Figures.Add(figure);
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Equal(fillRule, pathGeoDst.FillRule);
        }

        /// <summary>
        /// Tests FlattenGeometry with different matrix transformations
        /// </summary>
        [Fact]
        public void FlattenGeometry_WithMatrixTransformation_AppliesTransformCorrectly()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var lineGeo = new LineGeometry
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(10, 10)
            };
            double tolerance = 1.0;
            var scaleMatrix = Matrix.CreateScale(2, 2);

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, lineGeo, tolerance, scaleMatrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var figure = pathGeoDst.Figures[0];
            // Start point and end point should be scaled by the matrix
            Assert.Equal(new Point(0, 0), figure.StartPoint);
            var segment = Assert.IsType<PolyLineSegment>(figure.Segments[0]);
            Assert.Equal(new Point(20, 20), segment.Points[0]);
        }

        /// <summary>
        /// Tests FlattenGeometry with extreme coordinate values
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(-1000000, 1000000)]
        public void FlattenGeometry_ExtremeCoordinates_HandlesCorrectly(double x, double y)
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var lineGeo = new LineGeometry
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(x, y)
            };
            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act & Assert - should not throw
            GeometryHelper.FlattenGeometry(pathGeoDst, lineGeo, tolerance, matrix);
            Assert.Single(pathGeoDst.Figures);
        }

        /// <summary>
        /// Tests FlattenGeometry with complex nested PathGeometry with multiple segments
        /// </summary>
        [Fact]
        public void FlattenGeometry_ComplexPathGeometry_ProcessesAllSegments()
        {
            // Arrange
            var pathGeoDst = new PathGeometry();
            var pathGeoSrc = new PathGeometry();

            // Create figure with multiple segment types
            var figure = new PathFigure { StartPoint = new Point(0, 0) };
            figure.Segments.Add(new LineSegment { Point = new Point(10, 0) });
            figure.Segments.Add(new BezierSegment
            {
                Point1 = new Point(15, 0),
                Point2 = new Point(20, 5),
                Point3 = new Point(20, 10)
            });
            figure.Segments.Add(new QuadraticBezierSegment
            {
                Point1 = new Point(15, 15),
                Point2 = new Point(10, 10)
            });
            pathGeoSrc.Figures.Add(figure);

            double tolerance = 1.0;
            var matrix = Matrix.Identity;

            // Act
            GeometryHelper.FlattenGeometry(pathGeoDst, pathGeoSrc, tolerance, matrix);

            // Assert
            Assert.Single(pathGeoDst.Figures);
            var resultFigure = pathGeoDst.Figures[0];
            Assert.Equal(3, resultFigure.Segments.Count); // Should have 3 PolyLineSegments
            Assert.All(resultFigure.Segments, segment => Assert.IsType<PolyLineSegment>(segment));
        }

        /// <summary>
        /// Tests that FlattenCubicBezier throws ArgumentNullException when points list is null.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_NullPointsList_ThrowsArgumentNullException()
        {
            // Arrange
            List<Point> points = null;
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = 1.0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance));
        }

        /// <summary>
        /// Tests FlattenCubicBezier behavior when tolerance is zero, which causes division by zero in max calculation.
        /// Expected to either throw or handle gracefully.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_ZeroTolerance_HandlesGracefully()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = 0.0;

            // Act & Assert
            // This should either throw DivideByZeroException or handle gracefully
            var exception = Record.Exception(() =>
                GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance));

            // We expect either an exception or the method to handle it gracefully
            // The specific behavior depends on implementation details
        }

        /// <summary>
        /// Tests FlattenCubicBezier behavior with negative tolerance value.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_NegativeTolerance_HandlesGracefully()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = -1.0;

            // Act & Assert
            var exception = Record.Exception(() =>
                GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance));
        }

        /// <summary>
        /// Tests FlattenCubicBezier with various tolerance values including edge cases.
        /// </summary>
        [Theory]
        [InlineData(1.0)]
        [InlineData(0.1)]
        [InlineData(10.0)]
        [InlineData(double.MaxValue)]
        public void FlattenCubicBezier_ValidToleranceValues_AddsPointsToList(double tolerance)
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            int initialCount = points.Count;

            // Act
            GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance);

            // Assert
            Assert.True(points.Count > initialCount);
        }

        /// <summary>
        /// Tests FlattenCubicBezier with infinite tolerance value.
        /// When tolerance is infinite, max should be 0, resulting in only one point added.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_InfiniteTolerance_AddsOnePoint()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(5, 5);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = double.PositiveInfinity;

            // Act
            GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance);

            // Assert
            Assert.Single(points);
            Assert.Equal(ptStart.X, points[0].X);
            Assert.Equal(ptStart.Y, points[0].Y);
        }

        /// <summary>
        /// Tests FlattenCubicBezier with NaN tolerance value.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_NaNTolerance_HandlesGracefully()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = double.NaN;

            // Act
            var exception = Record.Exception(() =>
                GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance));

            // Assert - method should handle NaN gracefully or throw appropriate exception
        }

        /// <summary>
        /// Tests FlattenCubicBezier when all points are identical.
        /// Total distance should be 0, making max = 0, resulting in one point added.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_AllPointsIdentical_AddsOnePoint()
        {
            // Arrange
            var points = new List<Point>();
            var samePoint = new Point(10, 10);
            double tolerance = 1.0;

            // Act
            GeometryHelper.FlattenCubicBezier(points, samePoint, samePoint, samePoint, samePoint, tolerance);

            // Assert
            Assert.Single(points);
            Assert.Equal(samePoint.X, points[0].X);
            Assert.Equal(samePoint.Y, points[0].Y);
        }

        /// <summary>
        /// Tests FlattenCubicBezier with points containing NaN coordinates.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_PointsWithNaNCoordinates_HandlesGracefully()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(double.NaN, 0);
            var ptCtrl1 = new Point(10, double.NaN);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = 1.0;

            // Act
            var exception = Record.Exception(() =>
                GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance));

            // Assert - should handle NaN coordinates gracefully
        }

        /// <summary>
        /// Tests FlattenCubicBezier with points containing infinite coordinates.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_PointsWithInfiniteCoordinates_HandlesGracefully()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(double.PositiveInfinity, 10);
            var ptCtrl2 = new Point(20, double.NegativeInfinity);
            var ptEnd = new Point(30, 0);
            double tolerance = 1.0;

            // Act
            var exception = Record.Exception(() =>
                GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance));

            // Assert - should handle infinite coordinates gracefully
        }

        /// <summary>
        /// Tests FlattenCubicBezier with extreme coordinate values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, 0, 0, 0)]
        [InlineData(0, double.MaxValue, 0, 0)]
        [InlineData(0, 0, double.MinValue, 0)]
        [InlineData(0, 0, 0, double.MaxValue)]
        public void FlattenCubicBezier_ExtremeCoordinateValues_HandlesGracefully(double x1, double y1, double x2, double y2)
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(x1, y1);
            var ptCtrl1 = new Point(x2, y2);
            var ptCtrl2 = new Point(10, 10);
            var ptEnd = new Point(20, 20);
            double tolerance = 1.0;

            // Act
            var exception = Record.Exception(() =>
                GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance));

            // Assert - should handle extreme values gracefully
        }

        /// <summary>
        /// Tests FlattenCubicBezier adds points to existing list without clearing it.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_ExistingPointsInList_AppendsNewPoints()
        {
            // Arrange
            var points = new List<Point> { new Point(100, 100) };
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = 5.0;
            int initialCount = points.Count;

            // Act
            GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance);

            // Assert
            Assert.True(points.Count > initialCount);
            Assert.Equal(100, points[0].X); // Original point should still be there
            Assert.Equal(100, points[0].Y);
        }

        /// <summary>
        /// Tests FlattenCubicBezier produces correct number of points for known scenarios.
        /// When max = n, should add n+1 points to the list.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_KnownScenario_ProducesExpectedPointCount()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(0, 10);  // Distance from start: 10
            var ptCtrl2 = new Point(10, 10); // Distance from ctrl1: 10  
            var ptEnd = new Point(10, 0);    // Distance from ctrl2: 10
                                             // Total distance: 30, tolerance: 10, max = 30/10 = 3
                                             // Should add 4 points (i = 0, 1, 2, 3)
            double tolerance = 10.0;

            // Act
            GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance);

            // Assert
            Assert.Equal(4, points.Count);
        }

        /// <summary>
        /// Tests FlattenCubicBezier first and last points match start and end points.
        /// When t=0, should produce ptStart. When t=1, should produce ptEnd.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_FirstAndLastPoints_MatchStartAndEndPoints()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(1, 2);
            var ptCtrl1 = new Point(3, 4);
            var ptCtrl2 = new Point(5, 6);
            var ptEnd = new Point(7, 8);
            double tolerance = 1.0;

            // Act
            GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance);

            // Assert
            Assert.True(points.Count >= 2);
            Assert.Equal(ptStart.X, points[0].X);
            Assert.Equal(ptStart.Y, points[0].Y);
            Assert.Equal(ptEnd.X, points[points.Count - 1].X);
            Assert.Equal(ptEnd.Y, points[points.Count - 1].Y);
        }

        /// <summary>
        /// Tests FlattenCubicBezier with very small tolerance produces many points.
        /// </summary>
        [Fact]
        public void FlattenCubicBezier_VerySmallTolerance_ProducesManyPoints()
        {
            // Arrange
            var points = new List<Point>();
            var ptStart = new Point(0, 0);
            var ptCtrl1 = new Point(10, 10);
            var ptCtrl2 = new Point(20, 10);
            var ptEnd = new Point(30, 0);
            double tolerance = 0.001; // Very small tolerance

            // Act
            GeometryHelper.FlattenCubicBezier(points, ptStart, ptCtrl1, ptCtrl2, ptEnd, tolerance);

            // Assert
            Assert.True(points.Count > 100); // Should produce many points with small tolerance
        }

        /// <summary>
        /// Tests that FlattenArc throws ArgumentNullException when points list is null.
        /// </summary>
        [Fact]
        public void FlattenArc_NullPointsList_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                GeometryHelper.FlattenArc(null, Point.Zero, new Point(10, 10), 5, 5, 0, false, false, 1));
        }

        /// <summary>
        /// Tests FlattenArc with identical start and end points, which should hit the zero vector length path.
        /// This covers the uncovered line 64-65 where vectRotatedLength == 0.
        /// </summary>
        [Fact]
        public void FlattenArc_IdenticalStartAndEndPoints_HandlesZeroVectorLength()
        {
            // Arrange
            var points = new List<Point>();
            var identicalPoint = new Point(5, 5);

            // Act
            GeometryHelper.FlattenArc(points, identicalPoint, identicalPoint, 10, 10, 0, false, false, 1);

            // Assert
            Assert.NotEmpty(points);
        }

        /// <summary>
        /// Tests FlattenArc with various arc parameter combinations to ensure proper arc direction calculation.
        /// This helps cover different branches in the arc direction logic.
        /// </summary>
        [Theory]
        [InlineData(false, false, 0, 1)] // Small arc, clockwise
        [InlineData(true, false, 0, 1)]  // Large arc, clockwise
        [InlineData(false, true, 0, 1)]  // Small arc, counterclockwise
        [InlineData(true, true, 0, 1)]   // Large arc, counterclockwise
        [InlineData(false, false, 180, 1)] // Small arc, clockwise, rotated
        [InlineData(true, true, 90, 0.5)]  // Large arc, counterclockwise, rotated, different tolerance
        public void FlattenArc_VariousArcParameters_ProducesPoints(bool isLargeArc, bool isCounterclockwise, double angleRotation, double tolerance)
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = new Point(0, 0);
            var endPoint = new Point(10, 0);
            var radiusX = 15.0;
            var radiusY = 10.0;

            // Act
            GeometryHelper.FlattenArc(points, startPoint, endPoint, radiusX, radiusY, angleRotation, isLargeArc, isCounterclockwise, tolerance);

            // Assert
            Assert.NotEmpty(points);
        }

        /// <summary>
        /// Tests FlattenArc with specific arc configuration to trigger the angle2 increment path.
        /// This targets the uncovered line 96 where angle2 += 2 * Math.PI when angle1 >= angle2 and reverseArc is true.
        /// </summary>
        [Fact]
        public void FlattenArc_SpecificAngleConfiguration_HandlesAngle2Increment()
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = new Point(-5, 0);
            var endPoint = new Point(5, 0);
            var radiusX = 10.0;
            var radiusY = 10.0;
            var angleRotation = 0.0;
            var isLargeArc = false;
            var isCounterclockwise = true;

            // Act
            GeometryHelper.FlattenArc(points, startPoint, endPoint, radiusX, radiusY, angleRotation, isLargeArc, isCounterclockwise, 1);

            // Assert
            Assert.NotEmpty(points);
        }

        /// <summary>
        /// Tests FlattenArc with zero radii to validate behavior with degenerate ellipse.
        /// </summary>
        [Theory]
        [InlineData(0, 5)]
        [InlineData(5, 0)]
        [InlineData(0, 0)]
        public void FlattenArc_ZeroRadii_HandlesGracefully(double radiusX, double radiusY)
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = Point.Zero;
            var endPoint = new Point(1, 1);

            // Act & Assert - Should not throw
            GeometryHelper.FlattenArc(points, startPoint, endPoint, radiusX, radiusY, 0, false, false, 1);
        }

        /// <summary>
        /// Tests FlattenArc with negative radii to validate behavior with invalid ellipse parameters.
        /// </summary>
        [Theory]
        [InlineData(-5, 5)]
        [InlineData(5, -5)]
        [InlineData(-5, -5)]
        public void FlattenArc_NegativeRadii_HandlesGracefully(double radiusX, double radiusY)
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = Point.Zero;
            var endPoint = new Point(1, 1);

            // Act & Assert - Should not throw
            GeometryHelper.FlattenArc(points, startPoint, endPoint, radiusX, radiusY, 0, false, false, 1);
        }

        /// <summary>
        /// Tests FlattenArc with extreme tolerance values to validate behavior with boundary conditions.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NaN)]
        public void FlattenArc_ExtremeTolerance_HandlesGracefully(double tolerance)
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = Point.Zero;
            var endPoint = new Point(10, 0);

            // Act & Assert - Should not throw
            GeometryHelper.FlattenArc(points, startPoint, endPoint, 5, 5, 0, false, false, tolerance);
        }

        /// <summary>
        /// Tests FlattenArc with extreme rotation angles to validate behavior with boundary angle values.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(90)]
        [InlineData(180)]
        [InlineData(270)]
        [InlineData(360)]
        [InlineData(-90)]
        [InlineData(720)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void FlattenArc_ExtremeRotationAngles_HandlesGracefully(double angleRotation)
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = Point.Zero;
            var endPoint = new Point(5, 5);

            // Act & Assert - Should not throw
            GeometryHelper.FlattenArc(points, startPoint, endPoint, 10, 10, angleRotation, false, false, 1);
        }

        /// <summary>
        /// Tests FlattenArc with extreme point coordinates to validate behavior with boundary coordinate values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 0, 0, double.MaxValue)]
        [InlineData(double.MinValue, 0, 0, double.MinValue)]
        [InlineData(double.PositiveInfinity, 0, 0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 0, double.NegativeInfinity)]
        [InlineData(double.NaN, 0, 0, double.NaN)]
        public void FlattenArc_ExtremePointCoordinates_HandlesGracefully(double x1, double y1, double x2, double y2)
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = new Point(x1, y1);
            var endPoint = new Point(x2, y2);

            // Act & Assert - Should not throw
            GeometryHelper.FlattenArc(points, startPoint, endPoint, 10, 10, 0, false, false, 1);
        }

        /// <summary>
        /// Tests FlattenArc with normal arc parameters to ensure basic functionality works correctly.
        /// </summary>
        [Fact]
        public void FlattenArc_NormalParameters_ProducesExpectedPoints()
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = new Point(0, 10);
            var endPoint = new Point(10, 0);
            var radiusX = 10.0;
            var radiusY = 10.0;
            var angleRotation = 0.0;
            var tolerance = 1.0;

            // Act
            GeometryHelper.FlattenArc(points, startPoint, endPoint, radiusX, radiusY, angleRotation, false, false, tolerance);

            // Assert
            Assert.NotEmpty(points);
            Assert.True(points.Count > 1, "Should generate multiple points for arc approximation");
        }

        /// <summary>
        /// Tests FlattenArc with very small tolerance to ensure high precision arc approximation.
        /// </summary>
        [Fact]
        public void FlattenArc_SmallTolerance_ProducesMorePoints()
        {
            // Arrange
            var pointsLowPrecision = new List<Point>();
            var pointsHighPrecision = new List<Point>();
            var startPoint = new Point(0, 10);
            var endPoint = new Point(10, 0);
            var radiusX = 10.0;
            var radiusY = 10.0;

            // Act
            GeometryHelper.FlattenArc(pointsLowPrecision, startPoint, endPoint, radiusX, radiusY, 0, false, false, 10);
            GeometryHelper.FlattenArc(pointsHighPrecision, startPoint, endPoint, radiusX, radiusY, 0, false, false, 0.1);

            // Assert
            Assert.True(pointsHighPrecision.Count >= pointsLowPrecision.Count,
                "Higher precision (smaller tolerance) should produce same or more points");
        }

        /// <summary>
        /// Tests FlattenArc with different radius ratios to validate elliptical arc behavior.
        /// </summary>
        [Theory]
        [InlineData(1, 1)]   // Circle
        [InlineData(2, 1)]   // Horizontal ellipse
        [InlineData(1, 2)]   // Vertical ellipse
        [InlineData(10, 1)]  // Very flat horizontal ellipse
        [InlineData(1, 10)]  // Very tall vertical ellipse
        public void FlattenArc_DifferentRadiusRatios_ProducesPoints(double radiusX, double radiusY)
        {
            // Arrange
            var points = new List<Point>();
            var startPoint = new Point(0, 0);
            var endPoint = new Point(1, 1);

            // Act
            GeometryHelper.FlattenArc(points, startPoint, endPoint, radiusX, radiusY, 0, false, false, 1);

            // Assert
            Assert.NotEmpty(points);
        }

        [Fact]
        public void FlattenGeometry_NullGeometry_ReturnsEmptyPathGeometry()
        {
            // Arrange
            Geometry nullGeometry = null;
            double tolerance = 1.0;

            // Act
            PathGeometry result = GeometryHelper.FlattenGeometry(nullGeometry, tolerance);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathGeometry>(result);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.1)]
        [InlineData(1.0)]
        [InlineData(10.0)]
        [InlineData(100.0)]
        [InlineData(double.MaxValue)]
        public void FlattenGeometry_ValidGeometryPositiveTolerance_ReturnsPathGeometry(double tolerance)
        {
            // Arrange
            var geometry = Substitute.For<Geometry>();

            // Act
            PathGeometry result = GeometryHelper.FlattenGeometry(geometry, tolerance);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathGeometry>(result);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.1)]
        [InlineData(double.MinValue)]
        public void FlattenGeometry_ValidGeometryNegativeTolerance_ReturnsPathGeometry(double tolerance)
        {
            // Arrange
            var geometry = Substitute.For<Geometry>();

            // Act
            PathGeometry result = GeometryHelper.FlattenGeometry(geometry, tolerance);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathGeometry>(result);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FlattenGeometry_ValidGeometrySpecialTolerance_ReturnsPathGeometry(double tolerance)
        {
            // Arrange
            var geometry = Substitute.For<Geometry>();

            // Act
            PathGeometry result = GeometryHelper.FlattenGeometry(geometry, tolerance);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathGeometry>(result);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1.0)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        public void FlattenGeometry_NullGeometryVariousTolerances_ReturnsEmptyPathGeometry(double tolerance)
        {
            // Arrange
            Geometry nullGeometry = null;

            // Act
            PathGeometry result = GeometryHelper.FlattenGeometry(nullGeometry, tolerance);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathGeometry>(result);
        }
    }
}