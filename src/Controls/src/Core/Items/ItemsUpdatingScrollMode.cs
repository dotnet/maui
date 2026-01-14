using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies the scroll behavior when items are added, removed, or updated in an items view.
	/// </summary>
	public enum ItemsUpdatingScrollMode
	{
		/// <summary>
		/// The view scrolls to keep currently visible items in view as items are updated.
		/// This is the default behavior, maintaining the current scroll position relative to visible items.
		/// </summary>
		KeepItemsInView = 0,
		
		/// <summary>
		/// The view maintains the current scroll offset as items are updated.
		/// The absolute scroll position remains fixed, which may cause different items to become visible.
		/// </summary>
		KeepScrollOffset,
		
		/// <summary>
		/// The view scrolls to keep the last item visible as items are updated.
		/// This is useful for chat-like interfaces where new items are added at the end.
		/// </summary>
		KeepLastItemInView
	}
}
