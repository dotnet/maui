using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerTests
	{
		UI.Xaml.Controls.TimePicker GetPlatformControl(TimePickerHandler timePickerHandler) =>
			timePickerHandler.PlatformView;

		Task<string> GetPlatformText(TimePickerHandler timePickerHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(timePickerHandler).Time.ToString());
		}
	}
}