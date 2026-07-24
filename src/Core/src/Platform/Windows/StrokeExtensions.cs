namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this ContentPanel platformView, IBorderStroke border)
		{
			if (border is null)
				return;

			platformView.UpdateBorderStroke(border);
		}

		public static void UpdateStroke(this ContentPanel platformView, IBorderStroke border)
		{
			var stroke = border.Stroke;

			if (stroke == null)
				return;

			platformView.BorderPath?.UpdateStroke(stroke);
		}

		public static void UpdateStrokeThickness(this ContentPanel platformView, IBorderStroke border)
		{
			var strokeThickness = border.StrokeThickness;
			platformView.BorderPath?.UpdateStrokeThickness(strokeThickness);
		}

		public static void UpdateStrokeDashPattern(this ContentPanel platformView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;

			platformView.BorderPath?.UpdateStrokeDashPattern(strokeDashPattern);
		}

		public static void UpdateStrokeDashOffset(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeDashOffset = border.StrokeDashOffset;
			platformView.BorderPath?.UpdateBorderDashOffset(strokeDashOffset);
		}

		public static void UpdateStrokeMiterLimit(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeMiterLimit = border.StrokeMiterLimit;
			platformView.BorderPath?.UpdateStrokeMiterLimit(strokeMiterLimit);
		}

		public static void UpdateStrokeLineCap(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineCap = border.StrokeLineCap;
			platformView.BorderPath?.UpdateStrokeLineCap(strokeLineCap);
		}

		public static void UpdateStrokeLineJoin(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineJoin = border.StrokeLineJoin;
			platformView.BorderPath?.UpdateStrokeLineJoin(strokeLineJoin);
		}
	}
}
