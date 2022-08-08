using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		public static void UpdateBorderStroke(this AView platformView, IBorderStroke border)
		{
			// Always set the drawable first
			platformView.UpdateMauiDrawable(border);

			var borderShape = border.Shape;
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;

			if (mauiDrawable == null && borderShape == null)
				return;

			mauiDrawable?.SetBorderBrush(border.Stroke);
			mauiDrawable?.SetBorderWidth(border.StrokeThickness);
			platformView.UpdateStrokeDashPattern(border);
			platformView.UpdateStrokeDashOffset(border);
			mauiDrawable?.SetBorderMiterLimit(border.StrokeMiterLimit);
			mauiDrawable?.SetBorderLineCap(border.StrokeLineCap);
			mauiDrawable?.SetBorderLineJoin(border.StrokeLineJoin);

		}

		public static void UpdateStrokeShape(this AView platformView, IBorderStroke border)
		{
			var borderShape = border.Shape;
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;

			if (mauiDrawable == null && borderShape == null)
				return;

			platformView.UpdateMauiDrawable(border);
		}

		public static void UpdateStroke(this AView platformView, IBorderStroke border)
		{
			var stroke = border.Stroke;
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;

			if (mauiDrawable == null && stroke.IsNullOrEmpty())
				return;

			platformView.UpdateMauiDrawable(border);
			mauiDrawable?.SetBorderBrush(border.Stroke);
		}

		public static void UpdateStrokeThickness(this AView platformView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderWidth(border.StrokeThickness);
		}

		public static void UpdateStrokeDashPattern(this AView platformView, IBorderStroke border)
		{
			var strokeDashPattern = border.StrokeDashPattern;
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			mauiDrawable?.SetBorderDash(border.StrokeDashPattern, border.StrokeDashOffset);
		}

		public static void UpdateStrokeDashOffset(this AView platformView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderDash(border.StrokeDashPattern, border.StrokeDashOffset);
		}

		public static void UpdateStrokeMiterLimit(this AView platformView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;

			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderMiterLimit(border.StrokeMiterLimit);
		}

		public static void UpdateStrokeLineCap(this AView platformView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderLineCap(border.StrokeLineCap);
		}

		public static void UpdateStrokeLineJoin(this AView platformView, IBorderStroke border)
		{
			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (mauiDrawable == null && !hasBorder)
				return;

			mauiDrawable?.SetBorderLineJoin(border.StrokeLineJoin);
		}

		public static void InvalidateBorderStrokeBounds(this AView platformView)
		{
			if (platformView.Background is not MauiDrawable mauiDrawable)
				return;

			mauiDrawable.InvalidateBorderBounds();
		}

		internal static void UpdateMauiDrawable(this AView platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null;

			if (!hasBorder)
				return;

			MauiDrawable? mauiDrawable = platformView.Background as MauiDrawable;

			if (mauiDrawable == null)
			{
				mauiDrawable = new MauiDrawable(platformView.Context);

				platformView.Background = mauiDrawable;
			}

			if (border is IView v)
				mauiDrawable.SetBackground(v.Background);
			else
				mauiDrawable.SetBackground(new SolidPaint(Colors.Transparent));

			mauiDrawable.SetBorderShape(border.Shape);

			if (platformView is ContentViewGroup contentViewGroup)
				contentViewGroup.Clip = border;
		}
	}
}
