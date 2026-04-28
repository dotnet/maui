#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	internal static class VisualStateGroupListExtensions
	{
		internal static IList<VisualStateGroup> Clone(this IList<VisualStateGroup> groups)
		{
			var clone = new VisualStateGroupList();

			foreach (var group in groups)
			{
				group.VisualElement = clone.VisualElement;
				clone.Add(group.Clone());
			}
			
			// Preserve specificity when cloning (issue #27202)
			if (groups is VisualStateGroupList sourceList)
			{
				clone.Specificity = sourceList.Specificity;
			}

			if (VisualDiagnostics.IsEnabled && VisualDiagnostics.GetSourceInfo(groups) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

			return clone;
		}

		internal static bool HasVisualState(this VisualElement element, string name)
		{
			IList<VisualStateGroup> list = VisualStateManager.GetVisualStateGroups(element);
			for (var i = 0; i < list.Count; i++)
			{
				VisualStateGroup group = list[i];
				for (var j = 0; j < group.States.Count; j++)
				{
					if (group.States[j].Name == name)
						return true;
				}
			}

			return false;
		}

		internal static bool IsElementInSelectedState(this VisualElement element)
		{
			var groups = VisualStateManager.GetVisualStateGroups(element);
			foreach (var group in groups)
			{
				if (group.CurrentState?.Name == VisualStateManager.CommonStates.Selected)
				{
					return true;
				}
			}

			return false;
		}
	}
}
