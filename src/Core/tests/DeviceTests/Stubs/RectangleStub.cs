namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RectangleStub : ShapeViewStub, IShapeView
	{
		public RectangleStub()
		{

		}

		public RectangleStub(double radiusX, double radiusY)
		{
			Shape = new RectangleShapeStub(radiusX, radiusY);
		}
	}

	public class RectangleShapeStub : StubBase, IShape
	{
		public RectangleShapeStub()
		{

		}

		public RectangleShapeStub(double radiusX, double radiusY) : this()
		{
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		public double RadiusX { get; set; }
		public double RadiusY { get; set; }

		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			path.AppendRoundedRectangle(rect, (float)RadiusY + (float)RadiusX);

			return path;
		}
	}
}