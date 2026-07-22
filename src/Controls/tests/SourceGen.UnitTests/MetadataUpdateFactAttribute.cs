#nullable enable

using System.Reflection.Metadata;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// A <see cref="FactAttribute"/> that skips the test when the runtime does not support applying
/// metadata updates (<see cref="MetadataUpdater.IsSupported"/> is <see langword="false"/>).
///
/// The XAML Incremental Hot Reload end-to-end tests apply real metadata deltas via
/// <see cref="MetadataUpdater.ApplyUpdate"/>. On CoreCLR that requires the process to be launched
/// with <c>DOTNET_MODIFIABLE_ASSEMBLIES=debug</c> (Visual Studio sets it automatically when
/// debugging; <c>dotnet test</c> honors it when set in the environment). When it is not set,
/// <see cref="MetadataUpdater.IsSupported"/> is <see langword="false"/> and the tests cannot run,
/// so they are skipped rather than failed. This mirrors how dotnet/runtime gates its own
/// <c>ApplyUpdate</c> tests.
/// </summary>
public sealed class MetadataUpdateFactAttribute : FactAttribute
{
	public MetadataUpdateFactAttribute()
	{
		if (!MetadataUpdater.IsSupported)
		{
			Skip = "Requires runtime support for applying metadata updates. " +
				"Set DOTNET_MODIFIABLE_ASSEMBLIES=debug in the environment to run these tests.";
		}
	}
}
