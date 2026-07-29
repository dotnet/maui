using Android.Content;
using Android.Runtime;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Material 3 TimePicker field on Android: an outlined text field with a trailing clock icon.
/// Tapping the clock icon opens the platform <c>MaterialTimePicker</c> dialog.
/// </summary>
public class MauiMaterialTimePicker : MauiMaterialDateTimePickerBase
{
    public MauiMaterialTimePicker(Context context)
        : base(context, Resource.Drawable.ic_clock_black_24dp)
    {
    }

    protected MauiMaterialTimePicker(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }
}
