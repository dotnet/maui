using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Defines a contract for adding diagnostic tags to a <see cref="TagList"/> based on a specific object.
/// </summary>
internal interface IDiagnosticTagger
{
	/// <summary>
	/// Called to add diagnostic tags to a <see cref="TagList"/> based on the provided source object.
	/// </summary>
	/// <param name="source">The object to use to generate tags.</param>
	/// <param name="tagList">The <see cref="TagList"/> to add tags to.</param>
	void AddTags(object? source, ref TagList tagList);
}
