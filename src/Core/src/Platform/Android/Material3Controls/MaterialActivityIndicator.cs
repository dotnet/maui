using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Google.Android.Material.ProgressIndicator;

namespace Microsoft.Maui.Platform;

internal partial class MaterialActivityIndicator : CircularProgressIndicator
{
    public MaterialActivityIndicator(Context context)
     : base(context)
    {
    }

    protected MaterialActivityIndicator(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    public MaterialActivityIndicator(Context context, IAttributeSet? attrs) : base(MauiMaterialContextThemeWrapper.Create(context), attrs)
    {
    }

    public MaterialActivityIndicator(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
    {

    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        // Intrinsic desired size (Material2-aligned)
        var desiredSize =
            IndicatorSize +
            PaddingLeft + PaddingRight;

        var width = ResolveSize(desiredSize, widthMeasureSpec);
        var height = ResolveSize(desiredSize, heightMeasureSpec);

        // ActivityIndicator must always be square
        var finalSize = Math.Min(width, height);

        SetMeasuredDimension(finalSize, finalSize);
    }
}