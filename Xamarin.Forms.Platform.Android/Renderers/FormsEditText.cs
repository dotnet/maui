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

		public FormsEditText(Context context) : base(context)
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

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);
			SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(selStart, selEnd));
		}

		internal event EventHandler OnKeyboardBackPressed;
		internal event EventHandler<SelectionChangedEventArgs> SelectionChanged;
	}

	public class SelectionChangedEventArgs : EventArgs
	{
		public int Start { get; private set; }
		public int End { get; private set; }

		public SelectionChangedEventArgs(int start, int end)
		{
			Start = start;
			End = end;
		}
	}

	[Obsolete("EntryEditText is obsolete as of version 2.4.0. Please use Xamarin.Forms.Platform.Android.FormsEditText instead.")]
	public class EntryEditText : FormsEditText
	{
		public EntryEditText(Context context) : base(context)
		{
		}
	}

	[Obsolete("EditorEditText is obsolete as of version 2.4.0. Please use Xamarin.Forms.Platform.Android.FormsEditText instead.")]
	public class EditorEditText : FormsEditText
	{
		public EditorEditText(Context context) : base(context)
		{
		}
	}
}