using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class EllipseStub : IShape
    {
        public PathF PathForBounds(Rectangle rect, float density = 1)
        {
            var path = new PathF();

            path.AppendEllipse(rect);

            return path;
        }
    }
}