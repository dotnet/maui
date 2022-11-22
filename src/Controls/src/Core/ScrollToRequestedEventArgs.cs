using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ScrollToRequestedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ScrollToRequestedEventArgs']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollToRequestedEventArgs.xml" path="//Member[@MemberName='Element']/Docs/*" />
		public Element Element { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollToRequestedEventArgs.xml" path="//Member[@MemberName='Mode']/Docs/*" />
		public ScrollToMode Mode { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollToRequestedEventArgs.xml" path="//Member[@MemberName='Position']/Docs/*" />
		public ScrollToPosition Position { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollToRequestedEventArgs.xml" path="//Member[@MemberName='ScrollX']/Docs/*" />
		public double ScrollX { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollToRequestedEventArgs.xml" path="//Member[@MemberName='ScrollY']/Docs/*" />
		public double ScrollY { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollToRequestedEventArgs.xml" path="//Member[@MemberName='ShouldAnimate']/Docs/*" />
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
