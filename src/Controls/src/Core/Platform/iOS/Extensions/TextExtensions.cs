using Microsoft.Maui.Controls.Internals;
using UIKit;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Entry;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextExtensions
	{
		public static void UpdateCursorColor(this UITextView textView, Entry entry)
		{
			if (entry.IsSet(Specifics.CursorColorProperty))
			{
				var color = entry.OnThisPlatform().GetCursorColor();

				if (color != null)
					textView.TintColor = color.ToPlatform();
			}
		}
		
		public static void UpdateText(this UITextView textView, InputView inputView)
		{
			textView.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}

		public static void UpdateText(this UITextField textField, InputView inputView)
		{
			textField.Text = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);
		}
	}
}