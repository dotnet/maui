#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class GeometryStub
	{
		public void AppendToPath(PathF path)
		{

		}
	}

	public class PathStub : StubBase, IShape
	{
		public PathStub()
		{

		}

		public GeometryStub? Data { get; set; }

		public PathF PathForBounds(Rectangle rect)
		{
			var path = new PathF();

			Data?.AppendToPath(path);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}