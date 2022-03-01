using System;
using Android.Views;

namespace Microsoft.Maui.Platform
{
	public interface IMauiEditText
	{
		bool OnKeyPreIme(Keycode keyCode, KeyEvent? e); 
		
		event EventHandler OnKeyboardBackPressed;
	}
}