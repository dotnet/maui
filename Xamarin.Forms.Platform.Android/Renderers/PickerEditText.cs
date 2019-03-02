using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class PickerEditText : FormsEditTextBase, IPopupTrigger
	{
		public bool ShowPopupOnFocus { get; set; }

		public PickerEditText(Context context) : base(context)
		{
			PickerManager.Init(this);
		}

		public PickerEditText(Context context, IPickerRenderer pickerRenderer) : this(context)
		{

		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			PickerManager.OnTouchEvent(this, e);
			return base.OnTouchEvent(e); // raises the OnClick event if focus is already received
		}

		protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
			PickerManager.OnFocusChanged(gainFocus, this, this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				PickerManager.Dispose(this);

			base.Dispose(disposing);
		}
	}
}