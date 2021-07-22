using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.ViewPager.Widget;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat
{
	internal class FormsViewPager : MauiViewPager
	{
		public FormsViewPager(Context context) : base(context)
		{
		}

		public FormsViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		protected FormsViewPager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
	}
}