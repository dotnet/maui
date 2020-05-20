using System;
using System.Drawing;
using System.Maui.Graphics;

namespace System.Maui.Shapes
{
	public class Circle : IShape
	{
		public Path PathForBounds(Maui.Rectangle rect)
		{
			var size = Math.Min(rect.Width, rect.Height);
			var x = rect.X + (rect.Width - size) / 2;
			var y = rect.Y + (rect.Height - size) / 2;
			var path = new Path();
			path.AppendEllipse(x, y, size, size);
			return path;
		}
	}
}
