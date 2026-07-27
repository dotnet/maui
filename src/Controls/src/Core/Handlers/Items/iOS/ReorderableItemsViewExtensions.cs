using Foundation;

namespace Microsoft.Maui.Controls.Handlers.Items;

/// <summary>
/// Shared helper methods for reorderable items view delegators.
/// Consolidates logic shared between Items/ and Items2/ handler stacks.
/// </summary>
static class ReorderableItemsViewExtensions
{
    /// <summary>
    /// Determines the target index path for a grouped reorder move, handling cross-group
    /// movement, empty group redirects, and boundary validation.
    /// </summary>
    internal static NSIndexPath GetTargetIndexPathForGroupedMove(
        NSIndexPath originalIndexPath,
        NSIndexPath proposedIndexPath,
        ReorderableItemsView itemsView,
        IItemsViewSource itemsSource,
        bool hasInteractivelyMoved)
    {
        if (originalIndexPath.Section != proposedIndexPath.Section && !itemsView.CanMixGroups)
        {
            return originalIndexPath;
        }

        var totalSections = itemsSource.GroupCount;

        // UICollectionView falls back to proposedIndexPath == originalIndexPath when the
        // user drags over an area with no cells (e.g. an empty group's header region).
        // In that case, redirect to the nearest empty group so the drop can succeed.
        // Guard with hasInteractivelyMoved to avoid redirecting at drag start before the
        // user has actually moved, when original == proposed is also true.
        if (originalIndexPath.Equals(proposedIndexPath) && itemsView.CanMixGroups && hasInteractivelyMoved)
        {
            var emptyGroupTarget = FindNearestEmptyGroup(itemsSource, totalSections, originalIndexPath.Section);
            if (emptyGroupTarget != null)
            {
                return emptyGroupTarget;
            }
        }

        if (proposedIndexPath.Section >= totalSections)
        {
            return originalIndexPath;
        }

        var targetGroupItemCount = itemsSource.ItemCountInGroup(proposedIndexPath.Section);

        if (targetGroupItemCount == 0)
        {
            return NSIndexPath.FromRowSection(0, proposedIndexPath.Section);
        }

        if (proposedIndexPath.Row >= targetGroupItemCount)
        {
            if (proposedIndexPath.Section < totalSections - 1)
            {
                var nextSectionItemCount = itemsSource.ItemCountInGroup(proposedIndexPath.Section + 1);
                if (nextSectionItemCount == 0)
                {
                    return NSIndexPath.FromRowSection(0, proposedIndexPath.Section + 1);
                }
            }

            return NSIndexPath.FromRowSection(targetGroupItemCount, proposedIndexPath.Section);
        }

        return proposedIndexPath;
    }

    /// <summary>
    /// Searches outward from the current section to find the closest empty group.
    /// Returns <c>null</c> if no empty group exists.
    /// </summary>
    internal static NSIndexPath? FindNearestEmptyGroup(IItemsViewSource itemsSource, int totalSections, nint currentSection)
    {
        for (int offset = 1; offset < totalSections; offset++)
        {
            var before = (int)currentSection - offset;
            if (before >= 0 && itemsSource.ItemCountInGroup(before) == 0)
            {
                return NSIndexPath.FromRowSection(0, before);
            }

            var after = (int)currentSection + offset;
            if (after < totalSections && itemsSource.ItemCountInGroup(after) == 0)
            {
                return NSIndexPath.FromRowSection(0, after);
            }
        }
        return null;
    }
}
