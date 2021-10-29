using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class BorderDrawable : IDrawable
	{
		Paint _paint;
		IBorder _border;

		public BorderDrawable(Paint paint, IBorder border)
		{
			_paint = paint;
			_border = border;
		}

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			canvas.SaveState();

			var borderPath = _border.Shape?.PathForBounds(dirtyRect) ?? null;
			if (borderPath != null)
			{
				canvas.MiterLimit = _border.StrokeMiterLimit;
				canvas.StrokeColor = _border.Stroke.ToColor();
				canvas.StrokeDashPattern = _border.StrokeDashPattern;
				canvas.StrokeLineCap = _border.StrokeLineCap;
				canvas.StrokeLineJoin = _border.StrokeLineJoin;
				canvas.StrokeSize = (float)_border.StrokeThickness;

				canvas.DrawPath(borderPath);

				canvas.SetFillPaint(_paint, dirtyRect);
				canvas.FillPath(borderPath);
			}

			canvas.RestoreState();
		}
	}
}
