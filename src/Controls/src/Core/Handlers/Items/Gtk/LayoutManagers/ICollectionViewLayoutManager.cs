using Gtk;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using View = Gtk.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items.Platform
{
    /// <summary>
    /// Interface for positioning item views within a CollectionView
    /// </summary>
	public interface ICollectionViewLayoutManager
	{
        /// <summary>
        /// CollectionViewController interact with layout manager
        /// </summary>
		ICollectionViewController? Controller { get; set; }

        /// <summary>
        /// Whether the item is a layout horizontally 
        /// </summary>
		bool IsHorizontal { get; }

        /// <summary>
        /// Inform a view Size of CollectionView
        /// </summary>
        /// <param name="size"></param>
		void SizeAllocated(Size size);

        /// <summary>
        /// Calculate scrolling canvas size
        /// </summary>
        /// <returns>scrolling area size</returns>
		Size GetScrollCanvasSize();
       
        /// <summary>
        /// Layout items
        /// </summary>
        /// <param name="bound">A view port area on scrolling canvas</param>
        /// <param name="force">Forced layout</param>
		void LayoutItems(Rect bound, bool force = false);

        /// <summary>
        /// Gets item bound indicated by index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Bound of view</returns>
		Rect GetItemBound(int index);

        /// <summary>
        /// Inform a new item was inserted
        /// </summary>
        /// <param name="index">Index of new item</param>
		void ItemInserted(int index);

        /// <summary>
        /// Inform item was removed
        /// </summary>
        /// <param name="index">Index of reomved item</param>
		void ItemRemoved(int index);

        /// <summary>
        /// Inform item was updated
        /// </summary>
        /// <param name="index">Index of updated item</param>
		void ItemUpdated(int index);

        /// <summary>
        /// Inform item source was updated
        /// </summary>
		void ItemSourceUpdated();

        /// <summary>
        /// Reset layouting cache
        /// </summary>
		void Reset();

        /// <summary>
        /// Update item measuring result
        /// </summary>
        /// <param name="index">Index of updated item</param>
		void ItemMeasureInvalidated(int index);

        /// <summary>
        /// Get item index by position
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Index of postion</returns>
		int GetVisibleItemIndex(double x, double y);

        /// <summary>
        /// Get Item size to scroll at once
        /// </summary>
        /// <returns></returns>
		double GetScrollBlockSize();

        /// <summary>
        /// Get Item size to scroll at once
        /// </summary>
        /// <returns></returns>
        double GetScrollColumnSize();

        /// <summary>
        /// Sets header on layout
        /// </summary>
        /// <param name="header">Header view</param>
        /// <param name="size">Size of header</param>
		void SetHeader(Widget? header, Size size);

        /// <summary>
        /// Sets footer on layout
        /// </summary>
        /// <param name="footer">Fotter view</param>
        /// <param name="size">Size of footer</param>
		void SetFooter(Widget? footer, Size size);

        /// <summary>
        /// Gets index of next row item
        /// </summary>
        /// <param name="index">Current item index</param>
        /// <returns></returns>
        int NextRowItemIndex(int index);

        /// <summary>
        /// Gets index of previous row item
        /// </summary>
        /// <param name="index">Current item index</param>
        /// <returns></returns>
        int PreviousRowItemIndex(int index);
    }
}
