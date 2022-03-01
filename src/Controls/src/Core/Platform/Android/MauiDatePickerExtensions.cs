using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class MauiDatePickerExtensions
	{
		public static void UpdateText(this MauiDatePicker platformDatePicker, IDatePicker datePicker, TextTransform tranform)
		{
			platformDatePicker.Text = TextTransformUtilites.GetTransformedText(datePicker.Text, tranform);
		}
	}
}