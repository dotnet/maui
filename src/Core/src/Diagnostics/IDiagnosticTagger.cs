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
	/// <remarks>
	/// The TagList is passed by reference to allow the method to modify it directly instead of a copy.
	/// </remarks>
	/// <param name="source">The object to use to generate tags.</param>
	/// <param name="tagList">The <see cref="TagList"/> struct to add tags to.</param>
	void AddTags(object? source, ref TagList tagList);
}
