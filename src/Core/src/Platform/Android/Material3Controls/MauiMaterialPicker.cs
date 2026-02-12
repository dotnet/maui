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
	public MauiMaterialPickerBase(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
	{
		if (Background is not null)
		{
			DrawableCompat.Wrap(Background);
		}
	}

	// MovementMethod handles cursor positioning, scrolling, and text selection (per Android docs).
	// Since text is readonly, we disable it to avoid unnecessary cursor navigation during keyboard input.
	protected override IMovementMethod? DefaultMovementMethod => null;
}