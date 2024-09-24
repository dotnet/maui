using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using TReturnType = Tizen.UIExtensions.Common.ReturnType;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;

namespace Microsoft.Maui.Platform
{
	public static class EntryExtensions
	{
		public static void UpdateText(this Entry platformEntry, IText entry)
		{
			if (platformEntry.Text != entry.Text)
				platformEntry.Text = entry.Text ?? "";
		}

		public static void UpdateTextColor(this Entry platformEntry, ITextStyle entry)
		{
			platformEntry.TextColor = entry.TextColor.ToPlatform();
		}

		public static void UpdateHorizontalTextAlignment(this Entry platformEntry, ITextAlignment entry)
		{
			platformEntry.HorizontalTextAlignment = entry.HorizontalTextAlignment.ToPlatform();
		}

		public static void UpdateVerticalTextAlignment(this Entry platformEntry, ITextAlignment entry)
		{
			platformEntry.VerticalTextAlignment = entry.VerticalTextAlignment.ToPlatform();
		}

		public static void UpdateIsPassword(this Entry platformEntry, IEntry entry)
		{
			platformEntry.IsPassword = entry.IsPassword;

			// it is workaround, Text does not instantly changed
			platformEntry.Text = platformEntry.Text;
		}

		public static void UpdateReturnType(this Entry platformEntry, IEntry entry)
		{
			platformEntry.ReturnType = entry.ReturnType.ToPlatform();
		}

		public static void UpdateFont(this Entry platformEntry, ITextStyle textStyle, IFontManager fontManager)
		{
			platformEntry.FontSize = textStyle.Font.Size > 0 ? textStyle.Font.Size.ToScaledPoint() : 25d.ToScaledPoint();
			platformEntry.FontAttributes = textStyle.Font.GetFontAttributes();
			platformEntry.FontFamily = fontManager.GetFontFamily(textStyle.Font.Family) ?? "";
		}

		public static void UpdatePlaceholder(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.Placeholder = entry.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholder(this Entry platformEntry, string placeholder)
		{
			platformEntry.Placeholder = placeholder;
		}

		public static void UpdatePlaceholderColor(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.PlaceholderColor = entry.PlaceholderColor.ToPlatform();
		}

		public static void UpdatePlaceholderColor(this Entry platformEntry, GColor color)
		{
			platformEntry.PlaceholderColor = color.ToPlatform();
		}

		public static void UpdateIsReadOnly(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.IsReadOnly = entry.IsReadOnly;
		}

		public static void UpdateIsTextPredictionEnabled(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.IsTextPredictionEnabled = entry.IsTextPredictionEnabled;
		}

		public static void UpdateMaxLength(this Entry platformEntry, ITextInput entry) =>
			platformEntry.MaxLength = entry.MaxLength;

		public static void UpdateKeyboard(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.Keyboard = entry.Keyboard.ToPlatform();
		}

		public static void UpdateReturnType(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.ReturnType = entry.ReturnType.ToPlatform();
		}

		public static void UpdateCursorPosition(this Entry platformEntry, IEntry entry)
		{
			platformEntry.PrimaryCursorPosition = entry.CursorPosition;
		}

		public static void UpdateSelectionLength(this Entry platformEntry, IEntry entry)
		{
			if (entry.SelectionLength == 0)
			{
				platformEntry.SelectNone();
			}
			else
			{
				platformEntry.SelectText(entry.CursorPosition, entry.CursorPosition + entry.SelectionLength);
			}
		}

		public static void UpdateCharacterSpacing(this Entry platformEntry, ITextStyle entry)
		{
			platformEntry.CharacterSpacing = entry.CharacterSpacing.ToScaledPixel();
		}

		public static TReturnType ToPlatform(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return TReturnType.Go;
				case ReturnType.Next:
					return TReturnType.Next;
				case ReturnType.Send:
					return TReturnType.Send;
				case ReturnType.Search:
					return TReturnType.Search;
				case ReturnType.Done:
					return TReturnType.Done;
				case ReturnType.Default:
					return TReturnType.Default;
				default:
					throw new System.NotImplementedException($"ReturnType {returnType} not supported");
			}
		}

		public static TTextAlignment ToPlatform(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return TTextAlignment.Center;

				case TextAlignment.Start:
					return TTextAlignment.Start;

				case TextAlignment.End:
					return TTextAlignment.End;

				default:
					Log.Warn("Warning: unrecognized HorizontalTextAlignment value {0}. " +
						"Expected: {Start|Center|End}.", alignment);
					Log.Debug("Falling back to platform's default settings.");
					return TTextAlignment.Auto;
			}
		}
	}
}
