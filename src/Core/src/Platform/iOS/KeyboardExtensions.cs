using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
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
			textInput.SetAutocapitalizationType(UITextAutocapitalizationType.None);
			textInput.SetAutocorrectionType(UITextAutocorrectionType.No);
			textInput.SetSpellCheckingType(UITextSpellCheckingType.No);
			textInput.SetKeyboardType(UIKeyboardType.Default);

			if (keyboard == Keyboard.Chat || keyboard == Keyboard.Default || keyboard == Keyboard.Text)
			{
				// Since IsSpellCheckEnabled and IsTextPredictionEnabled are true by default
				// Autocorrection and Spellchecking will be forced to be true
				// This makes chat, default, and text keyboards the same thing on iOS
				textInput.SetAutocapitalizationType(UITextAutocapitalizationType.Sentences);
				textInput.SetAutocorrectionType(UITextAutocorrectionType.Yes);
				textInput.SetSpellCheckingType(UITextSpellCheckingType.Yes);
			}
			else if (keyboard == Keyboard.Email)
				textInput.SetKeyboardType(UIKeyboardType.EmailAddress);
			else if (keyboard == Keyboard.Numeric)
				textInput.SetKeyboardType(UIKeyboardType.DecimalPad);
			else if (keyboard == Keyboard.Telephone)
				textInput.SetKeyboardType(UIKeyboardType.PhonePad);
			else if (keyboard == Keyboard.Url)
				textInput.SetKeyboardType(UIKeyboardType.Url);
			else if (keyboard == Keyboard.Date || keyboard == Keyboard.Time)
			{
				textInput.SetTextContentType(UITextContentType.DateTime);
				textInput.SetKeyboardType(UIKeyboardType.NumbersAndPunctuation);
			}
			else if (keyboard == Keyboard.Password)
			{
				textInput.SetKeyboardType(UIKeyboardType.Default);
				textInput.SetSecureTextEntry(true);
			}
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

				textInput.SetAutocapitalizationType(capSettings);
				textInput.SetAutocorrectionType(suggestionsEnabled ? UITextAutocorrectionType.Yes : UITextAutocorrectionType.No);
				textInput.SetSpellCheckingType(spellcheckEnabled ? UITextSpellCheckingType.Yes : UITextSpellCheckingType.No);
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