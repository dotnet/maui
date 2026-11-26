using System;
using System.Net;
using System.Runtime.CompilerServices;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using static Android.Widget.TextView;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;

namespace Microsoft.Maui.Platform
{
	public static class TextViewExtensions
	{
		static readonly ConditionalWeakTable<TextView, StrongBox<int>> s_htmlGenerations = new();
		static readonly ConditionalWeakTable<TextView, ColorStateList> defaultTextColors = new();
		static readonly ConditionalWeakTable<TextView, InteractionState> s_preSelectableInteractionStates = new();

		sealed record InteractionState(global::Android.Text.Method.IMovementMethod? MovementMethod, ViewFocusability? FocusableMode, bool Focusable, bool FocusableInTouchMode, bool Clickable, bool LongClickable);

		public static void UpdateIsTextSelectable(this TextView textView, ILabel label)
		{
			var isSelectable = label.IsTextSelectable;

			// SetTextIsSelectable rewrites focusable, clickable and long-clickable and replaces the
			// movement method, so only call it on a change to not disturb other components that
			// own these flags (for example the gesture manager).
			if (textView.IsTextSelectable == isSelectable)
			{
				return;
			}

			// Selection that this mapping did not turn on (for example a custom handler calling SetTextIsSelectable)
			// is left alone while the property is unset.
			if (!isSelectable && !s_preSelectableInteractionStates.TryGetValue(textView, out _))
			{
				return;
			}

			if (isSelectable)
			{
				s_preSelectableInteractionStates.AddOrUpdate(textView, new InteractionState(
					textView.MovementMethod,
					OperatingSystem.IsAndroidVersionAtLeast(26) ? textView.GetFocusable() : null,
					textView.Focusable,
					textView.FocusableInTouchMode,
					textView.Clickable,
					textView.LongClickable));

				textView.SetTextIsSelectable(true);
				return;
			}

			textView.SetTextIsSelectable(false);

			// Turning selection off clears the flags, so give them back what they were before.
			if (s_preSelectableInteractionStates.TryGetValue(textView, out var state))
			{
				s_preSelectableInteractionStates.Remove(textView);

				// Assigning a movement method recalculates focusability, so it has to be restored before the focus flags.
				textView.MovementMethod = state.MovementMethod;

				// Focusable can be "auto" (focusable only while clickable), which a plain bool can't represent.
				if (state.FocusableMode is { } focusableMode && OperatingSystem.IsAndroidVersionAtLeast(26))
				{
					textView.SetFocusable(focusableMode);
				}
				else
				{
					textView.Focusable = state.Focusable;
				}

				textView.FocusableInTouchMode = state.FocusableInTouchMode;
				textView.Clickable = state.Clickable;
				textView.LongClickable = state.LongClickable;
			}
		}

		public static void UpdateTextPlainText(this TextView textView, IText label)
		{
			textView.Text = label.Text;
		}

		public static void UpdateTextHtml(this TextView textView, ILabel label)
		{
			var text = label.Text ?? string.Empty;
			textView.UpdateTextHtml(text);
		}

		internal static void UpdateTextHtml(this TextView textView, string text)
		{
			var htmlText = WebUtility.HtmlDecode(text);

			// Track generation to prevent stale image-load callbacks from overwriting newer text
			var generation = s_htmlGenerations.GetOrCreateValue(textView);
			int currentGen = ++generation.Value;

			ImageGetter? imageGetter = null;

			void SetTextHtml()
			{
				if (generation.Value != currentGen)
					return;

				imageGetter ??= new ImageGetter(textView.Resources!, SetTextHtml);

				if (OperatingSystem.IsAndroidVersionAtLeast(24))
					textView.SetText(Html.FromHtml(htmlText, FromHtmlOptions.ModeCompact, imageGetter, null), BufferType.Spannable);
				else
#pragma warning disable CS0618 // Type or member is obsolete
					textView.SetText(Html.FromHtml(htmlText, imageGetter, null), BufferType.Spannable);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			SetTextHtml();
		}

		public static void UpdateTextColor(this TextView textView, ITextStyle textStyle) =>
			textView.UpdateTextColor(textStyle.TextColor);

		// Applies an already-resolved text color, restoring the cached themed default when the color
		// is null. Callers that resolve the color themselves (for example the SwipeItem handler, which
		// also applies a background-contrast fallback) route through here so a later null still resets
		// the TextView instead of leaving a stale color.
		internal static void UpdateTextColor(this TextView textView, Microsoft.Maui.Graphics.Color? textColor)
		{
			// Cache the original themed TextColors the first time this control is updated.
			if (textView.TextColors is ColorStateList currentColors)
			{
				defaultTextColors.GetValue(textView, _ => currentColors);
			}

			if (textColor is null)
			{
				var defaultColors = defaultTextColors.TryGetValue(textView, out var cached) ? cached : null;
				textView.SetTextColor(defaultColors);
				return;
			}

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
				textView.Gravity = GravityFlags.Top | text.HorizontalTextAlignment.ToHorizontalGravityFlags();
			}

			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				textView.JustificationMode = text.HorizontalTextAlignment == TextAlignment.Justify
					? JustificationMode.InterWord
					: JustificationMode.None;
			}
		}

		public static void UpdateVerticalTextAlignment(this TextView textView, ITextAlignment textAlignment)
		{
			textView.UpdateVerticalAlignment(textAlignment.VerticalTextAlignment);
		}

		public static void UpdatePadding(this TextView textView, ILabel label)
		{
			textView.SetPaddingRelative(
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
			// Like iOS and Windows, 0 keeps the natural line height: as a multiplier it would stack every line on the first
			if (label.LineHeight <= 0)
				return;

			// The line spacing multiplier also scales the font padding that TextView adds to the first line,
			// making the gap below it larger, so plain text gets the same fixed extra on every line instead.
			// Spanned text (HTML) keeps the multiplier, which spaces lines of different sizes by their own height.
			if (!textView.IsTextSpanned() && textView.Paint?.GetFontMetricsInt() is { } fontMetrics)
				textView.SetLineSpacing((fontMetrics.Descent - fontMetrics.Ascent) * ((float)label.LineHeight - 1), 1);
			else
				textView.SetLineSpacing(0, (float)label.LineHeight);
		}

		// Plain text is a java.lang.String. "is ISpanned" doesn't work here: in a trimmed app,
		// formatted text (a SpannedString) can come back as a plain ICharSequence wrapper.
		internal static bool IsTextSpanned(this TextView textView) =>
			textView.TextFormatted is not Java.Lang.String;

		internal static void Focus(this TextView textView, FocusRequest request)
		{
			if (textView is null)
				return;

			textView.Focus(request, () =>
			{
				if (textView.ShowSoftInputOnFocus && textView.OnCheckIsTextEditor())
					ViewExtensions.PostShowSoftInput(textView);
			});
		}
	}
}