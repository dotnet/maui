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

			PathF? path = CreatePath(rect);

			if (path == null)
				return;

			DrawStrokePath(canvas, rect, path);
			DrawFillPath(canvas, rect, path);
		}
				
		public PathF? CreatePath(RectangleF rect, float density = 1)
		{
			IShape? shape = ShapeView?.Shape;

			if (shape is IEllipse)
				return CreateEllipsePath(rect);

			if (shape is ILine line)
				return CreateLinePath(line, density);

			if (shape is IPolygon polygon)
				return CreatePolygonPath(polygon, density);

			if (shape is IPolyline polyline)
				return CreatePolylinePath(polyline, density);

			if (shape is IPath path)
				return CreatePathPath(path);

			if (shape is IRectangle rectangle)
				return CreateRectanglePath(rectangle, rect);

			return null;
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

			if (ShapeView.Shape is ILine || ShapeView.Shape is IPolyline)
				return;

			canvas.SaveState();

			// Set Fill
			var fillPaint = ShapeView.Fill;
			canvas.SetFillPaint(fillPaint, dirtyRect);

			canvas.FillPath(path);

			canvas.RestoreState();
		}

		PathF? CreateEllipsePath(RectangleF rect)
		{
			var path = new PathF();

			path.AppendEllipse(rect);

			return path;
		}

		PathF? CreateLinePath(ILine line, float density = 1)
		{
			var path = new PathF();

			path.MoveTo(density * (float)line.X1, density * (float)line.Y1);
			path.LineTo(density * (float)line.X2, density * (float)line.Y2);

			return path;
		}

		PathF? CreatePolygonPath(IPolygon polygon, float density = 1)
		{
			var path = new PathF();

			if (polygon.Points?.Count > 0)
			{
				path.MoveTo(density * (float)polygon.Points[0].X, density * (float)polygon.Points[0].Y);

				for (int index = 1; index < polygon.Points.Count; index++)
					path.LineTo(density * (float)polygon.Points[index].X, density * (float)polygon.Points[index].Y);

				path.Close();
			}

			return path;
		}

		PathF? CreatePolylinePath(IPolyline polyline, float density = 1)
		{
			var path = new PathF();

			if (polyline.Points?.Count > 0)
			{
				path.MoveTo(density * (float)polyline.Points[0].X, density * (float)polyline.Points[0].Y);

				for (int index = 1; index < polyline.Points.Count; index++)
					path.LineTo(density * (float)polyline.Points[index].X, density * (float)polyline.Points[index].Y);
			}

			return path;
		}

		PathF? CreatePathPath(IPath path)
		{
			return PathBuilder.Build(path.Data);
		}

		PathF? CreateRectanglePath(IRectangle rectangle, RectangleF rect)
		{
			var path = new PathF();

			path.AppendRoundedRectangle(
				rect,
				(float)rectangle.CornerRadius.TopLeft,
				(float)rectangle.CornerRadius.TopRight,
				(float)rectangle.CornerRadius.BottomLeft,
				(float)rectangle.CornerRadius.BottomRight);

			return path;
		}
	}
}