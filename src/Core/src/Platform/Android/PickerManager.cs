using System;
using System.Collections.Generic;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	internal static class PickerManager
	{
		public static void Init(EditText editText)
		{
			editText.Focusable = true;
			editText.FocusableInTouchMode = false;
			editText.Clickable = true;

			// InputType needs to be set before setting KeyListener
			editText.InputType = InputTypes.Null;
			editText.KeyListener = null;
		}

		public static void Dispose(EditText editText)
		{
			editText.SetOnClickListener(null);
		}
	}
}