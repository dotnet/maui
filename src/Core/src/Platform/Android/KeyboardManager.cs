using System;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.View;
using AView = Android.Views.View;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
{
	internal static class KeyboardManager
	{
		internal static void HideKeyboard(this AView inputView)
		{
			if (inputView?.Context is null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be hidden.");

			var focusedView = inputView.Context?.GetActivity()?.Window?.CurrentFocus;
			AView tokenView = focusedView ?? inputView;
			var context = tokenView.Context;

			if (context is null)
				return;

			using (var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager)
			{
				var windowToken = tokenView.WindowToken;
				if (windowToken is not null && inputMethodManager is not null)
					inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
			}
		}

		internal static void ShowKeyboard(this TextView inputView)
		{
			if (inputView?.Context is null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be shown.");

			using (var inputMethodManager = inputView.Context.GetSystemService(Context.InputMethodService) as InputMethodManager)
			{
				// The zero value for the second parameter comes from 
				// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
				// Apparently there's no named value for zero in this case
				inputMethodManager?.ShowSoftInput(inputView, 0);
			}
		}

		internal static void ShowKeyboard(this SearchView searchView)
		{
			if (searchView?.Context is null || searchView?.Resources is null)
			{
				throw new ArgumentNullException(nameof(searchView));
			}

			var queryEditor = searchView.GetFirstChildOfType<EditText>();

			if (queryEditor is null)
				return;

			using (var inputMethodManager = (InputMethodManager)searchView.Context.GetSystemService(Context.InputMethodService)!)
			{
				// The zero value for the second parameter comes from 
				// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
				// Apparently there's no named value for zero in this case
				inputMethodManager?.ShowSoftInput(queryEditor, 0);
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

		public static bool IsSoftKeyboardVisible(this AView view)
		{
			var insets = ViewCompat.GetRootWindowInsets(view);
			if (insets is null)
				return false;

			var result = insets.IsVisible(WindowInsetsCompat.Type.Ime());
			return result;
		}
	}
}