// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using Foundation;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class EmptySource : ILoopItemsViewSource
	{
		public int GroupCount => 0;

		public int ItemCount => 0;

		public bool Loop { get; set; }

		public int LoopCount => 0;

		public object this[NSIndexPath indexPath] => throw new IndexOutOfRangeException("IItemsViewSource is empty");

		public int ItemCountInGroup(nint group) => 0;

		public object Group(NSIndexPath indexPath)
		{
			throw new IndexOutOfRangeException("IItemsViewSource is empty");
		}

		public IItemsViewSource GroupItemsViewSource(NSIndexPath indexPath)
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