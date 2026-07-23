using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls
{
    public partial class Shell
    {
        internal static new void RemapForControls()
        {
#if IOS || MACCATALYST
			// iOS-specific platform configuration mappers.
			// FlyoutBackground, FlyoutBackdrop etc. are already in the Core handler
			// Mapper (ShellHandler.iOS.cs) — no AppendToMapping needed for those.
			ShellHandler.Mapper.AppendToMapping(
				PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName,
				MapPrefersHomeIndicatorAutoHidden);
			ShellHandler.Mapper.AppendToMapping(
				PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName,
				MapPrefersStatusBarHidden);
			ShellHandler.Mapper.AppendToMapping(
				PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty.PropertyName,
				MapPreferredStatusBarUpdateAnimation);

			// FlyoutIcon and ForegroundColor affect toolbar items on displayed pages.
			ShellHandler.Mapper.AppendToMapping(nameof(Shell.FlyoutIcon), MapFlyoutIcon);
			ShellHandler.Mapper.AppendToMapping(Shell.ForegroundColorProperty.PropertyName, MapForegroundColor);
#endif
        }
    }
}
