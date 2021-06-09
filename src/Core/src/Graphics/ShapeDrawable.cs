using System;

namespace Microsoft.Maui.Graphics
{
	public class ShapeDrawable : IDrawable
	{
		public ShapeDrawable()
		{

		}

		public ShapeDrawable(IShapeView? shape)
		{
			ShapeView = shape;
		}

		public IShapeView? ShapeView { get; set; }

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			var rect = dirtyRect;

			if (ShapeView != null)
			{
				float strokeThickness = (float)ShapeView.StrokeThickness;
				rect = new RectangleF(dirtyRect.X + strokeThickness, dirtyRect.Y + strokeThickness, dirtyRect.Width - (strokeThickness * 2), dirtyRect.Height - (strokeThickness * 2));
			}

			IShape? shape = ShapeView?.Shape;

			if (shape == null)
				return;

			PathF? path = shape.GetPath();

			if (path == null)
				return;

			// Scale the path if needed depending on specified aspect
			var aspect = ShapeView?.Aspect;

			if (aspect.HasValue)
			{
				var viewWidth = (float)(ShapeView?.Width ?? 0);
				var viewHeight = (float)(ShapeView?.Height ?? 0);

				// TODO: Calculate path's bounds to get a width and height
				var pathWidth = viewWidth;// 0f;
				var pathHeight = viewHeight;// 0f;

				// If one dimension is 0, we have nothing to display anyway
				if (pathWidth > 0 && pathHeight > 0)
				{
					if (aspect == PathAspect.Stretch)
					{
						var scaleX = viewWidth / pathWidth;
						var scaleY = viewHeight / pathHeight;
						
						path.Transform(AffineTransform.GetScaleInstance(scaleX, scaleY));
					}
					else if (aspect == PathAspect.AspectFill)
					{
						var scaleX = viewWidth / pathWidth;
						var scaleY = viewHeight / pathHeight;
						var scaleDimension = Math.Max(scaleX, scaleY);

						path.Transform(AffineTransform.GetScaleInstance(scaleDimension, scaleDimension));
					}
					else if (aspect == PathAspect.AspectFit)
					{
						var scaleX = viewWidth / pathWidth;
						var scaleY = viewHeight / pathHeight;
						var scaleDimension = Math.Min(scaleX, scaleY);

						path.Transform(AffineTransform.GetScaleInstance(scaleDimension, scaleDimension));
					}
					else if (aspect == PathAspect.Center)
					{
						// TODO: Calculate move
					}
				}
			}

			DrawStrokePath(canvas, rect, path);
			DrawFillPath(canvas, rect, path);
		}
				
		void DrawStrokePath(ICanvas canvas, RectangleF dirtyRect, PathF path)
		{
			if (ShapeView == null || ShapeView.Shape == null)
				return;

			canvas.SaveState();

			// Set StrokeThickness
			float strokeThickness = (float)ShapeView.StrokeThickness;
			canvas.StrokeSize = strokeThickness;

			// Set Stroke
			var stroke = ShapeView.Stroke;
			canvas.StrokeColor = stroke;

			// Set StrokeLineCap
			var strokeLineCap = ShapeView.StrokeLineCap;
			canvas.StrokeLineCap = strokeLineCap;

			// Set StrokeLineJoin
			var strokeLineJoin = ShapeView.StrokeLineJoin;
			canvas.StrokeLineJoin = strokeLineJoin;

			// Set StrokeDashPattern
			var strokeDashPattern = ShapeView.StrokeDashPattern;
			canvas.StrokeDashPattern = strokeDashPattern;

			// Set StrokeMiterLimit
			var strokeMiterLimit = ShapeView.StrokeMiterLimit;
			canvas.MiterLimit = strokeMiterLimit;

			canvas.DrawPath(path);

			canvas.RestoreState();

			canvas.SaveState();
		}

		void DrawFillPath(ICanvas canvas, RectangleF dirtyRect, PathF path)
		{
			if (ShapeView == null || ShapeView.Shape == null)
				return;

			if(!path.Closed)
				return;

			canvas.SaveState();

			// Set Fill
			var fillPaint = ShapeView.Fill;
			canvas.SetFillPaint(fillPaint, dirtyRect);

			canvas.FillPath(path);

			canvas.RestoreState();
		}
	}
}