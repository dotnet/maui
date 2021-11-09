using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class RectangleGridAdornerBorder : RectangleAdornerBorder
	{
		public RectangleGridAdornerBorder(IView view, float dpi = 1, Rectangle? offset = null, Color? fillColor = null, Color? strokeColor = null)
			: base(view, dpi, offset, fillColor, strokeColor)
		{
		}

		public override void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			base.Draw(canvas, dirtyRect);
			var y = this.DrawnRectangle.Y;
			var x = this.DrawnRectangle.X;
			var width = this.DrawnRectangle.Width;
			var height = this.DrawnRectangle.Height;
			canvas.DrawLine(0, (float)y, (float)10000, (float)y);
			canvas.DrawLine(0, (float)(y + height), (float)10000, (float)(y + height));
			canvas.DrawLine((float)x, 0, (float)x, (float)10000);
			canvas.DrawLine((float)(x + width), 0, (float)(x + width), (float)10000);
		}
	}
}
