
using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.Material.Android
{
	public class MaterialPickerEditText : MaterialFormsEditTextBase
	{
		bool _disposed = false;

		public MaterialPickerEditText(Context context) : base(context) => PickerManager.Init(this);

		protected MaterialPickerEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) => PickerManager.Init(this);

		public MaterialPickerEditText(Context context, IAttributeSet attrs) : base(context, attrs) => PickerManager.Init(this);

		public MaterialPickerEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) => PickerManager.Init(this);

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
			if (disposing && !_disposed)
			{
				_disposed = true;
				PickerManager.Dispose(this);
			}

			base.Dispose(disposing);
		}
	}
}