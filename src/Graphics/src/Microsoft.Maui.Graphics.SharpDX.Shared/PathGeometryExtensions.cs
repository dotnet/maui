using SharpDX;
using SharpDX.Direct2D1;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public static class PathGeometryExtensions
    {
        public static void BeginFigure(this GeometrySink target, float x, float y, FigureBegin begin)
        {
            target.BeginFigure(new Vector2(x, y), begin);
        }

        public static void LineTo(this GeometrySink target, float x, float y)
        {
            target.AddLine(new Vector2(x, y));
        }

        public static void QuadTo(this GeometrySink target, float cx, float cy, float x, float y)
        {
            var vSegment = new QuadraticBezierSegment
            {
                Point1 = new Vector2(cx, cy),
                Point2 = new Vector2(x, y)
            };
            target.AddQuadraticBezier(vSegment);
        }

        public static void CubicTo(this GeometrySink target, float cx1, float cy1, float cx2, float cy2, float x, float y)
        {
            var segment = new BezierSegment
            {
                Point1 = new Vector2(cx1, cy1),
                Point2 = new Vector2(cx2, cy2),
                Point3 = new Vector2(x, y)
            };
            target.AddBezier(segment);
        }
    }
}