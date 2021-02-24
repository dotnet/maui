using System;
using Android.Views;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal interface IFormsEditText
	{
		bool OnKeyPreIme(Keycode keyCode, KeyEvent e);
		bool RequestFocus(FocusSearchDirection direction, ARect previouslyFocusedRect);
		event EventHandler OnKeyboardBackPressed;
		event EventHandler<SelectionChangedEventArgs> SelectionChanged;
	}
}