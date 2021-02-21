using Android.Widget;

namespace Xamarin.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateText(this TextView textView, ILabel label)
		{
			textView.Text = label.Text;
		}

		public static void UpdateTextColor(this TextView textView, ILabel label, Forms.Color defaultColor)
		{
			Forms.Color textColor = label.TextColor;

			if (textColor.IsDefault)
			{
				textView.SetTextColor(defaultColor.ToNative());
			}
			else
			{
				textView.SetTextColor(textColor.ToNative());
			}				
		}
	}
}