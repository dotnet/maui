using System;

namespace Microsoft.Maui.Graphics
{
    public class ArcFlattener
    {
		private float _cx;
		private float _cy;
		private float _diameter;
		private float _radius;
		private float _fx;
		private float _fy;
		private float _sweep;
		private float _startAngle;
		private PointF _startPoint;

		public ArcFlattener(
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float endAngle,
            bool clockwise)
        {
            _startAngle = startAngle;

            _cx = x + (width / 2);
            _cy = y + (height / 2);

            _diameter = Math.Max(width, height);
            _radius = _diameter / 2;
            _fx = width / _diameter;
            _fy = height / _diameter;

            _sweep = Math.Abs(endAngle - startAngle);
            if (clockwise)
                _sweep *= -1;

            _startPoint = GetPointOnArc(0);
        }

        private PointF GetPointOnArc(
            float percentage)
        {
            var angle = _startAngle + (_sweep * percentage);

            while (angle >= 360)
                angle -= 360;

            angle *= -1;

            var radians = (float)Geometry.DegreesToRadians(angle);
            var point = GetPointAtAngle(0, 0, _radius, radians);
            point.X = _cx + (point.X * _fx);
            point.Y = _cy + (point.Y * _fy);

            return point;
        }

        private static PointF GetPointAtAngle(
            float x,
            float y,
            float distance,
            float radians)
        {
            var x2 = x + (Math.Cos(radians) * distance);
            var y2 = y + (Math.Sin(radians) * distance);
            return new PointF((float)x2, (float)y2);
        }

        public PathF CreateFlattenedPath(
            float flatness = .5f)
        {
            var found = false;
            var n = 1;
            while ((!found) && (n < 1024))
            {
                var candidate = 1f / (float)n;
                var point = GetPointOnArc(candidate);
                if (Geometry.GetDistance(_startPoint.X, _startPoint.Y, point.X, point.Y) <= flatness)
                    found = true;
                else
                    n++;
            }

            var path = new PathF();
            path.MoveTo(_startPoint);

            float step = 1f / n;
            float percentage = 0;

            for (var i = 1; i < n; i++)
            {
                percentage += step;
                var point = GetPointOnArc(percentage);
                path.LineTo(point);
            }

            path.LineTo(GetPointOnArc(1));

            return path;
        }
    }
}
