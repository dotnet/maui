using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using static Android.Views.View;

namespace System.Maui.Platform
{
	public class PickerView : AppCompatTextView, IOnClickListener
	{
		public PickerView(Context context) : base(context)
		{
			Initialize();
		}

		public PickerView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Initialize();
		}

		public PickerView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Initialize();
		}

		protected PickerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		private void Initialize() 
		{
			Focusable = true;
			SetOnClickListener(this);
		}

		public Action ShowPicker { get; set; }
		public Action HidePicker { get; set; }

		public void OnClick(View v)
		{
			ShowPicker();
		}
	}
}