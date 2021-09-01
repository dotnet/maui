using System.Linq;
using CoreAnimation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class StrokeExtensions
	{
		public static void UpdateStrokeShape(this UIView nativeView, ILayout view)
		{
			var borderShape = view.Shape;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderShape == null)
				return;

			nativeView.UpdateMauiCALayer(view);
		}

		public static void UpdateStroke(this UIView nativeView, ILayout view)
		{
			var borderBrush = view.Stroke;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null && borderBrush.IsNullOrEmpty())
				return;

			nativeView.UpdateMauiCALayer(view);
		}

		public static void UpdateStrokeThickness(this UIView nativeView, ILayout layout)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(layout);
		}

		public static void UpdateStrokeDashPattern(this UIView nativeView, ILayout layout)
		{
			var strokeDashPattern = layout.StrokeDashPattern;
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (backgroundLayer == null && !hasBorder && (strokeDashPattern == null || strokeDashPattern.Length == 0))
				return;

			nativeView.UpdateMauiCALayer(layout);
		}

		public static void UpdateStrokeMiterLimit(this UIView nativeView, ILayout layout)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(layout);
		}

		public static void UpdateStrokeLineCap(this UIView nativeView, ILayout layout)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(layout);
		}

		public static void UpdateStrokeLineJoin(this UIView nativeView, ILayout layout)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;
			bool hasBorder = layout.Shape != null && layout.Stroke != null;

			if (backgroundLayer == null && !hasBorder)
				return;

			nativeView.UpdateMauiCALayer(layout);
		}

		internal static void UpdateMauiCALayer(this UIView nativeView, ILayout layout)
		{
			CALayer? backgroundLayer = nativeView.Layer as MauiCALayer;

			if (backgroundLayer == null)
			{
				backgroundLayer = nativeView.Layer?.Sublayers?
					.FirstOrDefault(x => x is MauiCALayer);

				if (backgroundLayer == null)
				{
					backgroundLayer = new MauiCALayer
					{
						Name = ViewExtensions.BackgroundLayerName
					};

					nativeView.BackgroundColor = UIColor.Clear;
					nativeView.InsertBackgroundLayer(backgroundLayer, 0);
				}
			}

			if (backgroundLayer is MauiCALayer mauiCALayer)
			{
				mauiCALayer.SetBackground(layout.Background);
				mauiCALayer.SetBorderBrush(layout.Stroke);
				mauiCALayer.SetBorderWidth(layout.StrokeThickness);
				mauiCALayer.SetBorderDash(layout.StrokeDashPattern, layout.StrokeDashOffset);
				mauiCALayer.SetBorderMiterLimit(layout.StrokeMiterLimit);
				mauiCALayer.SetBorderLineJoin(layout.StrokeLineJoin);
				mauiCALayer.SetBorderLineCap(layout.StrokeLineCap);
				mauiCALayer.SetBorderShape(layout.Shape);
			}
		}
	}
}