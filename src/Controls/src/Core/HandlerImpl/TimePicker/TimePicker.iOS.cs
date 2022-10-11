using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		public static void MapUpdateMode(TimePickerHandler handler, TimePicker timePicker) =>
			MapUpdateMode((ITimePickerHandler)handler, timePicker);

		public static void MapUpdateMode(ITimePickerHandler handler, TimePicker timePicker)
		{
			if (handler is TimePickerHandler h)
				h.UpdateImmediately = timePicker.OnThisPlatform().UpdateMode() == UpdateMode.Immediately;
		}
	}
}