using System;
using System.Collections;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class LoopObservableItemsSource : ObservableItemsSource, ILoopItemsViewSource
	{
		const int LoopBy = 3;

		public LoopObservableItemsSource(IEnumerable itemSource, UICollectionViewController collectionViewController, bool loop, int group = -1) : base(itemSource, collectionViewController, group)
		{
			Loop = loop;
		}

		public bool Loop { get; set; }

		public int LoopCount => Loop ? Count * LoopBy : Count;

		protected override NSIndexPath[] CreateIndexesFrom(int startIndex, int count)
		{
			if (Loop)
				count *= LoopBy;

			var result = new NSIndexPath[count];

			for (int n = 0; n < count; n++)
			{
				var index = startIndex + n;
				if (Loop)
				{
					index = startIndex + n * Count;
				}
				result[n] = NSIndexPath.Create(Section, index);
			}

			return result;
		}
	}
}
