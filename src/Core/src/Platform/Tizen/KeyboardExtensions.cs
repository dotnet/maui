using ElmSharp;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TKeyboard = Tizen.UIExtensions.Common.Keyboard;

namespace Microsoft.Maui.Platform
{
	public static class KeyboardExtensions
	{
		public static TKeyboard ToPlatform(this Keyboard keyboard)
		{
			if (keyboard == Keyboard.Numeric)
			{
				return TKeyboard.Numeric;
			}
			else if (keyboard == Keyboard.Telephone)
			{
				return TKeyboard.PhoneNumber;
			}
			else if (keyboard == Keyboard.Email)
			{
				return TKeyboard.Email;
			}
			else if (keyboard == Keyboard.Url)
			{
				return TKeyboard.Url;
			}
			else
			{
				return TKeyboard.Normal;
			}
		}

		public static AutoCapital ToAutoCapital(this KeyboardFlags keyboardFlags)
		{
			if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				return AutoCapital.Sentence;
			}
			else if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				return AutoCapital.Word;
			}
			else if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeCharacter))
			{
				return AutoCapital.All;
			}
			else
			{
				return AutoCapital.None;
			}
		}

		public static InputHints ToInputHints(this Keyboard keyboard, bool isSpellCheckEnabled, bool isTextPredictionEnabled)
		{
			if (keyboard is CustomKeyboard customKeyboard)
			{
				return customKeyboard.Flags.HasFlag(KeyboardFlags.Suggestions) || customKeyboard.Flags.HasFlag(KeyboardFlags.Spellcheck) ? InputHints.AutoComplete : InputHints.None;
			}
			return isSpellCheckEnabled && isTextPredictionEnabled ? InputHints.AutoComplete : InputHints.None;
		}

		public static void UpdateKeyboard(this TEntry control, Keyboard keyboard, bool isSpellCheckEnabled, bool isTextPredictionEnabled)
		{
			control.Keyboard = keyboard.ToPlatform();
			if (keyboard is CustomKeyboard customKeyboard)
			{
				control.AutoCapital = customKeyboard.Flags.ToAutoCapital();
			}
			else
			{
				control.AutoCapital = AutoCapital.None;
			}
			control.InputHint = keyboard.ToInputHints(isSpellCheckEnabled, isTextPredictionEnabled);
		}
	}
}