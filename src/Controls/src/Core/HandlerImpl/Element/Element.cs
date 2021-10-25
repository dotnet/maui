using System;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static IPropertyMapper<Microsoft.Maui.IElement, IElementHandler> Mapper = new ControlsRemapper<Element, IElementHandler>(ViewHandler.ViewMapper)
		{
			[AutomationProperties.IsInAccessibleTreeProperty.PropertyName] = MapIsInAccessibleTree,
			[AutomationProperties.ExcludedWithChildrenProperty.PropertyName] = MapExcludedWithChildren,
		};

		public static void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text
			// And we map some of the other property handlers to Controls-specific versions that avoid stepping on HTML text settings


			if (Mapper is ControlsRemapper<Element, IElementHandler> cr)
			{
				// Grab all keys that the user set themselves on here and propagate them to the ViewhandlerMapper
				foreach (var kvp in cr.Mapper)
				{
					ElementHandler.ElementMapper.Add(kvp.Key, cr.Mapper[kvp.Key]);
				}
			}

			Mapper = ElementHandler.ElementMapper;
		}
	}
}
