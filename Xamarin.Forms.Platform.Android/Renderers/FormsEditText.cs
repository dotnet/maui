using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Graphics.Drawable;

namespace Xamarin.Forms.Platform.Android
{
	public class FormsEditText : EditText, IDescendantFocusToggler
	{
		DescendantFocusToggler _descendantFocusToggler;

		internal FormsEditText(Context context) : base(context)
		{
			DrawableCompat.Wrap(Background);
		}

		bool IDescendantFocusToggler.RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus)
		{
			_descendantFocusToggler = _descendantFocusToggler ?? new DescendantFocusToggler();

			return _descendantFocusToggler.RequestFocus(control, baseRequestFocus);
		}

		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
		{
			if (keyCode != Keycode.Back || e.Action != KeyEventActions.Down)
			{
				return base.OnKeyPreIme(keyCode, e);
			}

			this.HideKeyboard();

			OnKeyboardBackPressed?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public override bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			return (this as IDescendantFocusToggler).RequestFocus(this, () => base.RequestFocus(direction, previouslyFocusedRect));
		}

		internal event EventHandler OnKeyboardBackPressed;
	}

	[Obsolete("EntryEditText is obsolete as of version 2.4.0. Please use Xamarin.Forms.Platform.Android.FormsEditText instead.")]
	public class EntryEditText : FormsEditText
	{
		internal EntryEditText(Context context) : base(context)
		{
		}
	}

	[Obsolete("EditorEditText is obsolete as of version 2.4.0. Please use Xamarin.Forms.Platform.Android.FormsEditText instead.")]
	public class EditorEditText : FormsEditText
	{
		internal EditorEditText(Context context) : base(context)
		{
		}
	}
}