using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.ControlGallery.Android
{
	/// <summary>
	///     This is a custom Android control which deliberately does some incorrect measuring
	/// </summary>
	internal class BrokenNativeControl : TextView
	{
		bool _on;

		public BrokenNativeControl (IntPtr javaReference, JniHandleOwnership transfer) : base (javaReference, transfer)
		{
		}

		public BrokenNativeControl (Context context) : base (context)
		{
		}

		public BrokenNativeControl (Context context, IAttributeSet attrs) : base (context, attrs)
		{
		}

		public BrokenNativeControl (Context context, IAttributeSet attrs, int defStyleAttr)
			: base (context, attrs, defStyleAttr)
		{
		}

		public BrokenNativeControl (Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
			: base (context, attrs, defStyleAttr, defStyleRes)
		{
		}

		public override bool OnTouchEvent (MotionEvent e)
		{
			_on = !_on;

			SetTypeface (null, _on ? TypefaceStyle.Bold : TypefaceStyle.Normal);

			return base.OnTouchEvent (e);
		}

		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			int width = MeasureSpec.GetSize (widthMeasureSpec);

			// Force the width to 1/2 of what's being requested. This is deliberately wrong so we can demo 
			// giving the LayoutExtensions an override to fix it with
			int widthSpec = MeasureSpec.MakeMeasureSpec (width / 2, MeasureSpec.GetMode (widthMeasureSpec));

			base.OnMeasure (widthSpec, heightMeasureSpec);
		}
	}
}