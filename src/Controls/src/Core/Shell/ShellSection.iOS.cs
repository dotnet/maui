using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class ShellSection
    {
        // ── IAppearanceObserver callback ─────────────────────────────────────────
        // Called by the Shell appearance system when nav bar appearance changes
        // for this section (e.g. Shell.SetNavBarColor / Shell appearance attached props).
        internal static void OnNavBarAppearanceChanged(ShellSection section, ShellAppearance? appearance)
        {
            if (section.Handler is not ShellSectionHandler handler)
                return;

            var navController = handler._navigationController;
            if (navController is null)
                return;

            var tracker = handler._appearanceTracker;
            if (appearance is null)
                tracker?.ResetAppearance(navController);
            else
                tracker?.SetAppearance(navController, appearance);

            // Also update MoreNavigationController when this section lives in the More tab
            if (handler.IsInMoreTab &&
                navController.ParentViewController is UITabBarController tabVC)
            {
                var moreNav = tabVC.MoreNavigationController;
                if (appearance is null)
                    tracker?.ResetAppearance(moreNav);
                else
                    tracker?.SetAppearance(moreNav, appearance);
            }
        }

        // ── Tab bar item helper ───────────────────────────────────────────────────
        // Delegates to the handler's own UpdateTabBarItem() which handles
        // image loading, title, and accessibility identifier.
        // Badge updates are handled at the ShellItemHandler level via PropertyChanged.
        internal static void OnTabBarItemNeedsUpdate(ShellSection section)
        {
            if (section.Handler is not ShellSectionHandler handler)
                return;

            handler.UpdateTabBarItem();
        }

        // ── Mapper implementations ────────────────────────────────────────────────

        static void MapTitle(ShellSectionHandler handler, ShellSection section)
            => OnTabBarItemNeedsUpdate(section);

        static void MapIcon(ShellSectionHandler handler, ShellSection section)
            => OnTabBarItemNeedsUpdate(section);

        static void MapFlowDirection(ShellSectionHandler handler, ShellSection section)
            => handler.UpdateFlowDirectionForControls();

        // ── Nav bar visibility helpers ────────────────────────────────────────────
        // Called externally (e.g. from ShellPageRendererTracker) when the displayed
        // page's NavBarIsVisible or NavBarHasShadow attached property changes.

        internal static void UpdateNavBarVisibility(ShellSectionHandler handler, Page? displayedPage)
        {
            var navController = handler._navigationController;
            if (navController is null || displayedPage is null)
                return;

            bool visible = Shell.GetNavBarIsVisible(displayedPage);
            bool animated = Shell.GetNavBarVisibilityAnimationEnabled(displayedPage);
            navController.SetNavigationBarHidden(!visible, animated);
        }

        internal static void UpdateNavBarHasShadow(ShellSectionHandler handler, Page displayedPage)
        {
            if (handler._navigationController is null)
                return;

            handler._appearanceTracker?.SetHasShadow(
                handler._navigationController,
                Shell.GetNavBarHasShadow(displayedPage));
        }
    }
}

