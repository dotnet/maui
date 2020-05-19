using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
#if __ANDROID_29__
using AndroidX.Core.Graphics.Drawable;
#else
using Android.Support.V4.Graphics.Drawable;
#endif

namespace Xamarin.Forms.Platform.Android
{
	public class FormsEditText : FormsEditTextBase, IFormsEditText
	{
		public FormsEditText(Context context) : base(context)
		{
		}


		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
		{
			if (keyCode != Keycode.Back || e.Action != KeyEventActions.Down)
			{
				return base.OnKeyPreIme(keyCode, e);
			}

			this.HideKeyboard();

			_onKeyboardBackPressed?.Invoke(this, EventArgs.Empty);
			return true;
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);
			_selectionChanged?.Invoke(this, new SelectionChangedEventArgs(selStart, selEnd));
		}

		event EventHandler _onKeyboardBackPressed;
		event EventHandler IFormsEditText.OnKeyboardBackPressed
		{
			add => _onKeyboardBackPressed += value;
			remove => _onKeyboardBackPressed -= value;
		}

		event EventHandler<SelectionChangedEventArgs> _selectionChanged;
		event EventHandler<SelectionChangedEventArgs> IFormsEditText.SelectionChanged
		{
			add => _selectionChanged += value;
			remove => _selectionChanged -= value;
		}
	}

	public class FormsEditTextBase : EditText, IDescendantFocusToggler
	{
		DescendantFocusToggler _descendantFocusToggler;

		public FormsEditTextBase(Context context) : base(context)
		{
			DrawableCompat.Wrap(Background);
		}

		bool IDescendantFocusToggler.RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus)
		{
			_descendantFocusToggler = _descendantFocusToggler ?? new DescendantFocusToggler();

			return _descendantFocusToggler.RequestFocus(control, baseRequestFocus);
		}


		public override bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			return (this as IDescendantFocusToggler).RequestFocus(this, () => base.RequestFocus(direction, previouslyFocusedRect));
		}


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
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class EntryEditText : FormsEditText
	{
		public EntryEditText(Context context) : base(context)
		{
		}
	}

	[Obsolete("EditorEditText is obsolete as of version 2.4.0. Please use Xamarin.Forms.Platform.Android.FormsEditText instead.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class EditorEditText : FormsEditText
	{
		public EditorEditText(Context context) : base(context)
		{
		}
	}
}