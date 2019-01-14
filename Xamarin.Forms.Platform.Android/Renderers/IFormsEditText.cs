using Android.Graphics;
using Android.Views;
using System;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IFormsEditText
	{
		bool OnKeyPreIme(Keycode keyCode, KeyEvent e);
		bool RequestFocus(FocusSearchDirection direction, Rect previouslyFocusedRect);
		event EventHandler OnKeyboardBackPressed;
		event EventHandler<SelectionChangedEventArgs> SelectionChanged;
	}
}