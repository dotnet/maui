using System;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Element.xml" path="Type[@FullName='Microsoft.Maui.Controls.Element']/Docs" />
	public partial class Element
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='ControlsElementMapper']/Docs" />
		public static IPropertyMapper<Maui.IElement, IElementHandler> ControlsElementMapper = new PropertyMapper<Element, IElementHandler>(ViewHandler.ViewMapper)
		{
			[AutomationProperties.IsInAccessibleTreeProperty.PropertyName] = MapAutomationPropertiesIsInAccessibleTree,
			[AutomationProperties.ExcludedWithChildrenProperty.PropertyName] = MapAutomationPropertiesExcludedWithChildren,
		};
	}
}
