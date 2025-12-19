using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Google.Android.Material.Slider;

namespace Microsoft.Maui.Platform;

internal class MauiMaterialSlider : Slider
{
    public MauiMaterialSlider(Context context)
        : base(MauiMaterialContextThemeWrapper.Create(context))
    {
    }

    protected MauiMaterialSlider(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    public MauiMaterialSlider(Context context, IAttributeSet? attrs) : base(MauiMaterialContextThemeWrapper.Create(context), attrs)
    {
    }

    public MauiMaterialSlider(Context context, IAttributeSet? attrs, int defStyleAttr) : base(MauiMaterialContextThemeWrapper.Create(context), attrs, defStyleAttr)
    {
    }
}
