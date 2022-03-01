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
			return InvokeOnMainThreadAsync(() =>
			{
				var calendarDatePicker = GetPlatformControl(datePickerHandler);

				var dateText = calendarDatePicker.GetDescendantByName<TextBlock>("DateText");

				if (dateText == null)
					return string.Empty;

				return dateText.Text;
			});
		}
	}
}