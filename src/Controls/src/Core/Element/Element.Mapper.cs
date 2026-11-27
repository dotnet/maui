#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		static Element() => RemapIfNeeded();

		internal static void RemapIfNeeded()
		{
			RemappingHelper.RemapIfNeeded(typeof(Element), RemapForControls);
		}

		internal static void RemapForControls()
		{
			ViewHandler.ViewMapper.ReplaceMappingForControls<Maui.IElement, IElementHandler>(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, MapAutomationPropertiesIsInAccessibleTree);
			ViewHandler.ViewMapper.ReplaceMappingForControls<Maui.IElement, IElementHandler>(AutomationProperties.ExcludedWithChildrenProperty.PropertyName, MapAutomationPropertiesExcludedWithChildren);
		}
	}
}
