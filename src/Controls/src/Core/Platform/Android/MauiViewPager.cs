#nullable disable
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.ViewPager.Widget;

namespace Microsoft.Maui.Controls.Platform
{
	public class MauiViewPager : ViewPager
	{
		public MauiViewPager(Context context) : base(context)
		{
		}

		public MauiViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		protected MauiViewPager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public bool EnableGesture { get; set; } = true;

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			// Same as:
			// if (!EnableGesture) return false;
			// However this is, at least in theory a tidge faster which in this particular area is good
			return EnableGesture && base.OnInterceptTouchEvent(ev);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			return EnableGesture && base.OnTouchEvent(e);
		}
	}
}