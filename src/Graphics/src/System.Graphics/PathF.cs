using System.Collections.Generic;

namespace System.Graphics
{
    public class PathF : IDisposable
    {
        private readonly List<float> _arcAngles;
        private readonly List<bool> _arcClockwise;
        private readonly List<PointF> _points;
        private readonly List<PathOperation> _operations;
        private int _subPathCount;
        private readonly List<bool> _subPathsClosed;

        private object _nativePath;

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

        public PathF(float x, float y) : this(new PointF(x, y))
        {
            MoveTo(x,y);
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

        public PathF MoveTo(PointF aPoint)
        {
            _subPathCount++;
            _subPathsClosed.Add(false);
            _points.Add(aPoint);
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
                        if (s == segmentIndex) return type;

                        pointIndex++;
                    }
                    else if (type == PathOperation.Line)
                    {
                        if (s == segmentIndex) return type;
                        pointIndex++;
                    }
                    else if (type == PathOperation.Quad)
                    {
                        if (s == segmentIndex) return type;
                        pointIndex += 2;
                    }
                    else if (type == PathOperation.Cubic)
                    {
                        if (s == segmentIndex) return type;
                        pointIndex += 3;
                    }
                    else if (type == PathOperation.Arc)
                    {
                        if (s == segmentIndex) return type;
                        pointIndex += 2;
                        arcAngleIndex += 2;
                        arcClockwiseIndex++;
                    }
                    else if (type == PathOperation.Close)
                    {
                        if (s == segmentIndex) return type;
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
                            var points = new[] {_points[pointIndex]};
                            return points;
                        }

                        pointIndex++;
                    }
                    else if (segmentType == PathOperation.Line)
                    {
                        if (segment == segmentIndex)
                        {
                            var points = new[] {_points[pointIndex]};
                            return points;
                        }

                        pointIndex++;
                    }

                    else if (segmentType == PathOperation.Quad)
                    {
                        if (segment == segmentIndex)
                        {
                            var points = new[] {_points[pointIndex++], _points[pointIndex]};
                            return points;
                        }

                        pointIndex += 2;
                    }
                    else if (segmentType == PathOperation.Cubic)
                    {
                        if (segment == segmentIndex)
                        {
                            var points = new[] {_points[pointIndex++], _points[pointIndex++], _points[pointIndex]};
                            return points;
                        }

                        pointIndex += 3;
                    }
                    else if (segmentType == PathOperation.Arc)
                    {
                        if (segment == segmentIndex)
                        {
                            var points = new[] {_points[pointIndex++], _points[pointIndex]};
                            return points;
                        }

                        pointIndex += 2;
                    }
                    else if (segmentType == PathOperation.Close)
                    {
                        if (segment == segmentIndex)
                        {
                            return new PointF[] { };
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

            foreach (var vCommand in _operations)
            {
                if (vCommand == PathOperation.Move)
                {
                    _subPathCount++;
                    _subPathsClosed.Add(false);
                }
                else if (vCommand == PathOperation.Close)
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
                                        _points[pointIndex] = (PointF) points[points.Length - 1];
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
            return Geometry.RotatePoint(pivotPoint, point, angle);
        }

        public void Transform(AffineTransform transform)
        {
            for (var i = 0; i < _points.Count; i++)
                _points[i] = transform.Transform(_points[i]);

            Invalidate();
        }

        public List<PathF> Separate()
        {
            var vPaths = new List<PathF>();
            if (_points == null || _operations == null)
                return vPaths;

            PathF vPath = null;

            // ReSharper disable PossibleNullReferenceException
            var i = 0;
            var a = 0;
            var c = 0;

            foreach (var vType in _operations)
            {
                if (vType == PathOperation.Move)
                {
                    vPath = new PathF();
                    vPaths.Add(vPath);
                    vPath.MoveTo(_points[i++]);
                }
                else if (vType == PathOperation.Line)
                {
                    vPath.LineTo(_points[i++]);
                }
                else if (vType == PathOperation.Quad)
                {
                    vPath.QuadTo(_points[i++], _points[i++]);
                }
                else if (vType == PathOperation.Cubic)
                {
                    vPath.CurveTo(_points[i++], _points[i++], _points[i++]);
                }
                else if (vType == PathOperation.Arc)
                {
                    vPath.AddArc(_points[i++], _points[i++], _arcAngles[a++], _arcAngles[a++], _arcClockwise[c++]);
                }
                else if (vType == PathOperation.Close)
                {
                    vPath.Close();
                    vPath = null;
                }
            }
            // ReSharper restore PossibleNullReferenceException

            return vPaths;
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

        public void AppendEllipse(RectangleF rect)
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
            var offsetY = h / 2 * .55f;
            var offsetX = w / 2 * .55f;

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
            var offsetY = r * .55f;
            var offsetX = r * .55f;

            MoveTo(new PointF(minX, midY));
            CurveTo(new PointF(minX, midY - offsetY), new PointF(midX - offsetX, minY), new PointF(midX, minY));
            CurveTo(new PointF(midX + offsetX, minY), new PointF(maxX, midY - offsetY), new PointF(maxX, midY));
            CurveTo(new PointF(maxX, midY + offsetY), new PointF(midX + offsetX, maxY), new PointF(midX, maxY));
            CurveTo(new PointF(midX - offsetX, maxY), new PointF(minX, midY + offsetY), new PointF(minX, midY));
            Close();
        }

        public void AppendRectangle(RectangleF rect, bool includeLast = false)
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

        public void AppendRoundedRectangle(RectangleF rect, float cornerRadius, bool includeLast = false)
        {
            AppendRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius, includeLast);
        }

        public void AppendRoundedRectangle(float x, float y, float w, float h, float cornerRadius, bool includeLast = false)
        {
            cornerRadius = ClampCornerRadius(cornerRadius, w, h);

            var minx = x;
            var miny = y;
            var maxx = minx + w;
            var maxy = miny + h;

            var handleOffset = cornerRadius * .55f;
            var cornerOffset = cornerRadius - handleOffset;

            MoveTo(new PointF(minx, miny + cornerRadius));
            CurveTo(new PointF(minx, miny + cornerOffset), new PointF(minx + cornerOffset, miny), new PointF(minx + cornerRadius, miny));
            LineTo(new PointF(maxx - cornerRadius, miny));
            CurveTo(new PointF(maxx - cornerOffset, miny), new PointF(maxx, miny + cornerOffset), new PointF(maxx, miny + cornerRadius));
            LineTo(new PointF(maxx, maxy - cornerRadius));
            CurveTo(new PointF(maxx, maxy - cornerOffset), new PointF(maxx - cornerOffset, maxy), new PointF(maxx - cornerRadius, maxy));
            LineTo(new PointF(minx + cornerRadius, maxy));
            CurveTo(new PointF(minx + cornerOffset, maxy), new PointF(minx, maxy - cornerOffset), new PointF(minx, maxy - cornerRadius));

            if (includeLast)
            {
                LineTo(new PointF(minx, miny + cornerRadius));
            }

            Close();
        }

        public void AppendRoundedRectangle(RectangleF rect, float topLeftCornerRadius, float topRightCornerRadius, float bottomLeftCornerRadius, float bottomRightCornerRadius, bool includeLast = false)
        {
            AppendRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius, includeLast);
        }

