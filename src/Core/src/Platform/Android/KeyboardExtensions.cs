using System;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views.InputMethods;
using Android.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static class KeyboardExtensions
	{
		public static InputTypes ToInputType(this Keyboard self)
		{
			var result = new InputTypes();

			// ClassText:																						!autocaps, spellcheck, suggestions 
			// TextFlagNoSuggestions:																			!autocaps, !spellcheck, !suggestions
			// InputTypes.ClassText | InputTypes.TextFlagCapSentences											 autocaps,	spellcheck,  suggestions
			// InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagNoSuggestions;		 autocaps, !spellcheck, !suggestions

			if (self == Keyboard.Default)
				result = InputTypes.ClassText | InputTypes.TextVariationNormal;
			else if (self == Keyboard.Chat)
				result = InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagNoSuggestions;
			else if (self == Keyboard.Email)
				result = InputTypes.ClassText | InputTypes.TextVariationEmailAddress;
			else if (self == Keyboard.Numeric)
				result = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned;
			else if (self == Keyboard.Telephone)
				result = InputTypes.ClassPhone;
			else if (self == Keyboard.Text)
				result = InputTypes.ClassText | InputTypes.TextFlagCapSentences;
			else if (self == Keyboard.Url)
				result = InputTypes.ClassText | InputTypes.TextVariationUri;
			else if (self is CustomKeyboard custom)
			{
				var capitalizedSentenceEnabled = (custom.Flags & KeyboardFlags.CapitalizeSentence) == KeyboardFlags.CapitalizeSentence;
				var capitalizedWordsEnabled = (custom.Flags & KeyboardFlags.CapitalizeWord) == KeyboardFlags.CapitalizeWord;
				var capitalizedCharacterEnabled = (custom.Flags & KeyboardFlags.CapitalizeCharacter) == KeyboardFlags.CapitalizeCharacter;

				var spellcheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) == KeyboardFlags.Spellcheck;
				var suggestionsEnabled = (custom.Flags & KeyboardFlags.Suggestions) == KeyboardFlags.Suggestions;

				if (!capitalizedSentenceEnabled && !spellcheckEnabled && !suggestionsEnabled)
					result = InputTypes.ClassText | InputTypes.TextFlagNoSuggestions;

				if (!capitalizedSentenceEnabled && !spellcheckEnabled && suggestionsEnabled)
				{
					// Due to the nature of android, TextFlagAutoCorrect includes Spellcheck

					System.Diagnostics.Debug.WriteLine("On Android, KeyboardFlags.Suggestions enables KeyboardFlags.Spellcheck as well due to a platform limitation.");
					result = InputTypes.ClassText | InputTypes.TextFlagAutoCorrect;
				}
				if (!capitalizedSentenceEnabled && spellcheckEnabled && !suggestionsEnabled)
					result = InputTypes.ClassText | InputTypes.TextFlagAutoComplete;

				if (!capitalizedSentenceEnabled && spellcheckEnabled && suggestionsEnabled)
					result = InputTypes.ClassText | InputTypes.TextFlagAutoCorrect;

				if (capitalizedSentenceEnabled && !spellcheckEnabled && !suggestionsEnabled)
					result = InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagNoSuggestions;

				if (capitalizedSentenceEnabled && !spellcheckEnabled && suggestionsEnabled)
				{
					// Due to the nature of android, TextFlagAutoCorrect includes Spellcheck
					System.Diagnostics.Debug.WriteLine("On Android, KeyboardFlags.Suggestions enables KeyboardFlags.Spellcheck as well due to a platform limitation.");
					result = InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagAutoCorrect;
				}

				if (capitalizedSentenceEnabled && spellcheckEnabled && !suggestionsEnabled)
					result = InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagAutoComplete;

				if (capitalizedSentenceEnabled && spellcheckEnabled && suggestionsEnabled)
					result = InputTypes.ClassText | InputTypes.TextFlagCapSentences | InputTypes.TextFlagAutoCorrect;

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

		public static void HideKeyboard(this AView inputView, bool overrideValidation = false)
		{
			if (inputView == null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be hidden.");

			using (var inputMethodManager = (InputMethodManager?)inputView.Context?.GetSystemService(Context.InputMethodService))
			{
				if (!overrideValidation && !(inputView is EditText || inputView is TextView || inputView is SearchView))
					throw new ArgumentException("InputView should be of type EditText, SearchView, or TextView");

				IBinder? windowToken = inputView.WindowToken;

				if (windowToken != null && inputMethodManager != null)
					inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
			}
		}

		internal static void ShowKeyboard(this TextView inputView)
		{
			if (inputView == null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be shown.");

			using (var inputMethodManager = (InputMethodManager?)inputView.Context?.GetSystemService(Context.InputMethodService))
			{
				// The zero value for the second parameter comes from 
				// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
				// Apparently there's no named value for zero in this case
				inputMethodManager?.ShowSoftInput(inputView, 0);
			}
		}

		internal static void ShowKeyboard(this SearchView searchView)
		{
			if (searchView == null)
			{
				throw new ArgumentNullException(nameof(searchView));
			}

			// Dig into the SearchView and find the actual TextView that we want to show keyboard input for
			int? searchViewTextViewId = searchView.Resources?.GetIdentifier("android:id/search_src_text", null, null);

			if(searchViewTextViewId == 0)
			{
				// Cannot find the resource Id; nothing else to do
				return;
			}

			if (searchViewTextViewId.HasValue)
			{
				var textView = searchView.FindViewById(searchViewTextViewId.Value);

				if (textView == null)
				{
					// Cannot find the TextView; nothing else to do
					return;
				}

				using (var inputMethodManager = (InputMethodManager?)searchView.Context?.GetSystemService(Context.InputMethodService))
				{
					// The zero value for the second parameter comes from 
					// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
					// Apparently there's no named value for zero in this case
					inputMethodManager?.ShowSoftInput(textView, 0);
				}
			}
		}

		internal static void ShowKeyboard(this AView view)
		{
			switch (view)
			{
				case SearchView searchView:
					searchView.ShowKeyboard();
					break;
				case TextView textView:
					textView.ShowKeyboard();
					break;
			}
		}

		internal static void PostShowKeyboard(this AView view)
		{
			void ShowKeyboard()
			{
				// Since we're posting this on the queue, it's possible something else will have disposed of the view
				// by the time the looper is running this, so we have to verify that the view is still usable
				if (view.IsDisposed())
				{
					return;
				}

				view.ShowKeyboard();
			};

			view.Post(ShowKeyboard);
		}
	}
}