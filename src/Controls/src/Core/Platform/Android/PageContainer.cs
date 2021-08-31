using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{

	// TODO Probably just get rid of this?
	internal class PageContainer : ViewGroup
	{
		public PageContainer(Context context, IViewHandler child, bool inFragment = false) : base(context)
		{
			Id = AView.GenerateViewId();
			Child = child;
			IsInFragment = inFragment;
			AddView((AView)child.NativeView);
		}

		public IViewHandler Child { get; set; }

		public bool IsInFragment { get; set; }

		protected PageContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (changed && Child.NativeView is AView aView)
				aView.Layout(l, t, r, b);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Child.NativeView is AView aView)
			{
				aView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(aView.MeasuredWidth, aView.MeasuredHeight);
			}
		}
	}
}