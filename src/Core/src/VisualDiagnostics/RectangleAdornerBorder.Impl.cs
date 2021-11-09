using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class RectangleAdornerBorder : AdornerBorder
	{
		internal Rectangle DrawnRectangle { get; set; } = new Rectangle();

		public RectangleAdornerBorder(IView view, float dpi = 1, Rectangle? offset = null, Color? fillColor = null, Color? strokeColor = null)
			: base(view, dpi, offset, fillColor, strokeColor)
		{
		}

		public override void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			base.Draw(canvas, dirtyRect);

			var rect = this.VisualView.GetNativeViewBounds();
			var x = (rect.X / this.DPI) + (Offset.X);
			var y = (rect.Y / this.DPI) + (Offset.Y);
			var width = (rect.Width / this.DPI) + Offset.Width;
			var height = (rect.Height / this.DPI) + Offset.Height;
			this.DrawnRectangle = new Rectangle(x, y, width, height);
			canvas.FillRectangle(this.DrawnRectangle);
		}
	}
}
