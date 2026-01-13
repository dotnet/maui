using Microsoft.Maui.Controls;

// This XmlnsDefinition is declared IN the ExternalAssembly for the global xmlns.
// This should NOT be loaded by consuming assemblies - the filter should block it.
// Only the assembly that CONSUMES this library should be able to add types to global xmlns.
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/dotnet/maui/global",
	"Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497Negative")]

namespace Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly.Maui33497Negative;

/// <summary>
/// Type that an external library tries to add to global xmlns.
/// This should NOT be resolvable via global xmlns in consuming assemblies.
/// </summary>
public enum ExternalGlobalType
{
	Value1,
	Value2
}
