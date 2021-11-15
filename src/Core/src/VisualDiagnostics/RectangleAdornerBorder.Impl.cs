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
		/// <param name="density">Override density setting. Default: 1</param>
		/// <param name="offset">Offset point used for positioning drawable object. Default: null</param>
		/// <param name="fillColor">Canvas Fill Color.</param>
		/// <param name="strokeColor">Canvas Stroke Color.</param>
		public RectangleAdornerBorder(IView view, float density = 1, Point? offset = null, Color? fillColor = null, Color? strokeColor = null)
			: base(view, density, offset, fillColor, strokeColor)
		{
		}

		/// <inheritdoc/>
		public override bool IsPointInElement(Point point)
		{
			return DrawnRectangle.Contains(point);
		}

		/// <inheritdoc/>
		public override void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			base.Draw(canvas, dirtyRect);

			var rect = VisualView.GetNativeViewBounds();
			var x = (rect.X / Density) + (Offset.X);
			var y = (rect.Y / Density) + (Offset.Y);
			var width = (rect.Width / Density);
			var height = (rect.Height / Density);
			DrawnRectangle = new Rectangle(x, y, width, height);
			canvas.FillRectangle(DrawnRectangle);
		}

		/// <summary>
		/// Gets or sets the rectangle drawn by the 
		/// </summary>
		internal Rectangle DrawnRectangle { get; private set; } = new Rectangle();
	}
}
