using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Rectangle Adorner.
	/// </summary>
	public class RectangleAdorner : adorner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RectangleAdorner"/> class.
		/// </summary>
		/// <param name="view">An <see cref="IView"/> to create the Adorner around.</param>
		/// <param name="density">Override density setting. Default: 1</param>
		/// <param name="offset">Offset point used for positioning drawable object. Default: null</param>
		/// <param name="fillColor">Canvas Fill Color.</param>
		/// <param name="strokeColor">Canvas Stroke Color.</param>
		public RectangleAdorner(IView view, float density = 1, Point? offset = null, Color? fillColor = null, Color? strokeColor = null)
			: base(view, density, offset, fillColor, strokeColor)
		{
		}

		/// <inheritdoc/>
		public override bool Contains(Point point)
		{
			return DrawnRectangle.Contains(point);
		}

		/// <inheritdoc/>
		public override void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			base.Draw(canvas, dirtyRect);
			
			var boundingBox = VisualView.GetBoundingBox();
			var x = (boundingBox.X / Density) + (Offset.X);
			var y = (boundingBox.Y / Density) + (Offset.Y);
			var width = ((boundingBox.Width) / Density);
			var height = ((boundingBox.Height) / Density);
			DrawnRectangle = new Rectangle(x, y, width, height);
			canvas.FillRectangle(DrawnRectangle);
		}

		/// <summary>
		/// Gets or sets the rectangle drawn by the 
		/// </summary>
		internal Rectangle DrawnRectangle { get; private set; } = new Rectangle();
	}
}
