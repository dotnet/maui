using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class MauiTimePickerExtensions
	{
		public static void UpdateText(this MauiTimePicker platformTimePicker, ITimePicker timePicker, TextTransform tranform)
		{
			platformTimePicker.Text = TextTransformUtilites.GetTransformedText(timePicker.Text, tranform);
		}
	}
}