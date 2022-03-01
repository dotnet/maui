using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerTests
	{
		MauiDatePicker GetPlatformControl(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		Task<string> GetPlatformText(DatePickerHandler datePickerHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(datePickerHandler).Text);
		}
	}
}