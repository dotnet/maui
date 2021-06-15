using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RectangleStub : IShape
	{
		public RectangleStub()
		{

		}

		public RectangleStub(double radiusX, double radiusY) : this()
		{
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		public double RadiusX { get; set; }
		public double RadiusY { get; set; }

		public PathF PathForBounds(Rectangle rect)
		{
			var path = new PathF();

			path.AppendRoundedRectangle(rect, (float)RadiusY + (float)RadiusX);

			return path;
		}
	}
}