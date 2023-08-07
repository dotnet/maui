using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Input;
using Windows.UI.ViewManagement;

namespace Microsoft.Maui.Platform
{
	public static partial class KeyboardExtensions
	{
		public static InputScopeName ToInputScopeName(this Keyboard self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			var name = new InputScopeName();

			if (self == Keyboard.Default)
			{
				name.NameValue = InputScopeNameValue.Default;
			}
			else if (self == Keyboard.Chat)
			{
				name.NameValue = InputScopeNameValue.Chat;
			}
			else if (self == Keyboard.Email)
			{
				name.NameValue = InputScopeNameValue.EmailSmtpAddress;
			}
			else if (self == Keyboard.Numeric)
			{
				name.NameValue = InputScopeNameValue.Number;
			}
			else if (self == Keyboard.Telephone)
			{
				name.NameValue = InputScopeNameValue.TelephoneNumber;
			}
			else if (self == Keyboard.Text)
			{
				name.NameValue = InputScopeNameValue.Default;
			}
			else if (self == Keyboard.Url)
			{
				name.NameValue = InputScopeNameValue.Url;
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
				throw new ArgumentNullException("self");

			var result = new InputScope();
			var name = new InputScopeName();
			if (self == Keyboard.Default)
			{
				name.NameValue = InputScopeNameValue.Default;
			}
			else if (self == Keyboard.Chat)
			{
				name.NameValue = InputScopeNameValue.Chat;
			}
			else if (self == Keyboard.Email)
			{
				name.NameValue = InputScopeNameValue.EmailSmtpAddress;
			}
			else if (self == Keyboard.Numeric)
			{
				name.NameValue = InputScopeNameValue.Number;
			}
			else if (self == Keyboard.Telephone)
			{
				name.NameValue = InputScopeNameValue.TelephoneNumber;
			}
			else if (self == Keyboard.Text)
			{
				name.NameValue = InputScopeNameValue.Default;
			}
			else if (self == Keyboard.Url)
			{
				name.NameValue = InputScopeNameValue.Url;
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

			result.Names.Add(name);
			return result;
		}

		internal static bool HideKeyboard(this FrameworkElement element)
		{
			if (TryGetInputPane(out var inputPane))
			{
				return inputPane.TryHide();
			}

			return false;
		}

		internal static bool ShowKeyboard(this FrameworkElement element)
		{
			if (TryGetInputPane(out var inputPane))
			{
				return inputPane.TryShow();
			}

			return false;
		}

		internal static bool IsSoftKeyboardShowing(this FrameworkElement element)
		{
			if (TryGetInputPane(out var inputPane))
			{
				return inputPane.Visible;
			}

			return false;
		}

		internal static bool TryGetInputPane([NotNullWhen(true)] out InputPane? inputPane)
		{
			var handleId = Process.GetCurrentProcess().MainWindowHandle;
			if (handleId == IntPtr.Zero)
			{
				inputPane = null;

				return false;
			}

			inputPane = InputPaneInterop.GetForWindow(handleId);
			return true;
		}
	}
}