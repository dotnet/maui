using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapUpdateMode(IPickerHandler handler, Picker picker)
		{
			if (handler is PickerHandler ph)
				ph.UpdateImmediately = picker.OnThisPlatform().UpdateMode() == UpdateMode.Immediately;
		}

		public static void MapUpdateMode(PickerHandler handler, Picker picker) =>
			MapUpdateMode((IPickerHandler)handler, picker);
	}
}