using System;
using System.Collections.Generic;
using System.Numerics;
// ReSharper disable MemberCanBePrivate.Global

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a geometric path consisting of lines, curves, and shapes using single-precision floating-point coordinates.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A path is composed of one or more sub-paths, each beginning with a Move operation and consisting of connected
	/// line segments, curves, and arcs. For fill operations to work reliably, paths should typically be closed using
	/// the <see cref="Close()"/> method or by explicitly connecting the end point back to the starting point.
	/// </para>
	/// <para>
	/// When creating paths for filling, ensure proper path construction to avoid exceptions during rendering.
	/// Paths that start with <see cref="LineTo(PointF)"/> operations will automatically create an initial MoveTo operation.
	/// </para>
	/// </remarks>
	public class PathF : IDisposable
	{
		private const float K_RATIO = 0.551784777779014f; // ideal ratio of cubic Bezier points for a quarter circle

		private readonly List<float> _arcAngles;
		private readonly List<bool> _arcClockwise;
		private readonly List<PointF> _points;
		private readonly List<PathOperation> _operations;
		private int _subPathCount;
		private readonly List<bool> _subPathsClosed;

		private object _platformPath;
		private RectF? _cachedBounds;

		private PathF(List<PointF> points, List<float> arcSizes, List<bool> arcClockwise, List<PathOperation> operations, int subPathCount)
		{
			_points = points;
			_arcAngles = arcSizes;
			_arcClockwise = arcClockwise;
			_operations = operations;
			_subPathCount = subPathCount;
			_subPathsClosed = new List<bool>();

			var subPathIndex = 0;
			foreach (var operation in _operations)
			{
				if (operation == PathOperation.Move)
				{
					subPathIndex++;
					_subPathsClosed.Add(false);
				}
				else if (operation == PathOperation.Close)
				{
					_subPathsClosed.RemoveAt(subPathIndex - 1);
					_subPathsClosed.Add(true);
				}
			}
		}

		/// <summary>
		/// Initializes a new path by copying the segments, points, and arc metadata of another <see cref="PathF"/>.
		/// </summary>
		/// <param name="path">The path to copy.</param>
		public PathF(PathF path) : this()
		{
			_operations.AddRange(path._operations);
			_points = new List<PointF>(path._points);

			_arcAngles.AddRange(path._arcAngles);
			_arcClockwise.AddRange(path._arcClockwise);

			_subPathCount = path._subPathCount;
			_subPathsClosed = new List<bool>(path._subPathsClosed);
		}

		/// <summary>
		/// Initializes a new path whose first (move) point is the specified point.
		/// </summary>
		/// <param name="point">The starting point.</param>
		public PathF(PointF point) : this()
		{
			MoveTo(point.X, point.Y);
		}

		/// <summary>
		/// Initializes a new path whose first (move) point is at the specified coordinates.
		/// </summary>
		/// <param name="x">X coordinate of the starting point.</param>
		/// <param name="y">Y coordinate of the starting point.</param>
		public PathF(float x, float y) : this()
		{
			MoveTo(x, y);
		}

		/// <summary>
		/// Initializes an empty path with no segments.
		/// </summary>
		public PathF()
		{
			_subPathCount = 0;
			_arcAngles = new List<float>();
			_arcClockwise = new List<bool>();
			_points = new List<PointF>();
			_operations = new List<PathOperation>();
			_subPathsClosed = new List<bool>();
		}

		/// <summary>
		/// Gets the number of sub-paths (contiguous sequences beginning with <see cref="PathOperation.Move"/>) in the path.
		/// </summary>
		public int SubPathCount => _subPathCount;

		/// <summary>
		/// Gets a value indicating whether the last sub-path has been explicitly closed with <see cref="Close"/>.
		/// </summary>
		public bool Closed
		{
			get
			{
				if (_operations.Count > 0)
					return _operations[_operations.Count - 1] == PathOperation.Close;

				return false;
			}
		}

		/// <summary>
		/// Gets the first point in the path, or the default value if the path is empty.
		/// </summary>
		public PointF FirstPoint
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points[0];

				return default;
			}
		}

		/// <summary>
		/// Enumerates the sequence of segment operations composing the path.
		/// </summary>
		public IEnumerable<PathOperation> SegmentTypes
		{
			get
			{
				for (var i = 0; i < _operations.Count; i++)
					yield return _operations[i];
			}
		}

		/// <summary>
		/// Enumerates all points used by the path's segments in logical order.
		/// </summary>
		public IEnumerable<PointF> Points
		{
			get
			{
				for (var i = 0; i < _points.Count; i++)
					yield return _points[i];
			}
		}

		/// <summary>
		/// Gets the last point in the path, or the default value if the path is empty.
		/// </summary>
		public PointF LastPoint
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points[_points.Count - 1];

				return default;
			}
		}

		/// <summary>
		/// Gets the index of the last point, or -1 if the path is empty.
		/// </summary>
		public int LastPointIndex
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points.Count - 1;

				return -1;
			}
		}

		/// <summary>
		/// Gets the point at the specified index, or the default value if the index is out of range.
		/// </summary>
		/// <param name="index">Index of the point.</param>
		public PointF this[int index]
		{
			get
			{
				if (index < 0 || index >= _points.Count)
					return default;

				return _points[index];
			}
			//set { points[index] = value; }
		}

		/// <summary>
		/// Sets the coordinates of the point at the specified index.
		/// </summary>
		/// <param name="index">Index of the point.</param>
		/// <param name="x">New X value.</param>
		/// <param name="y">New Y value.</param>
		public void SetPoint(int index, float x, float y)
		{
			_points[index] = new PointF(x, y);
			Invalidate();
		}

		/// <summary>
		/// Sets the point at the specified index.
		/// </summary>
		/// <param name="index">Index of the point.</param>
		/// <param name="point">The new point value.</param>
		public void SetPoint(int index, PointF point)
		{
			_points[index] = point;
			Invalidate();
		}

		/// <summary>
		/// Gets the total number of points in the path.
		/// </summary>
		public int Count => _points.Count;

		/// <summary>
		/// Gets the number of segment operations (including move and close) in the path.
		/// </summary>
		public int OperationCount => _operations.Count;

		/// <summary>
		/// Gets the count of segment operations excluding a leading <see cref="PathOperation.Move"/> and a trailing <see cref="PathOperation.Close"/>, if present.
		/// </summary>
		public int SegmentCountExcludingOpenAndClose
		{
			get
			{
				if (_operations != null)
				{
					var operationsCount = _operations.Count;
					if (operationsCount > 0)
					{
						if (_operations[0] == PathOperation.Move)
						{
							operationsCount--;
						}

						if (_operations[_operations.Count - 1] == PathOperation.Close)
						{
							operationsCount--;
						}
					}

					return operationsCount;
				}

				return 0;
			}
		}

		/// <summary>
		/// Gets the segment operation type at the specified index.
		/// </summary>
		/// <param name="aIndex">Segment index.</param>
		/// <returns>The <see cref="PathOperation"/> value.</returns>
		public PathOperation GetSegmentType(int aIndex)
		{
			return _operations[aIndex];
		}

		/// <summary>
		/// Gets an arc angle value at the specified index (stored as degrees).
		/// </summary>
		/// <param name="aIndex">Angle index.</param>
		/// <returns>The angle in degrees, or 0 if out of range.</returns>
		public float GetArcAngle(int aIndex)
		{
			if (_arcAngles.Count > aIndex)
			{
				return _arcAngles[aIndex];
			}

			return 0;
		}

		/// <summary>
		/// Sets an arc angle value (degrees) at the specified index.
		/// </summary>
		/// <param name="aIndex">Angle index.</param>
		/// <param name="aValue">New angle in degrees.</param>
		public void SetArcAngle(int aIndex, float aValue)
		{
			if (_arcAngles.Count > aIndex)
			{
				_arcAngles[aIndex] = aValue;
			}

			Invalidate();
		}

		/// <summary>
		/// Gets the stored clockwise flag for an arc segment at the specified index.
		/// </summary>
		/// <param name="aIndex">Arc flag index.</param>
		/// <returns><c>true</c> if clockwise; otherwise <c>false</c>.</returns>
		public bool GetArcClockwise(int aIndex)
		{
			if (_arcClockwise.Count > aIndex)
			{
				return _arcClockwise[aIndex];
			}

			return false;
		}

		/// <summary>
		/// Sets the stored clockwise flag for an arc segment.
		/// </summary>
		/// <param name="aIndex">Arc flag index.</param>
		/// <param name="aValue">New clockwise value.</param>
		public void SetArcClockwise(int aIndex, bool aValue)
		{
			if (_arcClockwise.Count > aIndex)
			{
				_arcClockwise[aIndex] = aValue;
			}

			Invalidate();
		}

		/// <summary>
		/// Starts a new sub-path at the specified coordinates.
		/// </summary>
		/// <param name="x">X coordinate of the starting point.</param>
		/// <param name="y">Y coordinate of the starting point.</param>
		/// <returns>The current path for chaining.</returns>
		public PathF MoveTo(float x, float y)
		{
			return MoveTo(new PointF(x, y));
		}

		/// <summary>
		/// Starts a new sub-path at the specified point.
		/// </summary>
		/// <param name="point">Starting point of the new sub-path.</param>
		/// <returns>The current path for chaining.</returns>
		public PathF MoveTo(PointF point)
		{
			_subPathCount++;
			_subPathsClosed.Add(false);
			_points.Add(point);
			_operations.Add(PathOperation.Move);
			Invalidate();
			return this;
		}

		/// <summary>
		/// Closes the current sub-path by appending a close segment if it is not already closed.
		/// </summary>
		/// <remarks>
		/// Closing a path is typically required for fill operations to work correctly. Attempting to fill
		/// an unclosed path may result in undefined behavior or exceptions in some graphics implementations.
		/// A closed path ensures that the shape is properly defined for filling operations.
		/// </remarks>
		public void Close()
		{
			if (!Closed)
			{
				_subPathsClosed.RemoveAt(_subPathCount - 1);
				_subPathsClosed.Add(true);
				_operations.Add(PathOperation.Close);
			}

			Invalidate();
		}

		/// <summary>
		/// Reopens a previously closed last sub-path by removing its closing segment.
		/// </summary>
		public void Open()
		{
			if (_operations[_operations.Count - 1] == PathOperation.Close)
			{
				_subPathsClosed.RemoveAt(_subPathCount - 1);
				_subPathsClosed.Add(false);
				_operations.RemoveAt(_operations.Count - 1);
			}

			Invalidate();
		}

		/// <summary>
		/// Adds a straight line segment to the specified coordinates.
		/// </summary>
		/// <param name="x">The x-coordinate of the end point.</param>
		/// <param name="y">The y-coordinate of the end point.</param>
		/// <returns>The current path.</returns>
		public PathF LineTo(float x, float y)
		{
			return LineTo(new PointF(x, y));
		}

		/// <summary>
		/// Adds a straight line segment to the specified end point (starting a new sub-path if the path is empty).
		/// </summary>
		/// <param name="point">The end point.</param>
		/// <returns>The current path.</returns>
		/// <remarks>
		/// If this is the first operation on an empty path, it will automatically create an initial MoveTo operation
		/// to the specified point. For paths intended to be filled, ensure the path forms a closed shape by calling
		/// <see cref="Close()"/> or explicitly connecting back to the starting point.
		/// </remarks>
		public PathF LineTo(PointF point)
		{
			if (_points.Count == 0)
			{
				_points.Add(point);
				_subPathCount++;
				_subPathsClosed.Add(false);
				_operations.Add(PathOperation.Move);
			}
			else
			{
				_points.Add(point);
				_operations.Add(PathOperation.Line);
			}

			Invalidate();

			return this;
		}

		/// <summary>
		/// Inserts a line segment at a specific segment index.
		/// </summary>
		/// <param name="point">Line end point.</param>
		/// <param name="index">Segment index at which to insert.</param>
		/// <returns>The current path.</returns>
		public PathF InsertLineTo(PointF point, int index)
		{
			if (index == 0)
			{
				index = 1;
			}

			if (index == OperationCount)
			{
				LineTo(point);
			}
			else
			{
				var pointIndex = GetSegmentPointIndex(index);
				_points.Insert(pointIndex, point);
				_operations.Insert(index, PathOperation.Line);
				Invalidate();
			}

			return this;
		}

		/// <summary>
		/// Adds an elliptical arc segment using coordinate values instead of points.
		/// </summary>
		/// <param name="x1">The X coordinate of the top-left corner of the bounding rectangle of the ellipse.</param>
		/// <param name="y1">The Y coordinate of the top-left corner of the bounding rectangle of the ellipse.</param>
		/// <param name="x2">The X coordinate of the bottom-right corner of the bounding rectangle of the ellipse.</param>
		/// <param name="y2">The Y coordinate of the bottom-right corner of the bounding rectangle of the ellipse.</param>
		/// <param name="startAngle">Starting angle of the arc in degrees. 0° points to the right (along the positive X axis). Angles increase counter-clockwise.</param>
		/// <param name="endAngle">Ending angle of the arc in degrees, measured with the same convention as <paramref name="startAngle"/>.</param>
		/// <param name="clockwise">If <c>true</c>, the arc is drawn in the clockwise direction from <paramref name="startAngle"/> to <paramref name="endAngle"/>; otherwise it is drawn counter-clockwise (the positive angle direction).</param>
		/// <returns>The current path for chaining.</returns>
		public PathF AddArc(float x1, float y1, float x2, float y2, float startAngle, float endAngle, bool clockwise)
		{
			return AddArc(new PointF(x1, y1), new PointF(x2, y2), startAngle, endAngle, clockwise);
		}

		/// <summary>
		/// Adds an elliptical arc segment to the current sub-path.
		/// </summary>
		/// <param name="topLeft">The top-left point of the rectangle that bounds the full ellipse from which the arc segment is taken.</param>
		/// <param name="bottomRight">The bottom-right point of the bounding rectangle of the ellipse.</param>
		/// <param name="startAngle">Starting angle of the arc in degrees. 0° points to the right (along the positive X axis). Angles increase counter-clockwise.</param>
		/// <param name="endAngle">Ending angle of the arc in degrees, measured with the same convention as <paramref name="startAngle"/>.</param>
		/// <param name="clockwise">If <c>true</c>, the arc is drawn in the clockwise direction from <paramref name="startAngle"/> to <paramref name="endAngle"/>; otherwise it is drawn counter-clockwise (the positive angle direction).</param>
		/// <remarks>
		/// Angle values are specified in degrees (not radians). The angular coordinate system used by <see cref="Microsoft.Maui.Graphics"/> for arcs is:
		/// <list type="bullet">
		/// <item><description>0° is the point on the ellipse at the positive X axis (to the right of center).</description></item>
		/// <item><description>Positive angles advance counter-clockwise.</description></item>
		/// <item><description>The direction of increasing Y on the drawing surface (often downwards in device pixels) does not change the counter-clockwise convention used for angles.</description></item>
		/// </list>
		/// The current point is not implicitly connected to the start of the arc. If you need a straight line connection, call <see cref="LineTo(PointF)"/> first.
		/// </remarks>
		/// <returns>The current <see cref="PathF"/> so that calls can be chained fluently.</returns>
		public PathF AddArc(PointF topLeft, PointF bottomRight, float startAngle, float endAngle, bool clockwise)
		{
			if (Count == 0 || OperationCount == 0 || GetSegmentType(OperationCount - 1) == PathOperation.Close)
			{
				_subPathCount++;
				_subPathsClosed.Add(false);
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

		/// <summary>
		/// Adds a quadratic Bézier curve segment using coordinate values.
		/// </summary>
		/// <param name="cx">X-coordinate of the control point.</param>
		/// <param name="cy">Y-coordinate of the control point.</param>
		/// <param name="x">X-coordinate of the end point.</param>
		/// <param name="y">Y-coordinate of the end point.</param>
		/// <returns>The current path.</returns>
		public PathF QuadTo(float cx, float cy, float x, float y)
		{
			return QuadTo(new PointF(cx, cy), new PointF(x, y));
		}

		/// <summary>
		/// Adds a quadratic Bézier curve segment defined by a control point and an end point.
		/// </summary>
		/// <param name="controlPoint">Quadratic control point.</param>
		/// <param name="point">End point of the curve.</param>
		/// <returns>The current path.</returns>
		public PathF QuadTo(PointF controlPoint, PointF point)
		{
			_points.Add(controlPoint);
			_points.Add(point);
			_operations.Add(PathOperation.Quad);
			Invalidate();
			return this;
		}

		/// <summary>
		/// Inserts a quadratic Bézier segment at a specific segment index.
		/// </summary>
		/// <param name="controlPoint">Control point.</param>
		/// <param name="point">End point.</param>
		/// <param name="index">Insertion segment index.</param>
		/// <returns>The current path.</returns>
		public PathF InsertQuadTo(PointF controlPoint, PointF point, int index)
		{
			if (index == 0)
			{
				index = 1;
			}

			if (index == OperationCount)
			{
				QuadTo(controlPoint, point);
			}
			else
			{
				var pointIndex = GetSegmentPointIndex(index);
				_points.Insert(pointIndex, point);
				_points.Insert(pointIndex, controlPoint);
				_operations.Insert(index, PathOperation.Quad);
				Invalidate();
			}

			return this;
		}

		/// <summary>
		/// Adds a cubic Bézier curve segment using coordinate values.
		/// </summary>
		/// <param name="c1X">X-coordinate of the first control point.</param>
		/// <param name="c1Y">Y-coordinate of the first control point.</param>
		/// <param name="c2X">X-coordinate of the second control point.</param>
		/// <param name="c2Y">Y-coordinate of the second control point.</param>
		/// <param name="x">X-coordinate of the end point.</param>
		/// <param name="y">Y-coordinate of the end point.</param>
		/// <returns>The current path.</returns>
		public PathF CurveTo(float c1X, float c1Y, float c2X, float c2Y, float x, float y)
		{
			return CurveTo(new PointF(c1X, c1Y), new PointF(c2X, c2Y), new PointF(x, y));
		}

		/// <summary>
		/// Adds a cubic Bézier curve segment defined by two control points and an end point.
		/// </summary>
		/// <param name="controlPoint1">First control point.</param>
		/// <param name="controlPoint2">Second control point.</param>
		/// <param name="point">End point of the curve.</param>
		/// <returns>The current path.</returns>
		public PathF CurveTo(PointF controlPoint1, PointF controlPoint2, PointF point)
		{
			_points.Add(controlPoint1);
			_points.Add(controlPoint2);
			_points.Add(point);
			_operations.Add(PathOperation.Cubic);
			Invalidate();
			return this;
		}

		/// <summary>
		/// Inserts a cubic Bézier segment at a specific segment index.
		/// </summary>
		/// <param name="controlPoint1">First control point.</param>
		/// <param name="controlPoint2">Second control point.</param>
		/// <param name="point">End point.</param>
		/// <param name="index">Insertion segment index.</param>
		/// <returns>The current path.</returns>
		public PathF InsertCurveTo(PointF controlPoint1, PointF controlPoint2, PointF point, int index)
		{
			if (index == 0)
			{
				index = 1;
			}

			if (index == OperationCount)
			{
				CurveTo(controlPoint1, controlPoint2, point);
			}
			else
			{
				var pointIndex = GetSegmentPointIndex(index);
				_points.Insert(pointIndex, point);
				_points.Insert(pointIndex, controlPoint2);
				_points.Insert(pointIndex, controlPoint1);
				_operations.Insert(index, PathOperation.Cubic);
				Invalidate();
			}

			return this;
		}

		/// <summary>
		/// Computes the starting point index in the internal point list for a given segment index.
		/// </summary>
		/// <param name="index">Segment index.</param>
		/// <returns>The point index, or -1 if not found.</returns>
		public int GetSegmentPointIndex(int index)
		{
			if (index <= OperationCount)
			{
				var pointIndex = 0;
				for (var operationIndex = 0; operationIndex < _operations.Count; operationIndex++)
				{
					var operation = _operations[operationIndex];
					if (operation == PathOperation.Move)
					{
						if (operationIndex == index)
							return pointIndex;

						pointIndex++;
					}
					else if (operation == PathOperation.Line)
					{
						if (operationIndex == index)
							return pointIndex;

						pointIndex++;
					}
					else if (operation == PathOperation.Quad)
					{
						if (operationIndex == index)
							return pointIndex;

						pointIndex += 2;
					}
					else if (operation == PathOperation.Cubic)
					{
						if (operationIndex == index)
							return pointIndex;

						pointIndex += 3;
					}
					else if (operation == PathOperation.Arc)
					{
						if (operationIndex == index)
							return pointIndex;

						pointIndex += 2;
					}
					else if (operation == PathOperation.Close)
					{
						if (operationIndex == index)
							return pointIndex;
					}
				}
			}

			return -1;
		}

		/// <summary>
		/// Retrieves segment metadata, returning the segment type and output indices pointing into internal collections.
		/// </summary>
		/// <param name="segmentIndex">Segment index.</param>
		/// <param name="pointIndex">Receives the starting point index.</param>
		/// <param name="arcAngleIndex">Receives the starting arc angle index.</param>
		/// <param name="arcClockwiseIndex">Receives the arc clockwise flag index.</param>
		/// <returns>The segment operation type, or <see cref="PathOperation.Close"/> if invalid.</returns>
		public PathOperation GetSegmentInfo(int segmentIndex, out int pointIndex, out int arcAngleIndex, out int arcClockwiseIndex)
		{
			pointIndex = 0;
			arcAngleIndex = 0;
			arcClockwiseIndex = 0;

			if (segmentIndex <= OperationCount)
			{
				for (var s = 0; s < _operations.Count; s++)
				{
					var type = _operations[s];
					if (type == PathOperation.Move)
					{
						if (s == segmentIndex)
							return type;

						pointIndex++;
					}
					else if (type == PathOperation.Line)
					{
						if (s == segmentIndex)
							return type;
						pointIndex++;
					}
					else if (type == PathOperation.Quad)
					{
						if (s == segmentIndex)
							return type;
						pointIndex += 2;
					}
					else if (type == PathOperation.Cubic)
					{
						if (s == segmentIndex)
							return type;
						pointIndex += 3;
					}
					else if (type == PathOperation.Arc)
					{
						if (s == segmentIndex)
							return type;
						pointIndex += 2;
						arcAngleIndex += 2;
						arcClockwiseIndex++;
					}
					else if (type == PathOperation.Close)
					{
						if (s == segmentIndex)
							return type;
					}
				}
			}

			return PathOperation.Close;
		}

		/// <summary>
		/// Determines which segment uses the point at a specified index.
		/// </summary>
		/// <param name="pointIndex">Point index.</param>
		/// <returns>The segment index, or -1 if not found.</returns>
		public int GetSegmentForPoint(int pointIndex)
		{
			if (pointIndex < _points.Count)
			{
				var index = 0;
				for (var segment = 0; segment < _operations.Count; segment++)
				{
					var segmentType = _operations[segment];
					if (segmentType == PathOperation.Move)
					{
						if (pointIndex == index++)
						{
							return segment;
						}
					}
					else if (segmentType == PathOperation.Line)
					{
						if (pointIndex == index++)
						{
							return segment;
						}
					}
					else if (segmentType == PathOperation.Quad)
					{
						if (pointIndex == index++)
						{
							return segment;
						}

						if (pointIndex == index++)
						{
							return segment;
						}
					}
					else if (segmentType == PathOperation.Cubic)
					{
						if (pointIndex == index++)
						{
							return segment;
						}

						if (pointIndex == index++)
						{
							return segment;
						}

						if (pointIndex == index++)
						{
							return segment;
						}
					}
					else if (segmentType == PathOperation.Arc)
					{
						if (pointIndex == index++)
						{
							return segment;
						}

						if (pointIndex == index++)
						{
							return segment;
						}
					}
				}
			}

			return -1;
		}

		/// <summary>
		/// Gets the points defining the segment at the specified index.
		/// </summary>
		/// <param name="segmentIndex">Segment index.</param>
		/// <returns>An array of points (length varies by segment type), an empty array for close segments, or <c>null</c> if invalid.</returns>
		public PointF[] GetPointsForSegment(int segmentIndex)
		{
			if (segmentIndex <= OperationCount)
			{
				var pointIndex = 0;
				for (var segment = 0; segment < _operations.Count; segment++)
				{
					var segmentType = _operations[segment];
					if (segmentType == PathOperation.Move)
					{
						if (segment == segmentIndex)
						{
							var points = new[] { _points[pointIndex] };
							return points;
						}

						pointIndex++;
					}
					else if (segmentType == PathOperation.Line)
					{
						if (segment == segmentIndex)
						{
							var points = new[] { _points[pointIndex] };
							return points;
						}

						pointIndex++;
					}

					else if (segmentType == PathOperation.Quad)
					{
						if (segment == segmentIndex)
						{
							var points = new[] { _points[pointIndex++], _points[pointIndex] };
							return points;
						}

						pointIndex += 2;
					}
					else if (segmentType == PathOperation.Cubic)
					{
						if (segment == segmentIndex)
						{
							var points = new[] { _points[pointIndex++], _points[pointIndex++], _points[pointIndex] };
							return points;
						}

						pointIndex += 3;
					}
					else if (segmentType == PathOperation.Arc)
					{
						if (segment == segmentIndex)
						{
							var points = new[] { _points[pointIndex++], _points[pointIndex] };
							return points;
						}

						pointIndex += 2;
					}
					else if (segmentType == PathOperation.Close)
					{
						if (segment == segmentIndex)
						{
							return Array.Empty<PointF>();
						}
					}
				}
			}

			return null;
		}

		private void RemoveAllAfter(int pointIndex, int segmentIndex, int arcIndex, int arcClockwiseIndex)
		{
			_points.RemoveRange(pointIndex, _points.Count - pointIndex);
			_operations.RemoveRange(segmentIndex, _operations.Count - segmentIndex);
			_arcAngles.RemoveRange(arcIndex, _arcAngles.Count - arcIndex);
			_arcClockwise.RemoveRange(arcClockwiseIndex, _arcClockwise.Count - arcClockwiseIndex);

			_subPathCount = 0;
			_subPathsClosed.Clear();

			foreach (var operation in _operations)
			{
				if (operation == PathOperation.Move)
				{
					_subPathCount++;
					_subPathsClosed.Add(false);
				}
				else if (operation == PathOperation.Close)
				{
					_subPathsClosed.RemoveAt(_subPathCount);
					_subPathsClosed.Add(true);
				}
			}

			if (_subPathCount > 0)
			{
				_subPathCount--;
			}

			Invalidate();
		}

		/// <summary>
		/// Removes the specified segment and all segments that follow it.
		/// </summary>
		/// <param name="segmentIndex">Segment index at which truncation begins.</param>
		public void RemoveAllSegmentsAfter(int segmentIndex)
		{
			if (segmentIndex <= OperationCount)
			{
				var pointIndex = 0;
				var arcIndex = 0;
				var arcClockwiseIndex = 0;
				for (var segment = 0; segment < _operations.Count; segment++)
				{
					var segmentType = _operations[segment];

					if (segment == segmentIndex)
					{
						RemoveAllAfter(pointIndex, segment, arcIndex, arcClockwiseIndex);
						return;
					}

					if (segmentType == PathOperation.Move)
					{
						pointIndex++;
					}
					else if (segmentType == PathOperation.Line)
					{
						pointIndex++;
					}
					else if (segmentType == PathOperation.Quad)
					{
						pointIndex += 2;
					}
					else if (segmentType == PathOperation.Cubic)
					{
						pointIndex += 3;
					}
					else if (segmentType == PathOperation.Arc)
					{
						pointIndex += 2;
						arcIndex += 2;
						arcClockwiseIndex += 1;
					}
				}
			}

			Invalidate();
		}

		/// <summary>
		/// Removes a single segment, adjusting internal point and arc data accordingly.
		/// </summary>
		/// <param name="segmentIndex">Segment index to remove.</param>
		public void RemoveSegment(int segmentIndex)
		{
			if (segmentIndex <= OperationCount)
			{
				Invalidate();

				var pointIndex = 0;
				var arcIndex = 0;
				var arcClockwiseIndex = 0;

				for (var segment = 0; segment < _operations.Count; segment++)
				{
					var segmentType = _operations[segment];
					if (segmentType == PathOperation.Move)
					{
						if (segment == segmentIndex)
						{
							if (segmentIndex == _operations.Count - 1)
							{
								var points = GetPointsForSegment(segmentIndex);
								if (points != null)
								{
									for (var i = 0; i < points.Length; i++)
									{
										_points.RemoveAt(pointIndex);
									}
								}

								_operations.RemoveAt(segmentIndex);
							}
							else
							{
								var points = GetPointsForSegment(segmentIndex + 1);
								if (points != null)
								{
									if (points.Length > 0)
									{
										_points[pointIndex] = (PointF)points[points.Length - 1];
										for (var i = 0; i < points.Length; i++)
										{
											_points.RemoveAt(pointIndex + 1);
										}
									}

									_operations.RemoveAt(segmentIndex + 1);
								}
							}

							return;
						}

						pointIndex++;
					}
					else if (segmentType == PathOperation.Line)
					{
						if (segment == segmentIndex)
						{
							_points.RemoveAt(pointIndex);
							_operations.RemoveAt(segmentIndex);
							return;
						}

						pointIndex++;
					}
					else if (segmentType == PathOperation.Quad)
					{
						if (segment == segmentIndex)
						{
							_points.RemoveAt(pointIndex);
							_points.RemoveAt(pointIndex);
							_operations.RemoveAt(segmentIndex);
							return;
						}

						pointIndex += 2;
					}
					else if (segmentType == PathOperation.Cubic)
					{
						if (segment == segmentIndex)
						{
							_points.RemoveAt(pointIndex);
							_points.RemoveAt(pointIndex);
							_points.RemoveAt(pointIndex);
							_operations.RemoveAt(segmentIndex);
							return;
						}

						pointIndex += 3;
					}
					else if (segmentType == PathOperation.Arc)
					{
						if (segment == segmentIndex)
						{
							_points.RemoveAt(pointIndex);
							_points.RemoveAt(pointIndex);
							_operations.RemoveAt(segmentIndex);
							_arcAngles.RemoveAt(arcIndex);
							_arcAngles.RemoveAt(arcIndex);
							_arcClockwise.RemoveAt(arcClockwiseIndex);
							return;
						}

						pointIndex += 2;
						arcIndex += 2;
						arcClockwiseIndex += 1;
					}
					else if (segmentType == PathOperation.Close)
					{
						if (segment == segmentIndex)
						{
							_operations.RemoveAt(segmentIndex);
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a new <see cref="PathF"/> representing this path rotated by the specified angle about a pivot point.
		/// </summary>
		/// <param name="angleAsDegrees">The rotation angle in degrees. Positive angles rotate counter-clockwise; 0° keeps the path unchanged.</param>
		/// <param name="pivot">The pivot point about which all points (and ellipse bounding rectangles for arc segments) are rotated.</param>
		/// <returns>A new <see cref="PathF"/> containing the rotated geometry. The original path is not modified.</returns>
		/// <remarks>
		/// Rotation uses the same degree-based, counter-clockwise positive convention as arc angles (see <see cref="AddArc(PointF, PointF, float, float, bool)"/>).
		/// Arc segments preserve their original start and end angle values; only their bounding rectangle corner points are rotated.
		/// </remarks>
		public PathF Rotate(float angleAsDegrees, PointF pivot)
		{
			var path = new PathF();

			var index = 0;
			var arcIndex = 0;
			var clockwiseIndex = 0;

			foreach (var segmentType in _operations)
			{
				if (segmentType == PathOperation.Move)
				{
					var rotatedPoint = GetRotatedPoint(index++, pivot, angleAsDegrees);
					path.MoveTo(rotatedPoint);
				}
				else if (segmentType == PathOperation.Line)
				{
					var rotatedPoint = GetRotatedPoint(index++, pivot, angleAsDegrees);
					path.LineTo(rotatedPoint.X, rotatedPoint.Y);
				}
				else if (segmentType == PathOperation.Quad)
				{
					var rotatedControlPoint = GetRotatedPoint(index++, pivot, angleAsDegrees);
					var rotatedEndPoint = GetRotatedPoint(index++, pivot, angleAsDegrees);
					path.QuadTo(rotatedControlPoint.X, rotatedControlPoint.Y, rotatedEndPoint.X, rotatedEndPoint.Y);
				}
				else if (segmentType == PathOperation.Cubic)
				{
					var rotatedControlPoint1 = GetRotatedPoint(index++, pivot, angleAsDegrees);
					var rotatedControlPoint2 = GetRotatedPoint(index++, pivot, angleAsDegrees);
					var rotatedEndPoint = GetRotatedPoint(index++, pivot, angleAsDegrees);
					path.CurveTo(rotatedControlPoint1.X, rotatedControlPoint1.Y, rotatedControlPoint2.X, rotatedControlPoint2.Y, rotatedEndPoint.X, rotatedEndPoint.Y);
				}
				else if (segmentType == PathOperation.Arc)
				{
					var topLeft = GetRotatedPoint(index++, pivot, angleAsDegrees);
					var bottomRight = GetRotatedPoint(index++, pivot, angleAsDegrees);
					var startAngle = _arcAngles[arcIndex++];
					var endAngle = _arcAngles[arcIndex++];
					var clockwise = _arcClockwise[clockwiseIndex++];

					path.AddArc(topLeft, bottomRight, startAngle, endAngle, clockwise);
				}
				else if (segmentType == PathOperation.Close)
				{
					path.Close();
				}
			}

			return path;
		}

		/// <summary>
		/// Computes the position of a point in the path after rotation about a pivot.
		/// </summary>
		/// <param name="pointIndex">Index into the internal point list.</param>
		/// <param name="pivotPoint">The pivot point for rotation.</param>
		/// <param name="angle">Rotation angle in degrees (counter-clockwise positive).</param>
		/// <returns>The rotated point.</returns>
		/// <remarks>
		/// This helper applies the same rotation semantics used by <see cref="Rotate(float, PointF)"/> and does not cache results.
		/// </remarks>
		public PointF GetRotatedPoint(int pointIndex, PointF pivotPoint, float angle)
		{
			var point = _points[pointIndex];
			return GeometryUtil.RotatePoint(pivotPoint, point, angle);
		}

		/// <summary>
		/// Applies a 2D affine transformation matrix to all points in the path in place.
		/// </summary>
		/// <param name="transform">The transformation matrix.</param>
		public void Transform(Matrix3x2 transform)
		{
			for (var i = 0; i < _points.Count; i++)
				_points[i] = Vector2.Transform((Vector2)_points[i], transform);

			Invalidate();
		}

		/// <summary>
		/// Splits the path into individual sub-path objects.
		/// </summary>
		/// <returns>A list of sub-paths.</returns>
		public List<PathF> Separate()
		{
			var paths = new List<PathF>();
			if (_points == null || _operations == null)
				return paths;

			PathF path = null;

			// ReSharper disable PossibleNullReferenceException
			var i = 0;
			var a = 0;
			var c = 0;

			foreach (var operation in _operations)
			{
				if (operation == PathOperation.Move)
				{
					path = new PathF();
					paths.Add(path);
					path.MoveTo(_points[i++]);
				}
				else if (operation == PathOperation.Line)
				{
					path.LineTo(_points[i++]);
				}
				else if (operation == PathOperation.Quad)
				{
					path.QuadTo(_points[i++], _points[i++]);
				}
				else if (operation == PathOperation.Cubic)
				{
					path.CurveTo(_points[i++], _points[i++], _points[i++]);
				}
				else if (operation == PathOperation.Arc)
				{
					path.AddArc(_points[i++], _points[i++], _arcAngles[a++], _arcAngles[a++], _arcClockwise[c++]);
				}
				else if (operation == PathOperation.Close)
				{
					path.Close();
					path = null;
				}
			}
			// ReSharper restore PossibleNullReferenceException

			return paths;
		}

		/// <summary>
		/// Creates a new path with the segment and point order reversed.
		/// </summary>
		/// <returns>A new reversed path.</returns>
		public PathF Reverse()
		{
			var points = new List<PointF>(_points);
			points.Reverse();

			var arcSizes = new List<float>(_arcAngles);
			arcSizes.Reverse();

			var arcClockwise = new List<bool>(_arcClockwise);
			arcClockwise.Reverse();

			var operations = new List<PathOperation>(_operations);
			operations.Reverse();

			var segmentClosed = false;
			var segmentStart = -1;

			for (var i = 0; i < operations.Count; i++)
			{
				if (operations[i] == PathOperation.Move)
				{
					if (segmentStart == -1)
					{
						operations.RemoveAt(i);
						operations.Insert(0, PathOperation.Move);
					}
					else if (segmentClosed)
					{
						operations[segmentStart] = PathOperation.Move;
						operations[i] = PathOperation.Close;
					}

					segmentStart = i + 1;
				}
				else if (operations[i] == PathOperation.Close)
				{
					segmentStart = i;
					segmentClosed = true;
				}
			}

			return new PathF(points, arcSizes, arcClockwise, operations, _subPathCount);
		}

		/// <summary>
		/// Appends an approximated ellipse path inside the specified rectangle.
		/// </summary>
		/// <param name="rect">The bounding rectangle for the ellipse.</param>
		public void AppendEllipse(RectF rect)
		{
			AppendEllipse(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>
		/// Appends an approximated ellipse path (using 4 cubic curves) inside the specified rectangle.
		/// </summary>
		/// <param name="x">Left.</param>
		/// <param name="y">Top.</param>
		/// <param name="w">Width.</param>
		/// <param name="h">Height.</param>
		public void AppendEllipse(float x, float y, float w, float h)
		{
			var minX = x;
			var minY = y;
			var maxX = minX + w;
			var maxY = minY + h;
			var midX = minX + w / 2;
			var midY = minY + h / 2;
			var offsetY = h / 2 * K_RATIO;
			var offsetX = w / 2 * K_RATIO;

			MoveTo(new PointF(minX, midY));
			CurveTo(new PointF(minX, midY - offsetY), new PointF(midX - offsetX, minY), new PointF(midX, minY));
			CurveTo(new PointF(midX + offsetX, minY), new PointF(maxX, midY - offsetY), new PointF(maxX, midY));
			CurveTo(new PointF(maxX, midY + offsetY), new PointF(midX + offsetX, maxY), new PointF(midX, maxY));
			CurveTo(new PointF(midX - offsetX, maxY), new PointF(minX, midY + offsetY), new PointF(minX, midY));
			Close();
		}

		/// <summary>
		/// Appends an approximated circle path centered at the specified point.
		/// </summary>
		/// <param name="center">Center point.</param>
		/// <param name="r">Radius.</param>
		public void AppendCircle(PointF center, float r)
		{
			AppendCircle(center.X, center.Y, r);
		}

		/// <summary>
		/// Appends an approximated circle path centered at the specified point.
		/// </summary>
		/// <param name="cx">Center X.</param>
		/// <param name="cy">Center Y.</param>
		/// <param name="r">Radius.</param>
		public void AppendCircle(float cx, float cy, float r)
		{
			var minX = cx - r;
			var minY = cy - r;
			var maxX = cx + r;
			var maxY = cy + r;
			var midX = cx;
			var midY = cy;
			var offsetY = r * K_RATIO;
			var offsetX = r * K_RATIO;

			MoveTo(new PointF(minX, midY));
			CurveTo(new PointF(minX, midY - offsetY), new PointF(midX - offsetX, minY), new PointF(midX, minY));
			CurveTo(new PointF(midX + offsetX, minY), new PointF(maxX, midY - offsetY), new PointF(maxX, midY));
			CurveTo(new PointF(maxX, midY + offsetY), new PointF(midX + offsetX, maxY), new PointF(midX, maxY));
			CurveTo(new PointF(midX - offsetX, maxY), new PointF(minX, midY + offsetY), new PointF(minX, midY));
			Close();
		}

		/// <summary>
		/// Appends a rectangle path using the specified rectangle bounds.
		/// </summary>
		/// <param name="rect">The rectangle bounds.</param>
		/// <param name="includeLast">Include a final duplicate line to the first point before closing.</param>
		public void AppendRectangle(RectF rect, bool includeLast = false)
		{
			AppendRectangle(rect.X, rect.Y, rect.Width, rect.Height, includeLast);
		}

		/// <summary>
		/// Appends a rectangle path.
		/// </summary>
		/// <param name="x">Left.</param>
		/// <param name="y">Top.</param>
		/// <param name="w">Width.</param>
		/// <param name="h">Height.</param>
		/// <param name="includeLast">Include a final duplicate line to the first point before closing.</param>
		public void AppendRectangle(float x, float y, float w, float h, bool includeLast = false)
		{
			var minX = x;
			var minY = y;
			var maxX = minX + w;
			var maxY = minY + h;

			MoveTo(new PointF(minX, minY));
			LineTo(new PointF(maxX, minY));
			LineTo(new PointF(maxX, maxY));
			LineTo(new PointF(minX, maxY));

			if (includeLast)
			{
				LineTo(new PointF(minX, minY));
			}

			Close();
		}

		/// <summary>
		/// Appends a rounded rectangle using the specified rectangle bounds and uniform corner radius.
		/// </summary>
		/// <param name="rect">The rectangle bounds.</param>
		/// <param name="cornerRadius">Corner radius (clamped to half width/height).</param>
		/// <param name="includeLast">Include a duplicate final line before closing.</param>
		public void AppendRoundedRectangle(RectF rect, float cornerRadius, bool includeLast = false)
		{
			AppendRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius, includeLast);
		}

		/// <summary>
		/// Appends a rounded rectangle where all four corners share the same radius.
		/// </summary>
		/// <param name="x">Left.</param>
		/// <param name="y">Top.</param>
		/// <param name="w">Width.</param>
		/// <param name="h">Height.</param>
		/// <param name="cornerRadius">Corner radius (clamped to half width/height).</param>
		/// <param name="includeLast">Include a duplicate final line before closing.</param>
		public void AppendRoundedRectangle(float x, float y, float w, float h, float cornerRadius, bool includeLast = false)
		{
			cornerRadius = ClampCornerRadius(cornerRadius, w, h);

			var minX = x;
			var minY = y;
			var maxX = minX + w;
			var maxY = minY + h;

			var handleOffset = cornerRadius * K_RATIO;
			var cornerOffset = cornerRadius - handleOffset;

			MoveTo(new PointF(minX, minY + cornerRadius));
			CurveTo(new PointF(minX, minY + cornerOffset), new PointF(minX + cornerOffset, minY), new PointF(minX + cornerRadius, minY));
			LineTo(new PointF(maxX - cornerRadius, minY));
			CurveTo(new PointF(maxX - cornerOffset, minY), new PointF(maxX, minY + cornerOffset), new PointF(maxX, minY + cornerRadius));
			LineTo(new PointF(maxX, maxY - cornerRadius));
			CurveTo(new PointF(maxX, maxY - cornerOffset), new PointF(maxX - cornerOffset, maxY), new PointF(maxX - cornerRadius, maxY));
			LineTo(new PointF(minX + cornerRadius, maxY));
			CurveTo(new PointF(minX + cornerOffset, maxY), new PointF(minX, maxY - cornerOffset), new PointF(minX, maxY - cornerRadius));

			if (includeLast)
			{
				LineTo(new PointF(minX, minY + cornerRadius));
			}

			Close();
		}

		/// <summary>
		/// Appends a rounded rectangle using the specified rectangle bounds and individual corner radii.
		/// </summary>
		/// <param name="rect">Bounding rectangle.</param>
		/// <param name="topLeftCornerRadius">Top-left corner radius.</param>
		/// <param name="topRightCornerRadius">Top-right corner radius.</param>
		/// <param name="bottomLeftCornerRadius">Bottom-left corner radius.</param>
		/// <param name="bottomRightCornerRadius">Bottom-right corner radius.</param>
		/// <param name="includeLast">Include a duplicate final line before closing.</param>
		public void AppendRoundedRectangle(RectF rect, float topLeftCornerRadius, float topRightCornerRadius, float bottomLeftCornerRadius, float bottomRightCornerRadius, bool includeLast = false)
		{
			AppendRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius, includeLast);
		}

		/// <summary>
		/// Appends a rounded rectangle using distinct horizontal and vertical radii (elliptical corners).
		/// </summary>
		/// <param name="rect">Bounding rectangle.</param>
		/// <param name="xCornerRadius">Horizontal corner radius.</param>
		/// <param name="yCornerRadius">Vertical corner radius.</param>
		public void AppendRoundedRectangle(RectF rect, float xCornerRadius, float yCornerRadius)
		{
			xCornerRadius = Math.Min(xCornerRadius, rect.Width / 2);
			yCornerRadius = Math.Min(yCornerRadius, rect.Height / 2);

			float minX = Math.Min(rect.X, rect.X + rect.Width);
			float minY = Math.Min(rect.Y, rect.Y + rect.Height);
			float maxX = Math.Max(rect.X, rect.X + rect.Width);
			float maxY = Math.Max(rect.Y, rect.Y + rect.Height);

			var xHandleOffset = xCornerRadius * K_RATIO;
			var xCornerOffset = xCornerRadius - xHandleOffset;

			var yHandleOffset = yCornerRadius * K_RATIO;
			var yCornerOffset = yCornerRadius - yHandleOffset;

			MoveTo(new PointF(minX, minY + yCornerRadius));

			CurveTo(
				new PointF(minX, minY + yCornerOffset),
				new PointF(minX + xCornerOffset, minY),
				new PointF(minX + xCornerRadius, minY));

			LineTo(new PointF(maxX - xCornerRadius, minY));

			CurveTo(
				new PointF(maxX - xCornerOffset, minY),
				new PointF(maxX, minY + yCornerOffset),
				new PointF(maxX, minY + yCornerRadius));

			LineTo(new PointF(maxX, maxY - yCornerRadius));

			CurveTo(
				new PointF(maxX, maxY - yCornerOffset),
				new PointF(maxX - xCornerOffset, maxY),
				new PointF(maxX - xCornerRadius, maxY));

			LineTo(new PointF(minX + xCornerRadius, maxY));

			CurveTo(
				new PointF(minX + xCornerOffset, maxY),
				new PointF(minX, maxY - yCornerOffset),
				new PointF(minX, maxY - yCornerRadius));

			LineTo(new PointF(minX, minY + yCornerRadius));
		}

		/// <summary>
		/// Appends a rounded rectangle specifying individual corner radii.
		/// </summary>
		/// <param name="x">Left.</param>
		/// <param name="y">Top.</param>
		/// <param name="w">Width.</param>
		/// <param name="h">Height.</param>
		/// <param name="topLeftCornerRadius">Top-left radius.</param>
		/// <param name="topRightCornerRadius">Top-right radius.</param>
		/// <param name="bottomLeftCornerRadius">Bottom-left radius.</param>
		/// <param name="bottomRightCornerRadius">Bottom-right radius.</param>
		/// <param name="includeLast">Include a duplicate final line before closing.</param>
		public void AppendRoundedRectangle(float x, float y, float w, float h, float topLeftCornerRadius, float topRightCornerRadius, float bottomLeftCornerRadius, float bottomRightCornerRadius, bool includeLast = false)
		{
			topLeftCornerRadius = ClampCornerRadius(topLeftCornerRadius, w, h);
			topRightCornerRadius = ClampCornerRadius(topRightCornerRadius, w, h);
			bottomLeftCornerRadius = ClampCornerRadius(bottomLeftCornerRadius, w, h);
			bottomRightCornerRadius = ClampCornerRadius(bottomRightCornerRadius, w, h);

			var minX = x;
			var minY = y;
			var maxX = minX + w;
			var maxY = minY + h;

			var topLeftCornerOffset = topLeftCornerRadius - (topLeftCornerRadius * K_RATIO);
			var topRightCornerOffset = topRightCornerRadius - (topRightCornerRadius * K_RATIO);
			var bottomLeftCornerOffset = bottomLeftCornerRadius - (bottomLeftCornerRadius * K_RATIO);
			var bottomRightCornerOffset = bottomRightCornerRadius - (bottomRightCornerRadius * K_RATIO);

			MoveTo(new PointF(minX, minY + topLeftCornerRadius));
			CurveTo(new PointF(minX, minY + topLeftCornerOffset), new PointF(minX + topLeftCornerOffset, minY), new PointF(minX + topLeftCornerRadius, minY));
			LineTo(new PointF(maxX - topRightCornerRadius, minY));
			CurveTo(new PointF(maxX - topRightCornerOffset, minY), new PointF(maxX, minY + topRightCornerOffset), new PointF(maxX, minY + topRightCornerRadius));
			LineTo(new PointF(maxX, maxY - bottomRightCornerRadius));
			CurveTo(new PointF(maxX, maxY - bottomRightCornerOffset), new PointF(maxX - bottomRightCornerOffset, maxY), new PointF(maxX - bottomRightCornerRadius, maxY));
			LineTo(new PointF(minX + bottomLeftCornerRadius, maxY));
			CurveTo(new PointF(minX + bottomLeftCornerOffset, maxY), new PointF(minX, maxY - bottomLeftCornerOffset), new PointF(minX, maxY - bottomLeftCornerRadius));

			if (includeLast)
			{
				LineTo(new PointF(minX, minY + topLeftCornerRadius));
			}

			Close();
		}

		private float ClampCornerRadius(float cornerRadius, float w, float h)
		{
			if (cornerRadius > h / 2)
				cornerRadius = h / 2;

			if (cornerRadius > w / 2)
				cornerRadius = w / 2;

			return cornerRadius;
		}

		/// <summary>
		/// Indicates whether the specified sub-path is closed.
		/// </summary>
		/// <param name="subPathIndex">Zero-based sub-path index.</param>
		/// <returns><c>true</c> if closed; otherwise <c>false</c>.</returns>
		public bool IsSubPathClosed(int subPathIndex)
		{
			if (subPathIndex >= 0 && subPathIndex < SubPathCount)
			{
				return _subPathsClosed[subPathIndex];
			}

			return false;
		}

		/// <summary>
		/// Gets or sets a platform-specific native path object associated with this path. Setting a new value disposes the previous one if disposable.
		/// </summary>
		public object PlatformPath
		{
			get => _platformPath;
			set
			{
				ReleaseNative();
				_platformPath = value;
			}
		}

		/// <summary>
		/// Clears cached bounds and releases any native platform path.
		/// </summary>
		public void Invalidate()
		{
			_cachedBounds = null;
			ReleaseNative();
		}

		/// <summary>
		/// Releases native resources associated with the path.
		/// </summary>
		public void Dispose()
		{
			ReleaseNative();
		}

		private void ReleaseNative()
		{
			if (_platformPath is IDisposable disposable)
				disposable.Dispose();

			_platformPath = null;
		}

		/// <summary>
		/// Offsets every point in the path by the specified amounts.
		/// </summary>
		/// <param name="x">Delta X.</param>
		/// <param name="y">Delta Y.</param>
		public void Move(float x, float y)
		{
			for (var i = 0; i < _points.Count; i++)
			{
				_points[i] = _points[i].Offset(x, y);
			}

			Invalidate();
		}

		/// <summary>
		/// Offsets a single point by the specified deltas.
		/// </summary>
		/// <param name="index">Point index.</param>
		/// <param name="dx">Delta X.</param>
		/// <param name="dy">Delta Y.</param>
		public void MovePoint(int index, float dx, float dy)
		{
			_points[index] = _points[index].Offset(dx, dy);
			Invalidate();
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (obj is PathF compareTo)
			{
				if (OperationCount != compareTo.OperationCount)
					return false;

				for (var i = 0; i < _operations.Count; i++)
				{
					var segmentType = _operations[i];
					if (segmentType != compareTo.GetSegmentType(i))
						return false;
				}

				for (var i = 0; i < _points.Count; i++)
				{
					var point = _points[i];
					if (!point.Equals(compareTo[i], GeometryUtil.Epsilon))
						return false;
				}

				if (_arcAngles != null)
				{
					for (var i = 0; i < _arcAngles.Count; i++)
					{
						var arcAngle = _arcAngles[i];
						if (Math.Abs(arcAngle - compareTo.GetArcAngle(i)) > GeometryUtil.Epsilon)
							return false;
					}
				}

				if (_arcClockwise != null)
				{
					for (var i = 0; i < _arcClockwise.Count; i++)
					{
						var arcClockwise = _arcClockwise[i];
						if (arcClockwise != compareTo.GetArcClockwise(i))
							return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (_arcAngles != null ? _arcAngles.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_arcClockwise != null ? _arcClockwise.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_points != null ? _points.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_operations != null ? _operations.GetHashCode() : 0);
				return hashCode;
			}
		}

		/// <summary>
		/// Determines whether this path and another have equivalent geometry within a tolerance.
		/// </summary>
		/// <param name="obj">The other path.</param>
		/// <param name="epsilon">Maximum allowed difference per component.</param>
		/// <returns><c>true</c> if equivalent; otherwise <c>false</c>.</returns>
		public bool Equals(object obj, float epsilon)
		{
			if (obj is PathF compareTo)
			{
				if (OperationCount != compareTo.OperationCount)
					return false;

				for (var i = 0; i < _operations.Count; i++)
				{
					var segmentType = _operations[i];
					if (segmentType != compareTo.GetSegmentType(i))
						return false;
				}

				for (var i = 0; i < _points.Count; i++)
				{
					var point = _points[i];
					if (!point.Equals(compareTo[i], epsilon))
						return false;
				}

				if (_arcAngles != null)
				{
					for (var i = 0; i < _arcAngles.Count; i++)
					{
						var arcAngle = _arcAngles[i];
						if (Math.Abs(arcAngle - compareTo.GetArcAngle(i)) > epsilon)
							return false;
					}
				}

				if (_arcClockwise != null)
				{
					for (var i = 0; i < _arcClockwise.Count; i++)
					{
						var arcClockwise = _arcClockwise[i];
						if (arcClockwise != compareTo.GetArcClockwise(i))
							return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Gets the axis-aligned bounding box of the path (cached until modified).
		/// </summary>
		public RectF Bounds
		{
			get
			{
				if (_cachedBounds != null)
					return (RectF)_cachedBounds;

#if IOS || MACCATALYST || __IOS__

				if (PlatformPath is not global::CoreGraphics.CGPath cgPath)
				{
					PlatformPath = cgPath = Platform.GraphicsExtensions.AsCGPath(this);
				}

				_cachedBounds = Platform.GraphicsExtensions.AsRectangleF(cgPath.PathBoundingBox);
#else
				_cachedBounds = GetBoundsByFlattening();
#endif

				return (RectF)_cachedBounds;
			}
		}

		/// <summary>
		/// Computes bounds by flattening curves with the given flatness, updating the cache.
		/// </summary>
		/// <param name="flatness">Maximum allowed deviation when flattening (smaller = more points).</param>
		/// <returns>The bounding rectangle.</returns>
		public RectF GetBoundsByFlattening(float flatness = 0.001f)
		{
			if (_cachedBounds != null)
				return (RectF)_cachedBounds;

			var path = GetFlattenedPath(flatness, true);

			float l = 0f;
			float t = 0f;
			float r = l;
			float b = t;

			// Make sure the path actually has points in it.
			if (path != null && path.Count > 0)
			{
				l = path[0].X;
				t = path[0].Y;
				r = l;
				b = t;

				for (int i = 1; i < path.Count; i++)
				{
					var point = path[i];
					if (point.X < l)
						l = point.X;
					if (point.Y < t)
						t = point.Y;
					if (point.X > r)
						r = point.X;
					if (point.Y > b)
						b = point.Y;
				}
			}

			_cachedBounds = new RectF(l, t, r - l, b - t);
			return (RectF)_cachedBounds;
		}

		/// <summary>
		/// Creates a new path consisting only of line segments approximating all curves and arcs.
		/// </summary>
		/// <param name="flatness">Maximum allowed deviation per segment (smaller = more segments).</param>
		/// <param name="includeSubPaths">If <c>true</c>, flattens all sub-paths; otherwise stops after the first closed one.</param>
		/// <returns>A flattened path.</returns>
		public PathF GetFlattenedPath(float flatness = .001f, bool includeSubPaths = false)
		{
			var flattenedPath = new PathF();
			List<PointF> flattenedPoints = null;
			List<PointF> curvePoints = null;
			bool foundClosed = false;
			var pointIndex = 0;
			int arcAngleIndex = 0;
			int arcClockwiseIndex = 0;

			for (var i = 0; i < _operations.Count && !foundClosed; i++)
			{
				var operation = _operations[i];
				switch (operation)
				{
					case PathOperation.Move:
						flattenedPath.MoveTo(_points[pointIndex++]);
						break;
					case PathOperation.Line:
						flattenedPath.LineTo(_points[pointIndex++]);
						break;
					case PathOperation.Quad:
						flattenedPoints ??= new List<PointF>();
						flattenedPoints.Clear();
						curvePoints ??= new List<PointF>();
						curvePoints.Clear();
						QuadToCubic(pointIndex, curvePoints);
						FlattenCubicSegment(0, flatness, curvePoints, flattenedPoints);
						foreach (var point in flattenedPoints)
							flattenedPath.LineTo(point);
						pointIndex += 2;
						break;
					case PathOperation.Cubic:
						flattenedPoints ??= new List<PointF>();
						flattenedPoints.Clear();
						FlattenCubicSegment(pointIndex - 1, flatness, _points, flattenedPoints);
						foreach (var point in flattenedPoints)
							flattenedPath.LineTo(point);
						pointIndex += 3;
						break;
					case PathOperation.Arc:
						var topLeft = _points[pointIndex++];
						var bottomRight = _points[pointIndex++];
						float startAngle = GetArcAngle(arcAngleIndex++);
						float endAngle = GetArcAngle(arcAngleIndex++);
						var clockwise = GetArcClockwise(arcClockwiseIndex++);
						var flattenedArcPath = FlattenArc(topLeft, bottomRight, startAngle, endAngle, clockwise, flatness);
						foreach (var point in flattenedArcPath.Points)
							flattenedPath.LineTo(point);
						break;
					case PathOperation.Close:
						flattenedPath.Close();
						if (!includeSubPaths)
						{
							foundClosed = true;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return flattenedPath;
		}

		private PathF FlattenArc(PointF topLeft, PointF bottomRight, float startAngle, float endAngle, bool clockwise, float flattness)
		{
			var arcFlattener = new ArcFlattener(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, startAngle, endAngle, clockwise);
			var flattenedPath = arcFlattener.CreateFlattenedPath(flattness);
			return flattenedPath.GetFlattenedPath();
		}

		private void QuadToCubic(int pointIndex, List<PointF> curvePoints)
		{
			var startPoint = _points[pointIndex - 1];
			var quadControlPoint = _points[pointIndex];
			var endPoint = _points[pointIndex + 1];

			var controlPoint1 = new PointF(startPoint.X + 2.0f * (quadControlPoint.X - startPoint.X) / 3.0f, startPoint.Y + 2.0f * (quadControlPoint.Y - startPoint.Y) / 3.0f);
			var controlPoint2 = new PointF(endPoint.X + 2.0f * (quadControlPoint.X - endPoint.X) / 3.0f, endPoint.Y + 2.0f * (quadControlPoint.Y - endPoint.Y) / 3.0f);

			curvePoints.Add(startPoint);
			curvePoints.Add(controlPoint1);
			curvePoints.Add(controlPoint2);
			curvePoints.Add(endPoint);
		}

		private void FlattenCubicSegment(int index, double flatness, List<PointF> curvePoints, List<PointF> flattenedPoints)
		{
			int i, k;
			var numberOfPoints = 1;
			var vectors = new Vector2[4];

			double rCurve = 0;

			for (i = index + 1; i <= index + 2; i++)
			{
				vectors[0] = (GetPointAsVector(curvePoints, i - 1) + GetPointAsVector(curvePoints, i + 1)) * 0.5f - GetPointAsVector(curvePoints, i);

				double r = vectors[0].Length();

				if (r > rCurve)
					rCurve = r;
			}

			if (rCurve <= 0.5 * flatness)
			{
				var vector = GetPointAsVector(curvePoints, index + 3);
				flattenedPoints.Add(new Point(vector.X, vector.Y));
				return;
			}

			numberOfPoints = (int)(Math.Sqrt(rCurve / flatness)) + 3;
			if (numberOfPoints > 1000)
				numberOfPoints = 1000;

			var d = 1.0f / numberOfPoints;

			vectors[0] = GetPointAsVector(curvePoints, index);
			for (i = 1; i <= 3; i++)
			{
				vectors[i] = DeCasteljau(curvePoints, index, i * d);
				flattenedPoints.Add(new Point(vectors[i].X, vectors[i].Y));
			}

			for (i = 1; i <= 3; i++)
				for (k = 0; k <= (3 - i); k++)
					vectors[k] = vectors[k + 1] - vectors[k];

			for (i = 4; i <= numberOfPoints; i++)
			{
				for (k = 1; k <= 3; k++)
					vectors[k] += vectors[k - 1];

				flattenedPoints.Add(new Point(vectors[3].X, vectors[3].Y));
			}
		}

		private Vector2 DeCasteljau(List<PointF> curvePoints, int index, float t)
		{
			var s = 1.0f - t;

			var vector0 = s * GetPointAsVector(curvePoints, index) + t * GetPointAsVector(curvePoints, index + 1);
			var vector1 = s * GetPointAsVector(curvePoints, index + 1) + t * GetPointAsVector(curvePoints, index + 2);
			var vector2 = s * GetPointAsVector(curvePoints, index + 2) + t * GetPointAsVector(curvePoints, index + 3);

			vector0 = s * vector0 + t * vector1;
			vector1 = s * vector1 + t * vector2;
			return s * vector0 + t * vector1;
		}

		private Vector2 GetPointAsVector(List<PointF> curvePoints, int index)
		{
			var point = curvePoints[index];
			return new Vector2(point.X, point.Y);
		}
	}
}
