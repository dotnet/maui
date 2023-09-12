using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Graphics.Drawable;
using Microsoft.Maui.Controls.Platform;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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

			this.HideSoftInput();

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

	public class FormsEditTextBase : AppCompatEditText, IDescendantFocusToggler
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


		public override bool RequestFocus(FocusSearchDirection direction, ARect previouslyFocusedRect)
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
}