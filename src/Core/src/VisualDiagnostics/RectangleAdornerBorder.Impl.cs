using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Rectangle Adorner Border.
	/// </summary>
	public class RectangleAdornerBorder : AdornerBorder
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RectangleAdornerBorder"/> class.
		/// </summary>
		/// <param name="view">An <see cref="IView"/> to create the Adorner Border around.</param>
		/// <param name="dpi">Override DPI setting. Default: 1</param>
		/// <param name="offset">Offset Rectangle used for positioning drawable object. Default: null</param>
		/// <param name="fillColor">Canvas Fill Color.</param>
		/// <param name="strokeColor">Canvas Stroke Color.</param>
		public RectangleAdornerBorder(IView view, float dpi = 1, Rectangle? offset = null, Color? fillColor = null, Color? strokeColor = null)
			: base(view, dpi, offset, fillColor, strokeColor)
		{
		}

		/// <inheritdoc/>
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

		/// <summary>
		/// Gets or sets the rectangle drawn by the 
		/// </summary>
		internal Rectangle DrawnRectangle { get; private set; } = new Rectangle();
	}
}
