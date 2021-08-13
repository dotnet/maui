using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Controls.Core.Platform.Android.Extensions
{
	public static class TextViewExtensions
	{
		public static void UpdateText(this TextView textView, Label label)
		{
			switch (label.TextType)
			{
				case TextType.Text:
					textView.UpdateTextPlainText(label);
					break;
				case TextType.Html:
					textView.UpdateTextHtml(label);
					break;
			}
		}
	}
}
