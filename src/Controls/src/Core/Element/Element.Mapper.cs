#nullable disable
using System;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Element.xml" path="Type[@FullName='Microsoft.Maui.Controls.Element']/Docs/*" />
	public partial class Element
	{
		public static IPropertyMapper<Maui.IElement, IElementHandler> ControlsElementMapper = new PropertyMapper<IElement, IElementHandler>(ViewHandler.ViewMapper);

		internal static void RemapForControls()
		{
			ViewHandler.ViewMapper.AppendToMapping(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, MapAutomationPropertiesIsInAccessibleTree);
			ViewHandler.ViewMapper.AppendToMapping(AutomationProperties.ExcludedWithChildrenProperty.PropertyName, MapAutomationPropertiesExcludedWithChildren);
		}
	}
}
