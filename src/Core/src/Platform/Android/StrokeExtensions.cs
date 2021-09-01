using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this AView nativeView, ILayout layout)
		{
			var borderShape = layout.Shape;
			MauiDrawable? background = nativeView.Background as MauiDrawable;

			if (background == null && borderShape == null)
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		public static void UpdateStroke(this AView nativeView, ILayout layout)
		{
			var stroke = layout.Stroke;
			MauiDrawable? background = nativeView.Background as MauiDrawable;

			if (background == null && stroke.IsNullOrEmpty())
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		public static void UpdateStrokeThickness(this AView nativeView, ILayout layout)
		{
			MauiDrawable? background = nativeView.Background as MauiDrawable;
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (background == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		public static void UpdateStrokeDashPattern(this AView nativeView, ILayout layout)
		{
			var strokeDashPattern = layout.StrokeDashPattern;
			MauiDrawable? background = nativeView.Background as MauiDrawable;

			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (background == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		public static void UpdateStrokeDashOffset(this AView nativeView, ILayout layout)
		{
			MauiDrawable? background = nativeView.Background as MauiDrawable;

			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (background == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		public static void UpdateStrokeMiterLimit(this AView nativeView, ILayout layout)
		{
			MauiDrawable? background = nativeView.Background as MauiDrawable;

			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (background == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		public static void UpdateStrokeLineCap(this AView nativeView, ILayout layout)
		{
			MauiDrawable? background = nativeView.Background as MauiDrawable;
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (background == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		public static void UpdateStrokeLineJoin(this AView nativeView, ILayout layout)
		{
			MauiDrawable? background = nativeView.Background as MauiDrawable;
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (background == null && !hasBorder)
				return;

			nativeView.UpdateMauiDrawable(layout);
		}

		internal static void UpdateMauiDrawable(this AView nativeView, ILayout layout)
		{
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (!hasBorder)
				return;

			MauiDrawable? mauiDrawable = nativeView.Background as MauiDrawable;

			if (mauiDrawable == null)
			{
				mauiDrawable = new MauiDrawable(nativeView.Context);

				nativeView.Background = mauiDrawable;
			}

			mauiDrawable.SetBackground(layout.Background);
			mauiDrawable.SetBorderBrush(layout.Stroke);
			mauiDrawable.SetBorderWidth(layout.StrokeThickness);
			mauiDrawable.SetBorderDash(layout.StrokeDashPattern, layout.StrokeDashOffset);
			mauiDrawable.SetBorderMiterLimit(layout.StrokeMiterLimit);
			mauiDrawable.SetBorderLineJoin(layout.StrokeLineJoin);
			mauiDrawable.SetBorderLineCap(layout.StrokeLineCap);
			mauiDrawable.SetBorderShape(layout.Shape);
		}
	}
}