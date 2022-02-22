using Android.Content;
using Android.Views;
using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeableImageView : ShapeableImageView, IMauiButton
	{
		IView? _imageButton;

		public MauiShapeableImageView(Context context) : base(context)
		{
		}

		public void SetVirtualView(IView imageButton)
		{
			_imageButton = imageButton;
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			bool inputTransparent = _imageButton != null && _imageButton.InputTransparent;

			if (!Enabled || (inputTransparent && Enabled))
				return false;

			return base.OnTouchEvent(e);
		}
	}
}