using System;

namespace Microsoft.Maui
{
	internal static class RuntimeFeature
	{
		private const bool IsXamlLoadingEnabledByDefault = true;

		internal static bool IsXamlLoadingEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsXamlLoadingEnabled", out bool isEnabled)
				? isEnabled
				: IsXamlLoadingEnabledByDefault;

		internal static bool IsDebuggerSupported =>
			AppContext.TryGetSwitch("System.Diagnostics.Debugger.IsSupported", out bool isSupported) && isSupported;
	}
}
