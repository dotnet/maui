using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using WDoubleCollection = Microsoft.UI.Xaml.Media.DoubleCollection;
using WPenLineCap = Microsoft.UI.Xaml.Media.PenLineCap;
using WPenLineJoin = Microsoft.UI.Xaml.Media.PenLineJoin;

namespace Microsoft.Maui.Platform
{
	public static class BorderExtensions
	{
		public static void UpdateBorderShape(this Path borderPath, IShape? borderShape, double width, double height)
		{
			borderPath.UpdatePath(borderShape, width, height);
		}

		internal static void UpdatePath(this Path borderPath, IShape? borderShape, double width, double height)
		{
			if (borderShape is null || width <= 0 || height <= 0)
				return;

			var strokeThickness = borderPath?.StrokeThickness ?? 0;
			var pathSize = new Graphics.Rect(0, 0, width - strokeThickness, height - strokeThickness);
			var shapePath = borderShape.PathForBounds(pathSize);
			var geometry = shapePath.AsPathGeometry();

			if (borderPath is not null)
			{
				borderPath.Data = geometry;
				borderPath.RenderTransform = new TranslateTransform() { X = strokeThickness / 2, Y = strokeThickness / 2 };
			}
		}

		public static void UpdateBackground(this Path borderPath, Paint? background)
		{
			if (borderPath == null)
				return;

			borderPath.Fill = background?.ToPlatform();
		}

		public static void UpdateStroke(this Path borderPath, Paint? borderBrush)
		{
			if (borderPath == null)
				return;

			borderPath.Stroke = borderBrush?.ToPlatform();
		}

		public static void UpdateStrokeThickness(this Path borderPath, double borderWidth)
		{
			if (borderPath == null)
				return;

			borderPath.StrokeThickness = borderWidth;
		}

		public static void UpdateStrokeDashPattern(this Path borderPath, float[]? borderDashArray)
		{
			if (borderPath == null)
				return;

			borderPath.StrokeDashArray?.Clear();

			if (borderDashArray != null && borderDashArray.Length > 0)
			{
				if (borderPath.StrokeDashArray == null)
					borderPath.StrokeDashArray = new WDoubleCollection();

				double[] array = new double[borderDashArray.Length];
				borderDashArray.CopyTo(array, 0);

				foreach (double value in array)
				{
					borderPath.StrokeDashArray.Add(value);
				}
			}
			else if (borderDashArray is null)
			{
				borderPath.StrokeDashArray = null;
			}
		}

		public static void UpdateBorderDashOffset(this Path borderPath, double borderDashOffset)
		{
			if (borderPath == null)
				return;

			borderPath.StrokeDashOffset = borderDashOffset;
		}

		public static void UpdateStrokeMiterLimit(this Path borderPath, double strokeMiterLimit)
		{
			if (borderPath == null)
				return;

			borderPath.StrokeMiterLimit = strokeMiterLimit;
		}

		public static void UpdateStrokeLineCap(this Path borderPath, LineCap strokeLineCap)
		{
			if (borderPath == null)
				return;

			WPenLineCap wLineCap = WPenLineCap.Flat;

			switch (strokeLineCap)
			{
				case LineCap.Butt:
					wLineCap = WPenLineCap.Flat;
					break;
				case LineCap.Square:
					wLineCap = WPenLineCap.Square;
					break;
				case LineCap.Round:
					wLineCap = WPenLineCap.Round;
					break;
			}

			borderPath.StrokeStartLineCap = borderPath.StrokeEndLineCap = wLineCap;
		}

		public static void UpdateStrokeLineJoin(this Path borderPath, LineJoin strokeLineJoin)
		{
			if (borderPath == null)
				return;

			WPenLineJoin wLineJoin = WPenLineJoin.Miter;

			switch (strokeLineJoin)
			{
				case LineJoin.Miter:
					wLineJoin = WPenLineJoin.Miter;
					break;
				case LineJoin.Bevel:
					wLineJoin = WPenLineJoin.Bevel;
					break;
				case LineJoin.Round:
					wLineJoin = WPenLineJoin.Round;
					break;
			}

			borderPath.StrokeLineJoin = wLineJoin;
		}
	}
}