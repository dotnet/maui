using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this ContentPanel nativeView, IBorder border)
		{
			var shape = border.Shape;

			if (shape == null)
				return;

			nativeView.UpdateBorderShape(shape);
		}

		public static void UpdateStroke(this ContentPanel nativeView, IBorder border)
		{
			var stroke = border.Stroke;

			if (stroke == null)
				return;

			nativeView.UpdateStroke(stroke);
		}

		public static void UpdateStrokeThickness(this ContentPanel nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeThickness = border.StrokeThickness;
			nativeView.UpdateStrokeThickness(strokeThickness);
		}

		public static void UpdateStrokeDashPattern(this ContentPanel nativeView, IBorder border)
		{
			var strokeDashPattern = border.StrokeDashPattern;

			if (strokeDashPattern == null)
				return;

			nativeView.UpdateStrokeDashPattern(strokeDashPattern);
		}

		public static void UpdateStrokeDashOffset(this ContentPanel nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeDashOffset = border.StrokeDashOffset;
			nativeView.UpdateBorderDashOffset(strokeDashOffset);
		}

		public static void UpdateStrokeMiterLimit(this ContentPanel nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeMiterLimit = border.StrokeMiterLimit;
			nativeView.UpdateStrokeMiterLimit(strokeMiterLimit);
		}

		public static void UpdateStrokeLineCap(this ContentPanel nativeView, IBorder border) 
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineCap = border.StrokeLineCap;
			nativeView.UpdateStrokeLineCap(strokeLineCap);
		}

		public static void UpdateStrokeLineJoin(this ContentPanel nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineJoin = border.StrokeLineJoin;
			nativeView.UpdateStrokeLineJoin(strokeLineJoin);
		}
	}
}
