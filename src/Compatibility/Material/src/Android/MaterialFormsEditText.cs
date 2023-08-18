
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Microsoft.Maui.Controls.Platform.Android;

namespace Microsoft.Maui.Controls.Compatibility.Material.Android
{
	public class MaterialFormsEditText : MaterialFormsEditTextBase, IFormsEditText
	{

		public MaterialFormsEditText(Context context) : base(context)
		{
		}

		protected MaterialFormsEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public MaterialFormsEditText(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public MaterialFormsEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
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

		event EventHandler _onKeyboardBackPressed;
		event EventHandler IFormsEditText.OnKeyboardBackPressed
		{
			add => _onKeyboardBackPressed += value;
			remove => _onKeyboardBackPressed -= value;
		}

		event EventHandler<Platform.Android.SelectionChangedEventArgs> _selectionChanged;
		event EventHandler<Platform.Android.SelectionChangedEventArgs> IFormsEditText.SelectionChanged
		{
			add => _selectionChanged += value;
			remove => _selectionChanged -= value;
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);
			_selectionChanged?.Invoke(this, new Platform.Android.SelectionChangedEventArgs(selStart, selEnd));
		}
	}
}