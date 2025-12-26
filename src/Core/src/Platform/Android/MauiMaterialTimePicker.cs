using System;
using Android.Content;
using Android.Runtime;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using AndroidX.Core.Graphics.Drawable;
using Google.Android.Material.TextField;
using static Android.Views.View;

namespace Microsoft.Maui.Platform;

internal class MauiMaterialTimePicker : TextInputEditText, IOnClickListener
{
    public MauiMaterialTimePicker(Context context) : base(context)
    {
        Initialize();
    }

    public MauiMaterialTimePicker(Context context, IAttributeSet? attrs) : base(context, attrs)
    {
        Initialize();
    }

    public MauiMaterialTimePicker(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
    {
        Initialize();
    }

    protected MauiMaterialTimePicker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    // MovementMethod handles cursor positioning, scrolling, and text selection (per Android docs).
    // Since text is readonly, we disable it to avoid unnecessary cursor navigation during keyboard input.
    protected override IMovementMethod? DefaultMovementMethod => null;

    public Action? ShowPicker { get; set; }
    public Action? HidePicker { get; set; }

    public void OnClick(View? v)
    {
        ShowPicker?.Invoke();
    }

    void Initialize()
    {
        if (Background != null)
            DrawableCompat.Wrap(Background);

        PickerManager.Init(this);

        SetOnClickListener(this);
    }
}
