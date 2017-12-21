using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	internal static class KeyboardExtensions
	{
		public static InputScope ToInputScope(this Keyboard self)
		{
			var result = new InputScope();
			var name = new InputScopeName();
			if (self == Keyboard.Default)
				name.NameValue = InputScopeNameValue.Default;
			else if (self == Keyboard.Chat)
				name.NameValue = InputScopeNameValue.Default;
			else if (self == Keyboard.Email)
				name.NameValue = InputScopeNameValue.EmailUserName;
			else if (self == Keyboard.Numeric)
				name.NameValue = InputScopeNameValue.Number;
			else if (self == Keyboard.Telephone)
				name.NameValue = InputScopeNameValue.TelephoneNumber;
			else if (self == Keyboard.Text)
				name.NameValue = InputScopeNameValue.Default;
			else if (self == Keyboard.Url)
				name.NameValue = InputScopeNameValue.Url;
			else if (self is CustomKeyboard)
			{
				var custom = (CustomKeyboard)self;
				bool capitalizedSentenceEnabled = (custom.Flags & KeyboardFlags.CapitalizeSentence) == KeyboardFlags.CapitalizeSentence;
				bool spellcheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) == KeyboardFlags.Spellcheck;
				bool suggestionsEnabled = (custom.Flags & KeyboardFlags.Suggestions) == KeyboardFlags.Suggestions;

				if (!capitalizedSentenceEnabled && !spellcheckEnabled && !suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Default;
				if (!capitalizedSentenceEnabled && !spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Default;
				if (!capitalizedSentenceEnabled && spellcheckEnabled && !suggestionsEnabled)
				{
					Debug.WriteLine("Keyboard: Suggestions cannot be disabled in Windows Phone if spellcheck is enabled");
					name.NameValue = InputScopeNameValue.Default;
				}
				if (!capitalizedSentenceEnabled && spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Default;
				if (capitalizedSentenceEnabled && !spellcheckEnabled && !suggestionsEnabled)
				{
					Debug.WriteLine("Keyboard: Suggestions cannot be disabled in Windows Phone if auto Capitalization is enabled");
					name.NameValue = InputScopeNameValue.Default;
				}
				if (capitalizedSentenceEnabled && !spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Default;
				if (capitalizedSentenceEnabled && spellcheckEnabled && !suggestionsEnabled)
				{
					Debug.WriteLine("Keyboard: Suggestions cannot be disabled in Windows Phone if spellcheck is enabled");
					name.NameValue = InputScopeNameValue.Default;
				}
				if (capitalizedSentenceEnabled && spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Default;
			}
			else
			{
				// Should never happens
				name.NameValue = InputScopeNameValue.Default;
			}
			result.Names.Add(name);
			return result;
		}
	}
}
