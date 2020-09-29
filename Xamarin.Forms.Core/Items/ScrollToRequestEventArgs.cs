using System;

namespace Xamarin.Forms
{
	public class ScrollToRequestEventArgs : EventArgs
	{
		public ScrollToMode Mode { get; }

		public ScrollToPosition ScrollToPosition { get; }
		public bool IsAnimated { get; }

		public int Index { get; }
		public int GroupIndex { get; }

		public object Item { get; }
		public object Group { get; }

		public ScrollToRequestEventArgs(int index, int groupIndex,
			ScrollToPosition scrollToPosition, bool isAnimated)
		{
			Mode = ScrollToMode.Position;

			Index = index;
			GroupIndex = groupIndex;
			ScrollToPosition = scrollToPosition;
			IsAnimated = isAnimated;
		}

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