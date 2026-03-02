using System.Threading.Tasks;
using Android.Graphics.Drawables;
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Microsoft.Maui.Controls.Platform;

/// <summary>
/// Represents an item that can be displayed in a bottom navigation view or tab layout.
/// This interface enables code sharing between TabbedPage and Shell tab handling.
/// </summary>
public interface ITabItem
{
    /// <summary>
    /// Gets the title text to display for this tab item.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the icon image source for this tab item.
    /// </summary>
    ImageSource? Icon { get; }

    /// <summary>
    /// Gets whether this tab item is enabled and can be selected.
    /// </summary>
    bool IsEnabled { get; }
}

/// <summary>
/// Provides extension methods for loading tab item icons.
/// </summary>
public static class TabItemExtensions
{
    /// <summary>
    /// Loads the icon drawable for a tab item asynchronously.
    /// </summary>
    /// <param name="tabItem">The tab item to load the icon for.</param>
    /// <param name="mauiContext">The MAUI context for image loading.</param>
    /// <returns>The loaded drawable, or null if no icon is set.</returns>
    public static async Task<Drawable?> GetIconDrawableAsync(this ITabItem tabItem, IMauiContext mauiContext)
    {
        if (tabItem.Icon is null)
        {
            return null;
        }

        var result = await tabItem.Icon.GetPlatformImageAsync(mauiContext);
        return result?.Value;
    }
}

/// <summary>
/// Adapter that wraps a TabbedPage child Page as an ITabItem.
/// </summary>
internal class PageTabItem : ITabItem
{
    private readonly Page _page;

    public PageTabItem(Page page)
    {
        _page = page ?? throw new System.ArgumentNullException(nameof(page));
    }

    public string Title => _page.Title ?? string.Empty;

    public ImageSource? Icon => _page.IconImageSource;

    public bool IsEnabled => _page.IsEnabled;

    /// <summary>
    /// Gets the underlying Page.
    /// </summary>
    public Page Page => _page;
}

/// <summary>
/// Adapter that wraps a ShellSection as an ITabItem.
/// </summary>
internal class ShellSectionTabItem : ITabItem
{
    private readonly ShellSection _section;

    public ShellSectionTabItem(ShellSection section)
    {
        _section = section ?? throw new System.ArgumentNullException(nameof(section));
    }

    public string Title => !string.IsNullOrWhiteSpace(_section.Title) ? _section.Title : "Tab";

    public ImageSource? Icon => _section.Icon;

    public bool IsEnabled => _section.IsEnabled;

    /// <summary>
    /// Gets the underlying ShellSection.
    /// </summary>
    public ShellSection Section => _section;
}
