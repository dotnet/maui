using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Widget;
using Microsoft.Maui.Platform.Android;

namespace Microsoft.Maui
{
	public static class EntryExtensions
	{
		static readonly int[][] ColorStates = {
			new[] { global::Android.Resource.Attribute.StateEnabled },
			new[] { -global::Android.Resource.Attribute.StateEnabled }
		};

		public static void UpdateText(this EditText editText, IEntry entry)
		{
			var newText = entry.Text ?? string.Empty;
			var oldText = editText.Text ?? string.Empty;

			if (oldText != newText)
				editText.Text = newText;
		}

		public static void UpdateTextColor(this EditText editText, IEntry entry, ColorStateList? defaultColor)
		{
			var textColor = entry.TextColor;
			if (textColor.IsDefault)
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

		public static void UpdateIsPassword(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		internal static void SetInputType(this EditText editText, IEntry entry)
		{
			if (entry.IsReadOnly)
				editText.InputType = InputTypes.Null;
			else
			{
				var keyboard = entry.Keyboard;
				var nativeInputTypeToUpdate = keyboard.ToInputType();

				if (!(keyboard is CustomKeyboard))
				{
					// TODO: IsSpellCheckEnabled handling must be here.

					if ((nativeInputTypeToUpdate & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
					{
						if (!entry.IsTextPredictionEnabled)
							nativeInputTypeToUpdate = nativeInputTypeToUpdate | InputTypes.TextFlagNoSuggestions;
					}
				}

				if (keyboard == Keyboard.Numeric)
				{
					// editText.KeyListener = GetDigitsKeyListener(editText.InputType);
				}

				if (entry.IsPassword)
				{
					if (((nativeInputTypeToUpdate & InputTypes.ClassText) == InputTypes.ClassText))
						nativeInputTypeToUpdate = nativeInputTypeToUpdate | InputTypes.TextVariationPassword;

					if (((nativeInputTypeToUpdate & InputTypes.ClassNumber) == InputTypes.ClassNumber))
						nativeInputTypeToUpdate = nativeInputTypeToUpdate | InputTypes.NumberVariationPassword;
				}

				if (!entry.IsTextPredictionEnabled && ((nativeInputTypeToUpdate & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions))
					nativeInputTypeToUpdate |= InputTypes.TextFlagNoSuggestions;

				editText.InputType = nativeInputTypeToUpdate;
			}
		}

		public static void UpdateIsTextPredictionEnabled(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateKeyboard(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdatePlaceholder(this EditText editText, IEntry entry)
		{
			if (editText.Hint == entry.Placeholder)
				return;

			editText.Hint = entry.Placeholder;
		}

		public static void UpdateIsReadOnly(this EditText editText, IEntry entry)
		{
			bool isEditable = !entry.IsReadOnly;

			editText.SetInputType(entry);

			editText.FocusableInTouchMode = isEditable;
			editText.Focusable = isEditable;
		}

		//protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		//{
		//	// Override this in a custom renderer to use a different NumberKeyListener
		//	// or to filter out input types you don't want to allow
		//	// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
		//	return LocalizedDigitsKeyListener.Create(inputTypes);
		//}
	}
}
