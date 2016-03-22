using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class EditorEditText : EditText, IDescendantFocusToggler
	{
		DescendantFocusToggler _descendantFocusToggler;

		internal EditorEditText(Context context) : base(context)
		{
		}

		bool IDescendantFocusToggler.RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus)
		{
			_descendantFocusToggler = _descendantFocusToggler ?? new DescendantFocusToggler();

			return _descendantFocusToggler.RequestFocus(control, baseRequestFocus);
		}

		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
			{
				EventHandler handler = OnBackKeyboardPressed;
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
			return base.OnKeyPreIme(keyCode, e);
		}

		public override bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			return (this as IDescendantFocusToggler).RequestFocus(this, () => base.RequestFocus(direction, previouslyFocusedRect));
		}

		internal event EventHandler OnBackKeyboardPressed;
	}
}