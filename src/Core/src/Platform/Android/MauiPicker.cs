using Android.Content;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Graphics.Drawable;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public class MauiPicker : MauiPickerBase
	{

		public MauiPicker(Context context) : base(context)
		{
			PickerManager.Init(this);
			LongClickable = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				PickerManager.Dispose(this);

			base.Dispose(disposing);
		}
	}

	public class MauiPickerBase : AppCompatEditText
	{
		readonly PickerDragGestureFilter _dragGestureFilter = new();

		public MauiPickerBase(Context context) : base(context)
		{
			if (Background != null)
				DrawableCompat.Wrap(Background);
		}

		// Allow overflow text to scroll without enabling EditText cursor movement.
		protected override IMovementMethod? DefaultMovementMethod => ScrollingMovementMethod.Instance;

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (_dragGestureFilter.ShouldCancelClick(this, e))
				DispatchCancelToBase(e!);

			return base.OnTouchEvent(e);
		}

		void DispatchCancelToBase(MotionEvent e)
		{
			using var cancelEvent = MotionEvent.Obtain(e);

			if (cancelEvent is null)
				return;

			cancelEvent.Action = MotionEventActions.Cancel;
			base.OnTouchEvent(cancelEvent);
		}
	}
}