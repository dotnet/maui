#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class PolygonStub : IPolygon
	{
		public PolygonStub()
		{

		}

		public PolygonStub(PointCollection? points)
		{
			Points = points;
		}

		public PointCollection? Points { get; set; }
	}
}
