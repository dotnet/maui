#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class Polyline : IPolyline
	{
		public Polyline()
		{

		}

		public Polyline(PointCollection? points)
		{
			Points = points;
		}

		public PointCollection? Points { get; set; }
	}
}