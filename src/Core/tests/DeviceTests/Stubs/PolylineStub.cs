#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class PolylineStub : IPolyline
	{
		public PolylineStub()
		{

		}

		public PolylineStub(PointCollection? points)
		{
			Points = points;
		}

		public PointCollection? Points { get; set; }
	}
}