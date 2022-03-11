using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class EllipseGeometryStub : IShape
	{
		public EllipseGeometryStub()
		{

		}

		public EllipseGeometryStub(Point center, double radiusX, double radiusY)
		{
			Center = center;
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		public Point Center { get; set; }

		public double RadiusX { get; set; }

		public double RadiusY { get; set; }

		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			var radiusX = (float)RadiusX;
			var radiusY = (float)RadiusY;

			path.AppendEllipse((float)Center.X - radiusX, (float)Center.Y - radiusY, radiusX * 2f, radiusY * 2f);

			return path;
		}
	}
}