using System;
using Android.Views;
using ARect = Android.Graphics.Rect;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IFormsEditText
	{
		bool OnKeyPreIme(Keycode keyCode, KeyEvent e);
		bool RequestFocus(FocusSearchDirection direction, ARect previouslyFocusedRect);
		event EventHandler OnKeyboardBackPressed;
		event EventHandler<SelectionChangedEventArgs> SelectionChanged;
	}
}