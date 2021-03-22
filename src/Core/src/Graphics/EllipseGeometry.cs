namespace Microsoft.Maui.Graphics
{
	public class EllipseGeometry : Geometry
	{
		public EllipseGeometry()
		{

		}

		public EllipseGeometry(Point center, double radiusX, double radiusY)
		{
			Center = center;
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		public Point Center { get; set; }

		public double RadiusX { get; set; }

		public double RadiusY { get; set; }
	}
}