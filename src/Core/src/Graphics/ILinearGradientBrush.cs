namespace Microsoft.Maui.Graphics
{
	public interface ILinearGradientBrush : IGradientBrush
	{
		Point StartPoint { get; }

		Point EndPoint { get; }
	}
}