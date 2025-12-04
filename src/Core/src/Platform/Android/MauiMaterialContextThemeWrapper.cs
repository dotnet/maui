using Android.Content;
using Android.Views;

namespace Microsoft.Maui.Platform;

internal class MauiMaterialContextThemeWrapper : ContextThemeWrapper
{
    public MauiMaterialContextThemeWrapper(Context context) : this(context, Resource.Style.Maui_MainTheme_Base)
    {
    }

    MauiMaterialContextThemeWrapper(Context context, int themeResId) : base(context, themeResId)
    {

    }

    public static MauiMaterialContextThemeWrapper Create(Context context)
    {
        if (context is MauiMaterialContextThemeWrapper materialContext)
        {
            return materialContext;
        }

        return new MauiMaterialContextThemeWrapper(context);
    }
}
