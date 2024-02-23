using System.Xml.Schema;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.View;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static partial class KeyboardExtensions
	{
		public static InputTypes ToInputType(this Keyboard self)
		{
			var result = new InputTypes();

			if (self == Keyboard.Default)
				result = InputTypes.ClassText | InputTypes.TextVariationNormal;
			else if (self == Keyboard.Chat || self == Keyboard.Text)
				result = InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagAutoComplete;
			else if (self == Keyboard.Email)
				result = InputTypes.ClassText | InputTypes.TextVariationEmailAddress;
			else if (self == Keyboard.Numeric)
				result = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned;
			else if (self == Keyboard.Telephone)
				result = InputTypes.ClassPhone;
			else if (self == Keyboard.Url)
				result = InputTypes.ClassText | InputTypes.TextVariationUri;
			else if (self == Keyboard.Date)
				result = InputTypes.ClassDatetime | InputTypes.DatetimeVariationNormal;
			else if (self == Keyboard.Time)
				result = InputTypes.ClassDatetime | InputTypes.DatetimeVariationTime;
			else if (self == Keyboard.Password)
				result = InputTypes.ClassText | InputTypes.TextVariationPassword;
			else if (self is CustomKeyboard custom)
			{
				var capitalizedSentenceEnabled = (custom.Flags & KeyboardFlags.CapitalizeSentence) == KeyboardFlags.CapitalizeSentence;
				var capitalizedWordsEnabled = (custom.Flags & KeyboardFlags.CapitalizeWord) == KeyboardFlags.CapitalizeWord;
				var capitalizedCharacterEnabled = (custom.Flags & KeyboardFlags.CapitalizeCharacter) == KeyboardFlags.CapitalizeCharacter;

				var spellcheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) == KeyboardFlags.Spellcheck;
				var suggestionsEnabled = (custom.Flags & KeyboardFlags.Suggestions) == KeyboardFlags.Suggestions;

				result |= InputTypes.ClassText;

				if (capitalizedSentenceEnabled)
					result |= InputTypes.TextFlagCapSentences;

				if (!spellcheckEnabled)
					result |= InputTypes.TextFlagNoSuggestions;

				if (suggestionsEnabled)
					result |= InputTypes.TextFlagAutoCorrect;

				// All existed before these settings. This ensures these changes are backwards compatible
				// without this check TextFlagCapCharacters would win
				if (custom.Flags != KeyboardFlags.All)
				{
					if (capitalizedWordsEnabled)
						result |= InputTypes.TextFlagCapWords;

					if (capitalizedCharacterEnabled)
						result |= InputTypes.TextFlagCapCharacters;
				}
			}
			else
			{
				// Should never happens
				result = InputTypes.TextVariationNormal;
			}

			return result;
		}
	}
}
