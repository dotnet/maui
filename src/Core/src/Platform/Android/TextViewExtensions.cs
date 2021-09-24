using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Widget;
using static Android.Widget.TextView;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;

namespace Microsoft.Maui
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

			if (NativeVersion.IsAtLeast(24))
				textView.SetText(Html.FromHtml(newText, FromHtmlOptions.ModeCompact), BufferType.Spannable);
			else
#pragma warning disable CS0618 // Type or member is obsolete
				textView.SetText(Html.FromHtml(newText), BufferType.Spannable);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public static void UpdateTextColor(this TextView textView, ITextStyle textStyle, Graphics.Color defaultColor)
		{
			var textColor = textStyle.TextColor?.ToNative() ?? defaultColor?.ToNative();

			if (textColor != null)
				textView.SetTextColor(textColor.Value);
		}

		public static void UpdateTextColor(this TextView textView, ITextStyle textStyle) =>
			textView.UpdateTextColor(textStyle, textView.TextColors);

		public static void UpdateTextColor(this TextView textView, ITextStyle textStyle, ColorStateList? defaultColor)
		{
			var textColor = textStyle.TextColor;

			if (textColor != null)
			{
				textView.SetTextColor(textColor.ToNative());
				return;
			}

			if (defaultColor != null)
				textView.SetTextColor(defaultColor);
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
			if (textView.Context!.HasRtlSupport())
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

		public static void UpdateLineBreakMode(this TextView textView, ILabel label)
		{
			textView.SetLineBreakMode(label);
		}

		public static void UpdateMaxLines(this TextView textView, ILabel label)
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

		public static void UpdateFlowDirection(this TextView nativeView, IView view)
		{
			if (view.FlowDirection == view.Handler?.MauiContext?.GetFlowDirection() ||
				view.FlowDirection == FlowDirection.MatchParent)
			{
				nativeView.LayoutDirection = ALayoutDirection.Inherit;
				nativeView.TextDirection = ATextDirection.Inherit;
			}
			else if (view.FlowDirection == FlowDirection.RightToLeft)
			{
				nativeView.LayoutDirection = ALayoutDirection.Rtl;
				nativeView.TextDirection = ATextDirection.Rtl;
			}
			else if (view.FlowDirection == FlowDirection.LeftToRight)
			{
				nativeView.LayoutDirection = ALayoutDirection.Ltr;
				nativeView.TextDirection = ATextDirection.Ltr;
			}
		}

		public static void UpdateLineHeight(this TextView textView, ILabel label)
		{
			if (label.LineHeight >= 0)
				textView.SetLineSpacing(0, (float)label.LineHeight);
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