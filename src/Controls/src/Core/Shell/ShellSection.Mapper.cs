using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls
{
    public partial class ShellSection
    {
        internal static new void RemapForControls()
        {
#if IOS || MACCATALYST
			// Replace Core mapper methods with Controls-aware versions.
			// ReplaceMapping adds the entry if it doesn't already exist.
			ShellSectionHandler.Mapper.ReplaceMapping<ShellSection, ShellSectionHandler>(
				nameof(BaseShellItem.Title), MapTitle);
			ShellSectionHandler.Mapper.ReplaceMapping<ShellSection, ShellSectionHandler>(
				nameof(BaseShellItem.Icon), MapIcon);
			ShellSectionHandler.Mapper.ReplaceMapping<ShellSection, ShellSectionHandler>(
				VisualElement.FlowDirectionProperty.PropertyName, MapFlowDirection);
#endif
        }
    }
}
