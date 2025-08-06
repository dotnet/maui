using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

internal class ViewDiagnosticTagger : IDiagnosticTagger
{
	public void AddTags(object? source, ref TagList tagList)
	{
		if (source is not IView view)
		{
			return;
		}

		tagList.Add("element.type", view.GetType().FullName);
	}
}
