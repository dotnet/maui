#nullable enable
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Adapts a <see cref="ShellItem"/> as an <see cref="ITabbedViewSource"/> for bottom tab management.
    /// Used by <see cref="ShellItemHandler"/> to delegate tab management to <see cref="TabbedViewManager"/>.
    /// Bar colors are null because Shell uses appearance trackers for styling.
    /// </summary>
    internal class ShellItemTabbedViewAdapter : ITabbedViewSource
    {
        readonly ShellItem _shellItem;

        public ShellItemTabbedViewAdapter(ShellItem shellItem)
        {
            _shellItem = shellItem;
        }

        public IReadOnlyList<ITab> Tabs =>
            ((IShellItemController)_shellItem).GetItems()
                .Select(s => (ITab)new ShellSectionTab(s)).ToList();

        public ITab? CurrentTab
        {
            get => _shellItem.CurrentItem is not null ? new ShellSectionTab(_shellItem.CurrentItem) : null;
            set
            {
                if (value is ShellSectionTab tab && tab.Section != _shellItem.CurrentItem)
                {
                    // Use ProposeSection to fire Shell.Navigating event and support cancellation
                    ((IShellItemController)_shellItem).ProposeSection(tab.Section);
                }
            }
        }

        public int CurrentTabIndex =>
            _shellItem.CurrentItem is not null
                ? ((IShellItemController)_shellItem).GetItems().IndexOf(_shellItem.CurrentItem)
                : -1;

        // Bar colors are null — Shell applies appearance via IShellBottomNavViewAppearanceTracker
        public Color? BarBackgroundColor => null;
        public object? BarBackground => null;
        public Color? BarTextColor => null;
        public Color? UnselectedTabColor => null;
        public Color? SelectedTabColor => null;

        public TabBarPlacement TabBarPlacement => TabBarPlacement.Bottom;
        public int OffscreenPageLimit => 0; // Not used — Shell manages VP2 directly
        public bool IsSwipePagingEnabled => false; // Bottom tabs don't support swipe
        public bool IsSmoothScrollEnabled => false; // Not used

        public Element? Owner => _shellItem;

        public event NotifyCollectionChangedEventHandler TabsChanged
        {
            add => ((IShellItemController)_shellItem).ItemsCollectionChanged += value;
            remove => ((IShellItemController)_shellItem).ItemsCollectionChanged -= value;
        }
    }

    /// <summary>
    /// Adapts a <see cref="ShellSection"/> as an <see cref="ITabbedViewSource"/> for top tab management.
    /// Used by <see cref="ShellSectionHandler"/> to delegate tab management to <see cref="TabbedViewManager"/>.
    /// </summary>
    internal class ShellSectionTabbedViewAdapter : ITabbedViewSource
    {
        readonly ShellSection _shellSection;
        IShellSectionController SectionController => (IShellSectionController)_shellSection;

        public ShellSectionTabbedViewAdapter(ShellSection shellSection)
        {
            _shellSection = shellSection;
        }

        public IReadOnlyList<ITab> Tabs =>
            SectionController.GetItems()
                .Select(c => (ITab)new ShellContentTab(c)).ToList();

        public ITab? CurrentTab
        {
            get => _shellSection.CurrentItem is not null ? new ShellContentTab(_shellSection.CurrentItem) : null;
            set
            {
                if (value is ShellContentTab tab)
                {
                    _shellSection.CurrentItem = tab.Content;
                }
            }
        }

        public int CurrentTabIndex =>
            _shellSection.CurrentItem is not null
                ? SectionController.GetItems().IndexOf(_shellSection.CurrentItem)
                : -1;

        // Bar colors are null — Shell applies appearance via IShellTabLayoutAppearanceTracker
        public Color? BarBackgroundColor => null;
        public object? BarBackground => null;
        public Color? BarTextColor => null;
        public Color? UnselectedTabColor => null;
        public Color? SelectedTabColor => null;

        public TabBarPlacement TabBarPlacement => TabBarPlacement.Top;
        public int OffscreenPageLimit => 0; // Not used — Shell manages VP2 directly
        public bool IsSwipePagingEnabled => false; // Not used
        public bool IsSmoothScrollEnabled => false; // Not used

        public Element? Owner => _shellSection;

        public event NotifyCollectionChangedEventHandler TabsChanged
        {
            add => SectionController.ItemsCollectionChanged += value;
            remove => SectionController.ItemsCollectionChanged -= value;
        }
    }

    /// <summary>
    /// Wraps a <see cref="ShellSection"/> as an <see cref="ITab"/> for bottom tab items.
    /// </summary>
    internal class ShellSectionTab : ITab
    {
        readonly ShellSection _section;

        public ShellSectionTab(ShellSection section)
        {
            _section = section;
        }

        public string Title => _section.Title ?? string.Empty;
        public IImageSource Icon => _section.Icon;
        public bool IsEnabled => _section.IsEnabled;

        /// <summary>
        /// The underlying ShellSection — used by <see cref="ShellItemTabbedViewAdapter"/>
        /// to map tab selection back to ShellItem.CurrentItem.
        /// </summary>
        internal ShellSection Section => _section;

        public override bool Equals(object? obj) => obj is ShellSectionTab other && _section == other._section;
        public override int GetHashCode() => _section.GetHashCode();
    }

    /// <summary>
    /// Wraps a <see cref="ShellContent"/> as an <see cref="ITab"/> for top tab items.
    /// </summary>
    internal class ShellContentTab : ITab
    {
        readonly ShellContent _content;

        public ShellContentTab(ShellContent content)
        {
            _content = content;
        }

        public string Title => _content.Title ?? string.Empty;

        // Shell top tabs are text-only — icons were never shown in the old ShellSectionRenderer.
        // Return null to preserve this behavior (TabbedViewManager calls UpdateTabIcons for all tabs).
        public IImageSource? Icon => null;
        public bool IsEnabled => _content.IsEnabled;

        /// <summary>
        /// The underlying ShellContent — used by <see cref="ShellSectionTabbedViewAdapter"/>
        /// to map tab selection back to ShellSection.CurrentItem.
        /// </summary>
        internal ShellContent Content => _content;

        public override bool Equals(object? obj) => obj is ShellContentTab other && _content == other._content;
        public override int GetHashCode() => _content.GetHashCode();
    }
}
