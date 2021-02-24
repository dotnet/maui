using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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

		protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, ARect previouslyFocusedRect)
		{
			Window window = Context.GetActivity().Window;
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