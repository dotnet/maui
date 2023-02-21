using System;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Graphics.Drawable;
using static Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public class MauiTimePicker : AppCompatEditText, IOnClickListener
	{
		public MauiTimePicker(Context context) : base(context)
		{
			Initialize();
		}

		public MauiTimePicker(Context context, IAttributeSet? attrs) : base(context, attrs)
		{
			Initialize();
		}

		public MauiTimePicker(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Initialize();
		}

		protected MauiTimePicker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public Action? ShowPicker { get; set; }
		public Action? HidePicker { get; set; }

		public void OnClick(View? v)
		{
			ShowPicker?.Invoke();
		}

		void Initialize()
		{
			if (Background != null)
				DrawableCompat.Wrap(Background);

			Focusable = true;
			FocusableInTouchMode = false;
			Clickable = true;
			InputType = InputTypes.Null;

			SetOnClickListener(this);
		}
	}
}