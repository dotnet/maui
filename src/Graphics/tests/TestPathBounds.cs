using System;
using Microsoft.Maui.Graphics;

class TestPathBounds
{
    static void Main()
    {
        // Test case with a cubic bezier curve that has control points far outside the curve
        var path = new PathF();
        path.MoveTo(0, 0);
        path.CurveTo(0, 500, 444, 500, 444, 0);

        var tightBounds = path.CalculateTightBounds();
        var flattenedBounds = path.GetBoundsByFlattening();

        Console.WriteLine("Tight bounds: " + tightBounds);
        Console.WriteLine("Flattened bounds: " + flattenedBounds);
    }
}