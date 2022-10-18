using System;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using static Android.Views.View;
using static Android.Widget.TextView;

namespace Microsoft.Maui.Platform
{
	public static class EditTextExtensions
	{
		public static void UpdateText(this EditText editText, IEntry entry)
		{
			var previousTextLength = editText.Length();

			// Setting the text causes the cursor to reset to position zero
			// Therefore if:
			// User Types => VirtualView Updated => Triggers Native Update
			// Then it will cause the cursor to reset to position zero as the user typed
			editText.Text = entry.Text;
			editText.SetSelection(editText.Text?.Length ?? 0);

			// TODO ezhart The renderer sets the text to selected and shows the keyboard if the EditText is focused
		}

		public static void UpdateText(this EditText editText, IEditor editor)
		{
			editText.Text = editor.Text;
			editText.SetSelection(editText.Text?.Length ?? 0);
		}

		public static void UpdateTextColor(this EditText editText, ITextStyle entry)
		{
			editText.UpdateTextColor(entry.TextColor);
		}

		public static void UpdateTextColor(this EditText editText, Graphics.Color textColor)
		{
			if (textColor != null)
			{
				if (PlatformInterop.CreateEditTextColorStateList(editText.TextColors, textColor.ToPlatform()) is ColorStateList c)
					editText.SetTextColor(c);
			}
		}

		public static void UpdateIsPassword(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateHorizontalTextAlignment(this EditText editText, ITextAlignment textAlignment)
		{
			editText.UpdateHorizontalAlignment(textAlignment.HorizontalTextAlignment);
		}

		public static void UpdateVerticalTextAlignment(this EditText editText, ITextAlignment entry)
		{
			editText.UpdateVerticalAlignment(entry.VerticalTextAlignment);
		}

		public static void UpdateIsTextPredictionEnabled(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateIsTextPredictionEnabled(this EditText editText, IEditor editor)
		{
			if (editor.IsTextPredictionEnabled)
				editText.InputType &= ~InputTypes.TextFlagNoSuggestions;
			else
				editText.InputType |= InputTypes.TextFlagNoSuggestions;
		}

		public static void UpdateMaxLength(this EditText editText, IEntry entry) =>
			UpdateMaxLength(editText, entry.MaxLength);

		public static void UpdateMaxLength(this EditText editText, IEditor editor) =>
			UpdateMaxLength(editText, editor.MaxLength);

		public static void UpdateMaxLength(this EditText editText, int maxLength)
		{
			editText.SetLengthFilter(maxLength);

			var newText = editText.Text.TrimToMaxLength(maxLength);
			if (editText.Text != newText)
				editText.Text = newText;
		}

		public static void SetLengthFilter(this EditText editText, int maxLength)
		{
			if (maxLength == -1)
				maxLength = int.MaxValue;

			var currentFilters = new List<IInputFilter>(editText.GetFilters() ?? new IInputFilter[0]);
			var changed = false;

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					changed = true;
					break;
				}
			}

			if (maxLength >= 0)
			{
				currentFilters.Add(new InputFilterLengthFilter(maxLength));
				changed = true;
			}

			if (changed)
				editText.SetFilters(currentFilters.ToArray());
		}

		public static void UpdatePlaceholder(this EditText editText, IPlaceholder textInput)
		{
			if (editText.Hint == textInput.Placeholder)
				return;

			editText.Hint = textInput.Placeholder;
		}

		public static void UpdatePlaceholderColor(this EditText editText, IPlaceholder placeholder)
		{
			editText.UpdatePlaceholderColor(placeholder.PlaceholderColor);
		}

		public static void UpdatePlaceholderColor(this EditText editText, Graphics.Color placeholderTextColor)
		{
			if (placeholderTextColor != null)
			{
				if (PlatformInterop.CreateEditTextColorStateList(editText.HintTextColors, placeholderTextColor.ToPlatform()) is ColorStateList c)
					editText.SetHintTextColor(c);
			}
		}

