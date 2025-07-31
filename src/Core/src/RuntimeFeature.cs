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
		const string FeatureSwitchPrefix = "Microsoft.Maui.RuntimeFeature";

		const bool IsIVisualAssemblyScanningEnabledByDefault = false;
		const bool IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault = true;
		const bool IsQueryPropertyAttributeSupportedByDefault = true;
		const bool IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault = true;
		const bool AreBindingInterceptorsSupportedByDefault = true;
		const bool IsXamlCBindingWithSourceCompilationEnabledByDefault = false;
		const bool IsHybridWebViewSupportedByDefault = true;
		const bool SupportNamescopesByDefault = true;
		const bool IsMetricsSupportedByDefault = true;
		const bool EnableDiagnosticsByDefault = false;

#pragma warning disable IL4000 // Return value does not match FeatureGuardAttribute 'System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute'. 
#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(IsIVisualAssemblyScanningEnabled)}")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsIVisualAssemblyScanningEnabled =>
			AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsIVisualAssemblyScanningEnabled)}", out bool isEnabled)
				? isEnabled
				: IsIVisualAssemblyScanningEnabledByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(IsShellSearchResultsRendererDisplayMemberNameSupported)}")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsShellSearchResultsRendererDisplayMemberNameSupported
			=> AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsShellSearchResultsRendererDisplayMemberNameSupported)}", out bool isSupported)
				? isSupported
				: IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(IsQueryPropertyAttributeSupported)}")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsQueryPropertyAttributeSupported =>
			AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsQueryPropertyAttributeSupported)}", out bool isSupported)
				? isSupported
				: IsQueryPropertyAttributeSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(IsImplicitCastOperatorsUsageViaReflectionSupported)}")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
		public static bool IsImplicitCastOperatorsUsageViaReflectionSupported =>
			AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsImplicitCastOperatorsUsageViaReflectionSupported)}", out bool isSupported)
				? isSupported
				: IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(AreBindingInterceptorsSupported)}")]
#endif
		public static bool AreBindingInterceptorsSupported =>
			AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(AreBindingInterceptorsSupported)}", out bool areSupported)
				? areSupported
				: AreBindingInterceptorsSupportedByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(IsXamlCBindingWithSourceCompilationEnabled)}")]
#endif
		public static bool IsXamlCBindingWithSourceCompilationEnabled =>
			AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsXamlCBindingWithSourceCompilationEnabled)}", out bool areSupported)
				? areSupported
				: IsXamlCBindingWithSourceCompilationEnabledByDefault;

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(IsHybridWebViewSupported)}")]
		[FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
		[FeatureGuard(typeof(RequiresDynamicCodeAttribute))]
#endif
		public static bool IsHybridWebViewSupported =>
			AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsHybridWebViewSupported)}", out bool isSupported)
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

#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(EnableDiagnostics)}")]
#endif
		public static bool EnableDiagnostics => AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(EnableDiagnostics)}", out bool isEnabled)
				? isEnabled
				: EnableDiagnosticsByDefault;


#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition($"{FeatureSwitchPrefix}.{nameof(EnableMauiDiagnostics)}")]
#endif
		public static bool EnableMauiDiagnostics
		{
			get
			{
				return AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(EnableMauiDiagnostics)}", out bool isEnabled)
				? isEnabled
				: EnableDiagnostics;
			}
			internal set
			{
				// This property is internal settable to allow tests to enable diagnostics.
				// It should not be set in production code.
				AppContext.SetSwitch($"{FeatureSwitchPrefix}.{nameof(EnableMauiDiagnostics)}", value);
			}
		}
		
#if NET9_0_OR_GREATER
		[FeatureSwitchDefinition("System.Diagnostics.Metrics.Meter.IsSupported")]
#endif
		public static bool IsMetricsSupported
		{
			get => AppContext.TryGetSwitch($"{FeatureSwitchPrefix}.{nameof(IsMetricsSupported)}", out bool isSupported)
				? isSupported
				: IsMetricsSupportedByDefault;
			// This property is internal settable to allow tests to enable metrics.
			// It should not be set in production code.
			internal set => AppContext.SetSwitch($"{FeatureSwitchPrefix}.{nameof(IsMetricsSupported)}", value);

		}
#pragma warning restore IL4000
	}
}
