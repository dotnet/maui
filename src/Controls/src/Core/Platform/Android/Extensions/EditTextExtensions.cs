using Android.Widget;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class EditTextExtensions
	{
		public static void UpdateText(this EditText editText, InputView inputView)
		{
			editText.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}
}
