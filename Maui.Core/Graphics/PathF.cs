using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace System.Maui.Graphics {
	public class Path : IDisposable
	{
		private readonly List<Point> _points;
		private readonly List<PathOperation> _operations;

		private List<double> _arcAngles;
		private List<bool> _arcClockwise;

		private Rectangle? _cachedBounds;
		private object _nativePath;

		public Path (Path prototype, AffineTransformF transform = null) : this()
		{
			_operations.AddRange(prototype._operations);
			foreach (var point in prototype.Points)
			{
				var newPoint = point;

				if (transform != null)
					newPoint = transform.Transform(point);

				_points.Add(newPoint);
			}
			if (prototype._arcAngles != null)
			{
				_arcAngles = new List<double>();
				_arcClockwise = new List<bool>();

				_arcAngles.AddRange(prototype._arcAngles);
				_arcClockwise.AddRange(prototype._arcClockwise);
			}
		}

		public Path(Point point) : this()
		{
			MoveTo(point);
		}

		public Path(double x, double y) : this(new Point(x, y))
		{
		}

		public Path()
		{
			_points = new List<Point>();
			_operations = new List<PathOperation>();
		}

		public bool Closed
		{
			get
			{
				if (_operations.Count > 0)
					return _operations[_operations.Count - 1] == PathOperation.Close;

				return false;
			}
		}

		public Point? FirstPoint
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points[0];

				return null;
			}
		}

		public IEnumerable<PathOperation> PathOperations
		{
			get
			{
				for (var i = 0; i < _operations.Count; i++)
					yield return _operations[i];
			}
		}

		public IEnumerable<Point> Points
		{
			get
			{
				for (var i = 0; i < _points.Count; i++)
					yield return _points[i];
			}
		}

		public Rectangle Bounds
		{
			get
			{
				if (_cachedBounds != null)
					return (Rectangle)_cachedBounds;

				_cachedBounds = CalculateBounds();

				/* var graphicsService = Device.GraphicsService;
                if (graphicsService != null)
                    _cachedBounds = graphicsService.GetPathBounds(this);
                else
                {
                    
                }*/

				return (Rectangle)_cachedBounds;
			}
		}

		private Rectangle CalculateBounds()
		{
			var xValues = new List<double>();
			var yValues = new List<double>();

			int pointIndex = 0;
			int arcAngleIndex = 0;
			int arcClockwiseIndex = 0;

			foreach (var operation in PathOperations)
			{
				if (operation == PathOperation.MoveTo)
				{
					pointIndex++;
				}
				else if (operation == PathOperation.Line)
				{
					var startPoint = _points[pointIndex - 1];
					var endPoint = _points[pointIndex++];

					xValues.Add(startPoint.X);
					xValues.Add(endPoint.X);
					yValues.Add(startPoint.Y);
					yValues.Add(endPoint.Y);
				}
				else if (operation == PathOperation.Quad)
				{
					var startPoint = _points[pointIndex - 1];
					var controlPoint = _points[pointIndex++];
					var endPoint = _points[pointIndex++];

					var bounds = GraphicsOperations.GetBoundsOfQuadraticCurve(startPoint, controlPoint, endPoint);

					xValues.Add(bounds.Left);
					xValues.Add(bounds.Right);
					yValues.Add(bounds.Top);
					yValues.Add(bounds.Bottom);
				}
				else if (operation == PathOperation.Cubic)
				{
					var startPoint = _points[pointIndex - 1];
					var controlPoint1 = _points[pointIndex++];
					var controlPoint2 = _points[pointIndex++];
					var endPoint = _points[pointIndex++];

					var bounds = GraphicsOperations.GetBoundsOfCubicCurve(startPoint, controlPoint1, controlPoint2, endPoint);

					xValues.Add(bounds.Left);
					xValues.Add(bounds.Right);
					yValues.Add(bounds.Top);
					yValues.Add(bounds.Bottom);
				}
				else if (operation == PathOperation.Arc)
				{
					var topLeft = _points[pointIndex++];
					var bottomRight = _points[pointIndex++];
					double startAngle = GetArcAngle(arcAngleIndex++);
					double endAngle = GetArcAngle(arcAngleIndex++);
					var clockwise = IsArcClockwise(arcClockwiseIndex++);

					var bounds = GraphicsOperations.GetBoundsOfArc(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, startAngle, endAngle, clockwise);

					xValues.Add(bounds.Left);
					xValues.Add(bounds.Right);
					yValues.Add(bounds.Top);
					yValues.Add(bounds.Bottom);
				}
			}

			var minX = xValues.Min();
			var minY = yValues.Min();
			var maxX = xValues.Max();
			var maxY = yValues.Max();

			return new Rectangle(minX, minY, maxX - minX, maxY - minY);
		}

		public Point? LastPoint
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points[_points.Count - 1];

				return null;
			}
		}

		public Point this[int index]
		{
			get
			{
				if (index < 0 || index >= _points.Count)
					throw new IndexOutOfRangeException();

				return _points[index];
			}
		}

		public int PointCount => _points.Count;

		public int OperationCount => _operations.Count;

		public PathOperation GetOperationType(int index)
		{
			return _operations[index];
		}

		public double GetArcAngle(int index)
		{
			if (_arcAngles != null && _arcAngles.Count > index)
				return _arcAngles[index];

			return 0;
		}

		public bool IsArcClockwise(int index)
		{
			if (_arcClockwise != null && _arcClockwise.Count > index)
				return _arcClockwise[index];

			return false;
		}

		public Path MoveTo(double x, double y)
		{
			return MoveTo(new Point(x, y));
		}

		public Path MoveTo(Point point)
		{
			_points.Add(point);
			_operations.Add(PathOperation.MoveTo);
			Invalidate();
			return this;
		}

		public void Close()
		{
			if (!Closed)
				_operations.Add(PathOperation.Close);

			Invalidate();
		}

		public Path LineTo(double x, double y)
		{
			return LineTo(new Point(x, y));
		}

		public Path LineTo(Point point)
		{
			if (_points.Count == 0)
			{
				_points.Add(point);
				_operations.Add(PathOperation.MoveTo);
			}
			else
			{
				_points.Add(point);
				_operations.Add(PathOperation.Line);
			}

			Invalidate();

			return this;
		}

		public Path AddArc(double x1, double y1, double x2, double y2, double startAngle, double endAngle, bool clockwise)
		{
			return AddArc(new Point(x1, y1), new Point(x2, y2), startAngle, endAngle, clockwise);
		}

		public Path AddArc(Point topLeft, Point bottomRight, double startAngle, double endAngle, bool clockwise)
		{
			if (_arcAngles == null)
			{
				_arcAngles = new List<double>();
				_arcClockwise = new List<bool>();
			}
			_points.Add(topLeft);
			_points.Add(bottomRight);
			_arcAngles.Add(startAngle);
			_arcAngles.Add(endAngle);
			_arcClockwise.Add(clockwise);
			_operations.Add(PathOperation.Arc);
			Invalidate();
			return this;
		}

		public Path QuadTo(double cx, double cy, double x, double y)
		{
			return QuadTo(new Point(cx, cy), new Point(x, y));
		}

		public Path QuadTo(Point controlPoint, Point point)
		{
			_points.Add(controlPoint);
			_points.Add(point);
			_operations.Add(PathOperation.Quad);
			Invalidate();
			return this;
		}

		public Path CurveTo(double c1X, double c1Y, double c2X, double c2Y, double x, double y)
		{
			return CurveTo(new Point(c1X, c1Y), new Point(c2X, c2Y), new Point(x, y));
		}

		public Path CurveTo(Point controlPoint1, Point controlPoint2, Point point)
		{
			_points.Add(controlPoint1);
			_points.Add(controlPoint2);
			_points.Add(point);
			_operations.Add(PathOperation.Cubic);
			Invalidate();
			return this;
		}

		public Path Rotate(double angle)
		{
			var center = Bounds.Center;
			return Rotate(angle, center);
		}

		public Path Rotate(double angle, Point pivotPoint)
		{
			var path = new Path();

			var index = 0;
			var arcIndex = 0;
			var clockwiseIndex = 0;

			foreach (var operation in _operations)
			{
				if (operation == PathOperation.MoveTo)
				{
					var point = GetRotatedPoint(index++, pivotPoint, angle);
					path.MoveTo(point);
				}
				else if (operation == PathOperation.Line)
				{
					var point = GetRotatedPoint(index++, pivotPoint, angle);
					path.LineTo(point.X, point.Y);
				}
				else if (operation == PathOperation.Quad)
				{
					var controlPoint = GetRotatedPoint(index++, pivotPoint, angle);
					var point = GetRotatedPoint(index++, pivotPoint, angle);
					path.QuadTo(controlPoint.X, controlPoint.Y, point.X, point.Y);
				}
				else if (operation == PathOperation.Cubic)
				{
					var controlPoint1 = GetRotatedPoint(index++, pivotPoint, angle);
					var controlPoint2 = GetRotatedPoint(index++, pivotPoint, angle);
					var point = GetRotatedPoint(index++, pivotPoint, angle);
					path.CurveTo(controlPoint1.X, controlPoint1.Y, controlPoint2.X, controlPoint2.Y, point.X, point.Y);
				}
				else if (operation == PathOperation.Arc)
				{
					var topLeft = GetRotatedPoint(index++, pivotPoint, angle);
					var bottomRight = GetRotatedPoint(index++, pivotPoint, angle);
					var startAngle = _arcAngles[arcIndex++];
					var endAngle = _arcAngles[arcIndex++];
					var clockwise = _arcClockwise[clockwiseIndex++];

					path.AddArc(topLeft, bottomRight, startAngle, endAngle, clockwise);
				}
				else if (operation == PathOperation.Close)
				{
					path.Close();
				}
			}

			return path;
		}

		private Point GetRotatedPoint(int index, Point center, double angleInDegrees)
		{
			var point = _points[index];
			return GraphicsOperations.RotatePoint(center, point, angleInDegrees);
		}

		public void AppendEllipse(Rectangle rect)
		{
			AppendEllipse(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void AppendEllipse(double x, double y, double w, double h)
		{
			var minx = x;
			var miny = y;
			var maxx = minx + w;
			var maxy = miny + h;
			var midx = minx + (w / 2);
			var midy = miny + (h / 2);
			var offsetY = h / 2 * .55f;
			var offsetX = w / 2 * .55f;

			MoveTo(new Point(minx, midy));
			CurveTo(new Point(minx, midy - offsetY), new Point(midx - offsetX, miny), new Point(midx, miny));
			CurveTo(new Point(midx + offsetX, miny), new Point(maxx, midy - offsetY), new Point(maxx, midy));
			CurveTo(new Point(maxx, midy + offsetY), new Point(midx + offsetX, maxy), new Point(midx, maxy));
			CurveTo(new Point(midx - offsetX, maxy), new Point(minx, midy + offsetY), new Point(minx, midy));
			Close();
		}

		public void AppendRectangle(Rectangle rect, bool includeLast = false)
		{
			AppendRectangle(rect.X, rect.Y, rect.Width, rect.Height, includeLast);
		}

		public void AppendRectangle(double x, double y, double w, double h, bool includeLast = false)
		{
			var minx = x;
			var miny = y;
			var maxx = minx + w;
			var maxy = miny + h;

			MoveTo(new Point(minx, miny));
			LineTo(new Point(maxx, miny));
			LineTo(new Point(maxx, maxy));
			LineTo(new Point(minx, maxy));

			if (includeLast)
				LineTo(new Point(minx, miny));

			Close();
		}

		public void AppendRoundedRectangle(Rectangle rect, double cornerRadius, bool includeLast = false)
		{
			AppendRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius, includeLast);
		}

		public void AppendRoundedRectangle(double x, double y, double w, double h, double cornerRadius, bool includeLast = false)
		{
			if (cornerRadius > h / 2)
				cornerRadius = h / 2;

			if (cornerRadius > w / 2)
				cornerRadius = w / 2;

			var minx = x;
			var miny = y;
			var maxx = minx + w;
			var maxy = miny + h;

			var handleOffset = cornerRadius * .55f;
			var cornerOffset = cornerRadius - handleOffset;

			MoveTo(new Point(minx, miny + cornerRadius));
			CurveTo(new Point(minx, miny + cornerOffset), new Point(minx + cornerOffset, miny), new Point(minx + cornerRadius, miny));
			LineTo(new Point(maxx - cornerRadius, miny));
			CurveTo(new Point(maxx - cornerOffset, miny), new Point(maxx, miny + cornerOffset), new Point(maxx, miny + cornerRadius));
			LineTo(new Point(maxx, maxy - cornerRadius));
			CurveTo(new Point(maxx, maxy - cornerOffset), new Point(maxx - cornerOffset, maxy), new Point(maxx - cornerRadius, maxy));
			LineTo(new Point(minx + cornerRadius, maxy));
			CurveTo(new Point(minx + cornerOffset, maxy), new Point(minx, maxy - cornerOffset), new Point(minx, maxy - cornerRadius));

			if (includeLast)
				LineTo(new Point(minx, miny + cornerRadius));

			Close();
		}

		public object NativePath
		{
			get => _nativePath;
			set
			{
				if (_nativePath is IDisposable disposable)
					disposable.Dispose();

				_nativePath = value;
			}
		}

		private void Invalidate()
		{
			_cachedBounds = null;
			ReleaseNative();
		}

		public void Dispose()
		{
			ReleaseNative();
		}

		private void ReleaseNative()
		{
			if (_nativePath is IDisposable disposable)
				disposable.Dispose();

			_nativePath = null;
		}

		public Path Transform(AffineTransformF transform)
		{
			return new Path(this, transform);
		}
	}
}
