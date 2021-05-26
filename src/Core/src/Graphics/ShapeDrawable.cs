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

			PathF? path = shape.PathForBounds(rect);

			if (path == null)
				return;

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