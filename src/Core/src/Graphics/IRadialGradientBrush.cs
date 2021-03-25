namespace Microsoft.Maui.Graphics
{
	public interface IRadialGradientBrush : IGradientBrush
	{
		Point Center { get; set; }

		double Radius { get; set; }
	}
}