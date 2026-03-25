namespace Microsoft.Maui
{
    /// <summary>
    /// Represents a single tab within an <see cref="ITabbedView"/>.
    /// </summary>
    public interface ITab
    {
        /// <summary>
        /// Gets the title text displayed for this tab.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the icon image source for this tab.
        /// </summary>
        IImageSource? Icon { get; }

        /// <summary>
        /// Gets whether this tab is enabled and can be selected.
        /// </summary>
        bool IsEnabled { get; }
    }
}