        public void AppendRoundedRectangle(float x, float y, float w, float h, float topLeftCornerRadius, float topRightCornerRadius, float bottomLeftCornerRadius, float bottomRightCornerRadius, bool includeLast = false)
        {
            topLeftCornerRadius = ClampCornerRadius(topLeftCornerRadius, w, h);
            topRightCornerRadius = ClampCornerRadius(topRightCornerRadius, w, h);
            bottomLeftCornerRadius = ClampCornerRadius(bottomLeftCornerRadius, w, h);
            bottomRightCornerRadius = ClampCornerRadius(bottomRightCornerRadius, w, h);

            var minx = x;
            var miny = y;
            var maxx = minx + w;
            var maxy = miny + h;

            var topLeftCornerOffset = topLeftCornerRadius - (topLeftCornerRadius * .55f);
            var topRightCornerOffset = topRightCornerRadius - (topRightCornerRadius * .55f);
            var bottomLeftCornerOffset = bottomLeftCornerRadius - (bottomLeftCornerRadius * .55f);
            var bottomRightCornerOffset = bottomRightCornerRadius - (bottomRightCornerRadius * .55f);

            MoveTo(new PointF(minx, miny + topLeftCornerRadius));
            CurveTo(new PointF(minx, miny + topLeftCornerOffset), new PointF(minx + topLeftCornerOffset, miny), new PointF(minx + topLeftCornerRadius, miny));
            LineTo(new PointF(maxx - topRightCornerRadius, miny));
            CurveTo(new PointF(maxx - topRightCornerOffset, miny), new PointF(maxx, miny + topRightCornerOffset), new PointF(maxx, miny + topRightCornerRadius));
            LineTo(new PointF(maxx, maxy - bottomRightCornerRadius));
            CurveTo(new PointF(maxx, maxy - bottomRightCornerOffset), new PointF(maxx - bottomRightCornerOffset, maxy), new PointF(maxx - bottomRightCornerRadius, maxy));
            LineTo(new PointF(minx + bottomLeftCornerRadius, maxy));
            CurveTo(new PointF(minx + bottomLeftCornerOffset, maxy), new PointF(minx, maxy - bottomLeftCornerOffset), new PointF(minx, maxy - bottomLeftCornerRadius));

            if (includeLast)
            {
                LineTo(new PointF(minx, miny + topLeftCornerRadius));
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

        public object NativePath
        {
            get => _nativePath;
            set
            {
                ReleaseNative();
                _nativePath = value;
            }
        }

        public void Invalidate()
        {
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
            _points[index] = _points[index].Offset(dx,dy);
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
                    if (!point.Equals(compareTo[i], Geometry.Epsilon))
                        return false;
                }

                if (_arcAngles != null)
                {
                    for (var i = 0; i < _arcAngles.Count; i++)
                    {
                        var arcAngle = _arcAngles[i];
                        if (Math.Abs(arcAngle - compareTo.GetArcAngle(i)) > Geometry.Epsilon)
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
    }
}