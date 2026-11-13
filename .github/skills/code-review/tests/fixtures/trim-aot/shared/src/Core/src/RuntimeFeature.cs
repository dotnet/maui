using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	static class RuntimeFeature
	{
		const string FeatureSwitchPrefix = "Microsoft.Maui.RuntimeFeature";
		const bool IsHybridWebViewSupportedByDefault = true;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(IsHybridWebViewSupported)}")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
		[FeatureGuard(typeof(RequiresDynamicCodeAttribute))]
#endif
		public static bool IsHybridWebViewSupported =>
			AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsHybridWebViewSupported)}", out bool isSupported)
				? isSupported
				: IsHybridWebViewSupportedByDefault;
	}
}
