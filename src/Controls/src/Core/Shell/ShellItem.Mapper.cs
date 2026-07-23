using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls
{
    public partial class ShellItem
    {
        internal static new void RemapForControls()
        {
#if IOS || MACCATALYST
			// Replace CurrentItem mapper with Controls-aware version that also
			// updates tab bar visibility after navigation.
			ShellItemHandler.Mapper.ReplaceMapping<ShellItem, ShellItemHandler>(
				nameof(ShellItem.CurrentItem), MapCurrentItem);

			// iOS-specific platform configuration mappers.
			ShellItemHandler.Mapper.AppendToMapping(
				PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName,
				MapPrefersHomeIndicatorAutoHidden);
			ShellItemHandler.Mapper.AppendToMapping(
				PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName,
				MapPrefersStatusBarHidden);
#endif
        }
    }
}