		public static void UpdateIsReadOnly(this EditText editText, IEntry entry)
		{
			bool isEditable = !entry.IsReadOnly;

			editText.SetInputType(entry);

			editText.FocusableInTouchMode = isEditable;
			editText.Focusable = isEditable;

			editText.SetCursorVisible(isEditable);
		}

		public static void UpdateKeyboard(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateKeyboard(this EditText editText, IEditor editor)
		{
			editText.SetInputType(editor);
		}

		public static void UpdateIsReadOnly(this EditText editText, IEditor editor)
		{
			bool isReadOnly = !editor.IsReadOnly;

			editText.FocusableInTouchMode = isReadOnly;
			editText.Focusable = isReadOnly;
			editText.SetCursorVisible(isReadOnly);
		}

		// TODO: NET8 hartez - Remove this, nothing uses it
		public static void UpdateClearButtonVisibility(this EditText editText, IEntry entry, Drawable? clearButtonDrawable) =>
			UpdateClearButtonVisibility(editText, entry, () => clearButtonDrawable);

		// TODO: NET8 hartez - Remove the getClearButtonDrawable parameter, nothing uses it
		public static void UpdateClearButtonVisibility(this EditText editText, IEntry entry, Func<Drawable?>? getClearButtonDrawable)
		{
			if (entry?.Handler is not EntryHandler entryHandler)
			{
				return;
			}

			bool isFocused = editText.IsFocused;
			bool hasText = entry.Text?.Length > 0;

			bool shouldDisplayClearButton = entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing
				&& hasText
				&& isFocused;

			if (shouldDisplayClearButton)
			{
				entryHandler.ShowClearButton();
			}
			else
			{
				entryHandler.HideClearButton();
			}
		}

		public static void UpdateReturnType(this EditText editText, IEntry entry)
		{
			editText.ImeOptions = entry.ReturnType.ToPlatform();
		}

		// TODO: NET8 issoto - Revisit this, marking this method as `internal` to avoid breaking public API changes
		internal static int GetCursorPosition(this EditText editText, int cursorOffset = 0)
		{
			var newCursorPosition = editText.SelectionStart + cursorOffset;
			return Math.Max(0, newCursorPosition);
		}

		public static void UpdateCursorPosition(this EditText editText, ITextInput entry)
		{
			if (editText.SelectionStart != entry.CursorPosition)
				UpdateCursorSelection(editText, entry);
		}

		public static void UpdateSelectionLength(this EditText editText, ITextInput entry)
		{
			if ((editText.SelectionEnd - editText.SelectionStart) != entry.SelectionLength)
				UpdateCursorSelection(editText, entry);
		}

		/* Updates both the IEntry.CursorPosition and IEntry.SelectionLength properties. */
		static void UpdateCursorSelection(EditText editText, ITextInput entry)
		{
			if (!entry.IsReadOnly)// && editText.HasFocus)// || editText.RequestFocus()))//&& editText.RequestFocus())
			{
				int start = GetSelectionStart(editText, entry);
				int end = GetSelectionEnd(editText, entry, start);

				editText.SetSelection(start, end);
			}
		}

		static int GetSelectionStart(EditText editText, ITextInput entry)
		{
			int start = editText.Length();
			int cursorPosition = entry.CursorPosition;

			if (editText.Text != null)
			{
				// Capping cursorPosition to the end of the text if needed
				start = System.Math.Min(editText.Text.Length, cursorPosition);
			}

			if (start != cursorPosition)
			{
				// Update the interface if start was capped
				entry.CursorPosition = start;
			}

			return start;
		}

		static int GetSelectionEnd(EditText editText, ITextInput entry, int start)
		{
			int end = start;
			int selectionLength = entry.SelectionLength;
			end = System.Math.Max(start, System.Math.Min(editText.Length(), start + selectionLength));
			int newSelectionLength = System.Math.Max(0, end - start);
			// Updating this property results in UpdateSelectionLength being called again messing things up
			if (newSelectionLength != selectionLength)
				entry.SelectionLength = newSelectionLength;
			return end;
		}

		// TODO: NET8 issoto - Revisit this, marking this method as `internal` to avoid breaking public API changes
		internal static int GetSelectedTextLength(this EditText editText)
		{
			var selectedLength = editText.SelectionEnd - editText.SelectionStart;
			return Math.Max(0, selectedLength);
		}

		internal static void SetInputType(this EditText editText, ITextInput textInput)
		{
			var previousCursorPosition = editText.SelectionStart;
			var keyboard = textInput.Keyboard;
			var nativeInputTypeToUpdate = keyboard.ToInputType();

			if (keyboard is not CustomKeyboard)
			{
				// TODO: IsSpellCheckEnabled handling must be here.

				if ((nativeInputTypeToUpdate & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
				{
					if (!textInput.IsTextPredictionEnabled)
						nativeInputTypeToUpdate |= InputTypes.TextFlagNoSuggestions;
				}
			}

			if (keyboard == Keyboard.Numeric)
			{
				editText.KeyListener = LocalizedDigitsKeyListener.Create(editText.InputType);
			}

			bool hasPassword = false;

			if (textInput is IEntry entry && entry.IsPassword)
			{
				if ((nativeInputTypeToUpdate & InputTypes.ClassText) == InputTypes.ClassText)
					nativeInputTypeToUpdate |= InputTypes.TextVariationPassword;

				if ((nativeInputTypeToUpdate & InputTypes.ClassNumber) == InputTypes.ClassNumber)
					nativeInputTypeToUpdate |= InputTypes.NumberVariationPassword;

				hasPassword = true;
			}

			editText.InputType = nativeInputTypeToUpdate;

			if (textInput is IEditor)
				editText.InputType |= InputTypes.TextFlagMultiLine;

			if (hasPassword && textInput is IElement element)
			{
				var services = element.Handler?.MauiContext?.Services;

				if (services == null)
					return;

				var fontManager = services.GetRequiredService<IFontManager>();
				editText.UpdateFont(textInput, fontManager);
			}

			// If we implement the OnSelectionChanged method, this method is called after a keyboard layout change with SelectionStart = 0,
			// Let's restore the cursor position to its previous location.
			editText.SetSelection(previousCursorPosition);
		}

		internal static bool IsCompletedAction(this EditorActionEventArgs e, ImeAction? currentInputImeFlag)
		{
			var actionId = e.ActionId;
			var evt = e.Event;

			return
				actionId == ImeAction.Done ||
				actionId == currentInputImeFlag ||
				(actionId == ImeAction.ImeNull && evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Up);
		}

		/// <summary>
		/// Checks whether the touched position on the EditText is inbounds with clear button and clears if so.
		/// This will return True to handle OnTouch to prevent re-activating keyboard after clearing the text.
		/// </summary>
		/// <returns>True if clear button is clicked and Text is cleared. False if not.</returns>
		internal static bool HandleClearButtonTouched(this EditText? platformView, TouchEventArgs? touchEvent, Func<Drawable?>? getClearButtonDrawable)
		{
			if (platformView is null)
				return false;

			var motionEvent = touchEvent?.Event;
			if (motionEvent is null)
				return false;

			var rBounds = getClearButtonDrawable?.Invoke()?.Bounds;
			var buttonWidth = rBounds?.Width();

			if (buttonWidth <= 0)
				return false;

			if (motionEvent.Action != MotionEventActions.Up)
				return false;

			var x = motionEvent.RawX;
			var y = motionEvent.GetY();

			var flowDirection = platformView.LayoutDirection;

			if ((flowDirection != LayoutDirection.Ltr
				|| x < platformView.Right - buttonWidth
				|| x > platformView.Right - platformView.PaddingRight
				|| y < platformView.PaddingTop
				|| y > platformView.Height - platformView.PaddingBottom) &&
				(flowDirection != LayoutDirection.Rtl
				|| x < platformView.Left + platformView.PaddingLeft
				|| x > platformView.Left + buttonWidth
				|| y < platformView.PaddingTop
				|| y > platformView.Height - platformView.PaddingBottom))
			{
				return false;
			}

			platformView.Text = null;
			return true;
		}
	}
}