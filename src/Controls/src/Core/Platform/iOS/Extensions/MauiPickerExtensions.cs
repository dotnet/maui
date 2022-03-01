using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class MauiPickerExtensions
	{
		public static void UpdateText(this MauiPicker picker, IPicker inputView, TextTransform tranform)
		{
			picker.Text = TextTransformUtilites.GetTransformedText(inputView.Text, tranform);
		}
	}
}
