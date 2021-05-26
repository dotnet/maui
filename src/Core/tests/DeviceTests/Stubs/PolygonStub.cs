#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class PolygonStub : IShape
    {
        public PolygonStub()
        {

        }

        public PolygonStub(PointCollection? points)
        {
            Points = points;
        }

        public PointCollection? Points { get; set; }

        public PathF PathForBounds(Graphics.Rectangle rect, float density = 1)
        {
            var path = new PathF();

            if (Points?.Count > 0)
            {
                path.MoveTo(density * (float)Points[0].X, density * (float)Points[0].Y);

                for (int index = 1; index < Points.Count; index++)
                    path.LineTo(density * (float)Points[index].X, density * (float)Points[index].Y);

                path.Close();
            }

            return path;
        }
    }
}