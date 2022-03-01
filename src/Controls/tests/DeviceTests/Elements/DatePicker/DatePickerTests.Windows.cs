using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerTests
	{
		CalendarDatePicker GetPlatformControl(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		Task<string> GetPlatformText(DatePickerHandler datePickerHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(datePickerHandler).Date.ToString());
		}
	}
}