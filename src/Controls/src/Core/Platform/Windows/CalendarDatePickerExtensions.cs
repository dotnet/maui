using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	public static class CalendarDatePickerExtensions
	{
		public static void UpdateText(this CalendarDatePicker platformDatePicker, IDatePicker datePicker, TextTransform tranform)
		{
			var dateText = platformDatePicker.GetDescendantByName<TextBlock>("DateText");

			if (dateText == null)
				return;

			dateText.Text = TextTransformUtilites.GetTransformedText(datePicker.Text, tranform);
		}
	}
}