#if __ANDROID_28__
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialFormsTextInputLayout : MaterialFormsTextInputLayoutBase
	{
		public MaterialFormsTextInputLayout(Context context) : base(context)
		{
		}

		public MaterialFormsTextInputLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public MaterialFormsTextInputLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		protected MaterialFormsTextInputLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
	}
}
#endif