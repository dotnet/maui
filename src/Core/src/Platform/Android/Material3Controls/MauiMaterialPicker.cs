using Android.Content;
using Android.Text.Method;
using Android.Views;
using AndroidX.Core.Graphics.Drawable;
using Google.Android.Material.TextField;

namespace Microsoft.Maui.Platform;

// TODO: Material3 - make it public in .net 11
internal class MauiMaterialPicker : MauiMaterialPickerBase
{
	public MauiMaterialPicker(Context context) : base(context)
	{
		PickerManager.Init(this);
		LongClickable = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			PickerManager.Dispose(this);
		}

		base.Dispose(disposing);
	}
}

// TODO: Material3 - make it public in .net 11
internal class MauiMaterialPickerBase : TextInputEditText
{
	readonly PickerDragGestureFilter _dragGestureFilter = new();
	readonly PickerScrollingMovementMethod _movementMethod = new();

	public MauiMaterialPickerBase(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
	{
		if (Background is not null)
		{
			DrawableCompat.Wrap(Background);
		}
	}

	// Allow overflow text to scroll without enabling EditText cursor movement.
	protected override IMovementMethod? DefaultMovementMethod => _movementMethod;

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
