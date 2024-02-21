#nullable disable
using System;
using Android.Text;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class TextViewExtensions
	{
		public static void UpdateText(this TextView textView, Label label)
		{
			switch (label.TextType)
			{
				case TextType.Text:
					if (label.FormattedText != null)
						textView.TextFormatted = label.ToSpannableString();
					else
						textView.Text = TextTransformUtilites.GetTransformedText(label.Text, label.TextTransform);
					break;
				case TextType.Html:
					textView.UpdateTextHtml(label);
					break;
			}
		}

		public static void UpdateLineBreakMode(this TextView textView, Label label)
		{
			textView.SetLineBreakMode(label.LineBreakMode, label.MaxLines);
		}

		public static void UpdateMaxLines(this TextView textView, Label label)
		{
			// Linebreak mode also handles setting MaxLines
			textView.SetLineBreakMode(label.LineBreakMode, label.MaxLines);
		}

		internal static void SetLineBreakMode(this TextView textView, LineBreakMode lineBreakMode, int? maxLines = null)
		{
			if (!maxLines.HasValue || maxLines <= 0)
			{
				// Without setting the MaxLines property, to equalize behaviors across platforms
				// we set the max to 1.
				if (lineBreakMode == LineBreakMode.TailTruncation)
					maxLines = 1;
				else
					maxLines = int.MaxValue;
			}

			bool singleLine = false;
			bool shouldSetSingleLine = !OperatingSystem.IsAndroidVersionAtLeast(23);

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
					maxLines = 1; // If maxLines is anything greater than 1, the truncation will be ignored: https://developer.android.com/reference/android/widget/TextView#setEllipsize(android.text.TextUtils.TruncateAt)

					if (shouldSetSingleLine)
					{
						singleLine = true; // Workaround for bug in older Android API versions (https://issuetracker.google.com/issues/36950033) (https://bugzilla.xamarin.com/show_bug.cgi?id=49069)
					}

					textView.Ellipsize = TextUtils.TruncateAt.Start;
					break;
				case LineBreakMode.TailTruncation:

					// We don't have a mechanism for setting MaxLines on other controls (e.g., Button) right now, so we need to force it here or
					// they will potentially exceed a single line. 
					if (textView is AppCompatButton)
						maxLines = 1;

					textView.Ellipsize = TextUtils.TruncateAt.End;
					break;
				case LineBreakMode.MiddleTruncation:
					maxLines = 1; // If maxLines is anything greater than 1, the truncation will be ignored: https://developer.android.com/reference/android/widget/TextView#setEllipsize(android.text.TextUtils.TruncateAt)
					textView.Ellipsize = TextUtils.TruncateAt.Middle;
					break;
			}

			if (shouldSetSingleLine) // Save ourselves this trip across the bridge if we're on an API level that doesn't need it
			{
				textView.SetSingleLine(singleLine);
			}

			textView.SetMaxLines(maxLines.Value);
		}
	}
}
