using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this AView nativeView, IBorderStroke border)
		{
			var borderShape = border.Shape;
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			if (mauiDrawable == null && borderShape == null)
				return;

			nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateStroke(this AView nativeView, IBorderStroke border)
		{
			var stroke = border.Stroke;
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			if (mauiDrawable == null && stroke.IsNullOrEmpty())
				return;

			nativeView.UpdateMauiDrawable(border);
			mauiDrawable?.SetBorderBrush(border.Stroke);
		}

		public static void UpdateStrokeThickness(this AView nativeView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderWidth(border.StrokeThickness);
		}

		public static void UpdateStrokeDashPattern(this AView nativeView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			mauiDrawable?.SetBorderDash(border.StrokeDashPattern, border.StrokeDashOffset);
		}

		public static void UpdateStrokeDashOffset(this AView nativeView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderDash(border.StrokeDashPattern, border.StrokeDashOffset);
		}

		public static void UpdateStrokeMiterLimit(this AView nativeView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderMiterLimit(border.StrokeMiterLimit);
		}

		public static void UpdateStrokeLineCap(this AView nativeView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderLineCap(border.StrokeLineCap);
		}

		public static void UpdateStrokeLineJoin(this AView nativeView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderLineJoin(border.StrokeLineJoin);
		}

		internal static void UpdateMauiDrawable(this AView nativeView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (!hasBorder)
				return;

			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			if (mauiDrawable == null)
			{
				mauiDrawable = new MauiDrawable(nativeView.Context);

				nativeView.Background = mauiDrawable;
			}

			if (border is IView v)
				mauiDrawable.SetBackground(v.Background);
			else
				mauiDrawable.SetBackground(new SolidPaint(Colors.Transparent));
			mauiDrawable.SetBorderShape(border.Shape);
		}
	}
}