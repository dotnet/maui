using System;
using Android.Content;
using Android.Views.InputMethods;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Android.Widget;
using Android.Runtime;
using Android.Util;

namespace Microsoft.Maui.Platform
{
	public class MauiAppCompatEditText : AppCompatEditText
	{
		public event EventHandler? SelectionChanged;

		public MauiAppCompatEditText(Context context) : base(context)
		{
			ImeOptions = ImeAction.Done;
			Gravity = GravityFlags.Top;
			TextAlignment = Android.Views.TextAlignment.ViewStart;
			SetSingleLine(false);
			SetHorizontallyScrolling(false);
		}

		protected MauiAppCompatEditText(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public MauiAppCompatEditText(Context context, IAttributeSet? attrs) : base(context, attrs)
		{
		}

		public MauiAppCompatEditText(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
