using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace Microsoft.Maui.Controls
{
    public partial class Shell
    {
        // ── Flyout gesture write-back ─────────────────────────────────────────────
        // Called when a pan gesture or tap-to-close changes the flyout open state.
        // Writes the new value back to Shell.FlyoutIsPresented via SetValueFromRenderer
        // so bindings and MVVM subscribers see the change.
        internal static void OnPresentedChangedByGesture(Shell shell, bool isPresented)
        {
            shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, isPresented);
        }

        // ── Status bar / home indicator ───────────────────────────────────────────
        // Propagate to the current item renderer's VC so UIKit re-queries the
        // preference from the full VC hierarchy (TabBarController → NavController → Page).

        static void MapPrefersHomeIndicatorAutoHidden(ShellHandler handler, Shell shell)
        {
            ((IShellContext)handler).CurrentShellItemRenderer?.ViewController
                ?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
        }

        static void MapPrefersStatusBarHidden(ShellHandler handler, Shell shell)
        {
            ((IShellContext)handler).CurrentShellItemRenderer?.ViewController
                ?.SetNeedsStatusBarAppearanceUpdate();
        }

        static void MapPreferredStatusBarUpdateAnimation(ShellHandler handler, Shell shell)
        {
            ((IShellContext)handler).CurrentShellItemRenderer?.ViewController
                ?.SetNeedsStatusBarAppearanceUpdate();
        }

        // ── FlyoutIcon / ForegroundColor ──────────────────────────────────────────
        // These properties affect the toolbar items on displayed pages.
        // Trigger an update via the active ShellSectionHandler's page tracker.

        static void MapFlyoutIcon(ShellHandler handler, Shell shell)
            => TriggerLeftBarButtonUpdate(handler, shell);

        static void MapForegroundColor(ShellHandler handler, Shell shell)
        {
            TriggerLeftBarButtonUpdate(handler, shell);
        }

        // Finds the current visible page's ShellPageRendererTracker and refreshes
        // its toolbar items so FlyoutIcon / ForegroundColor are reflected immediately.
        static void TriggerLeftBarButtonUpdate(ShellHandler handler, Shell shell)
        {
            var section = shell.CurrentItem?.CurrentItem;
            if (section?.Handler is not ShellSectionHandler sectionHandler)
                return;

            var displayedPage = section.DisplayedPage;
            if (displayedPage is null)
                return;

            if (sectionHandler._trackers.TryGetValue(displayedPage, out var tracker) &&
                tracker is ShellPageRendererTracker shellRendererTracker)
            {
                shellRendererTracker.UpdateToolbarItemsInternal();
            }
        }
    }
}

