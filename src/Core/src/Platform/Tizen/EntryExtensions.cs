using System;
using System.Runtime.InteropServices;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using InputPanelReturnKeyType = ElmSharp.InputPanelReturnKeyType;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;

namespace Microsoft.Maui.Platform
{
	public static class EntryExtensions
	{
		public static void UpdateText(this Entry platformEntry, IText entry)
		{
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
			platformEntry.SetVerticalTextAlignment(entry.VerticalTextAlignment.ToPlatformDouble());
			platformEntry.SetVerticalPlaceHolderTextAlignment(entry.VerticalTextAlignment.ToPlatformDouble());
		}

		public static void UpdateIsPassword(this Entry platformEntry, IEntry entry)
		{
			platformEntry.IsPassword = entry.IsPassword;
		}

		public static void UpdateReturnType(this Entry platformEntry, IEntry entry)
		{
			platformEntry.SetInputPanelReturnKeyType(entry.ReturnType.ToInputPanelReturnKeyType());
		}

		public static void UpdateClearButtonVisibility(this Entry platformEntry, IEntry entry)
		{
			if (entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing)
			{
				if (platformEntry is EditfieldEntry editfieldEntry)
				{
					editfieldEntry.EnableClearButton = true;
				}
			}
			else
			{
				if (platformEntry is EditfieldEntry editfieldEntry)
				{
					editfieldEntry.EnableClearButton = false;
				}
			}
		}

		public static void UpdateFont(this Entry platformEntry, ITextStyle textStyle, IFontManager fontManager)
		{
			platformEntry.BatchBegin();
			platformEntry.FontSize = textStyle.Font.Size > 0 ? textStyle.Font.Size : 25.ToDPFont();
			platformEntry.FontAttributes = textStyle.Font.GetFontAttributes();
			platformEntry.FontFamily = fontManager.GetFontFamily(textStyle.Font.Family) ?? "";
			platformEntry.BatchCommit();
		}

		public static void UpdatePlaceholder(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.Placeholder = entry.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholderColor(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.PlaceholderColor = entry.PlaceholderColor.ToPlatform();
		}

		public static void UpdateIsReadOnly(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.IsEditable = !entry.IsReadOnly;
		}

		public static void UpdateIsTextPredictionEnabled(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.InputHint = entry.Keyboard.ToInputHints(true, entry.IsTextPredictionEnabled);
		}

		public static void UpdateKeyboard(this Entry platformEntry, ITextInput entry)
		{
			platformEntry.UpdateKeyboard(entry.Keyboard, true, entry.IsTextPredictionEnabled);
		}

		public static void UpdateMaxLength(this Entry platformEntry, ITextInput entry)
		{
			if (entry.MaxLength > 0 && platformEntry.Text.Length > entry.MaxLength)
				platformEntry.Text = platformEntry.Text.Substring(0, entry.MaxLength);
		}

		/* Updates both the IEntry.CursorPosition and IEntry.SelectionLength properties. */
		[PortHandler]
		public static void UpdateSelectionLength(this Entry platformEntry, ITextInput entry)
		{
			if (platformEntry.IsUpdatingCursorPosition)
				return;

			int start = GetSelectionStart(platformEntry, entry);
			int end = GetSelectionEnd(platformEntry, entry, start);

			if (start < end)
			{
				platformEntry.SetSelectionRegion(start, end);
			}
			else
			{
				platformEntry.CursorPosition = entry.CursorPosition;
			}
		}

		static int GetSelectionStart(Entry platformEntry, ITextInput entry)
		{
			int start = platformEntry.Text?.Length ?? 0;
			int cursorPosition = entry.CursorPosition;

			if (!string.IsNullOrEmpty(platformEntry.Text))
				start = Math.Min(start, cursorPosition);

			if (start != cursorPosition)
				entry.CursorPosition = start;

			return start;
		}

		static int GetSelectionEnd(Entry platformEntry, ITextInput entry, int start)
		{
			int end = Math.Max(start, Math.Min(platformEntry.Text?.Length ?? 0, start + entry.SelectionLength));
			int selectionLength = end - start;
			if (selectionLength != entry.SelectionLength)
				entry.SelectionLength = selectionLength;
			return end;
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

		public static double ToPlatformDouble(this TextAlignment alignment)
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

		public static void GetSelectRegion(this Entry entry, out int start, out int end)
		{
			elm_entry_select_region_get(entry.RealHandle, out start, out end);
		}

		[DllImport("libelementary.so.1")]
		static extern void elm_entry_select_region_get(IntPtr obj, out int start, out int end);
	}
}
