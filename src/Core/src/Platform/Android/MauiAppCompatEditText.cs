using System;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiAppCompatEditText : AppCompatEditText
	{
		public event EventHandler? SelectionChanged;

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
		/// this method hides the keyboard and clears focus, matching the Xamarin.Forms
		/// FormsEditText behavior.
		/// </summary>
		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent? e)
		{
			if (keyCode != Keycode.Back || e?.Action != KeyEventActions.Down)
			{
				return base.OnKeyPreIme(keyCode, e);
			}

			this.HideSoftInput();
			ClearFocus();
			return base.OnKeyPreIme(keyCode, e);
		}
	}
}
