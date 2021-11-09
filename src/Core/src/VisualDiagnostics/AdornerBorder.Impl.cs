using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class AdornerBorder : IAdornerBorder, IDrawable
	{
		public float DPI { get; }

		public IView VisualView { get; }

		public Rectangle Offset { get; }

		public Color FillColor { get; } = Color.FromRgba(225, 0, 0, 125);

		public Color StrokeColor { get; } = Color.FromRgba(225, 0, 0, 125);

		public AdornerBorder(IView view, float dpi = 1, Rectangle? offset = null, Color? fillColor = null, Color? strokeColor = null)
		{
			if (fillColor != null)
				this.FillColor = fillColor;

			if (strokeColor != null)
				this.StrokeColor = strokeColor;

			if (offset == null)
				this.Offset = new Rectangle();
			else
				this.Offset = offset.Value;

			this.VisualView = view;
			this.DPI = dpi;
		}

		public virtual void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			canvas.FillColor = this.FillColor;
			canvas.StrokeColor = this.StrokeColor;
		}
	}
}
