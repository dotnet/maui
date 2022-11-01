namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RoundRectangleStub : ShapeViewStub, IShapeView
	{
		public RoundRectangleStub()
		{

		}

		public RoundRectangleStub(CornerRadius cornerRadius)
		{
			Shape = new RoundRectangleShapeStub(cornerRadius);
		}
	}

	public class RoundRectangleShapeStub : StubBase, IShape
	{
		public RoundRectangleShapeStub()
		{

		}

		public RoundRectangleShapeStub(CornerRadius cornerRadius) : this()
		{
			CornerRadius = cornerRadius;
		}

		public CornerRadius CornerRadius { get; set; }

		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			path.AppendRoundedRectangle(rect, (float)CornerRadius.TopLeft, (float)CornerRadius.TopRight, (float)CornerRadius.BottomLeft, (float)CornerRadius.BottomRight);

			return path;
		}
	}
}