using System;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using static Android.Views.View;
using static Android.Widget.TextView;

namespace Microsoft.Maui.Platform
{
	public static class EditTextExtensions
	{
		public static void UpdateText(this EditText editText, IEntry entry)
		{
			// Setting the text causes the cursor to reset to position zero
			// Therefore if:
			// User Types => VirtualView Updated => Triggers Native Update
			// Then it will cause the cursor to reset to position zero as the user typed
			if (entry.Text != editText.Text)
				editText.Text = entry.Text;

			// TODO ezhart The renderer sets the text to selected and shows the keyboard if the EditText is focused
		}

		public static void UpdateText(this EditText editText, IEditor editor)
		{
			editText.Text = editor.Text;

			editText.SetSelection(editText.Text?.Length ?? 0);
		}

		public static void UpdateTextColor(this EditText editText, ITextStyle entry, ColorStateList? defaultColor)
		{
			editText.UpdateTextColor(entry.TextColor, defaultColor);
		}

		public static void UpdateTextColor(this EditText editText, Graphics.Color textColor, ColorStateList? defaultColor)
		{
			if (textColor == null)
			{
				if (defaultColor != null)
					editText.SetTextColor(defaultColor);
			}
			else
			{
				var androidColor = textColor.ToPlatform();
				if (!editText.TextColors.IsOneColor(ColorStates.EditText, androidColor))
					editText.SetTextColor(ColorStateListExtensions.CreateEditText(androidColor));
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

			if (maxLength > 0)
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

		public static void UpdatePlaceholderColor(this EditText editText, IPlaceholder placeholder, ColorStateList? defaultColor)
		{
			editText.UpdatePlaceholderColor(placeholder.PlaceholderColor, defaultColor);
		}

		public static void UpdatePlaceholderColor(this EditText editText, Graphics.Color placeholderTextColor, ColorStateList? defaultColor)
		{
			if (placeholderTextColor == null)
			{
				editText.SetHintTextColor(defaultColor);
			}
			else
			{
				var androidColor = placeholderTextColor.ToPlatform();
				if (!editText.HintTextColors.IsOneColor(ColorStates.EditText, androidColor))
					editText.SetHintTextColor(ColorStateListExtensions.CreateEditText(androidColor));
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

		public static void UpdateClearButtonVisibility(this EditText editText, IEntry entry, Drawable? clearButtonDrawable) =>
			UpdateClearButtonVisibility(editText, entry, () => clearButtonDrawable);

		public static void UpdateClearButtonVisibility(this EditText editText, IEntry entry, Func<Drawable?>? getClearButtonDrawable)
		{
			// Places clear button drawable at the end or start of the EditText based on FlowDirection.
			void ShowClearButton()
			{
				var drawable = getClearButtonDrawable?.Invoke();

				if (entry.FlowDirection == FlowDirection.RightToLeft)
				{
					editText.SetCompoundDrawablesWithIntrinsicBounds(drawable, null, null, null);
				}
				else
				{
					editText.SetCompoundDrawablesWithIntrinsicBounds(null, null, drawable, null);
				}
			}

			// Hides clear button drawable from the control.
			void HideClearButton()
			{
				editText.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
			}

			bool isFocused = editText.IsFocused;
			bool hasText = entry.Text?.Length > 0;

			bool shouldDisplayClearButton = entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing
				&& hasText
				&& isFocused;

			if (shouldDisplayClearButton)
			{
				ShowClearButton();
			}
			else
			{
				HideClearButton();
			}
		}

		public static void UpdateReturnType(this EditText editText, IEntry entry)
		{
			editText.ImeOptions = entry.ReturnType.ToPlatform();
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
				if (!editText.HasFocus)
					editText.RequestFocus();

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

		internal static void SetInputType(this EditText editText, ITextInput textInput)
		{
			if (textInput.IsReadOnly)
			{
				editText.InputType = InputTypes.Null;
			}
			else
			{
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

				if (textInput is IEntry entry && entry.IsPassword)
				{
					if ((nativeInputTypeToUpdate & InputTypes.ClassText) == InputTypes.ClassText)
						nativeInputTypeToUpdate |= InputTypes.TextVariationPassword;

					if ((nativeInputTypeToUpdate & InputTypes.ClassNumber) == InputTypes.ClassNumber)
						nativeInputTypeToUpdate |= InputTypes.NumberVariationPassword;
				}

				editText.InputType = nativeInputTypeToUpdate;
			}

			if (textInput is IEditor)
				editText.InputType |= InputTypes.TextFlagMultiLine;
		}

		internal static bool IsCompletedAction(this EditorActionEventArgs e)
		{
			var actionId = e.ActionId;
			var evt = e.Event;

			return
				actionId == ImeAction.Done ||
				(actionId == ImeAction.ImeNull && evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Up);
		}

		/// <summary>
		/// Checks whether the touched position on the EditText is inbounds with clear button and clears if so.
		/// This will return True to handle OnTouch to prevent re-activating keyboard after clearing the text.
		/// </summary>
		/// <returns>True if clear button is clicked and Text is cleared. False if not.</returns>
		internal static bool HandleClearButtonTouched(this EditText? platformView, FlowDirection flowDirection, TouchEventArgs? touchEvent, Func<Drawable?>? getClearButtonDrawable)
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

			var x = motionEvent.GetX();
			var y = motionEvent.GetY();

			if ((flowDirection != FlowDirection.LeftToRight
				|| x < platformView.Right - buttonWidth
				|| x > platformView.Right - platformView.PaddingRight
				|| y < platformView.PaddingTop
				|| y > platformView.Height - platformView.PaddingBottom) &&
				(flowDirection != FlowDirection.RightToLeft
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