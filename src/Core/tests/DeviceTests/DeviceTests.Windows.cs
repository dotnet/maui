using Microsoft.UI.Xaml;

namespace Microsoft.Maui.DeviceTests
{
	public partial class Platform
	{
		public static Window DefaultWindow { get; private set; }

		public static void Init(Window context)
		{
			DefaultWindow = context;
		}
	}
}