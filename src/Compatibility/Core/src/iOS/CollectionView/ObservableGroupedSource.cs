using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class ObservableGroupedSource : IItemsViewSource
	{
		readonly UICollectionView _collectionView;
		readonly UICollectionViewController _collectionViewController;
		readonly IList _groupSource;
		bool _disposed;
		List<ObservableItemsSource> _groups = new List<ObservableItemsSource>();

		public ObservableGroupedSource(IEnumerable groupSource, UICollectionViewController collectionViewController)
		{
			_collectionViewController = collectionViewController;
			_collectionView = _collectionViewController.CollectionView;
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
				return GetGroupItemAt(indexPath.Section, (int)indexPath.Item);
			}
		}

		public int GroupCount => _groupSource.Count;

		public int ItemCount
		{
			get
			{
				var total = 0;

				for (int n = 0; n < _groupSource.Count; n++)
				{
					total += GetGroupCount(n);
				}

				return total;
			}
		}

		public NSIndexPath GetIndexForItem(object item)
		{
			for (int i = 0; i < _groupSource.Count; i++)
			{
				var j = IndexInGroup(item, _groupSource[i]);

				if (j == -1)
				{
					continue;
				}

				return NSIndexPath.Create(i, j);
			}

			return NSIndexPath.Create(-1, -1);
		}

		public object Group(NSIndexPath indexPath)
		{
			return _groupSource[indexPath.Section];
		}

		public int ItemCountInGroup(nint group)
		{
			return GetGroupCount((int)group);
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
				if (_groupSource[n] is INotifyCollectionChanged && _groupSource[n] is IEnumerable list)
				{
					_groups.Add(new ObservableItemsSource(list, _collectionViewController, n));
				}
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

		bool NotLoadedYet()
		{
			// If the UICollectionView hasn't actually been loaded, then calling InsertSections or DeleteSections is 
			// going to crash or get in an unusable state; instead, ReloadData should be used
			return !_collectionViewController.IsViewLoaded || _collectionViewController.View.Window == null;
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			if (ReloadRequired())
			{
				Reload();
				return;
			}

			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);
			var count = args.NewItems.Count;

			// Adding a group will change the section index for all subsequent groups, so the easiest thing to do
			// is to reset all the group tracking to get it up-to-date
			ResetGroupTracking();

			// Queue up the updates to the UICollectionView
			Update(() => _collectionView.InsertSections(CreateIndexSetFrom(startIndex, count)));
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a complete reload
				Reload();
				return;
			}

			if (ReloadRequired())
			{
				Reload();
				return;
			}

			// Removing a group will change the section index for all subsequent groups, so the easiest thing to do
			// is to reset all the group tracking to get it up-to-date
			ResetGroupTracking();

			// Since we have a start index, we can be more clever about removing the item(s) (and get the nifty animations)
			var count = args.OldItems.Count;

			// Queue up the updates to the UICollectionView
			Update(() => _collectionView.DeleteSections(CreateIndexSetFrom(startIndex, count)));
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newCount = args.NewItems.Count;

			if (newCount == args.OldItems.Count)
			{
				ResetGroupTracking();

				var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);

				// We are replacing one set of items with a set of equal size; we can do a simple item range update
				Update(() => _collectionView.ReloadSections(CreateIndexSetFrom(startIndex, newCount)));
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
				Update(() => _collectionView.MoveSection(args.OldStartingIndex, args.NewStartingIndex));
				return;
			}

			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count;

			Update(() => _collectionView.ReloadSections(CreateIndexSetFrom(start, end)));
		}

		int GetGroupCount(int groupIndex)
		{
			switch (_groupSource[groupIndex])
			{
				case IList list:
					return list.Count;
				case IEnumerable enumerable:
					var count = 0;
					var enumerator = enumerable.GetEnumerator();
					while (enumerator.MoveNext())
					{
						count += 1;
					}
					return count;
			}

			return 0;
		}

		object GetGroupItemAt(int groupIndex, int index)
		{
			switch (_groupSource[groupIndex])
			{
				case IList list:
					return list[index];
				case IEnumerable enumerable:
					var count = -1;
					var enumerator = enumerable.GetEnumerator();

					do
					{
						enumerator.MoveNext();
						count += 1;
					}
					while (count < index);

					return enumerator.Current;
			}

			return null;
		}

		int IndexInGroup(object item, object group)
		{
			switch (group)
			{
				case IList list:
					return list.IndexOf(item);
				case IEnumerable enumerable:
					var enumerator = enumerable.GetEnumerator();
					var index = 0;
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == item)
						{
							return index;
						}
					}
					return -1;
			}

			return -1;
		}

		bool ReloadRequired()
		{
			// If the UICollectionView has never been loaded, or doesn't yet have any sections, any insert/delete operations 
			// are gonna crash hard. We'll need to reload the data instead.

			return NotLoadedYet()
				|| _collectionView.NumberOfSections() == 0;
		}

		void Update(Action update)
		{
			if (_collectionView.Hidden)
			{
				return;
			}

			update();
		}
	}
}
