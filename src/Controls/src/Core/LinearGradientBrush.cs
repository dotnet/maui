using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class LinearGradientBrush : GradientBrush
	{
		public LinearGradientBrush()
		{

		}

		public LinearGradientBrush(GradientStopCollection gradientStops)
		{
			GradientStops = gradientStops;
		}

		public LinearGradientBrush(GradientStopCollection gradientStops, Point startPoint, Point endPoint)
		{
			GradientStops = gradientStops;
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public override bool IsEmpty
		{
			get
			{
				var linearGradientBrush = this;
				return linearGradientBrush == null || linearGradientBrush.GradientStops.Count == 0;
			}
		}

		public static readonly BindableProperty StartPointProperty = BindableProperty.Create(
			nameof(StartPoint), typeof(Point), typeof(LinearGradientBrush), new Point(0, 0));

		public Point StartPoint
		{
			get => (Point)GetValue(StartPointProperty);
			set => SetValue(StartPointProperty, value);
		}

		public static readonly BindableProperty EndPointProperty = BindableProperty.Create(
			nameof(EndPoint), typeof(Point), typeof(LinearGradientBrush), new Point(1, 1));

		public Point EndPoint
		{
			get => (Point)GetValue(EndPointProperty);
			set => SetValue(EndPointProperty, value);
		}
	}
}