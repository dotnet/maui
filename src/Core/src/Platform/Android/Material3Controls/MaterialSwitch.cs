using Android.Content;
using Android.Runtime;
using Android.Util;
using Google.Android.Material.MaterialSwitch;

namespace Microsoft.Maui.Platform;

internal class MauiMaterialSwitch : MaterialSwitch
{
	public MauiMaterialSwitch(Context context)
		: base(MauiMaterialContextThemeWrapper.Create(context))
	{
	}

	protected MauiMaterialSwitch(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	{
	}

	public MauiMaterialSwitch(Context context, IAttributeSet? attrs) : base(MauiMaterialContextThemeWrapper.Create(context), attrs)
	{
	}

	public MauiMaterialSwitch(Context context, IAttributeSet? attrs, int defStyleAttr) : base(MauiMaterialContextThemeWrapper.Create(context), attrs, defStyleAttr)
	{
	}
}