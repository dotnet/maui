using System;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiAppCompatEditText : AppCompatEditText
	{
		/// <summary>
		/// Occurs when the selection within the text changes.
		/// </summary>
		public event EventHandler? SelectionChanged;

		/// <summary>
		/// Occurs when the back button is pressed while the soft keyboard is visible.
		/// This event fires before the keyboard is dismissed, matching Xamarin.Forms behavior.
		/// See: https://github.com/dotnet/maui/issues/21013
		/// </summary>
		public event EventHandler? OnKeyboardBackPressed;

		public MauiAppCompatEditText(Context context) : base(context)
		{
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);

			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Intercepts key events before they reach the Input Method Editor (IME).
		/// When <see cref="Keycode.Back"/> is pressed while the soft keyboard is visible,
		/// this method hides the keyboard and raises <see cref="OnKeyboardBackPressed"/>,
		/// matching the Xamarin.Forms FormsEditText behavior.
		/// See: https://github.com/dotnet/maui/issues/21013
		/// </summary>
		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent? e)
		{
			if (keyCode != Keycode.Back || e?.Action != KeyEventActions.Down)
			{
				return base.OnKeyPreIme(keyCode, e!);
			}

			this.HideSoftInput();

			OnKeyboardBackPressed?.Invoke(this, EventArgs.Empty);
			return true;
		}
	}
}
