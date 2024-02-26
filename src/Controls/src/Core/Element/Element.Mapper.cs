#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		/// <summary>
		/// Maps UI information to platform-specific implementations for accessibility services
		/// </summary>
		[Obsolete("Use ViewHandler.ViewMapper instead.")]
		public static IPropertyMapper<Maui.IElement, IElementHandler> ControlsElementMapper = new ControlsMapper<IElement, IElementHandler>(ViewHandler.ViewMapper);

		internal static void RemapForControls()
		{
			ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, MapAutomationPropertiesIsInAccessibleTree);
			ViewHandler.ViewMapper.ReplaceMapping<Maui.IElement, IElementHandler>(AutomationProperties.ExcludedWithChildrenProperty.PropertyName, MapAutomationPropertiesExcludedWithChildren);
		}
	}
}
