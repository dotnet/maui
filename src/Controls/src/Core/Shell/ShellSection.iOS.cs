using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class ShellSection
    {
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
    }
}

