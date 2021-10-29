using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class ShadowDrawable : IDrawable
	{
		IShadow _shadow;
		PathF _path;

		public ShadowDrawable(IShadow shadow, PathF path)
		{
			_shadow = shadow;
			_path = path;
		}

		public void UpdateShadow(IShadow shadow, PathF path)
		{
			_shadow = shadow;
			_path = path;
		}

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			canvas.SaveState();
			Color color = _shadow.Paint.ToColor() != null ? _shadow.Paint.ToColor()!.MultiplyAlpha(_shadow.Opacity) : Colors.Black.MultiplyAlpha(_shadow.Opacity);
			canvas.SetShadow(
					new SizeF((float)_shadow.Offset.X, (float)_shadow.Offset.Y),
					(int)_shadow.Radius,
					color);
			canvas.FillPath(_path);
			canvas.RestoreState();

			canvas.SaveState();
			canvas.StrokeColor = Colors.Transparent;
			canvas.DrawPath(_path);
			canvas.ClipPath(_path, WindingMode.EvenOdd);
			canvas.RestoreState();
		}
	}
}