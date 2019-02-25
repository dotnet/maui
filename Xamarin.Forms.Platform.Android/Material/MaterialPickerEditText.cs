#if __ANDROID_28__
using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialPickerEditText : MaterialFormsEditTextBase
	{
		bool _isDisposed = false;
		public MaterialPickerEditText(Context context) : base(context)
		{
			PickerManager.Init(this);
		}

		protected MaterialPickerEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			PickerManager.Init(this);
		}

		public MaterialPickerEditText(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			PickerManager.Init(this);
		}

		public MaterialPickerEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			PickerManager.Init(this);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			PickerManager.OnTouchEvent(this, e);
			return base.OnTouchEvent(e); // raises the OnClick event if focus is already received
		}

		protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
		{
			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
			PickerManager.OnFocusChanged(gainFocus, this, (IPopupTrigger)Parent.Parent);

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				_isDisposed = true;
				PickerManager.Dispose(this);
			}

			base.Dispose(disposing);
		}
	}
}
#endif