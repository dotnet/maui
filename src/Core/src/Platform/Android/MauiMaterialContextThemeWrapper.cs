using Android.Content;
using Android.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Platform;

internal class MauiMaterialContextThemeWrapper : ContextThemeWrapper
{
    static int? _cachedThemeId;

    public MauiMaterialContextThemeWrapper(Context context)
        : this(context, GetThemeResourceId())
    {
    }

    MauiMaterialContextThemeWrapper(Context context, int themeResId)
        : base(context, themeResId)
    {
    }

    static int GetThemeResourceId()
    {
        if (_cachedThemeId.HasValue)
            return _cachedThemeId.Value;

        // Try to get configuration from DI
        var config = IPlatformApplication.Current?.Services
            ?.GetService<IMaterialConfiguration>();

        _cachedThemeId = config?.GetThemeResourceId()
            ?? Resource.Style.Maui_MainTheme_Base;

        return _cachedThemeId.Value;
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