using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Widget;
using static Android.Widget.TextView;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;

namespace Microsoft.Maui.Platform
{
	public static class TextViewExtensions
	{
		public static void UpdateTextPlainText(this TextView textView, IText label)
		{
			textView.Text = label.Text;
		}

		public static void UpdateTextHtml(this TextView textView, ILabel label)
		{
			var newText = label.Text ?? string.Empty;

			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				textView.SetText(Html.FromHtml(newText, FromHtmlOptions.ModeCompact), BufferType.Spannable);
			else
#pragma warning disable CS0618 // Type or member is obsolete
				textView.SetText(Html.FromHtml(newText), BufferType.Spannable);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public static void UpdateTextColor(this TextView textView, ITextStyle textStyle)
		{
			var textColor = textStyle.TextColor;

			if (textColor != null)
				textView.SetTextColor(textColor.ToPlatform());
		}

		public static void UpdateFont(this TextView textView, ITextStyle textStyle, IFontManager fontManager)
		{
			var font = textStyle.Font;

			var tf = fontManager.GetTypeface(font);
			textView.Typeface = tf;

			var fontSize = fontManager.GetFontSize(font);
			textView.SetTextSize(fontSize.Unit, fontSize.Value);
		}

		public static void UpdateCharacterSpacing(this TextView textView, ITextStyle textStyle) =>
			textView.LetterSpacing = textStyle.CharacterSpacing.ToEm();

		public static void UpdateHorizontalTextAlignment(this TextView textView, ITextAlignment text)
		{
			if (Rtl.IsSupported)
			{
				// We want to use TextAlignment where possible because it doesn't conflict with the
				// overall gravity of the underlying control
				textView.TextAlignment = text.HorizontalTextAlignment.ToTextAlignment();
			}
			else
			{
				// But if RTL support is not available for some reason, we have to resort
				// to gravity, because Android will simply ignore text alignment
				textView.Gravity = Android.Views.GravityFlags.Top | text.HorizontalTextAlignment.ToHorizontalGravityFlags();
			}
		}

		public static void UpdateVerticalTextAlignment(this TextView textView, ITextAlignment textAlignment)
		{
			textView.UpdateVerticalAlignment(textAlignment.VerticalTextAlignment);
		}

		public static void UpdateLineBreakMode(this TextView textView, ILabel label)
		{
			textView.SetLineBreakMode(label, label.MaxLines);
		}

		public static void UpdateMaxLines(this TextView textView, ILabel label)
		{
			// Linebreak mode also handles settng MaxLines
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

		public static void UpdateTextDecorations(this TextView textView, ILabel label)
		{
			var textDecorations = label.TextDecorations;

			if ((textDecorations & TextDecorations.Strikethrough) == 0)
				textView.PaintFlags &= ~PaintFlags.StrikeThruText;
			else
				textView.PaintFlags |= PaintFlags.StrikeThruText;

			if ((textDecorations & TextDecorations.Underline) == 0)
				textView.PaintFlags &= ~PaintFlags.UnderlineText;
			else
				textView.PaintFlags |= PaintFlags.UnderlineText;
		}

		public static void UpdateFlowDirection(this TextView platformView, IView view)
		{
			switch (view.FlowDirection)
			{
				case FlowDirection.MatchParent:
					platformView.LayoutDirection = ALayoutDirection.Inherit;
					platformView.TextDirection = ATextDirection.Inherit;
					break;
				case FlowDirection.RightToLeft:
					platformView.LayoutDirection = ALayoutDirection.Rtl;
					platformView.TextDirection = ATextDirection.Rtl;
					break;
				case FlowDirection.LeftToRight:
					platformView.LayoutDirection = ALayoutDirection.Ltr;
					platformView.TextDirection = ATextDirection.Ltr;
					break;
			}
		}

		public static void UpdateLineHeight(this TextView textView, ILabel label)
		{
			if (label.LineHeight >= 0)
				textView.SetLineSpacing(0, (float)label.LineHeight);
		}
			
		internal static void SetLineBreakMode(this TextView textView, ILineBreakMode breakMode, int? maxLines = null)
		{
			var lineBreakMode = breakMode.LineBreakMode;

			if (breakMode is ILabel label)
				maxLines = label.MaxLines;

			if (!maxLines.HasValue || maxLines <= 0)
				maxLines = int.MaxValue;

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

					// Leaving this in for now to preserve existing behavior
					// Technically, we don't _need_ this for Labels; they will handle Ellipsization at the end just fine, even with multiple lines
					// But we don't have a mechanism for setting MaxLines on other controls (e.g., Button) right now, so we need to force it here or
					// they will potentially exceed a single line. Also, changing this behavior the for Labels would technically be breaking (though
					// possibly less surprising than what happens currently).
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