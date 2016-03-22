using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class EntryCellEditText : EditText
	{
		SoftInput _startingMode;

		public EntryCellEditText(Context context) : base(context)
		{
		}

		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
			{
				EventHandler handler = BackButtonPressed;
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
			return base.OnKeyPreIme(keyCode, e);
		}

		protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			Window window = ((Activity)Context).Window;
			if (gainFocus)
			{
				_startingMode = window.Attributes.SoftInputMode;
				window.SetSoftInputMode(SoftInput.AdjustPan);
			}
			else
				window.SetSoftInputMode(_startingMode);

			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
		}

		internal event EventHandler BackButtonPressed;
	}
}