using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerTests
	{
		MauiTimePicker GetPlatformControl(TimePickerHandler timePickerHandler) =>		 
			timePickerHandler.PlatformView;

		Task<string> GetPlatformText(TimePickerHandler timePickerHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(timePickerHandler).Text);
		}
	}
}