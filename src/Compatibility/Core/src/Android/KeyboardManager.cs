using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.InputMethods;
using Android.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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

		internal static void ShowKeyboard(this TextView inputView)
		{
			if (inputView == null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be shown.");

			using (var inputMethodManager = (InputMethodManager)inputView.Context.GetSystemService(Context.InputMethodService))
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
			int searchViewTextViewId = searchView.Resources.GetIdentifier("android:id/search_src_text", null, null);

			if (searchViewTextViewId == 0)
			{
				// Cannot find the resource Id; nothing else to do
				return;
			}

			var textView = searchView.FindViewById(searchViewTextViewId);

			if (textView == null)
			{
				// Cannot find the TextView; nothing else to do
				return;
			}

			using (var inputMethodManager = (InputMethodManager)searchView.Context.GetSystemService(Context.InputMethodService))
			{
				// The zero value for the second parameter comes from 
				// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
				// Apparently there's no named value for zero in this case
				inputMethodManager?.ShowSoftInput(textView, 0);
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

			Device.BeginInvokeOnMainThread(ShowKeyboard);
		}
	}
}