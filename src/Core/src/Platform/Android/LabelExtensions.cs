using Android.Text;
using Android.Util;
using Android.Widget;

namespace Microsoft.Maui
{
	public static class LabelExtensions
	{
		public static void UpdateText(this TextView textView, ILabel label)
		{
			textView.Text = label.Text;
		}

		public static void UpdateTextColor(this TextView textView, ILabel label, Color defaultColor)
		{
			Color textColor = label.TextColor;

			if (textColor.IsDefault)
			{
				textView.SetTextColor(defaultColor.ToNative());
			}
			else
			{
				textView.SetTextColor(textColor.ToNative());
			}
		}

		public static void UpdateCharacterSpacing(this TextView textView, ILabel label) =>
			textView.LetterSpacing = label.CharacterSpacing.ToEm();

		public static void UpdateFont(this TextView textView, ILabel label, IFontManager fontManager)
		{
			var font = label.Font;

			var tf = fontManager.GetTypeface(font);
			textView.Typeface = tf;

			var sp = fontManager.GetScaledPixel(font);
			textView.SetTextSize(ComplexUnitType.Sp, sp);
		}

		public static void UpdateLineBreakMode(this TextView textView, ILabel label)
		{
			textView.SetLineBreakMode(label);
		}

		public static void UpdatePadding(this TextView textView, ILabel label) 
		{
			var context = textView.Context;

			if (context == null)
			{
				return;
			}

			textView.SetPadding(
				(int)context.ToPixels(label.Padding.Left),
				(int)context.ToPixels(label.Padding.Top),
				(int)context.ToPixels(label.Padding.Right),
				(int)context.ToPixels(label.Padding.Bottom));
		}

		internal static void SetLineBreakMode(this TextView textView, ILabel label)
		{
			var lineBreakMode = label.LineBreakMode;

			int maxLines = label.MaxLines;
			if (maxLines <= 0)
				maxLines = int.MaxValue;

			bool singleLine = false;

			switch (lineBreakMode)
			{
				case LineBreakMode.NoWrap:
					maxLines = 1;
					textView.Ellipsize = null;
					break;
				case LineBreakMode.WordWrap:
					textView.Ellipsize = null;
					break;
				case LineBreakMode.CharacterWrap:
					textView.Ellipsize = null;
					break;
				case LineBreakMode.HeadTruncation:
					maxLines = 1;
					singleLine = true; // Workaround for bug in older Android API versions (https://bugzilla.xamarin.com/show_bug.cgi?id=49069)
					textView.Ellipsize = TextUtils.TruncateAt.Start;
					break;
				case LineBreakMode.TailTruncation:
					maxLines = 1;
					textView.Ellipsize = TextUtils.TruncateAt.End;
					break;
				case LineBreakMode.MiddleTruncation:
					maxLines = 1;
					singleLine = true; // Workaround for bug in older Android API versions (https://bugzilla.xamarin.com/show_bug.cgi?id=49069)
					textView.Ellipsize = TextUtils.TruncateAt.Middle;
					break;
			}

			textView.SetSingleLine(singleLine);
			textView.SetMaxLines(maxLines);
		}
	}
}