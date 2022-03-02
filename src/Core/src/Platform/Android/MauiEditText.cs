using System;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiEditText : AppCompatEditText, IMauiEditText
	{
		event EventHandler? _onKeyboardBackPressed;
		event EventHandler? IMauiEditText.OnKeyboardBackPressed
		{
			add => _onKeyboardBackPressed += value;
			remove => _onKeyboardBackPressed -= value;
		}

		public MauiEditText(Context context) : base(context)
		{

		}

		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent? e)
		{
			if (keyCode != Keycode.Back || e?.Action != KeyEventActions.Down)
			{
				return base.OnKeyPreIme(keyCode, e);
			}

			this.HideKeyboard();

			_onKeyboardBackPressed?.Invoke(this, EventArgs.Empty);

			return true;
		}
	}
}