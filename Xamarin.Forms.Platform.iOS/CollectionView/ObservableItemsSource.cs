using System;
using System.Collections;
using System.Collections.Specialized;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ObservableItemsSource : IItemsViewSource
	{
		readonly UICollectionView _collectionView;
		readonly IList _itemsSource;

		public ObservableItemsSource(IEnumerable itemSource, UICollectionView collectionView)
		{
			_collectionView = collectionView;
			_itemsSource = (IList)itemSource;

			((INotifyCollectionChanged)itemSource).CollectionChanged += CollectionChanged;
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			// TODO hartez 2018/07/31 16:02:50 Handle the rest of these cases (implementing selection will make them much easier to test)	
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Reset:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static NSIndexPath[] CreateIndexesFrom(int startIndex, int count)
		{
			var result = new NSIndexPath[count];

			for (int n = 0; n < count; n++)
			{
				result[n] = NSIndexPath.Create(0, startIndex + n);
			}

			return result;
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _itemsSource.IndexOf(args.NewItems[0]);
			var count = args.NewItems.Count;

			_collectionView.InsertItems(CreateIndexesFrom(startIndex, count));
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex > -1 ? args.OldStartingIndex : _itemsSource.IndexOf(args.OldItems[0]);
			var count = args.OldItems.Count;

			_collectionView.DeleteItems(CreateIndexesFrom(startIndex, count));
		}

		public int Count => _itemsSource.Count;

		public object this[int index] => _itemsSource[index];
	}
}