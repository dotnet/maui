using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class EllipseStub : StubBase, IShape
	{
		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			path.AppendEllipse(0f, 0f, (float)Width, (float)Height);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}