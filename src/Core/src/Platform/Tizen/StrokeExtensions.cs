using ElmSharp;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this EvasObject nativeView, IBorder border)
		{
			var borderShape = border.Shape;
			var canvas = nativeView as IWrapperViewCanvas;
			if (canvas == null && borderShape == null)
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStroke(this EvasObject nativeView, IBorder border)
		{
			var stroke = border.Stroke;
			var canvas = nativeView as IWrapperViewCanvas;
			if (canvas == null && stroke.IsNullOrEmpty())
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeThickness(this EvasObject nativeView, IBorder border)
		{
			var canvas = nativeView as IWrapperViewCanvas;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashPattern(this EvasObject nativeView, IBorder border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			var canvas = nativeView as IWrapperViewCanvas;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashOffset(this EvasObject nativeView, IBorder border)
		{
			var canvas = nativeView as IWrapperViewCanvas;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeMiterLimit(this EvasObject nativeView, IBorder border)
		{
			var canvas = nativeView as IWrapperViewCanvas;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineCap(this EvasObject nativeView, IBorder border)
		{
			var canvas = nativeView as IWrapperViewCanvas;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineJoin(this EvasObject nativeView, IBorder border)
		{
			var canvas = nativeView as IWrapperViewCanvas;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		internal static void UpdateMauiDrawable(this EvasObject nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (!hasBorder)
				return;

			if (nativeView is IWrapperViewCanvas canvas)
			{
				canvas.Drawables.BorderDrawable = border.Background?.ToDrawable(border) ?? null;
			}
		}
	}
}