using System.Diagnostics;
using Microsoft.Maui.Diagnostics;

namespace Microsoft.Maui.Controls.Diagnostics;

/// <summary>
/// A diagnostic tagger for Controls' Element and VisualElement instances.
/// </summary>
internal class ControlsViewDiagnosticTagger : IDiagnosticTagger
{
	/// <inheritdoc/>
	public void AddTags(object? source, ref TagList tagList)
	{
		if (source is Element element)
		{
			tagList.Add("element.id", element.Id);
			tagList.Add("element.automation_id", element.AutomationId);
			tagList.Add("element.class_id", element.ClassId);
			tagList.Add("element.style_id", element.StyleId);
		}

		if (source is VisualElement ve)
		{
			tagList.Add("element.class", ve.@class);
			tagList.Add("element.frame", ve.Frame);
		}
	}
}
