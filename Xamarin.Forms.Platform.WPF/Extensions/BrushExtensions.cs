using System.Linq;
using WBrush = System.Windows.Media.Brush;
using WGradientStop = System.Windows.Media.GradientStop;
using WGradientStopCollection = System.Windows.Media.GradientStopCollection;
using WLinearGradientBrush = System.Windows.Media.LinearGradientBrush;
using WPoint = System.Windows.Point;
using WRadialGradientBrush = System.Windows.Media.RadialGradientBrush;

namespace Xamarin.Forms.Platform.WPF.Extensions
{
	public static class BrushExtensions
	{
		public static WBrush ToBrush(this Brush brush)
		{
			if (brush == null)
				return null;

			if (brush is SolidColorBrush solidColorBrush)
			{
				return solidColorBrush.Color.ToBrush();
			}

			if (brush is LinearGradientBrush linearGradientBrush)
			{
				var orderedStops = linearGradientBrush.GradientStops.OrderBy(x => x.Offset).ToList();
				var gradientStopCollection = new WGradientStopCollection();

				foreach (var item in orderedStops)
					gradientStopCollection.Add(new WGradientStop { Offset = item.Offset, Color = item.Color.ToMediaColor() });

				var p1 = linearGradientBrush.StartPoint;
				var p2 = linearGradientBrush.EndPoint;

				return new WLinearGradientBrush(gradientStopCollection, 0)
				{
					StartPoint = new WPoint(p1.X, p1.Y),
					EndPoint = new WPoint(p2.X, p2.Y)
				};
			}

			if (brush is RadialGradientBrush radialGradientBrush)
			{
				var orderedStops = radialGradientBrush.GradientStops.OrderBy(x => x.Offset).ToList();
				var gradientStopCollection = new WGradientStopCollection();

				foreach (var item in orderedStops)
					gradientStopCollection.Add(new WGradientStop { Offset = item.Offset, Color = item.Color.ToMediaColor() });

				return new WRadialGradientBrush(gradientStopCollection)
				{
					Center = new WPoint(radialGradientBrush.Center.X, radialGradientBrush.Center.Y),
					RadiusX = radialGradientBrush.Radius,
					RadiusY = radialGradientBrush.Radius
				};
			}

			return null;
		}
	}
}