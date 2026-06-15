#nullable disable
using System;
using System.Threading;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
		: IControlsMapperRemappable
	{
		void IControlsMapperRemappable.RemapForControls() => RemapForControls();

		static int s_remappedForControls;
		internal virtual void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, MapAutomationPropertiesIsInAccessibleTree);
			ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.ExcludedWithChildrenProperty.PropertyName, MapAutomationPropertiesExcludedWithChildren);
		}
	}
}
