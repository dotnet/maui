using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

internal interface IDiagnosticTagger
{
	void AddTags(object? source, ref TagList tagList);
}
