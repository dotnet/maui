using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	public class ShapeDrawable : IDrawable
	{
		bool _adjustedShape;

		public ShapeDrawable()
		{

		}

		public ShapeDrawable(IShapeView? shape)
		{
			UpdateShapeView(shape);
		}

		internal IShapeView? ShapeView { get; set; }
		internal WindingMode WindingMode { get; set; }
		internal Matrix3x2? RenderTransform { get; set; }

		public void UpdateShapeView(IShapeView? shape)
		{
			ShapeView = shape;
			_adjustedShape = false;
		}

		public void UpdateWindingMode(WindingMode windingMode)
		{
			WindingMode = windingMode;
		}

		public void UpdateRenderTransform(Matrix3x2? renderTransform)
		{
			RenderTransform = renderTransform;
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			if (ShapeView == null)
				return;

			IShape? shape = ShapeView?.Shape;

			if (shape == null)
				return;

			var rect = dirtyRect;
			
			PathF? path = shape.PathForBounds(rect);

			if (path == null)
				return;

			AdjustShape(canvas, dirtyRect, path);
			ApplyTransform(path);

			DrawStrokePath(canvas, rect, path);
			DrawFillPath(canvas, rect, path);
		}

		void AdjustShape(ICanvas canvas, RectF dirtyRect, PathF path)
		{
			// Adjust the Shape position and size based on the
			// StrokeThickness property value.

			if (_adjustedShape || ShapeView == null)
				return;

			float strokeThickness = (float)ShapeView.StrokeThickness;

			float tX = strokeThickness / 2;
			float tY = strokeThickness / 2;

			canvas.Translate(tX, tY);

			float viewWidth = dirtyRect.Width;
			float viewHeight = dirtyRect.Height;

			RectF bounds = path.GetBoundsByFlattening();

			float pathWidth = bounds.Width;
			float pathHeight = bounds.Height;

			if (pathHeight > viewHeight && pathWidth > viewWidth)
			{
				pathWidth = viewWidth;
				pathHeight = viewHeight;
			}

			float strokeWidth = pathWidth - strokeThickness;
			float strokeHeight = pathHeight - strokeThickness;

			float sX = strokeWidth / pathWidth;
			float sY = strokeHeight / pathHeight;

			canvas.Scale(sX, sY);

			_adjustedShape = true;
		}

		void DrawStrokePath(ICanvas canvas, RectF dirtyRect, PathF path)
		{
			if (ShapeView == null || ShapeView.Shape == null || ShapeView.StrokeThickness <= 0 || ShapeView.Stroke == null)
				return;

			canvas.SaveState();

			// Set StrokeThickness
			float strokeThickness = (float)ShapeView.StrokeThickness;
			canvas.StrokeSize = strokeThickness;

			// Set Stroke
			var stroke = ShapeView.Stroke;

			// TODO: Add Paint support for Stroke in Microsoft.Maui.Graphics.
			// For now, only support a solid color.
			canvas.StrokeColor = stroke.ToColor();

			// Set StrokeLineCap
			var strokeLineCap = ShapeView.StrokeLineCap;
			canvas.StrokeLineCap = strokeLineCap;

			// Set StrokeLineJoin
			var strokeLineJoin = ShapeView.StrokeLineJoin;
			canvas.StrokeLineJoin = strokeLineJoin;

			// Set StrokeDashPattern
			var strokeDashPattern = ShapeView.StrokeDashPattern;
			canvas.StrokeDashPattern = strokeDashPattern;

			// Set StrokeDashOffset	
			var strokeDashOffset = ShapeView.StrokeDashOffset;
			canvas.StrokeDashOffset = strokeDashOffset;
			
			// Set StrokeMiterLimit
			var strokeMiterLimit = ShapeView.StrokeMiterLimit;
			canvas.MiterLimit = strokeMiterLimit;

			canvas.DrawPath(path);

			canvas.RestoreState();
		}

		void DrawFillPath(ICanvas canvas, RectF dirtyRect, PathF path)
		{
			if (ShapeView == null || ShapeView.Shape == null)
				return;

			if (!path.Closed)
				return;

			canvas.SaveState();

			ClipPath(canvas, path);

			// Set Fill
			var fillPaint = ShapeView.Fill;
			canvas.SetFillPaint(fillPaint, dirtyRect);

			canvas.FillPath(path);

			canvas.RestoreState();
		}

		void ClipPath(ICanvas canvas, PathF path)
		{
			canvas.ClipPath(path, WindingMode);
		}

		void ApplyTransform(PathF path)
		{
			if (RenderTransform == null)
				return;

			path.Transform(RenderTransform.Value);
		}
	}
}