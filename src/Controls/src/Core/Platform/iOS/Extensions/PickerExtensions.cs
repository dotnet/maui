using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateText(this MauiPicker picker, InputView inputView)
		{
			picker.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}
}
