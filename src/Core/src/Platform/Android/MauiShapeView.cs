using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : PlatformGraphicsView
	{
		TouchEventFallbackHandler? _touchEventFallbackHandler;

		public MauiShapeView(Context? context) : base(context)
		{
			ClipToOutline = true;
		}

		public override bool OnTouchEvent(MotionEvent? e) =>
			base.OnTouchEvent(e) ||
			(_touchEventFallbackHandler ??= new()).OnTouchEvent(this, e);
	}
}