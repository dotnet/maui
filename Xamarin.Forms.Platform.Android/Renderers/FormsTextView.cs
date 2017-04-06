using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class FormsTextView : TextView
	{
		bool _skip;

		public FormsTextView(Context context) : base(context)
		{
		}

		public FormsTextView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public FormsTextView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
		}

		protected FormsTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void Invalidate()
		{
			if (!_skip)
				base.Invalidate();
			_skip = false;
		}

		public void SkipNextInvalidate()
		{
			_skip = true;
		}
	}
}