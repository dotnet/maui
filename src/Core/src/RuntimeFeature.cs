using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Contains all the runtime feature switches that are used throught the MAUI codebase.
	/// See <see href="https://github.com/dotnet/runtime/blob/main/docs/workflow/trimming/feature-switches.md" />
	/// for examples of how to add new feature switches.
	/// </summary>
	/// <remarks>
	/// Property names must be kept in sync with ILLink.Substitutions.xml for proper value substitutions.
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

		internal static bool IsXamlRuntimeParsingSupported
			=> AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsXamlRuntimeParsingSupported", out bool isSupported)
				? isSupported
				: IsXamlRuntimeParsingSupportedByDefault;

		internal const string XamlRuntimeParsingNotSupportedErrorMessage = "XAML runtime parsing is not supported. " +
			"Ensure all XAML resources are compiled using XamlC. Alternatively, enable parsing XAML resources at runtime by setting " +
			"the MauiXamlRuntimeParsingSupport MSBuild property to true. Note: this feature is not trimming-safe and it might not " +
			"behave as expected when the application is trimmed.";

		internal static bool IsIVisualAssemblyScanningEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled", out bool isEnabled)
				? isEnabled
				: IsIVisualAssemblyScanningEnabledByDefault;

		internal static bool IsShellSearchResultsRendererDisplayMemberNameSupported
			=> AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported", out bool isSupported)
				? isSupported
				: IsShellSearchResultsRendererDisplayMemberNameSupportedByDefault;

		internal static bool IsQueryPropertyAttributeSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported", out bool isSupported)
				? isSupported
				: IsQueryPropertyAttributeSupportedByDefault;

		internal static bool IsImplicitCastOperatorsUsageViaReflectionSupported =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported", out bool isSupported)
				? isSupported
				: IsImplicitCastOperatorsUsageViaReflectionSupportedByDefault;
	}
}
