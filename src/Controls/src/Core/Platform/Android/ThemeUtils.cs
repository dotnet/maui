#nullable disable
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ThemeUtils
	{
		internal static Context CreateThemedContext(Context context)
		{
			var requestedTheme = Application.Current?.RequestedTheme;
			using var configuration = new Configuration(context.Resources.Configuration);
			if (requestedTheme is AppTheme.Dark or AppTheme.Light)
			{
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(requestedTheme == AppTheme.Dark ? UiMode.NightYes : UiMode.NightNo);
			}

			var configuredContext = context.CreateConfigurationContext(configuration);
			if (requestedTheme is AppTheme.Dark or AppTheme.Light)
				configuredContext.SetTheme(Resource.Style.Maui_Material3_Theme_NoActionBar);
			else
				configuredContext.Theme.SetTo(context.Theme);

			return configuredContext;
		}
	}
}
