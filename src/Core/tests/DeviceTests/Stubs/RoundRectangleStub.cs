using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RoundRectangleStub : StubBase, IShape
	{
		public RoundRectangleStub()
		{

		}

		public RoundRectangleStub(CornerRadius cornerRadius) : this()
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