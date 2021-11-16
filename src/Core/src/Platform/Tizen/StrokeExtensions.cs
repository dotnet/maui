using ElmSharp;
using Microsoft.Maui.Graphics;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this NView platformView, IBorderStroke border)
		{
			var borderShape = border.Shape;
			var canvas = platformView as BorderView;
			if (canvas == null && borderShape == null)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStroke(this NView platformView, IBorderStroke border)
		{
			var stroke = border.Stroke;
			var canvas = platformView as BorderView;
			if (canvas == null && stroke.IsNullOrEmpty())
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeThickness(this NView platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashPattern(this NView platformView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashOffset(this NView platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeMiterLimit(this NView platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineCap(this NView platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;
			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineJoin(this NView platformView, IBorderStroke border)
		{
			var canvas = platformView as BorderView;
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (canvas == null && !hasBorder)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		internal static void UpdateMauiDrawable(this NView platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (!hasBorder)
				return;

			//if (nativeView is BorderView borderView)
			//{
			//	borderView.ContainerView?.UpdateBorder(border);
			//}
		}
	}
}