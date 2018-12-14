using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Text;
using Java.Lang;
using System.Collections.Generic;
using Android.Graphics;
using Android.Runtime;

namespace Xamarin.Forms.Platform.Android
{
	public class PickerEditText : EditText, IPopupTrigger
	{
		readonly static HashSet<Keycode> availableKeys = new HashSet<Keycode>(new[] {
			Keycode.Tab, Keycode.Forward, Keycode.Back, Keycode.DpadDown, Keycode.DpadLeft, Keycode.DpadRight, Keycode.DpadUp
		});

		System.WeakReference<IPickerRenderer> rendererRef;

		public bool ShowPopupOnFocus { get; set; }

		public PickerEditText(Context context, IPickerRenderer pickerRenderer) : base(context)
		{
			Focusable = true;
			Clickable = true;
			InputType = InputTypes.Null;
			KeyPress += OnKeyPress;
			rendererRef = new System.WeakReference<IPickerRenderer>(pickerRenderer);
			SetOnClickListener(PickerListener.Instance);
		}

		protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
			if (gainFocus && ShowPopupOnFocus)
				CallOnClick();
			ShowPopupOnFocus = false;
		}

		void OnKeyPress(object sender, KeyEventArgs e)
		{
			if (availableKeys.Contains(e.KeyCode))
			{
				e.Handled = false;
				return;
			}
			e.Handled = true;
			CallOnClick();
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (e.Action == MotionEventActions.Up && !IsFocused)
				RequestFocus();
			return base.OnTouchEvent(e); // raises the OnClick event if focus is already received
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				KeyPress -= OnKeyPress;
				rendererRef = null;
			}
			base.Dispose(disposing);
		}

		class PickerListener : Object, IOnClickListener
		{
			public static readonly PickerListener Instance = new PickerListener();

			public void OnClick(global::Android.Views.View v)
			{
				if (v is PickerEditText picker)
				{
					picker.rendererRef.TryGetTarget(out IPickerRenderer renderer);
					renderer?.OnClick();
				}
			}
		}
	}
}