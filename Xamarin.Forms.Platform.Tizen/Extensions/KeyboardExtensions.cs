using ElmSharp;
using Xamarin.Forms.Internals;
using EEntry = ElmSharp.Entry;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class KeyboardExtensions
	{
		/// <summary>
		/// Creates an instance of ElmSharp.Keyboard reflecting the provided Xamarin.Forms.Keyboard instance
		/// </summary>
		/// <returns>Keyboard type corresponding to the provided Xamarin.Forms.Keyboard</returns>
		/// <param name="keyboard">The Xamarin.Forms.Keyboard class instance to be converted to ElmSharp.Keyboard.</param>
		public static Native.Keyboard ToNative(this Keyboard keyboard)
		{
			if (keyboard == Keyboard.Numeric)
			{
				return Native.Keyboard.Numeric;
			}
			else if (keyboard == Keyboard.Telephone)
			{
				return Native.Keyboard.PhoneNumber;
			}
			else if (keyboard == Keyboard.Email)
			{
				return Native.Keyboard.Email;
			}
			else if (keyboard == Keyboard.Url)
			{
				return Native.Keyboard.Url;
			}
			else
			{
				return Native.Keyboard.Normal;
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

		public static void UpdateKeyboard(this Native.IEntry control, Keyboard keyboard, bool isSpellCheckEnabled, bool isTextPredictionEnabled)
		{
			control.Keyboard = keyboard.ToNative();
			if (keyboard is CustomKeyboard customKeyboard)
			{
				(control as EEntry).AutoCapital = customKeyboard.Flags.ToAutoCapital();
			}
			else
			{
				(control as EEntry).AutoCapital = AutoCapital.None;
			}
			(control as EEntry).InputHint = keyboard.ToInputHints(isSpellCheckEnabled, isTextPredictionEnabled);
		}
	}
}