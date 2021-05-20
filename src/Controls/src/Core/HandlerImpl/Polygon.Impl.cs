#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class Polygon : IPolygon
	{
		public Polygon()
		{

		}

		public Polygon(PointCollection? points)
		{
			Points = points;
		}

		public PointCollection? Points { get; set; }
	}
}
