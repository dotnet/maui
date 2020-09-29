using System.Collections.Generic;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Java.Lang;
using AView = global::Android.Views.View;

namespace Xamarin.Forms.Platform.Android
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

			editText.SetOnClickListener(PickerListener.Instance);
		}

		public static void OnTouchEvent(EditText sender, MotionEvent e)
		{
			if (e.Action == MotionEventActions.Up && !sender.IsFocused)
			{
				sender.RequestFocus();
			}
		}

		public static void OnFocusChanged(bool gainFocus, EditText sender, IPopupTrigger popupTrigger)
		{
			if (gainFocus && popupTrigger.ShowPopupOnFocus)
				sender.CallOnClick();

			popupTrigger.ShowPopupOnFocus = false;
		}

		static void OnKeyPress(object sender, AView.KeyEventArgs e)
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

		public static ICharSequence GetTitle(Color titleColor, string title)
		{
			if (titleColor == Color.Default)
				return new Java.Lang.String(title);

			var spannableTitle = new SpannableString(title ?? "");
			spannableTitle.SetSpan(new ForegroundColorSpan(titleColor.ToAndroid()), 0, spannableTitle.Length(), SpanTypes.ExclusiveExclusive);
			return spannableTitle;
		}

		class PickerListener : global::Java.Lang.Object, AView.IOnClickListener
		{
			public static readonly PickerListener Instance = new PickerListener();

			public void OnClick(global::Android.Views.View v)
			{
				if (v is AView picker)
				{
					picker.HideKeyboard();
					if (picker?.Parent is IPickerRenderer renderer1)
						renderer1.OnClick();
					else if (picker?.Parent?.Parent?.Parent is IPickerRenderer renderer2)
						renderer2.OnClick();
				}
			}
		}
	}
}