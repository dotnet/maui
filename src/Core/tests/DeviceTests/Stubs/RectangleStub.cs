using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RectangleStub : IShape
    {
        public RectangleStub()
        {

        }

        public RectangleStub(CornerRadius cornerRadius) : this()
        {
            CornerRadius = cornerRadius;
        }

        public CornerRadius CornerRadius { get; set; }

        public PathF PathForBounds(Graphics.Rectangle rect, float density = 1)
        {
            var path = new PathF();

            path.AppendRoundedRectangle(
                rect,
                (float)CornerRadius.TopLeft,
                (float)CornerRadius.TopRight,
                (float)CornerRadius.BottomLeft,
                (float)CornerRadius.BottomRight);

            return path;
        }
    }
}