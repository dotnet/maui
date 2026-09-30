using System;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiAppCompatEditText : AppCompatEditText
	{
		public event EventHandler? SelectionChanged;

		public MauiAppCompatEditText(Context context) : base(context)
		{
		}

		protected override void OnSelectionChanged(int selStart, int selEnd)
		{
			base.OnSelectionChanged(selStart, selEnd);

			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			var hadLayoutBeforeTouch = Layout is not null;
			var handled = base.OnTouchEvent(e);

			this.ShowSoftInputIfSkippedOnTouchUp(e, hadLayoutBeforeTouch);

			return handled;
		}
	}
}
