using Gtk.UIExtensions.Common;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Controls.Handlers.Items.Platform
{

    /// <summary>
    /// Interface to control CollectionView on internal modules
    /// these apis not open as CollectionView API
    /// </summary>
    public interface ICollectionViewController
    {
        /// <summary>
        /// Request realize a view, it create a view to represent a item
        /// </summary>
        /// <param name="index">index of item</param>
        /// <returns>Realized view</returns>
        ViewHolder RealizeView(int index);

        /// <summary>
        /// Request unrealize a view, it remove a view from CollectionView
        /// </summary>
        /// <param name="view">A view to unrealize</param>
        void UnrealizeView(ViewHolder view);

        /// <summary>
        /// Request layouting items newly
        /// </summary>
        void RequestLayoutItems();

        /// <summary>
        /// The number of items
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a item size
        /// </summary>
        /// <returns>Size of item</returns>
        Size GetItemSize();

        /// <summary>
        /// Gets a item size with contstraint
        /// </summary>
        /// <param name="widthConstraint">A width size that could be reached as maximum</param>
        /// <param name="heightConstraint">A height  size that could be reached as maximum</param>
        /// <returns>Size of item</returns>
        Size GetItemSize(double widthConstraint, double heightConstraint);

        /// <summary>
        /// Gets a item size with contstraint
        /// </summary>
        /// <param name="index">Index of item to get a size</param>
        /// <param name="widthConstraint">A width size that could be reached as maximum</param>
        /// <param name="heightConstraint">A height  size that could be reached as maximum</param>
        /// <returns>Size of item</returns>
        Size GetItemSize(int index, double widthConstraint, double heightConstraint);

        /// <summary>
        /// Notify scroll canvas size was changed
        /// </summary>
        void ContentSizeUpdated();

        /// <summary>
        /// Notify that item measure result is changed
        /// </summary>
        /// <param name="index"></param>
        public void ItemMeasureInvalidated(int index);

        /// <summary>
        /// Request item select
        /// </summary>
        /// <param name="index">Item index</param>
        public void RequestItemSelect(int index);

    }

}
