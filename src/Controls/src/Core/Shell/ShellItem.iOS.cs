using Microsoft.Maui.Controls.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class ShellItem
    {
        // ── IsEnabled per tab ─────────────────────────────────────────────────────
        // Called when ShellSection.IsEnabled changes to update the tab bar item state.
        internal static void MapIsEnabled(ShellItemHandler handler, ShellItem item)
        {
            if (handler._tabBarController.TabBar?.Items is null)
            {
                return;
            }

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
    }
}

