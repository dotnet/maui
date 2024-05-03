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
		private const bool IsXamlRuntimeParsingSupportedByDefault = true;
		private const bool IsIVisualAssemblyScanningEnabledByDefault = false;
		private const bool IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault = true;
		private const bool IsQueryPropertyAttributeSupportedByDefault = true;
		private const bool IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault = true;

#pragma warning disable IL4000 // Return value does not match FeatureGuardAttribute 'System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute'. 
#if !NETSTANDARD
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsXamlRuntimeParsingSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
		[FeatureGuard(typeof(RequiresDynamicCodeAttribute))]
#endif
		internal static bool IsXamlRuntimeParsingSupported
			=> AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsXamlRuntimeParsingSupported", out bool isSupported)
				? isSupported
				: IsXamlRuntimeParsingSupportedByDefault;

		internal const string XamlRuntimeParsingNotSupportedErrorMessage = "XAML runtime parsing is not supported. " +
			"Ensure all XAML resources are compiled using XamlC. Alternatively, enable parsing XAML resources at runtime by setting " +
			"the MauiXamlRuntimeParsingSupport MSBuild property to true. Note: this feature is not trimming-safe and it might not " +
			"behave as expected when the application is trimmed.";

#if !NETSTANDARD
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsIVisualAssemblyScanningEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled", out bool isEnabled)
				? isEnabled
				: IsIVisualAssemblyScanningEnabledByDefault;

#if !NETSTANDARD
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsShellSearchResultsRendererDisplayMemberNameSupported
			=> AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported", out bool isSupported)
				? isSupported
				: IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault;

#if !NETSTANDARD
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsQueryPropertyAttributeSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported", out bool isSupported)
				? isSupported
				: IsQueryPropertyAttributeSupportedByDefault;

#if !NETSTANDARD
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		internal static bool IsImplicitCastOperatorsUsageViaReflectionSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported", out bool isSupported)
				? isSupported
				: IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault;
#pragma warning restore IL4000
	}
}
