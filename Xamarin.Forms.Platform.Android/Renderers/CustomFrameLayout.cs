using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class CustomFrameLayout : FrameLayout
	{
		public CustomFrameLayout(Context context) : base(context)
		{
		}

		public CustomFrameLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public CustomFrameLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		public CustomFrameLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
		}

		protected CustomFrameLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override WindowInsets OnApplyWindowInsets(WindowInsets insets)
		{
			// We need to make sure we retain left padding.
			// Failure to do so will result in the padding being wrong if you set FlyoutBehavior to Locked
			// and then rotate the device.

			var leftPadding = PaddingLeft;

			var result = base.OnApplyWindowInsets(insets);

			SetPadding(leftPadding, PaddingTop, PaddingRight, PaddingBottom);

			return result;
		}
	}
}