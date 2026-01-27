using Android.Content;
using Android.Runtime;
using Android.Util;
using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeableImageView : ShapeableImageView
	{

		public MauiShapeableImageView(Context? context) : base(MauiMaterialContextThemeWrapper.Create(context))
		{
		}

		public MauiShapeableImageView(Context? context, IAttributeSet? attrs) : base(MauiMaterialContextThemeWrapper.Create(context), attrs)
		{
		}

		public MauiShapeableImageView(Context? context, IAttributeSet? attrs, int defStyle) : base(MauiMaterialContextThemeWrapper.Create(context), attrs, defStyle)
		{
		}

		protected MauiShapeableImageView(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			// The padding has a few issues. This is a workaround for the following issue:
			// https://github.com/material-components/material-components-android/issues/2063

			// ShapeableImageView combines ContentPadding with Padding and updates
			// Padding with the result.
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			// We need to reset the padding to 0 to avoid a double padding.
			SetPadding(0, 0, 0, 0);
		}
	}
}