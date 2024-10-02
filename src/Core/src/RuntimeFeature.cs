using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	/// <summary>
	/// Contains all the runtime feature switches that are used throught the MAUI codebase.
	/// See <see href="https://github.com/dotnet/runtime/blob/main/docs/workflow/trimming/feature-switches.md" />
	/// for examples of how to add new feature switches.
	/// </summary>
	/// <remarks>
	/// Mapping of MSBuild properties to feature switches and the default values of feature switches
	/// is defined in Microsoft.Maui.Sdk.Before.targets.
	/// </remarks>
	internal static class RuntimeFeature
	{
		private const bool IsIVisualAssemblyScanningEnabledByDefault = false;
		private const bool IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault = true;
		private const bool IsQueryPropertyAttributeSupportedByDefault = true;
		private const bool IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault = true;
		private const bool AreBindingInterceptorsSupportedByDefault = true;
		private const bool IsXamlCBindingWithSourceCompilationEnabledByDefault = false;

#pragma warning disable IL4000 // Return value does not match FeatureGuardAttribute 'System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute'. 
#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsIVisualAssemblyScanningEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled", out bool isEnabled)
				? isEnabled
				: IsIVisualAssemblyScanningEnabledByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsShellSearchResultsRendererDisplayMemberNameSupported
			=> AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported", out bool isSupported)
				? isSupported
				: IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsQueryPropertyAttributeSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported", out bool isSupported)
				? isSupported
				: IsQueryPropertyAttributeSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsImplicitCastOperatorsUsageViaReflectionSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported", out bool isSupported)
				? isSupported
				: IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported")]
#endif
		internal static bool AreBindingInterceptorsSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported", out bool areSupported)
				? areSupported
				: AreBindingInterceptorsSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled")]
#endif
		internal static bool IsXamlCBindingWithSourceCompilationEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled", out bool areSupported)
				? areSupported
				: IsXamlCBindingWithSourceCompilationEnabledByDefault;
#pragma warning restore IL4000
	}
}
