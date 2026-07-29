using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class ShellItem
    {
        // ── Helpers ───────────────────────────────────────────────────────────────

        static Shell? GetShell(ShellItem item) => item.FindParentOfType<Shell>();

        // ── IsEnabled per tab ─────────────────────────────────────────────────────
        // Called when ShellSection.IsEnabled changes to update the tab bar item state.
        internal static void MapIsEnabled(ShellItemHandler handler, ShellItem item)
        {
            if (handler._tabBarController.TabBar?.Items is null)
                return;

            var items = ((IShellItemController)item).GetItems();
            for (int i = 0; i < items.Count && i < handler._tabBarController.TabBar.Items.Length; i++)
                handler._tabBarController.TabBar.Items[i].Enabled = items[i].IsEnabled;
        }

        // ── Badge updates ─────────────────────────────────────────────────────────
        // Called when ShellSection.BadgeText/Color/TextColor changes.
        internal static void UpdateTabBarItemBadge(ShellItemHandler handler, ShellSection section, int index)
        {
            if (handler._tabBarController.TabBar?.Items is null ||
                index >= handler._tabBarController.TabBar.Items.Length)
                return;

            ShellItemHandler.UpdateTabBarItemBadge(
                handler._tabBarController.TabBar.Items[index],
                section);
        }

        // ── Tab bar visibility ────────────────────────────────────────────────────
        internal static void UpdateTabBarVisibility(ShellItemHandler handler, ShellItem item)
        {
            handler.UpdateTabBarHidden();
        }

        // ── Mapper implementations ────────────────────────────────────────────────

        static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
        {
            // Delegate to Core mapper (GoTo), then sync tab bar visibility.
            ShellItemHandler.MapCurrentItem(handler, item);
            UpdateTabBarVisibility(handler, item);
        }

        static void MapPrefersHomeIndicatorAutoHidden(ShellItemHandler handler, ShellItem item)
        {
            handler._tabBarController?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
        }

        static void MapPrefersStatusBarHidden(ShellItemHandler handler, ShellItem item)
        {
            handler._tabBarController?.SetNeedsStatusBarAppearanceUpdate();
        }
    }
}

