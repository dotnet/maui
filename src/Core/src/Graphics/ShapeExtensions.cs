//namespace Microsoft.Maui.Graphics
//{
//	public static class ShapeExtensions
//	{
//		public static PathF? CreatePath(this IShape shape, RectangleF rect, float density = 1)
//		{
//			if (shape is IEllipse ellipse)
//				return ellipse.CreateEllipsePath(rect);

//			if (shape is ILine line)
//				return line.CreateLinePath(density);

//			if (shape is IPolygon polygon)
//				return polygon.CreatePolygonPath(density);

//			if (shape is IPolyline polyline)
//				return polyline.CreatePolylinePath(density);

//			if (shape is IPath path)
//				return path.CreatePathPath();

//			if (shape is IRectangle rectangle)
//				return rectangle.CreateRectanglePath(rect);

//			return null;
//		}

//		internal static PathF? CreateEllipsePath(this IEllipse ellipse, RectangleF rect)
//		{
//			var path = new PathF();

//			path.AppendEllipse(rect);

//			return path;
//		}

//		internal static PathF? CreateLinePath(this ILine line, float density = 1)
//		{
//			var path = new PathF();

//			path.MoveTo(density * (float)line.X1, density * (float)line.Y1);
//			path.LineTo(density * (float)line.X2, density * (float)line.Y2);

//			return path;
//		}

//		internal static PathF? CreatePolygonPath(this IPolygon polygon, float density = 1)
//		{
//			var path = new PathF();

//			if (polygon.Points?.Count > 0)
//			{
//				path.MoveTo(density * (float)polygon.Points[0].X, density * (float)polygon.Points[0].Y);

//				for (int index = 1; index < polygon.Points.Count; index++)
//					path.LineTo(density * (float)polygon.Points[index].X, density * (float)polygon.Points[index].Y);

//				path.Close();
//			}

//			return path;
//		}

//		internal static PathF? CreatePolylinePath(this IPolyline polyline, float density = 1)
//		{
//			var path = new PathF();

//			if (polyline.Points?.Count > 0)
//			{
//				path.MoveTo(density * (float)polyline.Points[0].X, density * (float)polyline.Points[0].Y);

//				for (int index = 1; index < polyline.Points.Count; index++)
//					path.LineTo(density * (float)polyline.Points[index].X, density * (float)polyline.Points[index].Y);
//			}

//			return path;
//		}

//		internal static PathF? CreatePathPath(this IPath path)
//		{
//			return PathBuilder.Build(path.Data);
//		}

//		internal static PathF? CreateRectanglePath(this IRectangle rectangle, RectangleF rect)
//		{
//			var path = new PathF();

//			path.AppendRoundedRectangle(
//				rect,
//				(float)rectangle.CornerRadius.TopLeft,
//				(float)rectangle.CornerRadius.TopRight,
//				(float)rectangle.CornerRadius.BottomLeft,
//				(float)rectangle.CornerRadius.BottomRight);

//			return path;
//		}
//	}
//}