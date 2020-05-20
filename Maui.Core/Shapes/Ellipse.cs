using System.Maui.Graphics;

namespace System.Maui.Shapes
{
	public class Ellipse : IShape
	{
		public Path PathForBounds(Maui.Rectangle rect)
		{
			var path = new Path();
			path.AppendEllipse(rect);
			return path;
		}
	}
}
