#nullable enable
namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class PolylineStub : ShapeViewStub, IShapeView
	{
		public PolylineStub()
		{

		}

		public PolylineStub(PointCollectionStub? points)
		{
			Shape = new PolylineShapeStub(points);
		}
	}

	public class PolylineShapeStub : StubBase, IShape
	{
		public PolylineShapeStub()
		{

		}

		public PolylineShapeStub(PointCollectionStub? points)
		{
			Points = points;
		}

		public PointCollectionStub? Points { get; set; }

		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			if (Points?.Count > 0)
			{
				path.MoveTo((float)Points[0].X, (float)Points[0].Y);

				for (int index = 1; index < Points.Count; index++)
					path.LineTo((float)Points[index].X, (float)Points[index].Y);
			}

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}