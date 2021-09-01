namespace Microsoft.Maui
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this WrapperView nativeView, ILayout layout)
		{
			var shape = layout.Shape;

			if (shape == null)
				return;

			nativeView.UpdateBorderShape(shape);
		}

		public static void UpdateStroke(this WrapperView nativeView, ILayout layout)
		{
			var stroke = layout.Stroke;

			if (stroke == null)
				return;

			nativeView.UpdateStroke(stroke);
		}

		public static void UpdateStrokeThickness(this WrapperView nativeView, ILayout layout)
		{
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (!hasBorder)
				return;

			var strokeThickness = layout.StrokeThickness;
			nativeView.UpdateStrokeThickness(strokeThickness);
		}

		public static void UpdateStrokeDashPattern(this WrapperView nativeView, ILayout layout)
		{
			var strokeDashPattern = layout.StrokeDashPattern;

			if (strokeDashPattern == null)
				return;

			nativeView.UpdateStrokeDashPattern(strokeDashPattern);
		}

		public static void UpdateStrokeDashOffset(this WrapperView nativeView, ILayout layout)
		{
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (!hasBorder)
				return;

			var strokeDashOffset = layout.StrokeDashOffset;
			nativeView.UpdateBorderDashOffset(strokeDashOffset);
		}

		public static void UpdateStrokeMiterLimit(this WrapperView nativeView, ILayout layout)
		{
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (!hasBorder)
				return;

			var strokeMiterLimit = layout.StrokeMiterLimit;
			nativeView.UpdateStrokeMiterLimit(strokeMiterLimit);
		}

		public static void UpdateStrokeLineCap(this WrapperView nativeView, ILayout layout) 
		{
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineCap = layout.StrokeLineCap;
			nativeView.UpdateStrokeLineCap(strokeLineCap);
		}

		public static void UpdateStrokeLineJoin(this WrapperView nativeView, ILayout layout)
		{
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (!hasBorder)
				return;

			var strokeLineJoin = layout.StrokeLineJoin;
			nativeView.UpdateStrokeLineJoin(strokeLineJoin);
		}
	}
}
