using System;

namespace Microsoft.Maui.Controls
{
	internal static class FeatureFlags
	{
		private const bool IsXamlLoadingEnabledByDefault = true;

		internal static bool IsXamlLoadingEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.Controls.FeatureFlags.IsXamlLoadingEnabled", out bool isEnabled)
				? isEnabled
				: IsXamlLoadingEnabledByDefault;
	}
}
