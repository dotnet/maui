namespace Gtk.UIExtensions.NUI
{
    /// <summary>
    /// Enumerates values that control whether items in a collection view can or cannot be selected.
    /// </summary>
    public enum CollectionViewSelectionMode
    {
        /// <summary>
        /// Indicates that items cannot be selected.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that a single item can be selected.
        /// </summary>
        Single,

        /// <summary>
        /// Indicates that multiple items can be selected.
        /// </summary>
        Multiple,

        /// <summary>
        /// Indicates that a single item can be always selected.
        /// </summary>
        SingleAlways,
    }
}
