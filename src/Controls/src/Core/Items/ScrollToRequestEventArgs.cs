#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for scroll-to-item requests in items views.
	/// </summary>
	/// <remarks>
	/// This event args class contains information needed to scroll to a specific item or position in <see cref="CollectionView"/> and related controls.
	/// It supports two modes: scrolling by index position or by item reference, as indicated by the <see cref="Mode"/> property.
	/// </remarks>
	public class ScrollToRequestEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the scroll mode indicating whether to scroll by position or by element reference.
		/// </summary>
		/// <value>A <see cref="ScrollToMode"/> value indicating how the scroll target is specified.</value>
		public ScrollToMode Mode { get; }

		/// <summary>
		/// Gets the position where the target item should be positioned within the visible area.
		/// </summary>
		/// <value>A <see cref="ScrollToPosition"/> value specifying the alignment of the item after scrolling.</value>
		public ScrollToPosition ScrollToPosition { get; }
		
		/// <summary>
		/// Gets a value indicating whether the scrolling should be animated.
		/// </summary>
		/// <value><see langword="true"/> if the scroll should be animated; otherwise, <see langword="false"/> for an immediate jump.</value>
		public bool IsAnimated { get; }

		/// <summary>
		/// Gets the zero-based index of the item to scroll to.
		/// </summary>
		/// <value>The item index when <see cref="Mode"/> is <see cref="ScrollToMode.Position"/>; otherwise, not used.</value>
		public int Index { get; }
		
		/// <summary>
		/// Gets the zero-based index of the group containing the item to scroll to.
		/// </summary>
		/// <value>The group index when items are grouped and <see cref="Mode"/> is <see cref="ScrollToMode.Position"/>; otherwise, not used.</value>
		public int GroupIndex { get; }

		/// <summary>
		/// Gets the data item to scroll to.
		/// </summary>
		/// <value>The item object when <see cref="Mode"/> is <see cref="ScrollToMode.Element"/>; otherwise, <see langword="null"/>.</value>
		public object Item { get; }
		
		/// <summary>
		/// Gets the group object containing the item to scroll to.
		/// </summary>
		/// <value>The group object when items are grouped and <see cref="Mode"/> is <see cref="ScrollToMode.Element"/>; otherwise, <see langword="null"/>.</value>
		public object Group { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ScrollToRequestEventArgs"/> class for scrolling by position index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to scroll to.</param>
		/// <param name="groupIndex">The zero-based index of the group containing the item, or -1 if not grouped.</param>
		/// <param name="scrollToPosition">The position where the item should appear in the visible area.</param>
		/// <param name="isAnimated"><see langword="true"/> to animate the scroll; <see langword="false"/> to jump immediately.</param>
		public ScrollToRequestEventArgs(int index, int groupIndex,
			ScrollToPosition scrollToPosition, bool isAnimated)
		{
			Mode = ScrollToMode.Position;

			Index = index;
			GroupIndex = groupIndex;
			ScrollToPosition = scrollToPosition;
			IsAnimated = isAnimated;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScrollToRequestEventArgs"/> class for scrolling by item reference.
		/// </summary>
		/// <param name="item">The data item to scroll to.</param>
		/// <param name="group">The group object containing the item, or <see langword="null"/> if not grouped.</param>
		/// <param name="scrollToPosition">The position where the item should appear in the visible area.</param>
		/// <param name="isAnimated"><see langword="true"/> to animate the scroll; <see langword="false"/> to jump immediately.</param>
		public ScrollToRequestEventArgs(object item, object group,
			ScrollToPosition scrollToPosition, bool isAnimated)
		{
			Mode = ScrollToMode.Element;

			Item = item;
			Group = group;
			ScrollToPosition = scrollToPosition;
			IsAnimated = isAnimated;
		}
	}
}
