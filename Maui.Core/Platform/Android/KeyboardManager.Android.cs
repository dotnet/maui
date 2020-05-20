using Android.Content;
using Android.OS;
using Android.Views.InputMethods;
using Android.Widget;
using AView = Android.Views.View;

namespace System.Maui
{
	internal static class KeyboardManager
	{
		internal static void HideKeyboard(this AView inputView, bool overrideValidation = false)
		{
			if (inputView == null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be hidden.");

			using (var inputMethodManager = (InputMethodManager)inputView.Context.GetSystemService(Context.InputMethodService))
			{
				if (!overrideValidation && !(inputView is EditText || inputView is TextView || inputView is SearchView))
					throw new ArgumentException("inputView should be of type EditText, SearchView, or TextView");

				IBinder windowToken = inputView.WindowToken;
				if (windowToken != null && inputMethodManager != null)
					inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
			}
		}
	}
}