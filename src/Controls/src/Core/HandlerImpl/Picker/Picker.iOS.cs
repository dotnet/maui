using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapUpdateMode(PickerHandler handler, Picker picker)
		{
			handler.UpdateImmediately = picker.OnThisPlatform().UpdateMode() == UpdateMode.Immediately;
		}
	}
}