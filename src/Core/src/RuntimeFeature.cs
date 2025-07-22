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
	static class RuntimeFeature
	{
		const bool IsIVisualAssemblyScanningEnabledByDefault = false;
		const bool IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault = true;
		const bool IsQueryPropertyAttributeSupportedByDefault = true;
		const bool IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault = true;
		const bool AreBindingInterceptorsSupportedByDefault = true;
		const bool IsXamlCBindingWithSourceCompilationEnabledByDefault = false;
		const bool IsHybridWebViewSupportedByDefault = true;
		const bool SupportNamescopesByDefault = true;
		const string FeatureSwitchPrefix = "Microsoft.Maui.RuntimeFeature";

#pragma warning disable IL4000 // Return value does not match FeatureGuardAttribute 'System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute'. 
#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsIVisualAssemblyScanningEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled", out bool isEnabled)
				? isEnabled
				: IsIVisualAssemblyScanningEnabledByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsShellSearchResultsRendererDisplayMemberNameSupported
			=> AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported", out bool isSupported)
				? isSupported
				: IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsQueryPropertyAttributeSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported", out bool isSupported)
				? isSupported
				: IsQueryPropertyAttributeSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsImplicitCastOperatorsUsageViaReflectionSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported", out bool isSupported)
				? isSupported
				: IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported")]
#endif
		public static bool AreBindingInterceptorsSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported", out bool areSupported)
				? areSupported
				: AreBindingInterceptorsSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled")]
#endif
		public static bool IsXamlCBindingWithSourceCompilationEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled", out bool areSupported)
				? areSupported
				: IsXamlCBindingWithSourceCompilationEnabledByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("Microsoft.Maui.RuntimeFeature.IsHybridWebViewSupported")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
		[FeatureGuard(typeof(RequiresDynamicCodeAttribute))]
#endif
		public static bool IsHybridWebViewSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsHybridWebViewSupported", out bool isSupported)
				? isSupported
				: IsHybridWebViewSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(AreNamescopesSupported)}")]
#endif
		public static bool AreNamescopesSupported
		{
			get => AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(AreNamescopesSupported)}", out bool isSupported)
				? isSupported
				: SupportNamescopesByDefault;
			//for testing purposes only
			internal set => AppContext.SetSwitch($"{FeatureSwitchPrefix}.{nameof(AreNamescopesSupported)}", value);
		}
#pragma warning restore IL4000
    }
}