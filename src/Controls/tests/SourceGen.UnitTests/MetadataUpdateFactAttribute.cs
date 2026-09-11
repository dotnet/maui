#nullable enable

using System;
using System.Reflection.Metadata;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

internal static class MetadataUpdateRequirements
{
	internal const string EnableVariable = "DOTNET_MODIFIABLE_ASSEMBLIES";
	internal const string SkipMessage = "Requires runtime support for applying metadata updates. " +
		"Set DOTNET_MODIFIABLE_ASSEMBLIES=debug in the environment to run these tests.";

	internal static string? GetSkipReasonOrThrowIfMisconfigured()
		=> GetSkipReasonOrThrowIfMisconfigured(
			MetadataUpdater.IsSupported,
			Environment.GetEnvironmentVariable(EnableVariable));

	internal static string? GetSkipReasonOrThrowIfMisconfigured(bool isSupported, string? modifiableAssemblies)
	{
		if (isSupported)
			return null;

		if (!string.IsNullOrWhiteSpace(modifiableAssemblies))
		{
			throw new InvalidOperationException(
				$"Metadata update support was explicitly requested via {EnableVariable}={modifiableAssemblies}, " +
				"but MetadataUpdater.IsSupported is false. Fix the test host/runtime configuration rather than " +
				"silently skipping the hot reload tests.");
		}

		return SkipMessage;
	}
}

/// <summary>
/// A <see cref="FactAttribute"/> that skips the test when the runtime does not support applying
/// metadata updates (<see cref="MetadataUpdater.IsSupported"/> is <see langword="false"/>).
///
/// The XAML Incremental Hot Reload end-to-end tests apply real metadata deltas via
/// <see cref="MetadataUpdater.ApplyUpdate"/>. On CoreCLR that requires the process to be launched
/// with <c>DOTNET_MODIFIABLE_ASSEMBLIES=debug</c> (Visual Studio sets it automatically when
/// debugging; <c>dotnet test</c> honors it when set in the environment). When it is not set,
/// <see cref="MetadataUpdater.IsSupported"/> is <see langword="false"/> and the tests cannot run,
/// so they are skipped rather than failed. If metadata updates were explicitly requested via
/// <c>DOTNET_MODIFIABLE_ASSEMBLIES</c> but the runtime still reports
/// <see cref="MetadataUpdater.IsSupported"/> as <see langword="false"/>, test discovery throws so
/// the configuration issue cannot silently go green. This mirrors how dotnet/runtime gates its own
/// <c>ApplyUpdate</c> tests while still surfacing misconfiguration.
/// </summary>
public sealed class MetadataUpdateFactAttribute : FactAttribute
{
	public MetadataUpdateFactAttribute()
	{
		Skip = MetadataUpdateRequirements.GetSkipReasonOrThrowIfMisconfigured();
	}
}

/// <summary>
/// A <see cref="TheoryAttribute"/> that skips the test when the runtime does not support applying
/// metadata updates (<see cref="MetadataUpdater.IsSupported"/> is <see langword="false"/>).
/// </summary>
public sealed class MetadataUpdateTheoryAttribute : TheoryAttribute
{
	public MetadataUpdateTheoryAttribute()
	{
		Skip = MetadataUpdateRequirements.GetSkipReasonOrThrowIfMisconfigured();
	}
}
