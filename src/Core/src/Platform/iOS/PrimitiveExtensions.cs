using UIKit;

namespace Microsoft.Maui
{
	public static class PrimitiveExtensions
	{
		public static UIEdgeInsets ToNative(this Thickness thickness, UIEdgeInsets? defaultEdgeInsets = null)
		{
			return new UIEdgeInsets(
				(float)Get(thickness.Left, defaultEdgeInsets?.Left ?? 0.0),
				(float)Get(thickness.Top, defaultEdgeInsets?.Top ?? 0.0),
				(float)Get(thickness.Right, defaultEdgeInsets?.Right ?? 0.0),
				(float)Get(thickness.Bottom, defaultEdgeInsets?.Bottom ?? 0.0));

			static double Get(double side, double def) => double.IsNaN(side) ? def : side;
		}
	}
}