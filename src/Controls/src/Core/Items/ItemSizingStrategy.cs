namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies the strategy used to measure and size items in an items view.
	/// </summary>
	/// <remarks>
	/// The sizing strategy affects both performance and visual accuracy in collection views.
	/// For collections with uniform item sizes, use <see cref="MeasureFirstItem"/> for better performance.
	/// For collections with varying item sizes, use <see cref="MeasureAllItems"/> for accurate layout.
	/// </remarks>
	public enum ItemSizingStrategy
	{
		/// <summary>
		/// Each item is measured individually to determine its size.
		/// This provides the most accurate sizing but has a performance cost for large collections.
		/// </summary>
		MeasureAllItems,
		
		/// <summary>
		/// Only the first item is measured, and its size is used as a template for all items.
		/// This provides better performance but assumes all items have uniform size.
		/// </summary>
		MeasureFirstItem
	}
}