using System;
using System.Drawing;
using System.Maui.Graphics;

namespace System.Maui.Shapes
{
	/// <summary>
	/// A capsule shape is equivalent to a rounded rectangle where the corner radius is chosen
	/// as half the length of the rectangle’s smallest edge.
	/// </summary>
	public class Capsule : IShape
	{
		public Path PathForBounds(Maui.Rectangle rect)
		{
			var path = new Path();
			var cornerSize = Math.Min(rect.Width, rect.Height) / 2;
			path.AppendRoundedRectangle(rect, cornerSize);
			return path;
		}
	}
}
