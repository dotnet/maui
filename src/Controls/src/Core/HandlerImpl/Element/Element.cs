using System;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static IPropertyMapper<Maui.IElement, IElementHandler> ControlsElementMapper = new PropertyMapper<Element, IElementHandler>(ViewHandler.ViewMapper)
		{
			[AutomationProperties.IsInAccessibleTreeProperty.PropertyName] = MapIsInAccessibleTree,
			[AutomationProperties.ExcludedWithChildrenProperty.PropertyName] = MapExcludedWithChildren,
		};
	}
}
