using System.Collections.Generic;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	internal static class PickerManager
	{
		readonly static HashSet<Keycode> AvailableKeys = new HashSet<Keycode>(new[] {
			Keycode.Tab, Keycode.Forward, Keycode.DpadDown, Keycode.DpadLeft, Keycode.DpadRight, Keycode.DpadUp
		});

		public static void Init(EditText editText)
		{
			editText.Focusable = true;
			editText.Clickable = true;
			editText.InputType = InputTypes.Null;

			editText.KeyPress += OnKeyPress;
		}

		public static void OnTouchEvent(EditText sender, MotionEvent? e)
		{
			if (e != null && e.Action == MotionEventActions.Up && !sender.IsFocused)
			{
				sender.RequestFocus();
			}
		}

		public static void OnFocusChanged(bool gainFocus, EditText sender)
		{
			if (gainFocus)
				sender.CallOnClick();
		}

		static void OnKeyPress(object? sender, AView.KeyEventArgs e)
		{
			if (!AvailableKeys.Contains(e.KeyCode))
			{
				e.Handled = false;
				return;
			}
			e.Handled = true;
			(sender as AView)?.CallOnClick();
		}

		public static void Dispose(EditText editText)
		{
			editText.KeyPress -= OnKeyPress;
			editText.SetOnClickListener(null);
		}

		public static Java.Lang.ICharSequence GetTitle(Color titleColor, string title)
		{
			if (titleColor == null)
				return new Java.Lang.String(title);

			var spannableTitle = new SpannableString(title ?? string.Empty);
			spannableTitle.SetSpan(new ForegroundColorSpan(titleColor.ToNative()), 0, spannableTitle.Length(), SpanTypes.ExclusiveExclusive);
			return spannableTitle;
		}
	}
}