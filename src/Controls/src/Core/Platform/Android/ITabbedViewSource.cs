#nullable disable
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers;

/// <summary>
/// Internal interface that provides tab management data to <see cref="TabbedViewManager"/>.
/// Contains the same tab properties as <see cref="ITabbedView"/> but does NOT extend <see cref="IView"/>,
/// allowing Shell adapters to implement it without 40+ IView stub members.
/// <para>
/// Consumers: <see cref="ShellItemTabbedViewAdapter"/> (bottom tabs),
/// <see cref="ShellSectionTabbedViewAdapter"/> (top tabs), and future TabbedPage adapter (Phase 3).
/// </para>
/// </summary>
internal interface ITabbedViewSource
{
    IReadOnlyList<ITab> Tabs { get; }
    ITab CurrentTab { get; set; }
    Color BarBackgroundColor { get; }
    object BarBackground { get; }
    Color BarTextColor { get; }
    Color UnselectedTabColor { get; }
    Color SelectedTabColor { get; }
    TabBarPlacement TabBarPlacement { get; }
    int OffscreenPageLimit { get; }
    bool IsSwipePagingEnabled { get; }
    bool IsSmoothScrollEnabled { get; }
    event NotifyCollectionChangedEventHandler TabsChanged;
}
