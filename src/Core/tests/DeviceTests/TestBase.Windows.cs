using Microsoft.UI.Xaml;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TestBase
	{
		public const int EmCoefficientPrecision = 4;

		public Window DefaultWindow =>
			Platform.DefaultWindow;
	}
}
