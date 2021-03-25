namespace Microsoft.Maui.Graphics
{
	public interface IGradientBrush : IBrush
	{
		IGradientStopCollection GradientStops { get; set; }
	}
}