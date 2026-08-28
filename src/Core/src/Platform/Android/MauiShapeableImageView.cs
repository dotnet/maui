using Android.Content;
using Android.Runtime;
using Android.Util;
using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// A <see cref="ShapeableImageView"/> that applies the .NET MAUI Material theme and normalizes padding during measurement.
	/// </summary>
	public class MauiShapeableImageView : ShapeableImageView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MauiShapeableImageView"/> class.
		/// </summary>
		/// <param name="context">The Android context for the view.</param>
		public MauiShapeableImageView(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiShapeableImageView"/> class.
		/// </summary>
		/// <param name="context">The Android context for the view.</param>
		/// <param name="attrs">The attributes for the view.</param>
		public MauiShapeableImageView(Context context, IAttributeSet? attrs) : base(MauiMaterialContextThemeWrapper.Create(context), attrs)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiShapeableImageView"/> class.
		/// </summary>
		/// <param name="context">The Android context for the view.</param>
		/// <param name="attrs">The attributes for the view.</param>
		/// <param name="defStyle">The default style attribute for the view.</param>
		public MauiShapeableImageView(Context context, IAttributeSet? attrs, int defStyle) : base(MauiMaterialContextThemeWrapper.Create(context), attrs, defStyle)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiShapeableImageView"/> class from a JNI reference.
		/// </summary>
		/// <param name="javaReference">The JNI reference.</param>
		/// <param name="transfer">The ownership transfer behavior for the JNI reference.</param>
		protected MauiShapeableImageView(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		/// <inheritdoc />
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