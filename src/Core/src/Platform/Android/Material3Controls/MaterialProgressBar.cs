using Android.Content;
using Android.Runtime;
using Android.Util;
using Google.Android.Material.ProgressIndicator;

namespace Microsoft.Maui.Platform;

internal class MaterialProgressBar : LinearProgressIndicator
{
	public MaterialProgressBar(Context context)
		: base(MauiMaterialContextThemeWrapper.Create(context))
	{
	}

	protected MaterialProgressBar(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	{
	}

	public MaterialProgressBar(Context context, IAttributeSet? attrs) : base(MauiMaterialContextThemeWrapper.Create(context), attrs)
	{
	}

	public MaterialProgressBar(Context context, IAttributeSet? attrs, int defStyleAttr) : base(MauiMaterialContextThemeWrapper.Create(context), attrs, defStyleAttr)
	{
	}
}