using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Brush : IBrush
	{
		public static explicit operator Brush(Graphics.Brush brush)
		{
			if (brush is Graphics.SolidColorBrush solidColorBrush)
			{
				return new SolidColorBrush(solidColorBrush.Color);
			}

			if (brush is Graphics.GradientBrush gradientBrush)
			{
				if (gradientBrush is Graphics.LinearGradientBrush graphicsLinearGradientBrush)
				{
					var linearGradientBrush = new LinearGradientBrush
					{
						StartPoint = graphicsLinearGradientBrush.StartPoint,
						EndPoint = graphicsLinearGradientBrush.EndPoint
					};

					foreach (var gradientStop in graphicsLinearGradientBrush.GradientStops)
						linearGradientBrush.GradientStops.Add(new GradientStop { Color = gradientStop.Color, Offset = gradientStop.Offset });

					return linearGradientBrush;
				}

				if (gradientBrush is Graphics.RadialGradientBrush graphicsRadialGradientBrush)
				{
					var radialGradientBrush = new RadialGradientBrush
					{
						Center = graphicsRadialGradientBrush.Center,
						Radius = graphicsRadialGradientBrush.Radius
					};

					foreach (var gradientStop in graphicsRadialGradientBrush.GradientStops)
						radialGradientBrush.GradientStops.Add(new GradientStop { Color = gradientStop.Color, Offset = gradientStop.Offset });

					return radialGradientBrush;
				}
			}

			return null;
		}
	}
}