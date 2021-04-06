namespace Microsoft.Maui
{
	public static class PrimitiveExtensions
	{
		public static UI.Xaml.Thickness ToNative(this Thickness thickness, UI.Xaml.Thickness? defaultThickness = null)
		{
			return new UI.Xaml.Thickness(
				Get(thickness.Left, defaultThickness?.Left ?? 0.0),
				Get(thickness.Top, defaultThickness?.Top ?? 0.0),
				Get(thickness.Right, defaultThickness?.Right ?? 0.0),
				Get(thickness.Bottom, defaultThickness?.Bottom ?? 0.0));

			static double Get(double side, double def) => double.IsNaN(side) ? def : side;
		}
	}
}