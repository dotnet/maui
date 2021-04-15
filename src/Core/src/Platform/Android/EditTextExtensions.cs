using System.Collections.Generic;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class EditTextExtensions
	{
		static readonly int[][] ColorStates =
		{
			new[] { Android.Resource.Attribute.StateEnabled },
			new[] { -Android.Resource.Attribute.StateEnabled }
		};

		public static void UpdateText(this AppCompatEditText editText, IEntry entry)
		{
			editText.UpdateText(entry.Text);

			// TODO ezhart The renderer sets the text to selected and shows the keyboard if the EditText is focused
		}

		public static void UpdateText(this AppCompatEditText editText, IEditor editor)
		{
			editText.UpdateText(editor.Text);

			editText.SetSelection(editText.Text?.Length ?? 0);
		}

		public static void UpdateTextColor(this AppCompatEditText editText, ITextStyle entry, ColorStateList? defaultColor)
		{
			var textColor = entry.TextColor;

			if (textColor == null)
			{
				editText.SetTextColor(defaultColor);
			}
			else
			{
				var androidColor = textColor.ToNative();

				if (!editText.TextColors.IsOneColor(ColorStates, androidColor))
				{
					var acolor = androidColor.ToArgb();
					editText.SetTextColor(new ColorStateList(ColorStates, new[] { acolor, acolor }));
				}
			}
		}

		public static void UpdateIsPassword(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateHorizontalTextAlignment(this AppCompatEditText editText, IEntry entry)
		{
			editText.UpdateHorizontalAlignment(entry.HorizontalTextAlignment, editText.Context != null && editText.Context.HasRtlSupport());
		}

		public static void UpdateIsTextPredictionEnabled(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateIsTextPredictionEnabled(this AppCompatEditText editText, IEditor editor)
		{
			if (editor.IsTextPredictionEnabled)
				editText.InputType &= ~InputTypes.TextFlagNoSuggestions;
			else
				editText.InputType |= InputTypes.TextFlagNoSuggestions;
		}

		public static void UpdateMaxLength(this AppCompatEditText editText, IEntry entry) =>
			UpdateMaxLength(editText, entry.MaxLength);

		public static void UpdateMaxLength(this AppCompatEditText editText, IEditor editor) =>
			UpdateMaxLength(editText, editor.MaxLength);

		public static void UpdateMaxLength(this AppCompatEditText editText, int maxLength)
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

			var newText = editText.Text.TrimToMaxLength(maxLength);
			if (editText.Text != newText)
				editText.Text = newText;
		}

		public static void UpdatePlaceholder(this AppCompatEditText editText, IPlaceholder textInput)
		{
			if (editText.Hint == textInput.Placeholder)
				return;

			editText.Hint = textInput.Placeholder;
		}

		public static void UpdatePlaceholderColor(this AppCompatEditText editText, IEditor editor, ColorStateList? defaultColor)
		{
			var placeholderTextColor = editor.PlaceholderColor;
			if (placeholderTextColor == null)
			{
				editText.SetHintTextColor(defaultColor);
			}
			else
			{
				var androidColor = placeholderTextColor.ToNative();

				if (!editText.HintTextColors.IsOneColor(ColorExtensions.States, androidColor))
				{
					var acolor = androidColor.ToArgb();
					editText.SetHintTextColor(new ColorStateList(ColorExtensions.States, new[] { acolor, acolor }));
				}
			}
		}

		public static void UpdateIsReadOnly(this AppCompatEditText editText, IEntry entry)
		{
			bool isEditable = !entry.IsReadOnly;

			editText.SetInputType(entry);

			editText.FocusableInTouchMode = isEditable;
			editText.Focusable = isEditable;
		}

		public static void UpdateKeyboard(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateIsReadOnly(this AppCompatEditText editText, IEditor editor)
		{
			bool isReadOnly = !editor.IsReadOnly;

			editText.FocusableInTouchMode = isReadOnly;
			editText.Focusable = isReadOnly;
			editText.SetCursorVisible(isReadOnly);
		}

		public static void UpdateClearButtonVisibility(this AppCompatEditText editText, IEntry entry, Drawable? ClearButtonDrawable)
		{
			// Places clear button drawable at the end or start of the EditText based on FlowDirection.
			void ShowClearButton()
			{
				if (entry.FlowDirection == FlowDirection.RightToLeft)
				{
					editText.SetCompoundDrawablesWithIntrinsicBounds(ClearButtonDrawable, null, null, null);
				}
				else
				{
					editText.SetCompoundDrawablesWithIntrinsicBounds(null, null, ClearButtonDrawable, null);
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

		public static void UpdateReturnType(this AppCompatEditText editText, IEntry entry)
		{
			editText.ImeOptions = entry.ReturnType.ToNative();
		}

		internal static void SetInputType(this AppCompatEditText editText, IEntry entry)
		{
			if (entry.IsReadOnly)
			{
				editText.InputType = InputTypes.Null;
			}
			else
			{
				var keyboard = entry.Keyboard;
				var nativeInputTypeToUpdate = keyboard.ToInputType();

				if (keyboard is not CustomKeyboard)
				{
					// TODO: IsSpellCheckEnabled handling must be here.

					if ((nativeInputTypeToUpdate & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
					{
						if (!entry.IsTextPredictionEnabled)
							nativeInputTypeToUpdate |= InputTypes.TextFlagNoSuggestions;
					}
				}

				if (keyboard == Keyboard.Numeric)
				{
					editText.KeyListener = LocalizedDigitsKeyListener.Create(editText.InputType);
				}

				if (entry.IsPassword)
				{
					if ((nativeInputTypeToUpdate & InputTypes.ClassText) == InputTypes.ClassText)
						nativeInputTypeToUpdate |= InputTypes.TextVariationPassword;

					if ((nativeInputTypeToUpdate & InputTypes.ClassNumber) == InputTypes.ClassNumber)
						nativeInputTypeToUpdate |= InputTypes.NumberVariationPassword;
				}

				editText.InputType = nativeInputTypeToUpdate;
			}
		}
	}
}