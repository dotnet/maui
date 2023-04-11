#nullable disable
using System;
using System.Collections;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
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
			if (!Loop)
			{
				return base.CreateIndexesFrom(startIndex, count);
			}

			var collectionView = CollectionView;
			if (collectionView is null)
				return Array.Empty<NSIndexPath>();

			return IndexPathHelpers.GenerateLoopedIndexPathRange(Section,
				(int)collectionView.NumberOfItemsInSection(Section), LoopBy, startIndex, count);
		}
	}
}
