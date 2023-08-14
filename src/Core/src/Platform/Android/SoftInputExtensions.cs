using Android.Content;
using Android.Text;
using Android.Views.InputMethods;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static partial class SoftInputExtensions
	{
		internal static bool HideSoftInput(this AView inputView)
		{
			AView tokenView = inputView;

			using var inputMethodManager = (InputMethodManager?)tokenView.Context?.GetSystemService(Context.InputMethodService);
			var windowToken = tokenView.WindowToken;

			if (windowToken is not null && inputMethodManager is not null)
			{
				return inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
			}

			return false;
		}

		internal static bool ShowSoftInput(this TextView inputView)
		{
			using var inputMethodManager = (InputMethodManager?)inputView.Context?.GetSystemService(Context.InputMethodService);

			// The zero value for the second parameter comes from 
			// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
			// Apparently there's no named value for zero in this case
			return inputMethodManager?.ShowSoftInput(inputView, 0) is true;
		}

		internal static bool ShowSoftInput(this AView view) => view switch
		{
			TextView textView => textView.ShowSoftInput(),
			ViewGroup viewGroup => viewGroup.GetFirstChildOfType<TextView>()?.ShowSoftInput() is true,
			_ => false,
		};

		internal static bool IsSoftInputShowing(this AView view)
		{
			var insets = ViewCompat.GetRootWindowInsets(view);
			if (insets is null)
			{
				return false;
			}

			var result = insets.IsVisible(WindowInsetsCompat.Type.Ime());
			return result;
		}

		internal static void PostShowSoftInput(this AView view)
		{
			void ShowSoftInput()
			{
				// Since we're posting this on the queue, it's possible something else will have disposed of the view
				// by the time the looper is running this, so we have to verify that the view is still usable
				if (view.IsDisposed())
				{
					return;
				}

				view.ShowSoftInput();
			};

			view.Post(ShowSoftInput);
		}
	}
}