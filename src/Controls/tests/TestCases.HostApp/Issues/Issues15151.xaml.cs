using System;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 15151, "PathF.Bounds returns too big boxes", PlatformAffected.All)]
    public partial class Issues15151 : ContentPage
    {
        public Issues15151()
        {
            InitializeComponent();
            DrawRectangleExample();
            DrawBezierExample();
            DrawOvalExample();
        }

        private void DrawRectangleExample()
        {
            // Create a simple rectangle path
            var path = new PathF();
            path.AppendRectangle(50, 50, 150, 100);

            var tightBounds = path.CalculateTightBounds();
            var flattenedBounds = path.GetBoundsByFlattening();

            RectangleTightBounds.Text = $"Tight bounds: {tightBounds}";
            RectangleFlattenedBounds.Text = $"Flattened bounds: {flattenedBounds}";

            RectangleView.Drawable = new PathDrawable(path, tightBounds, flattenedBounds);
        }

        private void DrawBezierExample()
        {
            // Create a bezier curve with control points far outside
            var path = new PathF();
            path.MoveTo(50, 100);
            path.CurveTo(50, 300, 300, 300, 300, 100);

            var tightBounds = path.CalculateTightBounds();
            var flattenedBounds = path.GetBoundsByFlattening();

            BezierTightBounds.Text = $"Tight bounds: {tightBounds}";
            BezierFlattenedBounds.Text = $"Flattened bounds: {flattenedBounds}";

            BezierView.Drawable = new PathDrawable(path, tightBounds, flattenedBounds, true);
        }

        private void DrawOvalExample()
        {
            // Create an oval using the method from the issue
            float n = (float)(4 * (Math.Sqrt(2) - 1) / 3);
            var path = GetOval(150, 125, 100, 75, n * 100, n * 75, 1);

            var tightBounds = path.CalculateTightBounds();
            var flattenedBounds = path.GetBoundsByFlattening();

            OvalTightBounds.Text = $"Tight bounds: {tightBounds}";
            OvalFlattenedBounds.Text = $"Flattened bounds: {flattenedBounds}";

            OvalView.Drawable = new PathDrawable(path, tightBounds, flattenedBounds, true);
        }

        private PathF GetOval(float x, float y, float radiusX, float radiusY, float cDx, float cDy, float deviation)
        {
            PathF path = new PathF();
            
            float x1 = 0;
            float xm = radiusX;
            float x2 = radiusX * 2;
            
            float y1 = 0;
            float ym = radiusY;
            float y2 = radiusY * 2;

            x -= radiusX;
            y -= radiusY;

            float cX1 = xm - cDx;
            float cX2 = xm + cDx;
            
            float cY1 = ym - cDy;
            float cY2 = ym + cDy;

            path.MoveTo(x + xm, y + y2);
            path.CurveTo(x + cX1 + deviation * 2, y + y2, x + x1, y + cY2, x + x1, y + ym);
            path.CurveTo(x + x1, y + cY1, x + cX1 - deviation, y + y1, x + xm, y + y1);
            path.CurveTo(x + cX2, y + y1, x + x2, y + cY1, x + x2, y + ym);
            path.CurveTo(x + x2, y + cY2, x + cX2, y + y2, x + xm, y + y2);
            path.Close();

            return path;
        }
    }

    // Class to draw paths and their bounds
    public class PathDrawable : IDrawable
    {
        private readonly PathF _path;
        private readonly RectF _tightBounds;
        private readonly RectF _flattenedBounds;
        private readonly bool _showControlPoints;

        public PathDrawable(PathF path, RectF tightBounds, RectF flattenedBounds, bool showControlPoints = false)
        {
            _path = path;
            _tightBounds = tightBounds;
            _flattenedBounds = flattenedBounds;
            _showControlPoints = showControlPoints;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Draw the flattened bounds
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(_flattenedBounds);
            
            // Draw the tight bounds
            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(_tightBounds);
            
            // Draw the path
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.DrawPath(_path);

            // Draw control points if requested (for bezier curves)
            if (_showControlPoints)
            {
                canvas.StrokeColor = Colors.Red;
                canvas.FillColor = Colors.Red;
                
                // For simplicity, we're just showing some potential control points
                // In a real implementation, we'd extract the actual control points from the path
                if (_path.OperationCount > 0)
                {
                    // For cubic bezier, control points could be far outside the path
                    for (int i = 0; i < _path.Points.Count; i++)
                    {
                        canvas.FillCircle(_path.Points[i], 3);
                    }
                }
            }
            
            // Add a legend
            canvas.FontSize = 10;
            canvas.StrokeSize = 1;
            
            canvas.StrokeColor = Colors.Blue;
            canvas.DrawLine(10, 10, 30, 10);
            canvas.DrawString("Tight Bounds", 35, 13);
            
            canvas.StrokeColor = Colors.Red;
            canvas.DrawLine(10, 25, 30, 25);
            canvas.DrawString("Flattened Bounds", 35, 28);
        }
    }
}