using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.CoordinatorLayout.Widget;

namespace Microsoft.Maui.Controls.Platform
{
	public class NavigationLayout : CoordinatorLayout
	{
		[Preserve(Conditional = true)]
		public NavigationLayout(Context context) : base(context)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		[Preserve(Conditional = true)]
		protected NavigationLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
	}
}
