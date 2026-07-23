using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class ShellItem
    {
        // ── IAppearanceObserver callback ─────────────────────────────────────────
        // Called by the Shell appearance system when tab bar appearance changes
        // for this item (e.g. Shell.SetTabBarColor / Shell appearance attached props).
        internal static void OnTabBarAppearanceChanged(ShellItem item, ShellAppearance? appearance)
        {
            if (item.Handler is not ShellItemHandler handler)
                return;

            var tabBarController = handler._tabBarController;
            if (tabBarController is null)
                return;

            var tracker = handler._appearanceTracker;
            if (appearance is null)
                tracker?.ResetAppearance(tabBarController);
            else
                tracker?.SetAppearance(tabBarController, appearance);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        // Walks ShellItem → ShellSection → ShellContent → Page → ViewController.
        // Used for status bar and home indicator delegation.
        static UIViewController? GetCurrentPageViewController(ShellItem item)
        {
            if (item.Handler is not ShellItemHandler handler)
                return null;

            var currentSection = item.CurrentItem;
            if (currentSection is null)
                return null;

            var content = currentSection.CurrentItem;
            if (content is null)
                return null;

            var page = ((IShellContentController)content).GetOrCreateContent();
            return (page?.Handler as IPlatformViewHandler)?.ViewController;
        }

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

        // ── Large titles (per displayed page) ────────────────────────────────────
        internal static void UpdateLargeTitleDisplayMode(ShellItemHandler handler, Page displayedPage)
        {
            if (displayedPage is null ||
                !displayedPage.IsSet(
                    PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty))
                return;

            // Read the value directly to avoid 'Page' type/static-class ambiguity
            // that occurs when calling OnThisPlatform() inside the Controls namespace.
            var mode = (LargeTitleDisplayMode)displayedPage.GetValue(
                PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty);

            var selectedVC = handler._tabBarController?.SelectedViewController;

            if (selectedVC is UINavigationController navController)
            {
                navController.NavigationBar.PrefersLargeTitles =
                    mode != LargeTitleDisplayMode.Never;

                var top = navController.TopViewController;
                if (top is not null)
                {
                    top.NavigationItem.LargeTitleDisplayMode = mode switch
                    {
                        LargeTitleDisplayMode.Always => UINavigationItemLargeTitleDisplayMode.Always,
                        LargeTitleDisplayMode.Automatic => UINavigationItemLargeTitleDisplayMode.Automatic,
                        _ => UINavigationItemLargeTitleDisplayMode.Never
                    };
                }
            }
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

