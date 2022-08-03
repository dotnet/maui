using ElmSharp;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this EvasObject platformView, IBorderStroke border)
		{
			var borderShape = border.Shape;
			var canvas = platformView as BorderView;
			if (canvas == null && borderShape == null)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStroke(this EvasObject platformView, IBorderStroke border)
		{
			var stroke = border.Stroke;
			var canvas = platformView as BorderView;
			if (canvas == null && stroke.IsNullOrEmpty())
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeThickness(this EvasObject platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashPattern(this EvasObject platformView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashOffset(this EvasObject platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeMiterLimit(this EvasObject platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineCap(this EvasObject platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;
			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineJoin(this EvasObject platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		internal static void UpdateMauiDrawable(this EvasObject platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (!hasBorder)
				return;

			if (platformView is BorderView borderView)
			{
				borderView.ContainerView?.UpdateBorder(border);
			}
		}
	}
}