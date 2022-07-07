using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Graphics.Drawable;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public class MauiPicker : MauiPickerBase
	{
		public bool ShowPopupOnFocus { get; set; }

		public MauiPicker(Context context) : base(context)
		{
			PickerManager.Init(this);
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			PickerManager.OnTouchEvent(this, e);
			return base.OnTouchEvent(e); // Raises the OnClick event if focus is already received
		}

		protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, ARect? previouslyFocusedRect)
		{
			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
			PickerManager.OnFocusChanged(gainFocus, this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				PickerManager.Dispose(this);

			base.Dispose(disposing);
		}
	}

	public class MauiPickerBase : AppCompatEditText
	{
		public MauiPickerBase(Context context) : base(context)
		{
			if (Background != null)
				DrawableCompat.Wrap(Background);
		}
	}
}