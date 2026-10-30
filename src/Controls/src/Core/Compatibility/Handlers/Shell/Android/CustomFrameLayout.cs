#nullable disable
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Controls.Platform.Compatibility
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
			// Preserve the locked flyout's start padding across inset dispatches and layout direction changes.

			var startPadding = PaddingStart;

			var result = base.OnApplyWindowInsets(insets);

			SetPaddingRelative(startPadding, PaddingTop, PaddingEnd, PaddingBottom);

			return result;
		}
	}
}