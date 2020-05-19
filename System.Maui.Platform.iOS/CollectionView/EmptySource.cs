using System;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	internal class EmptySource : IItemsViewSource
	{
		public int GroupCount => 0;

		public int ItemCount => 0;

		public object this[NSIndexPath indexPath] => throw new IndexOutOfRangeException("IItemsViewSource is empty");

		public int ItemCountInGroup(nint group)
		{
			return 0;
		}

		public object Group(NSIndexPath indexPath)
		{
			throw new IndexOutOfRangeException("IItemsViewSource is empty");
		}

		public NSIndexPath GetIndexForItem(object item)
		{
			throw new IndexOutOfRangeException("IItemsViewSource is empty");
		}

		public void Dispose()
		{
		}
	}
}