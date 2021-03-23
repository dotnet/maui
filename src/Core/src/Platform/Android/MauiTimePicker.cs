using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using static Android.Views.View;

namespace Microsoft.Maui
{
	public class MauiTimePicker : EditText, IOnClickListener
	{
		public MauiTimePicker(Context? context) : base(context)
		{
			Initialize();
		}

		public MauiTimePicker(Context? context, IAttributeSet attrs) : base(context, attrs)
		{
			Initialize();
		}

		public MauiTimePicker(Context? context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Initialize();
		}

		protected MauiTimePicker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		private void Initialize()
		{
			Focusable = true;
			SetOnClickListener(this);
		}

		public Action? ShowPicker { get; set; }
		public Action? HidePicker { get; set; }

		public void OnClick(View? v)
		{
			ShowPicker?.Invoke();
		}
	}
}