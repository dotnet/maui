using Android.Graphics.Drawables;
using Android.Runtime;

namespace Microsoft.Maui.Platform;

internal class MauiLayerDrawable : LayerDrawable
{
	public MauiLayerDrawable(params Drawable[] layers)
		: base(layers)
	{
	}

	protected MauiLayerDrawable(nint javaReference, JniHandleOwnership transfer)
		: base(javaReference, transfer)
	{
	}
}
