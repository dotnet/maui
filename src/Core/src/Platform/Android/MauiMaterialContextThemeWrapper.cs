using Android.Content;
using Android.Views;

namespace Microsoft.Maui.Platform;

public class MauiMaterialContextThemeWrapper : ContextThemeWrapper
{
	// IsMaterial3Enabled Flag needed for Control Level theming. App Level theming is handled in MauiAppCompatActivity
	public MauiMaterialContextThemeWrapper(Context context)
		: this(context, RuntimeFeature.IsMaterial3Enabled)
	{
	}

	MauiMaterialContextThemeWrapper(Context context, bool useMaterial3)
		: base(context, useMaterial3 ? Resource.Style.Maui_Material3_Theme_Base : Resource.Style.Maui_MainTheme_Base)
	{
		UseMaterial3 = useMaterial3;
	}

	internal bool UseMaterial3 { get; }

	public static MauiMaterialContextThemeWrapper Create(Context context)
	{
		if (context is MauiMaterialContextThemeWrapper materialContext)
		{
			return materialContext;
		}

		return new MauiMaterialContextThemeWrapper(context);
	}

	internal static MauiMaterialContextThemeWrapper Create(Context context, bool useMaterial3)
	{
		if (context is MauiMaterialContextThemeWrapper materialContext)
		{
			if (materialContext.UseMaterial3 == useMaterial3)
			{
				return materialContext;
			}

			context = materialContext.BaseContext ?? context;
		}

		return new MauiMaterialContextThemeWrapper(context, useMaterial3);
	}
}
