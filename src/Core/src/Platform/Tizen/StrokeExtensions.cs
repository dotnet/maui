using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		internal static void UpdateStrokeShape(this NView platformView, IBorderStroke border, IMauiContext context)
		{
			var wrapperView = platformView.GetParent() as WrapperView;

			if (wrapperView == null)
				return;

			IShape? borderShape = border.Shape;

			if (borderShape is IView shapeView && shapeView.Handler is null)
				shapeView.Handler = shapeView.ToHandler(context);

			platformView.UpdateStrokeShape(border);
		}
	
		public static void UpdateStroke(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeThickness(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashPattern(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashOffset(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeMiterLimit(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineCap(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineJoin(this NView platformView, IBorderStroke border)
		{
			var wrapperView = platformView.GetParent() as WrapperView;
			if (wrapperView == null)
				return;

			wrapperView.UpdateMauiDrawable(border);
		}

		internal static void UpdateMauiDrawable(this WrapperView wrapperView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;
			if (!hasBorder)
				return;

			wrapperView.UpdateBorder(border);
		}
	}
}
