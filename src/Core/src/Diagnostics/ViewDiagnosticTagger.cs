using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// A diagnostic tagger for IView instances.
/// </summary>
internal class ViewDiagnosticTagger : IDiagnosticTagger
{
	/// <inheritdoc/>
	public void AddTags(object? source, ref TagList tagList)
	{
		if (source is not IView view)
		{
			return;
		}

		tagList.Add("element.type", view.GetType().FullName);
	}
}
