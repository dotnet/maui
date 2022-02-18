using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this ContentPanel platformView, IBorderStroke border)
		{
			var shape = border.Shape;

			if (shape == null)
				return;

			platformView.UpdateBorderShape(shape);
		}

		public static void UpdateStroke(this ContentPanel platformView, IBorderStroke border)
		{
			var stroke = border.Stroke;

			if (stroke == null)
				return;

			platformView.UpdateStroke(stroke);
		}

		public static void UpdateStrokeThickness(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeThickness = border.StrokeThickness;
			platformView.UpdateStrokeThickness(strokeThickness);
		}

		public static void UpdateStrokeDashPattern(this ContentPanel platformView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;

			if (strokeDashPattern == null)
				return;

			platformView.UpdateStrokeDashPattern(strokeDashPattern);
		}

		public static void UpdateStrokeDashOffset(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeDashOffset = border.StrokeDashOffset;
			platformView.UpdateBorderDashOffset(strokeDashOffset);
		}

		public static void UpdateStrokeMiterLimit(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeMiterLimit = border.StrokeMiterLimit;
			platformView.UpdateStrokeMiterLimit(strokeMiterLimit);
		}

		public static void UpdateStrokeLineCap(this ContentPanel platformView, IBorderStroke border) 
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineCap = border.StrokeLineCap;
			platformView.UpdateStrokeLineCap(strokeLineCap);
		}

		public static void UpdateStrokeLineJoin(this ContentPanel platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineJoin = border.StrokeLineJoin;
			platformView.UpdateStrokeLineJoin(strokeLineJoin);
		}
	}
}
