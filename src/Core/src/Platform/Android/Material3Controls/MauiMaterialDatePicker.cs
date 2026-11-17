using Android.Content;
using Android.Runtime;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Material 3 DatePicker field on Android: an outlined text field with a trailing calendar icon.
/// Tapping the calendar icon opens the platform <c>MaterialDatePicker</c> dialog.
/// </summary>
public class MauiMaterialDatePicker : MauiMaterialDateTimePickerBase
{
    public MauiMaterialDatePicker(Context context)
        : base(MauiMaterialContextThemeWrapper.Create(context), Resource.Drawable.material_ic_calendar_black_24dp)
    {
        SetEndIconContentDescription(Resource.String.maui_date_picker_open);
    }

    protected MauiMaterialDatePicker(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

}