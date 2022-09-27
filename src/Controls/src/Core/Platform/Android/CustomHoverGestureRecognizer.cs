using System;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Android;

internal class CustomHoverGestureRecognizer : Java.Lang.Object, AView.IOnHoverListener
{
	internal Func<AView, MotionEvent, bool> OnHoverAction { get; set; }

	internal CustomHoverGestureRecognizer(Func<AView, MotionEvent, bool> onHover)
	{
		OnHoverAction = onHover;
	}
	public virtual bool OnHover(AView v, MotionEvent e)
	{
		return OnHoverAction(v, e);
	}
}