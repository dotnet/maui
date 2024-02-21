using System;
using System.Collections.Generic;
using System.Numerics;
// ReSharper disable MemberCanBePrivate.Global

namespace Microsoft.Maui.Graphics
{
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

		public PathF(PathF path) : this()
		{
			_operations.AddRange(path._operations);
			_points = new List<PointF>(path._points);

			_arcAngles.AddRange(path._arcAngles);
			_arcClockwise.AddRange(path._arcClockwise);

			_subPathCount = path._subPathCount;
			_subPathsClosed = new List<bool>(path._subPathsClosed);
		}

		public PathF(PointF point) : this()
		{
			MoveTo(point.X, point.Y);
		}

		public PathF(float x, float y) : this()
		{
			MoveTo(x, y);
		}

		public PathF()
		{
			_subPathCount = 0;
			_arcAngles = new List<float>();
			_arcClockwise = new List<bool>();
			_points = new List<PointF>();
			_operations = new List<PathOperation>();
			_subPathsClosed = new List<bool>();
		}

		public int SubPathCount => _subPathCount;

		public bool Closed
		{
			get
			{
				if (_operations.Count > 0)
					return _operations[_operations.Count - 1] == PathOperation.Close;

				return false;
			}
		}

		public PointF FirstPoint
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points[0];

				return default;
			}
		}

		public IEnumerable<PathOperation> SegmentTypes
		{
			get
			{
				for (var i = 0; i < _operations.Count; i++)
					yield return _operations[i];
			}
		}

		public IEnumerable<PointF> Points
		{
			get
			{
				for (var i = 0; i < _points.Count; i++)
					yield return _points[i];
			}
		}

		public PointF LastPoint
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points[_points.Count - 1];

				return default;
			}
		}

		public int LastPointIndex
		{
			get
			{
				if (_points != null && _points.Count > 0)
					return _points.Count - 1;

				return -1;
			}
		}

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

		public void SetPoint(int index, float x, float y)
		{
			_points[index] = new PointF(x, y);
			Invalidate();
		}

		public void SetPoint(int index, PointF point)
		{
			_points[index] = point;
			Invalidate();
		}

		public int Count => _points.Count;

		public int OperationCount => _operations.Count;

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

		public PathOperation GetSegmentType(int aIndex)
		{
			return _operations[aIndex];
		}

		public float GetArcAngle(int aIndex)
		{
			if (_arcAngles.Count > aIndex)
			{
				return _arcAngles[aIndex];
			}

			return 0;
		}

		public void SetArcAngle(int aIndex, float aValue)
		{
			if (_arcAngles.Count > aIndex)
			{
				_arcAngles[aIndex] = aValue;
			}

			Invalidate();
		}

		public bool GetArcClockwise(int aIndex)
		{
			if (_arcClockwise.Count > aIndex)
			{
				return _arcClockwise[aIndex];
			}

			return false;
		}

		public void SetArcClockwise(int aIndex, bool aValue)
		{
			if (_arcClockwise.Count > aIndex)
			{
				_arcClockwise[aIndex] = aValue;
			}

			Invalidate();
		}

		public PathF MoveTo(float x, float y)
		{
			return MoveTo(new PointF(x, y));
		}

		public PathF MoveTo(PointF point)
		{
			_subPathCount++;
			_subPathsClosed.Add(false);
			_points.Add(point);
			_operations.Add(PathOperation.Move);
			Invalidate();
			return this;
		}

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

		public PathF LineTo(float x, float y)
		{
			return LineTo(new PointF(x, y));
		}

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

		public PathF AddArc(float x1, float y1, float x2, float y2, float startAngle, float endAngle, bool clockwise)
		{
			return AddArc(new PointF(x1, y1), new PointF(x2, y2), startAngle, endAngle, clockwise);
		}

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

		public PathF QuadTo(float cx, float cy, float x, float y)
		{
			return QuadTo(new PointF(cx, cy), new PointF(x, y));
		}

		public PathF QuadTo(PointF controlPoint, PointF point)
		{
			_points.Add(controlPoint);
			_points.Add(point);
			_operations.Add(PathOperation.Quad);
			Invalidate();
			return this;
		}

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

		public PathF CurveTo(float c1X, float c1Y, float c2X, float c2Y, float x, float y)
		{
			return CurveTo(new PointF(c1X, c1Y), new PointF(c2X, c2Y), new PointF(x, y));
		}

		public PathF CurveTo(PointF controlPoint1, PointF controlPoint2, PointF point)
		{
			_points.Add(controlPoint1);
			_points.Add(controlPoint2);
			_points.Add(point);
			_operations.Add(PathOperation.Cubic);
			Invalidate();
			return this;
		}

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

		public PointF GetRotatedPoint(int pointIndex, PointF pivotPoint, float angle)
		{
			var point = _points[pointIndex];
			return GeometryUtil.RotatePoint(pivotPoint, point, angle);
		}

		public void Transform(Matrix3x2 transform)
		{
			for (var i = 0; i < _points.Count; i++)
				_points[i] = Vector2.Transform((Vector2)_points[i], transform);

			Invalidate();
		}

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

		public void AppendEllipse(RectF rect)
		{
			AppendEllipse(rect.X, rect.Y, rect.Width, rect.Height);
		}

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

		public void AppendCircle(PointF center, float r)
		{
			AppendCircle(center.X, center.Y, r);
		}

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

		public void AppendRectangle(RectF rect, bool includeLast = false)
		{
			AppendRectangle(rect.X, rect.Y, rect.Width, rect.Height, includeLast);
		}

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

		public void AppendRoundedRectangle(RectF rect, float cornerRadius, bool includeLast = false)
		{
			AppendRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius, includeLast);
		}

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

		public void AppendRoundedRectangle(RectF rect, float topLeftCornerRadius, float topRightCornerRadius, float bottomLeftCornerRadius, float bottomRightCornerRadius, bool includeLast = false)
		{
			AppendRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius, includeLast);
		}

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

		public bool IsSubPathClosed(int subPathIndex)
		{
			if (subPathIndex >= 0 && subPathIndex < SubPathCount)
			{
				return _subPathsClosed[subPathIndex];
			}

			return false;
		}

		public object PlatformPath
		{
			get => _platformPath;
			set
			{
				ReleaseNative();
				_platformPath = value;
			}
		}

		public void Invalidate()
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
			if (_platformPath is IDisposable disposable)
				disposable.Dispose();

			_platformPath = null;
		}

		public void Move(float x, float y)
		{
			for (var i = 0; i < _points.Count; i++)
			{
				_points[i] = _points[i].Offset(x, y);
			}

			Invalidate();
		}

		public void MovePoint(int index, float dx, float dy)
		{
			_points[index] = _points[index].Offset(dx, dy);
			Invalidate();
		}

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
