using System;
using System.Collections;
using System.Collections.Generic;
using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	class ListSource : List<object>, IItemsViewSource
	{
		public ListSource()
		{
		}

		public ListSource(IEnumerable<object> enumerable) : base(enumerable)
		{

		}

		public ListSource(IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				Add(item);
			}
		}

		public void Dispose()
		{

		}

		public object this[NSIndexPath indexPath]
		{
			get
			{
				if (indexPath.Section > 0)
				{
					throw new ArgumentOutOfRangeException(nameof(indexPath));
				}

				return this[(int)indexPath.Item];
			}
		}

		public int GroupCount => 1;

		public int ItemCount => Count;

		public NSIndexPath GetIndexForItem(object item)
		{
			for (int n = 0; n < Count; n++)
			{
				if (this[n] == item)
				{
					return NSIndexPath.Create(0, n);
				}
			}

			return NSIndexPath.Create(-1, -1);
		}

		public object Group(NSIndexPath indexPath)
		{
			return null;
		}

		public int ItemCountInGroup(nint group)
		{
			if (group > 0)
			{
				throw new ArgumentOutOfRangeException(nameof(group));
			}

			return Count;
		}
	}
}