using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ObservableGroupedSource : IItemsViewSource
	{
		readonly UICollectionView _collectionView;
		readonly IList _groupSource;
		bool _disposed;
		List<ObservableItemsSource> _groups = new List<ObservableItemsSource>();

		public ObservableGroupedSource(IEnumerable groupSource, UICollectionView collectionView)
		{
			_collectionView = collectionView;
			_groupSource = groupSource as IList ?? new ListSource(groupSource);

			if (_groupSource is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += CollectionChanged;
			}

			ResetGroupTracking();
		}

		public object this[NSIndexPath indexPath]
		{
			get
			{
				var group = (IList)_groupSource[indexPath.Section];

				if (group.Count == 0)
				{
					return null;
				}

				return group[indexPath.Row];
			}
		}

		public int GroupCount => _groupSource.Count;

		int IItemsViewSource.ItemCount
		{
			get
			{
				// TODO hartez We should probably cache this value
				var total = 0;

				for (int n = 0; n < _groupSource.Count; n++)
				{
					var group = (IList)_groupSource[n];
					total += group.Count;
				}

				return total;
			}
		}

		public NSIndexPath GetIndexForItem(object item)
		{
			for (int i = 0; i < _groupSource.Count; i++)
			{
				var group = (IList)_groupSource[i];

				for (int j = 0; j < group.Count; j++)
				{
					if (group[j] == item)
					{
						return NSIndexPath.Create(i, j);
					}
				}
			}

			return NSIndexPath.Create(-1, -1);
		}

		public object Group(NSIndexPath indexPath)
		{
			return _groupSource[indexPath.Section];
		}

		public int ItemCountInGroup(nint group)
		{
			return ((IList)_groupSource[(int)group]).Count;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				ClearGroupTracking();
				if (_groupSource is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= CollectionChanged;
				}
			}
		}

		void ClearGroupTracking()
		{
			for (int n = _groups.Count - 1; n >= 0; n--)
			{
				_groups[n].Dispose();
				_groups.RemoveAt(n);
			}
		}

		void ResetGroupTracking()
		{
			ClearGroupTracking();

			for (int n = 0; n < _groupSource.Count; n++)
			{
				if (_groupSource[n] is INotifyCollectionChanged incc && _groupSource[n] is IList list)
				{
					_groups.Add(new ObservableItemsSource(list, _collectionView, n));
				}
			}
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
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
			ResetGroupTracking();
			_collectionView.ReloadData();
			_collectionView.CollectionViewLayout.InvalidateLayout();
		}

		NSIndexSet CreateIndexSetFrom(int startIndex, int count)
		{
			return NSIndexSet.FromNSRange(new NSRange(startIndex, count));
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);
			var count = args.NewItems.Count;

			// Adding a group will change the section index for all subsequent groups, so the easiest thing to do
			// is to reset all the group tracking to get it up-to-date
			ResetGroupTracking();

			_collectionView.InsertSections(CreateIndexSetFrom(startIndex, count));
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

			// Removing a group will change the section index for all subsequent groups, so the easiest thing to do
			// is to reset all the group tracking to get it up-to-date
			ResetGroupTracking();

			_collectionView.DeleteSections(CreateIndexSetFrom(startIndex, count));
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newCount = args.NewItems.Count;

			if (newCount == args.OldItems.Count)
			{
				ResetGroupTracking();

				var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);

				// We are replacing one set of items with a set of equal size; we can do a simple item range update
				_collectionView.ReloadSections(CreateIndexSetFrom(startIndex, newCount));
				return;
			}

			// The original and replacement sets are of unequal size; this means that everything currently in view will 
			// have to be updated. So we just have to use ReloadData and let the UICollectionView update everything
			Reload();
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			ResetGroupTracking();

			if (count == 1)
			{
				// For a single item, we can use MoveSection and get the animation
				_collectionView.MoveSection(args.OldStartingIndex, args.NewStartingIndex);
				return;
			}

			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count;

			_collectionView.ReloadSections(CreateIndexSetFrom(start, end));
		}
	}

}
