namespace Microsoft.Maui.Graphics
{
	public interface ILinearGradientBrush : IGradientBrush
	{
		Point StartPoint { get; set; }

		Point EndPoint { get; set; }
	}
}