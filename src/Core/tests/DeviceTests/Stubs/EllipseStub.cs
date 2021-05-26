using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class EllipseStub : IShape
	{

		PathF IShape.PathForBounds(Rectangle rect, float density)
		{
			var path = new PathF();
			path.AppendEllipse(rect);
			return path;
		}
	}
}