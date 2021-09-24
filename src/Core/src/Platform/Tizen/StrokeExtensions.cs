using ElmSharp;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	// TODO : Need to impl
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this EvasObject nativeView, IBorder border)
		{
			var borderShape = border.Shape;
			//MauiDrawable? background = nativeView.Background as MauiDrawable;

			//if (background == null && borderShape == null)
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStroke(this EvasObject nativeView, IBorder border)
		{
			var stroke = border.Stroke;
			//MauiDrawable? background = nativeView.Background as MauiDrawable;

			//if (background == null && stroke.IsNullOrEmpty())
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeThickness(this EvasObject nativeView, IBorder border)
		{
			//MauiDrawable? background = nativeView.Background as MauiDrawable;
			//bool hasBorder = border.Shape != null && border.Stroke != null;

			//if (background == null && !hasBorder)
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashPattern(this EvasObject nativeView, IBorder border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			//MauiDrawable? background = nativeView.Background as MauiDrawable;

			//bool hasBorder = border.Shape != null && border.Stroke != null;

			//if (background == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeDashOffset(this EvasObject nativeView, IBorder border)
		{
			//MauiDrawable? background = nativeView.Background as MauiDrawable;

			//bool hasBorder = border.Shape != null && border.Stroke != null;

			//if (background == null && !hasBorder)
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeMiterLimit(this EvasObject nativeView, IBorder border)
		{
			//MauiDrawable? background = nativeView.Background as MauiDrawable;

			//bool hasBorder = border.Shape != null && border.Stroke != null;

			//if (background == null && !hasBorder)
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineCap(this EvasObject nativeView, IBorder border)
		{
			//MauiDrawable? background = nativeView.Background as MauiDrawable;
			//bool hasBorder = border.Shape != null && border.Stroke != null;

			//if (background == null && !hasBorder)
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStrokeLineJoin(this EvasObject nativeView, IBorder border)
		{
			//MauiDrawable? background = nativeView.Background as MauiDrawable;
			//bool hasBorder = border.Shape != null && border.Stroke != null;

			//if (background == null && !hasBorder)
			//	return;

			nativeView.UpdateMauiDrawable(border);
		}

		internal static void UpdateMauiDrawable(this EvasObject nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			//MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			//if (mauiDrawable == null)
			//{
			//	mauiDrawable = new MauiDrawable(nativeView.Context);

			//	nativeView.Background = mauiDrawable;
			//}

			//mauiDrawable.SetBackground(border.Background);
			//mauiDrawable.SetBorderBrush(border.Stroke);
			//mauiDrawable.SetBorderWidth(border.StrokeThickness);
			//mauiDrawable.SetBorderDash(border.StrokeDashPattern, border.StrokeDashOffset);
			//mauiDrawable.SetBorderMiterLimit(border.StrokeMiterLimit);
			//mauiDrawable.SetBorderLineJoin(border.StrokeLineJoin);
			//mauiDrawable.SetBorderLineCap(border.StrokeLineCap);
			//mauiDrawable.SetBorderShape(border.Shape);
		}
	}
}