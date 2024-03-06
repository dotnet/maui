namespace Gtk.UIExtensions.NUI
{
    /// <summary>
    /// Enumerates values that specifies the behavior of snap points when scrolling..
    /// </summary>
    public enum SnapPointsType
    {
        /// <summary>
        /// indicates that scrolling does not snap to items.
        /// </summary>
        None,
        /// <summary>
        /// indicates that content always snaps to the closest snap point to where scrolling would naturally stop, along the direction of inertia.
        /// </summary>
        Mandatory,
        /// <summary>
        ///  indicates the same behavior as Mandatory, but only scrolls one item at a time.
        /// </summary>
        MandatorySingle,
    }
}