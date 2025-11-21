using Microsoft.Maui.Graphics;
using System;

namespace GraphicsTester.Scenarios
{
    public class PathBoundsTester : AbstractScenario
    {
        public PathBoundsTester() : base(720, 1024)
        {
        }

        public override void Draw(ICanvas canvas)
        {
            TestSimpleRectangle(canvas);
            TestCubicBezierCurve(canvas);
            TestComplexPath(canvas);
        }

        private void TestSimpleRectangle(ICanvas canvas)
        {
            var path = new PathF();
            path.MoveTo(50, 50);
            path.LineTo(250, 50);
            path.LineTo(250, 250);
            path.LineTo(50, 250);
            path.Close();

            var tightBounds = path.CalculateTightBounds();
            var flattenedBounds = path.GetBoundsByFlattening();

            canvas.StrokeColor = Colors.Black;
            canvas.DrawPath(path);
            
            canvas.FontSize = 12;
            canvas.DrawString($"Simple Rectangle", 50, 280);
            canvas.DrawString($"Tight bounds: {tightBounds}", 50, 300);
            canvas.DrawString($"Flattened bounds: {flattenedBounds}", 50, 320);
        }

        private void TestCubicBezierCurve(ICanvas canvas)
        {
            var path = new PathF();
            path.MoveTo(50, 400);
            path.CurveTo(50, 900, 494, 900, 494, 400);

            var tightBounds = path.CalculateTightBounds();
            var flattenedBounds = path.GetBoundsByFlattening();

            canvas.StrokeColor = Colors.Black;
            canvas.DrawPath(path);
            
            // Draw control points to show how they're outside the actual curve
            canvas.StrokeColor = Colors.Red;
            canvas.DrawCircle(50, 900, 5);
            canvas.DrawCircle(494, 900, 5);
            
            canvas.FontSize = 12;
            canvas.DrawString($"Cubic Bezier Curve with Far Control Points", 50, 600);
            canvas.DrawString($"Tight bounds: {tightBounds}", 50, 620);
            canvas.DrawString($"Flattened bounds: {flattenedBounds}", 50, 640);
            canvas.DrawString($"Red dots show control points outside curve", 50, 660);
        }

        private void TestComplexPath(ICanvas canvas)
        {
            var path = new PathF();
            path.MoveTo(50, 700);
            path.LineTo(150, 700);
            path.QuadTo(200, 750, 150, 800);
            path.CurveTo(100, 850, 0, 850, 50, 800);
            path.Close();

            var tightBounds = path.CalculateTightBounds();
            var flattenedBounds = path.GetBoundsByFlattening();

            canvas.StrokeColor = Colors.Black;
            canvas.DrawPath(path);
            
            canvas.FontSize = 12;
            canvas.DrawString($"Complex Path with Multiple Segment Types", 50, 900);
            canvas.DrawString($"Tight bounds: {tightBounds}", 50, 920);
            canvas.DrawString($"Flattened bounds: {flattenedBounds}", 50, 940);
        }
    }
}