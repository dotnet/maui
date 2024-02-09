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

		internal static bool IsXamlRuntimeParsingSupported
			=> AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsXamlRuntimeParsingSupported", out bool isEnabled)
				? isEnabled
				: IsXamlRuntimeParsingSupportedByDefault;

		internal static bool IsIVisualAssemblyScanningEnabled =>
			AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled", out bool isEnabled)
				? isEnabled
				: IsIVisualAssemblyScanningEnabledByDefault;
	}
}
