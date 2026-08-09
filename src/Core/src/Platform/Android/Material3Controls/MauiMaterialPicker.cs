using Android.Content;
using Android.Text.Method;
using AndroidX.Core.Graphics.Drawable;
using Google.Android.Material.TextField;

namespace Microsoft.Maui.Platform;

// TODO: Material3 - make it public in .net 11
internal class MauiMaterialPicker : MauiMaterialPickerBase
{
	public MauiMaterialPicker(Context context) : base(context)
	{
		PickerManager.Init(this);
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
	PickerScrollingMovementMethod? _movementMethod;

	public MauiMaterialPickerBase(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
	{
		if (Background is not null)
		{
			DrawableCompat.Wrap(Background);
		}
	}

	// Allow overflow text to scroll without enabling EditText cursor movement.
	protected override IMovementMethod? DefaultMovementMethod =>
		_movementMethod ??= new PickerScrollingMovementMethod();

	public override bool PerformClick()
	{
		if (_movementMethod?.ConsumeClick() == true)
			return false;

		return base.PerformClick();
	}
}
