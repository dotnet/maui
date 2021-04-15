using UIKit;

namespace Microsoft.Maui
{
	public static class KeyboardExtensions
	{
		public static void ApplyKeyboard(this IUITextInput textInput, Keyboard keyboard)
		{
			if (textInput is IUITextInputTraits traits)
				ApplyKeyboard(traits, keyboard);
		}

		public static void ApplyKeyboard(this IUITextInputTraits textInput, Keyboard keyboard)
		{
			textInput.AutocapitalizationType = UITextAutocapitalizationType.None;
			textInput.AutocorrectionType = UITextAutocorrectionType.No;
			textInput.SpellCheckingType = UITextSpellCheckingType.No;
			textInput.KeyboardType = UIKeyboardType.Default;

			if (keyboard == Keyboard.Default)
			{
				textInput.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
				textInput.AutocorrectionType = UITextAutocorrectionType.Default;
				textInput.SpellCheckingType = UITextSpellCheckingType.Default;
			}
			else if (keyboard == Keyboard.Chat)
			{
				textInput.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
				textInput.AutocorrectionType = UITextAutocorrectionType.Yes;
			}
			else if (keyboard == Keyboard.Email)
				textInput.KeyboardType = UIKeyboardType.EmailAddress;
			else if (keyboard == Keyboard.Numeric)
				textInput.KeyboardType = UIKeyboardType.DecimalPad;
			else if (keyboard == Keyboard.Telephone)
				textInput.KeyboardType = UIKeyboardType.PhonePad;
			else if (keyboard == Keyboard.Text)
			{
				textInput.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
				textInput.AutocorrectionType = UITextAutocorrectionType.Yes;
				textInput.SpellCheckingType = UITextSpellCheckingType.Yes;
			}
			else if (keyboard == Keyboard.Url)
				textInput.KeyboardType = UIKeyboardType.Url;
			else if (keyboard is CustomKeyboard)
			{
				var custom = (CustomKeyboard)keyboard;

				var capitalizedSentenceEnabled = (custom.Flags & KeyboardFlags.CapitalizeSentence) == KeyboardFlags.CapitalizeSentence;
				var capitalizedWordsEnabled = (custom.Flags & KeyboardFlags.CapitalizeWord) == KeyboardFlags.CapitalizeWord;
				var capitalizedCharacterEnabled = (custom.Flags & KeyboardFlags.CapitalizeCharacter) == KeyboardFlags.CapitalizeCharacter;
				var capitalizedNone = (custom.Flags & KeyboardFlags.None) == KeyboardFlags.None;

				var spellcheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) == KeyboardFlags.Spellcheck;
				var suggestionsEnabled = (custom.Flags & KeyboardFlags.Suggestions) == KeyboardFlags.Suggestions;


				UITextAutocapitalizationType capSettings = UITextAutocapitalizationType.None;

				// Sentence being first ensures that the behavior of ALL is backwards compatible
				if (capitalizedSentenceEnabled)
					capSettings = UITextAutocapitalizationType.Sentences;
				else if (capitalizedWordsEnabled)
					capSettings = UITextAutocapitalizationType.Words;
				else if (capitalizedCharacterEnabled)
					capSettings = UITextAutocapitalizationType.AllCharacters;
				else if (capitalizedNone)
					capSettings = UITextAutocapitalizationType.None;

				textInput.AutocapitalizationType = capSettings;
				textInput.AutocorrectionType = suggestionsEnabled ? UITextAutocorrectionType.Yes : UITextAutocorrectionType.No;
				textInput.SpellCheckingType = spellcheckEnabled ? UITextSpellCheckingType.Yes : UITextSpellCheckingType.No;
			}
		}

		public static UIReturnKeyType ToUIReturnKeyType(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return UIReturnKeyType.Go;
				case ReturnType.Next:
					return UIReturnKeyType.Next;
				case ReturnType.Send:
					return UIReturnKeyType.Send;
				case ReturnType.Search:
					return UIReturnKeyType.Search;
				case ReturnType.Done:
					return UIReturnKeyType.Done;
				case ReturnType.Default:
					return UIReturnKeyType.Default;
				default:
					throw new System.NotImplementedException($"ReturnType {returnType} not supported");
			}
		}
	}
}