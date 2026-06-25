using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Platform
{
	public static class KeyboardExtensions
	{
		// Canonical mapping from built-in Keyboard types to their InputScopeNameValue entries.
		// Returns multiple values for Date/Time so ToInputScope can emit a richer InputScope
		// without using invalid bitwise-OR combinations on a non-[Flags] enum.
		// Returns null for CustomKeyboard (callers handle those separately).
		static InputScopeNameValue[]? GetInputScopeNameValues(Keyboard keyboard)
		{
			if (keyboard == Keyboard.Default || keyboard == Keyboard.Text)
			{
				return new[] { InputScopeNameValue.Default };
			}
			else if (keyboard == Keyboard.Chat)
			{
				return new[] { InputScopeNameValue.Chat };
			}
			else if (keyboard == Keyboard.Email)
			{
				return new[] { InputScopeNameValue.EmailSmtpAddress };
			}
			else if (keyboard == Keyboard.Numeric)
			{
				return new[] { InputScopeNameValue.Number };
			}
			else if (keyboard == Keyboard.Telephone)
			{
				return new[] { InputScopeNameValue.TelephoneNumber };
			}
			else if (keyboard == Keyboard.Url)
			{
				return new[] { InputScopeNameValue.Url };
			}
			else if (keyboard == Keyboard.Password)
			{
				return new[] { InputScopeNameValue.Password };
			}
			else if (keyboard == Keyboard.Date)
			{
				return new[] { InputScopeNameValue.DateDayNumber, InputScopeNameValue.DateMonthNumber, InputScopeNameValue.DateYear };
			}
			else if (keyboard == Keyboard.Time)
			{
				return new[] { InputScopeNameValue.TimeHour, InputScopeNameValue.TimeMinutesOrSeconds };
			}

			return null;
		}

		public static InputScopeName ToInputScopeName(this Keyboard self)
		{
			if (self == null)
			{
				throw new ArgumentNullException(nameof(self));
			}

			var name = new InputScopeName();
			var values = GetInputScopeNameValues(self);
			if (values is not null)
			{
				name.NameValue = values[0];
			}
			else
			{
				var custom = (CustomKeyboard)self;
				var capitalizedSentenceEnabled = (custom.Flags & KeyboardFlags.CapitalizeSentence) == KeyboardFlags.CapitalizeSentence;
				var capitalizedWordsEnabled = (custom.Flags & KeyboardFlags.CapitalizeWord) == KeyboardFlags.CapitalizeWord;
				var capitalizedCharacterEnabled = (custom.Flags & KeyboardFlags.CapitalizeCharacter) == KeyboardFlags.CapitalizeCharacter;

				var spellcheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) == KeyboardFlags.Spellcheck;
				var suggestionsEnabled = (custom.Flags & KeyboardFlags.Suggestions) == KeyboardFlags.Suggestions;

				InputScopeNameValue nameValue = InputScopeNameValue.Default;

				if (capitalizedSentenceEnabled)
				{
					if (!spellcheckEnabled)
					{
						Debug.WriteLine(null, "CapitalizeSentence only works when spell check is enabled");
					}
				}
				else if (capitalizedWordsEnabled)
				{
					if (!spellcheckEnabled)
					{
						Debug.WriteLine(null, "CapitalizeWord only works when spell check is enabled");
					}

					nameValue = InputScopeNameValue.NameOrPhoneNumber;
				}

				if (capitalizedCharacterEnabled)
				{
					Debug.WriteLine(null, "WinUI does not support CapitalizeCharacter");
				}

				name.NameValue = nameValue;
			}

			return name;
		}

		public static InputScope ToInputScope(this Keyboard self)
		{
			if (self == null)
			{
				throw new ArgumentNullException(nameof(self));
			}

			var result = new InputScope();
			var values = GetInputScopeNameValues(self);
			if (values is not null)
			{
				foreach (var value in values)
				{
					result.Names.Add(new InputScopeName { NameValue = value });
				}
			}
			else
			{
				var name = new InputScopeName();
				var custom = (CustomKeyboard)self;
				var capitalizedSentenceEnabled = (custom.Flags & KeyboardFlags.CapitalizeSentence) == KeyboardFlags.CapitalizeSentence;
				var capitalizedWordsEnabled = (custom.Flags & KeyboardFlags.CapitalizeWord) == KeyboardFlags.CapitalizeWord;
				var capitalizedCharacterEnabled = (custom.Flags & KeyboardFlags.CapitalizeCharacter) == KeyboardFlags.CapitalizeCharacter;

				var spellcheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) == KeyboardFlags.Spellcheck;
				var suggestionsEnabled = (custom.Flags & KeyboardFlags.Suggestions) == KeyboardFlags.Suggestions;

				InputScopeNameValue nameValue = InputScopeNameValue.Default;

				if (capitalizedSentenceEnabled)
				{
					if (!spellcheckEnabled)
					{
						Debug.WriteLine(null, "CapitalizeSentence only works when spell check is enabled");
					}
				}
				else if (capitalizedWordsEnabled)
				{
					if (!spellcheckEnabled)
					{
						Debug.WriteLine(null, "CapitalizeWord only works when spell check is enabled");
					}

					nameValue = InputScopeNameValue.NameOrPhoneNumber;
				}

				if (capitalizedCharacterEnabled)
				{
					Debug.WriteLine(null, "WinUI does not support CapitalizeCharacter");
				}

				name.NameValue = nameValue;
				result.Names.Add(name);
			}

			return result;
		}
	}
}
