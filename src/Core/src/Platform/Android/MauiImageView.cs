using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	class MauiImageView : AppCompatImageView, IInputTransparentCapable
	{
		public MauiImageView(Context context) : base(context)
		{
		}

		bool IInputTransparentCapable.InputTransparent { get; set; }

		public override bool OnTouchEvent(MotionEvent? e) =>
			base.OnTouchEvent(e) ||
			TouchEventInterceptor.OnTouchEvent(this, e);
	}
}
