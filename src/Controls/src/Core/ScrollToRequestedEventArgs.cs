#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for scroll-to requests on scrollable views.</summary>
	public class ScrollToRequestedEventArgs : EventArgs, ITemplatedItemsListScrollToRequestedEventArgs
	{
		internal ScrollToRequestedEventArgs(double scrollX, double scrollY, bool shouldAnimate)
		{
			ScrollX = scrollX;
			ScrollY = scrollY;
			ShouldAnimate = shouldAnimate;
			Mode = ScrollToMode.Position;
		}

		internal ScrollToRequestedEventArgs(Element element, ScrollToPosition position, bool shouldAnimate)
		{
			Element = element;
			Position = position;
			ShouldAnimate = shouldAnimate;
			Mode = ScrollToMode.Element;
		}

		internal ScrollToRequestedEventArgs(object item, ScrollToPosition position, bool shouldAnimate)
		{
			Item = item;
			Position = position;
			ShouldAnimate = shouldAnimate;
			//Mode = ScrollToMode.Item;
		}

		internal ScrollToRequestedEventArgs(object item, object group, ScrollToPosition position, bool shouldAnimate)
		{
			Item = item;
			Group = group;
			Position = position;
			ShouldAnimate = shouldAnimate;
			//Mode = ScrollToMode.GroupAndIem;
		}

		/// <summary>Gets the element to scroll to when <see cref="Mode"/> is <see cref="ScrollToMode.Element"/>.</summary>
		public Element Element { get; private set; }

		/// <summary>Gets how the scroll request should be interpreted.</summary>
		public ScrollToMode Mode { get; private set; }

		/// <summary>Gets the desired final position of the target within the viewport.</summary>
		public ScrollToPosition Position { get; private set; }

		/// <summary>Gets the target horizontal scroll position when <see cref="Mode"/> is <see cref="ScrollToMode.Position"/>.</summary>
		public double ScrollX { get; private set; }

		/// <summary>Gets the target vertical scroll position when <see cref="Mode"/> is <see cref="ScrollToMode.Position"/>.</summary>
		public double ScrollY { get; private set; }

		/// <summary>Gets whether the scroll should be animated.</summary>
		public bool ShouldAnimate { get; private set; }

		internal object Group { get; private set; }
		object ITemplatedItemsListScrollToRequestedEventArgs.Group
		{
			get
			{
				return Group;
			}
		}

		internal object Item { get; private set; }
		object ITemplatedItemsListScrollToRequestedEventArgs.Item
		{
			get
			{
				return Item;
			}
		}

		public ScrollToRequest ToRequest()
		{
			return new ScrollToRequest(ScrollX, ScrollY, !ShouldAnimate);
		}
	}
}
