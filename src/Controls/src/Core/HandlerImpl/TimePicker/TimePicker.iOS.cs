using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		public static void MapUpdateMode(TimePickerHandler handler, TimePicker timePicker)
		{
			handler.UpdateImmediately = timePicker.OnThisPlatform().UpdateMode() == UpdateMode.Immediately;
		}
	}
}