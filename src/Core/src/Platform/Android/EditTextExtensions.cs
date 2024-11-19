using System;
using Android.Content;
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
			editText.UpdateIsTextPredictionEnabled(entry as ITextInput);
		}

		public static void UpdateIsSpellCheckEnabled(this EditText editText, IEntry entry)
		{
			editText.UpdateIsSpellCheckEnabled(entry as ITextInput);
		}

		public static void UpdateIsTextPredictionEnabled(this EditText editText, IEditor editor)
		{
			editText.UpdateIsTextPredictionEnabled(editor as ITextInput);
		}

		public static void UpdateIsSpellCheckEnabled(this EditText editText, IEditor editor)
		{
			editText.UpdateIsSpellCheckEnabled(editor as ITextInput);
		}

		private static void UpdateIsTextPredictionEnabled(this EditText editText, ITextInput textInput)
		{
			var keyboard = textInput.Keyboard;

			// TextFlagAutoCorrect will correct "Whats" -> "What's"
			// TextFlagAutoCorrect should not be confused with TextFlagAutocomplete
			// Autocomplete property pertains to fields that will "self-fill" - like an "Address" input box that fills with your saved data
			if (textInput.IsTextPredictionEnabled)
				editText.InputType |= InputTypes.TextFlagAutoCorrect;
			else
				editText.InputType &= ~InputTypes.TextFlagAutoCorrect;
		}

		private static void UpdateIsSpellCheckEnabled(this EditText editText, ITextInput textInput)
		{
			// TextFlagNoSuggestions disables spellchecking (the red squiggly lines)
			if (!textInput.IsSpellCheckEnabled)
				editText.InputType |= InputTypes.TextFlagNoSuggestions;
			else
				editText.InputType &= ~InputTypes.TextFlagNoSuggestions;
		}

		public static void UpdateMaxLength(this EditText editText, IEntry entry) =>
			UpdateMaxLength(editText, entry.MaxLength);

		public static void UpdateMaxLength(this EditText editText, IEditor editor) =>
			UpdateMaxLength(editText, editor.MaxLength);

		public static void UpdateMaxLength(this EditText editText, int maxLength) =>
			PlatformInterop.UpdateMaxLength(editText, maxLength);

		public static void SetLengthFilter(this EditText editText, int maxLength) =>
			PlatformInterop.SetLengthFilter(editText, maxLength);

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
			editText.SetInputType(entry);
			editText.ImeOptions = entry.ReturnType.ToPlatform();

			// Restart the input on the current focused EditText
			InputMethodManager? imm = (InputMethodManager?)editText.Context?.GetSystemService(Context.InputMethodService);
			imm?.RestartInput(editText);
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

			editText.InputType = keyboard.ToInputType();

			if (keyboard is not CustomKeyboard)
			{
				editText.UpdateIsTextPredictionEnabled(textInput);
				editText.UpdateIsSpellCheckEnabled(textInput);
			}

			if (keyboard == Keyboard.Numeric)
			{
				editText.KeyListener = LocalizedDigitsKeyListener.Create(editText.InputType);
			}

			if (textInput is IEntry entry && entry.IsPassword)
			{
				if (editText.InputType.HasFlag(InputTypes.ClassText))
					editText.InputType |= InputTypes.TextVariationPassword;
				if (editText.InputType.HasFlag(InputTypes.ClassNumber))
					editText.InputType |= InputTypes.NumberVariationPassword;
			}

			if (textInput is IEditor)
				editText.InputType |= InputTypes.TextFlagMultiLine;

			if (textInput is IElement element)
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

		internal static bool IsCompletedAction(this EditorActionEventArgs e, ImeAction currentInputImeFlag)
		{
			var actionId = e.ActionId;
			var evt = e.Event;

			// On API 34 it looks like they fixed the issue where the actionId is ImeAction.ImeNull when using a keyboard
			// so I'm just setting the actionId here to whatever the user has 
			if (actionId == ImeAction.ImeNull && evt?.KeyCode == Keycode.Enter)
			{
				actionId = currentInputImeFlag;
			}

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

			if (motionEvent.Action != MotionEventActions.Up)
				return false;

			var rBounds = getClearButtonDrawable?.Invoke()?.Bounds;

			if (rBounds is null)
			{
				// The button doesn't exist, or we can't retrieve it. 
				return false;
			}

			var buttonRect = GetClearButtonLocation(rBounds, platformView);

			if (!RectContainsMotionEvent(buttonRect, motionEvent))
			{
				return false;
			}

			platformView.Text = null;
			return true;
		}

		// Android.Graphics.Rect has a Containts(x,y) method, but it only takes `int` and the coordinates from
		// the motion event are `float`. The we use GetX() and GetY() so our coordinates are relative to the
		// bounds of the EditText.
		static bool RectContainsMotionEvent(Android.Graphics.Rect rect, MotionEvent motionEvent)
		{
			var x = motionEvent.GetX();

			if (x < rect.Left || x > rect.Right)
			{
				return false;
			}

			var y = motionEvent.GetY();

			if (y < rect.Top || y > rect.Bottom)
			{
				return false;
			}

			return true;
		}

		// Gets the location of the "Clear" button relative to the bounds of the EditText
		static Android.Graphics.Rect GetClearButtonLocation(Android.Graphics.Rect buttonRect, EditText platformView)
		{
			// Determine the top and bottom edges of the button
			// This assumes the button is vertically centered within the padded area of the EditText

			var buttonHeight = buttonRect.Height();
			var editAreaTop = platformView.Top + platformView.PaddingTop;
			var editAreaHeight = (platformView.Bottom - platformView.PaddingBottom) - (editAreaTop);
			var editAreaVerticalCenter = editAreaTop + (editAreaHeight / 2);

			var topEdge = editAreaVerticalCenter - (buttonHeight / 2);
			var bottomEdge = topEdge + buttonHeight;

			// The horizontal location of the button depends on the layout direction
			var flowDirection = platformView.LayoutDirection;

			if (flowDirection == LayoutDirection.Ltr)
			{
				var rightEdge = platformView.Width - platformView.PaddingRight;
				var leftEdge = rightEdge - buttonRect.Width();

				return new Android.Graphics.Rect(leftEdge, topEdge, rightEdge, bottomEdge);
			}
			else
			{
				var leftEdge = platformView.PaddingLeft;
				var rightEdge = leftEdge + buttonRect.Width();

				return new Android.Graphics.Rect(leftEdge, topEdge, rightEdge, bottomEdge);
			}
		}
	}
}