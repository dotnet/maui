using System;
using Android.Content;
using Android.Views;
using Google.Android.Material.TextField;

namespace Microsoft.Maui.Platform;

// TODO: material3 - make it public in .net 11
internal class MauiMaterialTextInputLayout : TextInputLayout
{
    public MauiMaterialTextInputLayout(Context context) : base(context)
    {
        // Set the search icon as the start/leading icon
        SetStartIconDrawable(Resource.Drawable.abc_ic_search_api_material);

        // Set the clear/cancel button as the end icon with custom behavior
        // Custom mode allows showing icon regardless of focus state (matching Material2)
        EndIconMode = EndIconCustom;
        SetEndIconDrawable(Resource.Drawable.abc_ic_clear_material);

        // Set up click listener for clear button
        SetEndIconOnClickListener(new ClearButtonClickListener(this));
    }

    public void UpdateCloseButtonVisibility(bool hasText)
    {
        // Show/hide the end icon based on whether text exists (regardless of focus)
        EndIconVisible = hasText;
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        // Get the measure spec mode and size
        var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
        var widthSize = MeasureSpec.GetSize(widthMeasureSpec);

        // If we have an AtMost constraint with a specific size, treat it as Exactly
        // to force the TextInputLayout to expand to the available width
        // This ensures the Material TextInputLayout fills the available space
        // instead of wrapping its content
        if (widthMode == MeasureSpecMode.AtMost && widthSize > 0)
        {
            widthMeasureSpec = MeasureSpec.MakeMeasureSpec(widthSize, MeasureSpecMode.Exactly);
        }

        base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
    }
}

// TODO: material3 - make it public in .net 11
internal class MauiMaterialTextInputEditText : TextInputEditText
{
    public event EventHandler? SelectionChanged;
    public MauiMaterialTextInputEditText(Context context) : base(context)
    {
    }

    protected override void OnSelectionChanged(int selStart, int selEnd)
    {
        base.OnSelectionChanged(selStart, selEnd);

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        // Get the measure spec mode and size
        var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
        var heightSize = MeasureSpec.GetSize(heightMeasureSpec);

        if (heightMode == MeasureSpecMode.AtMost && heightSize > 0)
        {
            heightMeasureSpec = MeasureSpec.MakeMeasureSpec(heightSize, MeasureSpecMode.Exactly);
        }

        base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
    }
}

class ClearButtonClickListener : Java.Lang.Object, View.IOnClickListener
{
    readonly MauiMaterialTextInputLayout _layout;

    public ClearButtonClickListener(MauiMaterialTextInputLayout layout)
    {
        _layout = layout;
    }

    void View.IOnClickListener.OnClick(View? v)
    {
        // Clear the text in the EditText
        var editText = _layout.GetFirstChildOfType<TextInputEditText>();
        editText?.Text = string.Empty;
    }
}