
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using System.Maui.Platform.Android;

namespace System.Maui.Material.Android
{
	public class MaterialPickerTextInputLayout : MaterialFormsTextInputLayoutBase, IPopupTrigger
	{
		public bool ShowPopupOnFocus { get; set; }

		public MaterialPickerTextInputLayout(Context context) : base(context)
		{
		}

		public MaterialPickerTextInputLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public MaterialPickerTextInputLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		protected MaterialPickerTextInputLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
	}
}