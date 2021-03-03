using System;
using System.Collections;
using System.Collections.Specialized;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class ObservableItemsSource : IItemsViewSource
	{
		readonly UICollectionViewController _collectionViewController;
		protected readonly UICollectionView CollectionView;
		readonly bool _grouped;
		readonly int _section;
		readonly IEnumerable _itemsSource;
		bool _disposed;

		public ObservableItemsSource(IEnumerable itemSource, UICollectionViewController collectionViewController, int group = -1)
		{
			_collectionViewController = collectionViewController;
			CollectionView = _collectionViewController.CollectionView;
		
			_section = group < 0 ? 0 : group;
			_grouped = group >= 0;

			_itemsSource = itemSource;

			Count = ItemsCount();

			((INotifyCollectionChanged)itemSource).CollectionChanged += CollectionChanged;
		}

		internal event NotifyCollectionChangedEventHandler CollectionViewUpdating;
		internal event NotifyCollectionChangedEventHandler CollectionViewUpdated;

		public int Count { get; private set; }

		public int Section => _section;

		public object this[int index] => ElementAt(index);

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					((INotifyCollectionChanged)_itemsSource).CollectionChanged -= CollectionChanged;
				}

				_disposed = true;
			}
		}

		public int ItemCountInGroup(nint group)
		{
			return Count;
		}

		public object Group(NSIndexPath indexPath)
		{
			return null;
		}

		public NSIndexPath GetIndexForItem(object item)
		{
			for (int n = 0; n < Count; n++)
			{
				if (this[n] == item)
				{
					return NSIndexPath.Create(_section, n);
				}
			}

			return NSIndexPath.Create(-1, -1);
		}

		public int GroupCount => 1;

		public int ItemCount => Count;

		public object this[NSIndexPath indexPath]
		{
			get
			{
				if (indexPath.Section != _section)
				{
					throw new ArgumentOutOfRangeException(nameof(indexPath));
				}

				return this[(int)indexPath.Item];
			}
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (Device.IsInvokeRequired)
			{
				Device.BeginInvokeOnMainThread(() => CollectionChanged(args));
			}
			else
			{
				CollectionChanged(args);
			}
		}

		void CollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			// Force UICollectionView to get the internal accounting straight 
			CollectionView.NumberOfItemsInSection(_section);

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					Reload();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void Reload()
		{
			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

			Count = ItemsCount();

			OnCollectionViewUpdating(args);

			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();

			OnCollectionViewUpdated(args);
		}

		protected virtual NSIndexPath[] CreateIndexesFrom(int startIndex, int count)
		{
			return IndexPathHelpers.GenerateIndexPathRange(_section, startIndex, count);
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;
			Count += count;
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : IndexOf(args.NewItems[0]);

			// Queue up the updates to the UICollectionView
			Update(() => CollectionView.InsertItems(CreateIndexesFrom(startIndex, count)), args);
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a ReloadData()
				Reload();
				return;
			}

			// If we have a start index, we can be more clever about removing the item(s) (and get the nifty animations)
			var count = args.OldItems.Count; 
			Count -= count;

			Update(() => CollectionView.DeleteItems(CreateIndexesFrom(startIndex, count)), args);
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newCount = args.NewItems.Count;

			if (newCount == args.OldItems.Count)
			{
				var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : IndexOf(args.NewItems[0]);

				// We are replacing one set of items with a set of equal size; we can do a simple item range update

				Update(() => CollectionView.ReloadItems(CreateIndexesFrom(startIndex, newCount)), args);
				return;
			}

			// The original and replacement sets are of unequal size; this means that everything currently in view will 
			// have to be updated. So we just have to use ReloadData and let the UICollectionView update everything
			Reload();
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			if (count == 1)
			{
				// For a single item, we can use MoveItem and get the animation
				var oldPath = NSIndexPath.Create(_section, args.OldStartingIndex);
				var newPath = NSIndexPath.Create(_section, args.NewStartingIndex);

				Update(() => CollectionView.MoveItem(oldPath, newPath), args);
				return;
			}

			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count;

			Update(() => CollectionView.ReloadItems(CreateIndexesFrom(start, end)), args);
		}

		internal int ItemsCount()
		{
			if (_itemsSource is IList list)
				return list.Count;

			int count = 0;
			foreach (var item in _itemsSource)
				count++;
			return count;
		}

		internal object ElementAt(int index)
		{
			if (_itemsSource is IList list)
				return list[index];

			int count = 0;
			foreach (var item in _itemsSource)
			{
				if (count == index)
					return item;
				count++;
			}

			return -1;
		}

		internal int IndexOf(object item)
		{
			if (_itemsSource is IList list)
				return list.IndexOf(item);

			int count = 0;
			foreach (var i in _itemsSource)
			{
				if (i == item)
					return count;
				count++;
			}

			return -1;
		}

		void Update(Action update, NotifyCollectionChangedEventArgs args)
		{
			if (CollectionView.Hidden)
			{
				return;
			}

			OnCollectionViewUpdating(args); 
			update(); 
			OnCollectionViewUpdated(args); 
		}

		void OnCollectionViewUpdating(NotifyCollectionChangedEventArgs args) 
		{
			CollectionViewUpdating?.Invoke(this, args);
		}

		void OnCollectionViewUpdated(NotifyCollectionChangedEventArgs args)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				CollectionViewUpdated?.Invoke(this, args);
			});
		}
	}
}
