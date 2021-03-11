namespace Microsoft.Maui.Graphics
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

		public Point StartPoint { get; set; } = new Point(0, 0);

		public Point EndPoint { get; set; } = new Point(1, 1);
	}
}