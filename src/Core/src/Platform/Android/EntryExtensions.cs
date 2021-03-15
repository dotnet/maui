using Android.Content.Res;
using Android.Text;
using Android.Util;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Platform.Android;

namespace Microsoft.Maui
{
	public static class EntryExtensions
	{
		static readonly int[][] ColorStates = {
			new[] { global::Android.Resource.Attribute.StateEnabled },
			new[] { -global::Android.Resource.Attribute.StateEnabled }
		};

		public static void UpdateText(this AppCompatEditText editText, IEntry entry)
		{
			var newText = entry.Text ?? string.Empty;
			var oldText = editText.Text ?? string.Empty;

			if (oldText != newText)
				editText.Text = newText;
		}

		public static void UpdateTextColor(this AppCompatEditText editText, IEntry entry, ColorStateList? defaultColor)
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

		public static void UpdateIsPassword(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		internal static void SetInputType(this AppCompatEditText editText, IEntry entry)
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
							nativeInputTypeToUpdate |= InputTypes.TextFlagNoSuggestions;
					}
				}

				if (keyboard == Keyboard.Numeric)
				{
					editText.KeyListener = LocalizedDigitsKeyListener.Create(editText.InputType);
				}

				if (entry.IsPassword)
				{
					if (((nativeInputTypeToUpdate & InputTypes.ClassText) == InputTypes.ClassText))
						nativeInputTypeToUpdate |= InputTypes.TextVariationPassword;

					if (((nativeInputTypeToUpdate & InputTypes.ClassNumber) == InputTypes.ClassNumber))
						nativeInputTypeToUpdate |= InputTypes.NumberVariationPassword;
				}

				editText.InputType = nativeInputTypeToUpdate;
			}
		}

		public static void UpdateIsTextPredictionEnabled(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateKeyboard(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdatePlaceholder(this AppCompatEditText editText, IEntry entry)
		{
			if (editText.Hint == entry.Placeholder)
				return;

			editText.Hint = entry.Placeholder;
		}

		public static void UpdateIsReadOnly(this AppCompatEditText editText, IEntry entry)
		{
			bool isEditable = !entry.IsReadOnly;

			editText.SetInputType(entry);

			editText.FocusableInTouchMode = isEditable;
			editText.Focusable = isEditable;
		}

		public static void UpdateFont(this AppCompatEditText editText, IEntry entry, IFontManager fontManager)
		{
			var font = entry.Font;

			var tf = fontManager.GetTypeface(font);
			editText.Typeface = tf;

			var sp = fontManager.GetScaledPixel(font);
			editText.SetTextSize(ComplexUnitType.Sp, sp);
		}
	}
}
