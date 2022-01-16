#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement> : UIView, INativeViewHandler
		where TElement : Element, IView
	{
		public virtual UIViewController? ViewController => null;

		//public void UpdateLayout()
		//{
		//	if (Element != null)
		//		this.InvalidateMeasure(Element);
		//}

		//protected override void OnLayout(bool changed, int l, int t, int r, int b)
		//{
		//	if (ChildCount > 0)
		//	{
		//		var platformView = GetChildAt(0);
		//		if (platformView != null)
		//		{
		//			platformView.Layout(l, t, r, b);
		//		}
		//	}
		//}

		//protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		//{
		//	if (ChildCount > 0)
		//	{
		//		var platformView = GetChildAt(0);
		//		if (platformView != null)
		//		{
		//			platformView.Measure(widthMeasureSpec, heightMeasureSpec);
		//			SetMeasuredDimension(platformView.MeasuredWidth, platformView.MeasuredHeight);
		//			return;
		//		}
		//	}

		//	SetMeasuredDimension(0, 0);
		//}



	}
}
