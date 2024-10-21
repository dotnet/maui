#if MACCATALYST || IOS
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;

namespace MauiApp._1;

public class AppShellRenderer : ShellRenderer
{
    private sealed class AppShellTabBarAppearanceTracker : ShellTabBarAppearanceTracker
    {
        public override void SetAppearance(UITabBarController controller, ShellAppearance appearance)
        {
            base.SetAppearance(controller, appearance);

            // Should apply to iOS and Catalyst
            if (OperatingSystem.IsIOSVersionAtLeast(18) || OperatingSystem.IsMacCatalystVersionAtLeast(18))
            {
                controller.Mode = UITabBarControllerMode.TabSidebar;
                controller.Sidebar.Hidden = true;
                controller.TabBarHidden = true;
            }
        }
    }
    
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker() => new AppShellTabBarAppearanceTracker();
}
#endif