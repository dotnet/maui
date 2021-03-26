namespace Microsoft.Maui.Graphics
{
	public interface IRadialGradientBrush : IGradientBrush
	{
		Point Center { get; }

		double Radius { get; }
	}
}