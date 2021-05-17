using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using InputPanelReturnKeyType = ElmSharp.InputPanelReturnKeyType;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;

namespace Microsoft.Maui
{
	public static class EntryExtensions
	{
		public static void UpdateText(this Entry nativeEntry, IText entry)
		{
			nativeEntry.Text = entry.Text ?? "";
		}

		public static void UpdateTextColor(this Entry nativeEntry, ITextStyle entry)
		{
			nativeEntry.TextColor = entry.TextColor.ToNative();
		}

		public static void UpdateHorizontalTextAlignment(this Entry nativeEntry, ITextAlignment entry)
		{
			nativeEntry.HorizontalTextAlignment = entry.HorizontalTextAlignment.ToNative();
		}

		public static void UpdateIsPassword(this Entry nativeEntry, IEntry entry)
		{
			nativeEntry.IsPassword = entry.IsPassword;
		}

		public static void UpdateReturnType(this Entry nativeEntry, IEntry entry)
		{
			nativeEntry.SetInputPanelReturnKeyType(entry.ReturnType.ToInputPanelReturnKeyType());
		}

		public static void UpdateClearButtonVisibility(this Entry nativeEntry, IEntry entry)
		{
			if (entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
			{
				if (nativeEntry is EditfieldEntry editfieldEntry)
				{
					editfieldEntry.EnableClearButton = true;
				}
			}
			else
			{
				if (nativeEntry is EditfieldEntry editfieldEntry)
				{
					editfieldEntry.EnableClearButton = false;
				}
			}
		}

		public static void UpdateFont(this Entry nativeEntry, ITextStyle textStyle, IFontManager fontManager)
		{
			nativeEntry.BatchBegin();
			nativeEntry.FontSize = textStyle.Font.FontSize;
			nativeEntry.FontAttributes = textStyle.Font.GetFontAttributes();
			nativeEntry.FontFamily = fontManager.GetFontFamily(textStyle.Font.FontFamily) ?? "";
			nativeEntry.BatchCommit();
		}

		public static void UpdatePlaceholder(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.Placeholder = entry.Placeholder;
		}

		public static void UpdateIsReadOnly(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.IsEditable = !entry.IsReadOnly;
		}

		public static void UpdateIsTextPredictionEnabled(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.InputHint = entry.Keyboard.ToInputHints(true, entry.IsTextPredictionEnabled);
		}

		public static void UpdateKeyboard(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.UpdateKeyboard(entry.Keyboard, true, entry.IsTextPredictionEnabled);
		}

		public static void UpdateMaxLength(this Entry nativeEntry, ITextInput entry)
		{
			if (nativeEntry.Text.Length > entry.MaxLength)
				nativeEntry.Text = nativeEntry.Text.Substring(0, entry.MaxLength);
		}

		public static InputPanelReturnKeyType ToInputPanelReturnKeyType(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return InputPanelReturnKeyType.Go;
				case ReturnType.Next:
					return InputPanelReturnKeyType.Next;
				case ReturnType.Send:
					return InputPanelReturnKeyType.Send;
				case ReturnType.Search:
					return InputPanelReturnKeyType.Search;
				case ReturnType.Done:
					return InputPanelReturnKeyType.Done;
				case ReturnType.Default:
					return InputPanelReturnKeyType.Default;
				default:
					throw new System.NotImplementedException($"ReturnType {returnType} not supported");
			}
		}

		public static TTextAlignment ToNative(this TextAlignment alignment)
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

		public static double ToNativeDouble(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return 0.5d;

				case TextAlignment.Start:
					return 0;

				case TextAlignment.End:
					return 1d;

				default:
					Log.Warn("Warning: unrecognized HorizontalTextAlignment value {0}. " +
						"Expected: {Start|Center|End}.", alignment);
					Log.Debug("Falling back to platform's default settings.");
					return 0.5d;
			}
		}
	}
}
