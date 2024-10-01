#nullable disable
using System;
using System.Collections;
using Foundation;
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class LoopObservableItemsSource2 : Items.ObservableItemsSource, Items.ILoopItemsViewSource
	{
		int _section = 0;

		public LoopObservableItemsSource2(IEnumerable itemSource, UICollectionViewController collectionViewController, bool loop, int group = -1) : base(itemSource, collectionViewController, group)
		{
			Loop = loop;
		}

		public bool Loop { get; set; }

		protected override NSIndexPath[] CreateIndexesFrom(int startIndex, int count)
		{
			if (ItemCount == 0)
			{
				count += 2;
				startIndex = 0;
			}
			return IndexPathHelpers.GenerateIndexPathRange(_section, startIndex, count);
		}

		//We are going to add 2 items since we are inserting 1 item at the beginning and 1 item at the end
		public int LoopCount
		{
			get
			{
				var newCount = ItemCount;
				if (newCount > 0)
				{
					newCount = ItemCount + 2;
				}
				return newCount;
			}
		}

	}
}
