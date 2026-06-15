#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
		: IControlsMapperRemappable
	{
		void IControlsMapperRemappable.RemapForControls(HashSet<Type> remapped) => RemapForControls(remapped);

		internal virtual void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(Element)))
			{
				ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, MapAutomationPropertiesIsInAccessibleTree);
				ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.ExcludedWithChildrenProperty.PropertyName, MapAutomationPropertiesExcludedWithChildren);
			}
		}
	}
}
