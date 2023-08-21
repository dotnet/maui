//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

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