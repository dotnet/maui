namespace Microsoft.Maui.Graphics
{
	public abstract class GradientBrush : Brush
	{
		public GradientBrush()
		{
			GradientStops = new GradientStopCollection();
		}

		public GradientStopCollection GradientStops { get; set; }
	}
}