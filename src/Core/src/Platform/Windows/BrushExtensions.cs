namespace Microsoft.Maui.Platform
{
	public static class BrushExtensions
	{
		public static UI.Xaml.Media.Brush? ChangeBrushBrightness(this UI.Xaml.Media.Brush brush, double correctionFactor)
		{
			// The brightness correction factor. Must be between -1 and 1. 
			// Negative values produce darker colors. Positive values produce lighter colors.
			if (brush is UI.Xaml.Media.SolidColorBrush solidColorBrush)
			{
				UI.Xaml.Media.SolidColorBrush newSolidColorBrush = new UI.Xaml.Media.SolidColorBrush
				{
					Color = solidColorBrush.Color.ChangeColorBrightness(correctionFactor)
				};

				return newSolidColorBrush;
			}

			if (brush is UI.Xaml.Media.LinearGradientBrush linearGradientBrush)
			{
				UI.Xaml.Media.LinearGradientBrush newLinearGradientBrush = new UI.Xaml.Media.LinearGradientBrush
				{
					StartPoint = linearGradientBrush.StartPoint,
					EndPoint = linearGradientBrush.EndPoint
				};

				foreach (var gradientStop in linearGradientBrush.GradientStops)
				{
					newLinearGradientBrush.GradientStops.Add(new UI.Xaml.Media.GradientStop
					{
						Color = gradientStop.Color.ChangeColorBrightness(correctionFactor),
						Offset = gradientStop.Offset
					});
				}

				return newLinearGradientBrush;
			}

			if (brush is UI.Xaml.Media.RadialGradientBrush radialGradientBrush)
			{
				UI.Xaml.Media.RadialGradientBrush newRadialGradientBrush = new UI.Xaml.Media.RadialGradientBrush
				{
					Center = radialGradientBrush.Center,
					RadiusX = radialGradientBrush.RadiusX,
					RadiusY = radialGradientBrush.RadiusY
				};

				foreach (var gradientStop in radialGradientBrush.GradientStops)
				{
					newRadialGradientBrush.GradientStops.Add(new UI.Xaml.Media.GradientStop
					{
						Color = gradientStop.Color.ChangeColorBrightness(correctionFactor),
						Offset = gradientStop.Offset
					});
				}

				return newRadialGradientBrush;
			}

			return null;
		}

		public static UI.Xaml.Media.Brush? Darker(this UI.Xaml.Media.Brush brush)
		{
			return brush.ChangeBrushBrightness(-0.15);
		}

		public static UI.Xaml.Media.Brush? Lighter(this UI.Xaml.Media.Brush brush)
		{
			return brush.ChangeBrushBrightness(0.15);
		}
	}
}
