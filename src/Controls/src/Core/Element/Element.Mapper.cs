#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
		: IControlsMapperRemappable
	{
		void IControlsMapperRemappable.RemapForControls() => RemapForControls();

		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal virtual void RemapForControls()
		{
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			ViewHandler.ViewMapper.ReplaceMappingForControls<Maui.IElement, IElementHandler>(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, MapAutomationPropertiesIsInAccessibleTree);
			ViewHandler.ViewMapper.ReplaceMappingForControls<Maui.IElement, IElementHandler>(AutomationProperties.ExcludedWithChildrenProperty.PropertyName, MapAutomationPropertiesExcludedWithChildren);
		}
	}
}
