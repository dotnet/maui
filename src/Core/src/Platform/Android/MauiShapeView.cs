using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : PlatformGraphicsView
	{
		UnhandledTouchEventHandler? _unhandledTouchEventHandler;

		public MauiShapeView(Context? context) : base(context)
		{
			ClipToOutline = true;
		}

		public override bool OnTouchEvent(MotionEvent? e) =>
			base.OnTouchEvent(e) ||
			(_unhandledTouchEventHandler ??= new()).OnTouchEvent(this, e);
	}
}