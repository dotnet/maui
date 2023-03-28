using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Microsoft.Maui.Platform
{
	public class MauiViewGroup : ViewGroup
	{
		public MauiViewGroup(Context context) : base(context)
		{

		}

		public MauiViewGroup(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}

		public MauiViewGroup(Context context, IAttributeSet attrs) : base(context, attrs)
		{

		}

		public MauiViewGroup(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{

		}

		public MauiViewGroup(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{

		}

		public bool NotReallyHandled { get; private set; }

		public override bool DispatchTouchEvent(MotionEvent? e)
		{
			NotReallyHandled = false;

			var result = base.DispatchTouchEvent(e);

			if (result && NotReallyHandled)
			{
				return OnTouchEvent(e);
			}

			return result;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{

		}

		internal void NotifyFakeHandling()
		{
			NotReallyHandled = true;
		}
	}
}
