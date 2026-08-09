using Android.Content;
using Android.Text;
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
		Focusable = true;
		FocusableInTouchMode = false;
		Clickable = true;
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

	public MauiMaterialPickerBase(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
	{
		if (Background is not null)
		{
			DrawableCompat.Wrap(Background);
		}
	}

	protected override IMovementMethod? DefaultMovementMethod => null;

	public override bool OnTouchEvent(MotionEvent? e)
	{
		if (_dragGestureFilter.ShouldCancelClick(this, e))
			DispatchCancelToBase(e!);

		var handled = base.OnTouchEvent(e);

		if (e is null || !Enabled || Layout is null || MovementMethod is not null || EditableText is not ISpannable buffer)
			return handled;

		return ScrollingMovementMethod.Instance?.OnTouchEvent(this, buffer, e) == true || handled;
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
