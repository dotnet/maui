namespace Microsoft.Maui
{
    /// <summary>
    /// Internal interface for views that can provide access to their window.
    /// This enables dependency injection for testing scenarios.
    /// </summary>
    // TODO Delete this in NET10 and just add it with a default implementation to IView
    internal interface IViewWithWindow
    {
        /// <summary>
        /// Gets the window associated with this view.
        /// </summary>
        IWindow? Window { get; }
    }
}