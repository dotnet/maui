using System;
using Android.Graphics;
using Android.Text;
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

		public static void UpdatePadding(this TextView textView, ILabel label)
		{
			textView.SetPadding(
				(int)textView.ToPixels(label.Padding.Left),
				(int)textView.ToPixels(label.Padding.Top),
				(int)textView.ToPixels(label.Padding.Right),
				(int)textView.ToPixels(label.Padding.Bottom));
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
					platformView.TextDirection = ATextDirection.FirstStrongRtl;
					break;
				case FlowDirection.LeftToRight:
					platformView.LayoutDirection = ALayoutDirection.Ltr;
					platformView.TextDirection = ATextDirection.FirstStrongLtr;
					break;
			}
		}

		public static void UpdateLineHeight(this TextView textView, ILabel label)
		{
			if (label.LineHeight >= 0)
				textView.SetLineSpacing(0, (float)label.LineHeight);
		}
	}
}