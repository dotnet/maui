using System.Maui.Graphics;

namespace System.Maui.Shapes
{
	public class RectangleShape : IShape
	{
		public Path PathForBounds (Maui.Rectangle rect)
		{
			var path = new Path();
			path.AppendRectangle(rect);
			return path;
		}
	}
}
