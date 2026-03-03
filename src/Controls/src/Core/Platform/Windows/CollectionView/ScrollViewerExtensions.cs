using Microsoft.UI.Xaml.Controls;
using WinUIScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;

namespace Microsoft.Maui.Controls.Platform;

internal static class ScrollViewerExtensions
{
    internal static void UpdateVerticalScrollBarVisibility(this ScrollViewer scrollViewer, ScrollBarVisibility visibility)
    {
        scrollViewer.VerticalScrollBarVisibility = visibility.ToWindowsScrollBarVisibility();
    }

    internal static void UpdateHorizontalScrollBarVisibility(this ScrollViewer scrollViewer, ScrollBarVisibility visibility)
    {
        scrollViewer.HorizontalScrollBarVisibility = visibility.ToWindowsScrollBarVisibility();
    }

    static WinUIScrollBarVisibility ToWindowsScrollBarVisibility(this ScrollBarVisibility visibility)
    {
        return visibility switch
        {
            ScrollBarVisibility.Always => WinUIScrollBarVisibility.Visible,
            ScrollBarVisibility.Never => WinUIScrollBarVisibility.Hidden,
            _ => WinUIScrollBarVisibility.Auto,
        };
    }
}
