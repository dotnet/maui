using System;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		public static IPropertyMapper<View, IViewHandler> ViewMapper = new PropertyMapper<View, IViewHandler>(ViewHandler.ViewMapper)
		{
			[AutomationProperties.IsInAccessibleTreeProperty.PropertyName] = MapIsInAccessibleTree,
			[AutomationProperties.ExcludedWithChildrenProperty.PropertyName] = MapExcludedWithChildren,
		};

		public static new void RemapForControls()
		{
			ViewHandler.ViewMapper = ViewMapper;
		}
	}
}
