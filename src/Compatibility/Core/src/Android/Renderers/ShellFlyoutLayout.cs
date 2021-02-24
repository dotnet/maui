using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.CoordinatorLayout.Widget;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	class ShellFlyoutLayout : CoordinatorLayout
	{
		public ShellFlyoutLayout(Context context) : base(context)
		{
		}

		public ShellFlyoutLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public ShellFlyoutLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		protected ShellFlyoutLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public Action LayoutChanging { get; set; }
		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			LayoutChanging?.Invoke();
			base.OnLayout(changed, left, top, right, bottom);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				LayoutChanging = null;

			base.Dispose(disposing);
		}
	}
}
