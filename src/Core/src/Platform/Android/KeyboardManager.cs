using System;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AView = Android.Views.View;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
{
	internal static class KeyboardManager
	{
		static AView GetInputView(this AView inputView)
		{
			if (inputView is WrapperView wv && wv.ChildCount > 0 && wv.GetChildAt(0) is AView view)
				inputView = view;

			if (inputView is SearchView searchView)
				inputView = searchView.GetFirstChildOfType<EditText>() ?? inputView;

			return inputView;
		}

		internal static bool HideKeyboard(this AView inputView)
		{
			inputView = inputView.GetInputView();

			if (inputView?.Context is null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be hidden.");

			using (var inputMethodManager = inputView.Context.GetSystemService(Context.InputMethodService) as InputMethodManager)
			{
				var windowToken = inputView.WindowToken;
				if (windowToken is not null && inputMethodManager is not null)
					return inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
			}

			return false;
		}

		internal static bool ShowKeyboard(this TextView inputView)
		{
			if (inputView?.Context is null)
				throw new ArgumentNullException(nameof(inputView) + " must be set before the keyboard can be shown.");

			using (var inputMethodManager = inputView.Context.GetSystemService(Context.InputMethodService) as InputMethodManager)
			{
				// The zero value for the second parameter comes from 
				// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
				// Apparently there's no named value for zero in this case
				return inputMethodManager?.ShowSoftInput(inputView, 0) == true;
			}
		}

		internal static bool ShowKeyboard(this AView view)
		{
			if (view.GetInputView() is TextView textView)
				return textView.ShowKeyboard();

			return false;
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
			var rootView = view.Context?.GetActivity()?.Window?.DecorView;

			if (rootView is null)
				return false;

			var insets = ViewCompat.GetRootWindowInsets(rootView);
			if (insets is null)
				return false;

			var size = insets.GetInsets(WindowInsetsCompat.Type.Ime());
			var isVisible = insets.IsVisible(WindowInsetsCompat.Type.Ime());
			var hasSize = size.Top > 0 || size.Bottom > 0;

			return hasSize || isVisible;
		}
	}
}