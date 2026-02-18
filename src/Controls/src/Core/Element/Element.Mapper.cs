#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		// Accessing this field from a derived type's static constructor forces
		// Element's static constructor to run first, guaranteeing that base-level
		// mapper remappings are applied before derived ones.
		private protected static bool s_forceStaticConstructor;

		static Element()
		{
			ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, MapAutomationPropertiesIsInAccessibleTree);
			ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.ExcludedWithChildrenProperty.PropertyName, MapAutomationPropertiesExcludedWithChildren);
		}
	}
}
