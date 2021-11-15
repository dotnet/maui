using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class MauiDrawable : IDrawable
	{
		IShadow? _shadow;
		IBorder? _border;
		IShape? _clipShape;
		IShape? _shape;
		Paint? _backgroundPaint;
		Rectangle _bounds;

		public MauiDrawable()
		{
			_bounds = new Rectangle();
		}

		public IShadow? Shadow
		{
			get
			{
				return _shadow;
			}
			set
			{
				_shadow = value;
			}
		}

		public Paint? Background
		{
			get
			{
				return _backgroundPaint;
			}
			set
			{
				_backgroundPaint = value;
			}
		}

		public IBorder? Border
		{
			get
			{
				return _border;
			}
			set
			{
				_border = value;
			}
		}

		public Rectangle Bounds
		{
			get
			{
				return _bounds;
			}
			set
			{
				_bounds = value;
			}
		}

		public IShape? Shape
		{
			get
			{
				return _shape;
			}
			set
			{
				_shape = value;
			}
		}

		public IShape? Clip
		{
			get
			{
				return _clipShape;
			}
			set
			{
				_clipShape = value;
			}
		}

		PathF GetBoundaryPath()
		{
			if (_clipShape != null)
			{
				var clipPath = _clipShape.PathForBounds(_bounds);
				clipPath.Move((float)_bounds.Left, (float)_bounds.Top);
				return clipPath;
			}

			if (_shape != null)
			{
				return _shape.PathForBounds(_bounds);
			}

			var path = new PathF();
			path.AppendRectangle(_bounds);
			return path;
		}

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			var drawablePath = GetBoundaryPath();
			if (_shadow != null)
			{
				canvas.SaveState();
				Color color = _shadow.Paint.ToColor() != null ? _shadow.Paint.ToColor()!.MultiplyAlpha(_shadow.Opacity) : Colors.Black.MultiplyAlpha(_shadow.Opacity);
				canvas.SetShadow(
						new SizeF((float)_shadow.Offset.X, (float)_shadow.Offset.Y),
						(int)_shadow.Radius,
						color);
				canvas.FillPath(drawablePath);
				canvas.RestoreState();

				canvas.SaveState();
				canvas.StrokeColor = Colors.Transparent;
				canvas.DrawPath(drawablePath);
				canvas.ClipPath(drawablePath, WindingMode.EvenOdd);
				canvas.RestoreState();
			}

			if (_backgroundPaint != null)
			{
				canvas.SaveState();
				canvas.SetFillPaint(_backgroundPaint, dirtyRect);
				canvas.FillPath(drawablePath);
				canvas.RestoreState();
			}

			if (_border != null)
			{
				canvas.SaveState();
				var borderPath = _border.Shape?.PathForBounds(_bounds);
				if (borderPath != null)
				{
					canvas.MiterLimit = _border.StrokeMiterLimit;
					canvas.StrokeColor = _border.Stroke.ToColor();
					canvas.StrokeDashPattern = _border.StrokeDashPattern;
					canvas.StrokeLineCap = _border.StrokeLineCap;
					canvas.StrokeLineJoin = _border.StrokeLineJoin;
					canvas.StrokeSize = (float)_border.StrokeThickness;
					canvas.DrawPath(borderPath);
				}
				canvas.RestoreState();
			}
		}
	}
}