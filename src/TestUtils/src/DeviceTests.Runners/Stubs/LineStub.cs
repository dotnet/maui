namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class LineStub : ShapeViewStub, IShapeView
	{
		public LineStub()
		{

		}

		public LineStub(double x1, double y1, double x2, double y2)
		{
			Shape = new LineShapeStub(x1, y1, x2, y2);
		}
	}

	public class LineShapeStub : StubBase, IShape
	{
		public LineShapeStub()
		{

		}

		public LineShapeStub(double x1, double y1, double x2, double y2)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

		public double X1 { get; set; }

		public double Y1 { get; set; }

		public double X2 { get; set; }

		public double Y2 { get; set; }

		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			path.MoveTo((float)X1, (float)Y1);
			path.LineTo((float)X2, (float)Y2);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}