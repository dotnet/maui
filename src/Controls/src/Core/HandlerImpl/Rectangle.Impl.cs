using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Rectangle : IShape
	{
		public override PathF GetPath()
		{
			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;
			float w = (float)(Width - StrokeThickness);
			float h = (float)(Height - StrokeThickness);
			float cornerRadius = (float)Math.Max(RadiusX, RadiusY);

			// TODO: Create specific Path taking into account RadiusX and RadiusY
			path.AppendRoundedRectangle(x, y, w, h, cornerRadius);
			return path;
		}
	}
}