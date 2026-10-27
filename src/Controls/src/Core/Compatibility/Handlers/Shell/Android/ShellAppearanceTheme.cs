#nullable disable
using Android.Content;
using Android.Content.Res;
using AndroidX.Core.Content.Resources;
using Microsoft.Maui.ApplicationModel;
using ContextThemeWrapper = AndroidX.AppCompat.View.ContextThemeWrapper;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal static class ShellAppearanceTheme
	{
		internal static UiMode GetNightMode(Context context) =>
			Application.Current?.RequestedTheme switch
			{
				AppTheme.Dark => UiMode.NightYes,
				AppTheme.Light => UiMode.NightNo,
				_ => context.Resources.Configuration.UiMode & UiMode.NightMask
			};

		internal static ContextThemeWrapper CreateContext(Context context)
		{
			using var configuration = new Configuration(context.Resources.Configuration);
			configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) | GetNightMode(context);
			var themedContext = new ContextThemeWrapper(context, 0);
			themedContext.ApplyOverrideConfiguration(configuration);

			// Copy the actual app theme (including widget overlays), then replay its styles against
			// the overridden resources. Copying alone retains already-resolved day/night values.
			themedContext.Theme.SetTo(context.Theme);
			ResourcesCompat.ThemeCompat.Rebase(themedContext.Theme);
			return themedContext;
		}
	}
}
