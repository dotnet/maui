#nullable enable
using System.Collections.Generic;
using System.Linq;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WGradientStop = Microsoft.UI.Xaml.Media.GradientStop;
using WLinearGradientBrush = Microsoft.UI.Xaml.Media.LinearGradientBrush;
using WRadialGradientBrush = Microsoft.UI.Xaml.Media.RadialGradientBrush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Graphics
{
	public static partial class BrushExtensions
	{
		public static WBrush? CreateBrush(this ISolidColorBrush solidColorBrush)
		{
			var brush = new WSolidColorBrush
			{
				Color = solidColorBrush.Color.ToWindowsColor()
			};

			return brush;
		}

		public static WBrush? CreateBrush(this ILinearGradientBrush linearGradientBrush)
		{
			var brush = new WLinearGradientBrush
			{
				StartPoint = linearGradientBrush.StartPoint.ToNative(),
				EndPoint = linearGradientBrush.EndPoint.ToNative()
			};

			brush.GradientStops.AddRange(linearGradientBrush.GradientStops);

			return brush;
		}

		public static WBrush? CreateBrush(this IRadialGradientBrush radialGradientBrush)
		{
			var brush = new WRadialGradientBrush
			{
				Center = radialGradientBrush.Center.ToNative(),
				RadiusX = radialGradientBrush.Radius,
				RadiusY = radialGradientBrush.Radius
			};

			brush.GradientStops.AddRange(radialGradientBrush.GradientStops);

			return brush;
		}

		static void AddRange(this IList<WGradientStop> nativeStops, IEnumerable<IGradientStop> stops)
		{
			foreach (var stop in stops.OrderBy(x => x.Offset))
			{
				var nativeStop = new WGradientStop
				{
					Color = stop.Color.ToWindowsColor(),
					Offset = stop.Offset
				};
				nativeStops.Add(nativeStop);
			}
		}
	}
}