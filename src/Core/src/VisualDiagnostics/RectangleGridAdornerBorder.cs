using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Rectangle Grid Adorner Border.
	/// </summary>
	public class RectangleGridAdornerBorder : RectangleAdornerBorder
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RectangleGridAdornerBorder"/> class.
		/// </summary>
		/// <param name="view">An <see cref="IView"/> to create the Adorner Border around.</param>
		/// <param name="dpi">Override DPI setting. Default: 1</param>
		/// <param name="offset">Offset Point used for positioning drawable object. Default: null</param>
		/// <param name="fillColor">Canvas Fill Color.</param>
		/// <param name="strokeColor">Canvas Stroke Color.</param>
		public RectangleGridAdornerBorder(IView view, float dpi = 1, Point? offset = null, Color? fillColor = null, Color? strokeColor = null)
			: base(view, dpi, offset, fillColor, strokeColor)
		{
		}

		/// <inheritdoc/>
		public override bool IsPointInElement(Point point)
		{
			return this.DrawnRectangle.Contains(point);
		}

		/// <inheritdoc/>
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
