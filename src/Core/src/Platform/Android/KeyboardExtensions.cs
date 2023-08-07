using Android.Content;
using Android.Text;
using Android.Views.InputMethods;
using Android.Views;
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

		internal static bool HideKeyboard(this AView inputView)
		{
			var focusedView = inputView.Context?.GetActivity()?.Window?.CurrentFocus;
			AView tokenView = focusedView ?? inputView;

			using var inputMethodManager = (InputMethodManager?)tokenView.Context?.GetSystemService(Context.InputMethodService);
			var windowToken = tokenView.WindowToken;

			if (windowToken is not null && inputMethodManager is not null)
			{
				return inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
			}

			return false;
		}

		internal static bool ShowKeyboard(this TextView inputView)
		{
			using var inputMethodManager = (InputMethodManager?)inputView.Context?.GetSystemService(Context.InputMethodService);

			// The zero value for the second parameter comes from 
			// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
			// Apparently there's no named value for zero in this case
			return inputMethodManager?.ShowSoftInput(inputView, 0) is true;
		}

		internal static bool ShowKeyboard(this AView view) => view switch
		{
			TextView textView => textView.ShowKeyboard(),
			ViewGroup viewGroup => viewGroup.GetFirstChildOfType<TextView>()?.ShowKeyboard() is true,
			_ => false,
		};

		internal static bool IsSoftKeyboardShowing(this AView view)
		{
			var insets = ViewCompat.GetRootWindowInsets(view);
			if (insets is null)
			{
				return false;
			}

			var result = insets.IsVisible(WindowInsetsCompat.Type.Ime());
			return result;
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