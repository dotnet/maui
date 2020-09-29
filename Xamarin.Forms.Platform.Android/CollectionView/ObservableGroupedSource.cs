using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.Android
{
	internal class ObservableGroupedSource : IGroupableItemsViewSource, ICollectionChangedNotifier
	{
		readonly ICollectionChangedNotifier _notifier;
		readonly IList _groupSource;
		List<IItemsViewSource> _groups = new List<IItemsViewSource>();
		readonly bool _hasGroupHeaders;
		readonly bool _hasGroupFooters;
		bool _disposed;

		public int Count
		{
			get
			{
				var groupContents = 0;

				for (int n = 0; n < _groups.Count; n++)
				{
					groupContents += _groups[n].Count;
				}

				return (HasHeader ? 1 : 0)
					 + (HasFooter ? 1 : 0)
					 + groupContents;
			}
		}

		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }

		public ObservableGroupedSource(GroupableItemsView groupableItemsView, ICollectionChangedNotifier notifier)
		{
			var groupSource = groupableItemsView.ItemsSource;

			_notifier = notifier;
			_groupSource = groupSource as IList ?? new ListSource(groupSource);

			_hasGroupFooters = groupableItemsView.GroupFooterTemplate != null;
			_hasGroupHeaders = groupableItemsView.GroupHeaderTemplate != null;

			if (_groupSource is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += CollectionChanged;
			}

			UpdateGroupTracking();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public bool IsFooter(int position)
		{
			if (!HasFooter)
			{
				return false;
			}

			return position == Count - 1;
		}

		public bool IsHeader(int position)
		{
			return HasHeader && position == 0;
		}

		public bool IsGroupHeader(int position)
		{
			if (IsFooter(position) || IsHeader(position))
			{
				return false;
			}

			var (group, inGroup) = GetGroupAndIndex(position);

			return _groups[group].IsHeader(inGroup);
		}

		public bool IsGroupFooter(int position)
		{
			if (IsFooter(position) || IsHeader(position))
			{
				return false;
			}

			var (group, inGroup) = GetGroupAndIndex(position);

			return _groups[group].IsFooter(inGroup);
		}

		public int GetPosition(object item)
		{
			int previousGroupsOffset = 0;

			for (int groupIndex = 0; groupIndex < _groupSource.Count; groupIndex++)
			{
				if (_groupSource[groupIndex] == item)
				{
					return AdjustPositionForHeader(groupIndex);
				}

				var group = _groups[groupIndex];
				var inGroup = group.GetPosition(item);

				if (inGroup > -1)
				{
					return AdjustPositionForHeader(previousGroupsOffset + inGroup);
				}

				previousGroupsOffset += group.Count;
			}

			return -1;
		}

		public object GetItem(int position)
		{
			var (group, inGroup) = GetGroupAndIndex(position);

			if (IsGroupFooter(position) || IsGroupHeader(position))
			{
				// This is looping to find the group/index twice, need to make it less inefficient
				return _groupSource[group];
			}

			return _groups[group].GetItem(inGroup);
		}

		// The ICollectionChangedNotifier methods are called by child observable items sources (i.e., the groups)
		// This class can then translate their local changes into global positions for upstream notification 
		// (e.g., to the actual RecyclerView.Adapter, so that it can notify the RecyclerView and handle animating
		// the changes)
		public void NotifyDataSetChanged()
		{
			Reload();
		}

		public void NotifyItemChanged(IItemsViewSource group, int localIndex)
		{
			localIndex = GetAbsolutePosition(group, localIndex);
			_notifier.NotifyItemChanged(this, localIndex);
		}

		public void NotifyItemInserted(IItemsViewSource group, int localIndex)
		{
			localIndex = GetAbsolutePosition(group, localIndex);
			_notifier.NotifyItemInserted(this, localIndex);
		}

		public void NotifyItemMoved(IItemsViewSource group, int localFromIndex, int localToIndex)
		{
			localFromIndex = GetAbsolutePosition(group, localFromIndex);
			localToIndex = GetAbsolutePosition(group, localToIndex);
			_notifier.NotifyItemMoved(this, localFromIndex, localToIndex);
		}

		public void NotifyItemRangeChanged(IItemsViewSource group, int localStartIndex, int localEndIndex)
		{
			localStartIndex = GetAbsolutePosition(group, localStartIndex);
			localEndIndex = GetAbsolutePosition(group, localEndIndex);
			_notifier.NotifyItemRangeChanged(this, localStartIndex, localEndIndex);
		}

		public void NotifyItemRangeInserted(IItemsViewSource group, int localIndex, int count)
		{
			localIndex = GetAbsolutePosition(group, localIndex);
			_notifier.NotifyItemRangeInserted(this, localIndex, count);
		}

		public void NotifyItemRangeRemoved(IItemsViewSource group, int localIndex, int count)
		{
			localIndex = GetAbsolutePosition(group, localIndex);
			_notifier.NotifyItemRangeRemoved(this, localIndex, count);
		}

		public void NotifyItemRemoved(IItemsViewSource group, int localIndex)
		{
			localIndex = GetAbsolutePosition(group, localIndex);
			_notifier.NotifyItemRemoved(this, localIndex);
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

				if (_groupSource is INotifyCollectionChanged notifyCollectionChanged)
				{
					notifyCollectionChanged.CollectionChanged -= CollectionChanged;
				}
			}
		}

		void UpdateGroupTracking()
		{
			ClearGroupTracking();

			for (int n = 0; n < _groupSource.Count; n++)
			{
				var source = ItemsSourceFactory.Create(_groupSource[n] as IEnumerable, this);
				source.HasFooter = _hasGroupFooters;
				source.HasHeader = _hasGroupHeaders;
				_groups.Add(source);
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
			UpdateGroupTracking();
			_notifier.NotifyDataSetChanged();
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var groupIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);
			var groupCount = args.NewItems.Count;

			UpdateGroupTracking();

			// Determine the absolute starting position and the number of items in the groups being added
			var absolutePosition = GetAbsolutePosition(_groups[groupIndex], 0);
			var itemCount = CountItemsInGroups(groupIndex, groupCount);

			if (itemCount == 1)
			{
				_notifier.NotifyItemInserted(this, absolutePosition);
				return;
			}

			_notifier.NotifyItemRangeInserted(this, absolutePosition, itemCount);
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var groupIndex = args.OldStartingIndex;

			if (groupIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed groups was in the
				// collection. So the best we can do is a full reload.
				Reload();
				return;
			}

			// If we have a start index, we can be more clever about removing the group(s) (and get the nifty animations)
			var groupCount = args.OldItems.Count;

			var absolutePosition = GetAbsolutePosition(_groups[groupIndex], 0);

			// Figure out how many items are in the groups we're removing
			var itemCount = CountItemsInGroups(groupIndex, groupCount);

			if (itemCount == 1)
			{
				_notifier.NotifyItemRemoved(this, absolutePosition);

				UpdateGroupTracking();

				return;
			}

			_notifier.NotifyItemRangeRemoved(this, absolutePosition, itemCount);

			UpdateGroupTracking();
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var groupCount = args.NewItems.Count;

			if (groupCount != args.OldItems.Count)
			{
				// The original and replacement sets are of unequal size; this means that most everything currently in 
				// view will have to be updated. So just reload the whole thing.
				Reload();
				return;
			}

			var newStartIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);
			var oldStartIndex = args.OldStartingIndex > -1 ? args.OldStartingIndex : _groupSource.IndexOf(args.OldItems[0]);

			var newItemCount = CountItemsInGroups(newStartIndex, groupCount);
			var oldItemCount = CountItemsInGroups(oldStartIndex, groupCount);

			if (newItemCount != oldItemCount)
			{
				// The original and replacement sets are of unequal size; this means that most everything currently in 
				// view will have to be updated. So just reload the whole thing.
				Reload();
				return;
			}

			// We are replacing one set of items with a set of equal size; we can do a simple item or range notification 
			var firstGroupIndex = Math.Min(newStartIndex, oldStartIndex);
			var absolutePosition = GetAbsolutePosition(_groups[firstGroupIndex], 0);

			if (newItemCount == 1)
			{
				_notifier.NotifyItemChanged(this, absolutePosition);
				UpdateGroupTracking();
			}
			else
			{
				_notifier.NotifyItemRangeChanged(this, absolutePosition, newItemCount * 2);
				UpdateGroupTracking();
			}
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + args.NewItems.Count;

			var itemCount = CountItemsInGroups(start, end - start);
			var absolutePosition = GetAbsolutePosition(_groups[start], 0);

			_notifier.NotifyItemRangeChanged(this, absolutePosition, itemCount);

			UpdateGroupTracking();
		}

		int GetAbsolutePosition(IItemsViewSource group, int indexInGroup)
		{
			var groupIndex = _groups.IndexOf(group);

			var runningIndex = 0;

			for (int n = 0; n < groupIndex; n++)
			{
				runningIndex += _groups[n].Count;
			}

			return AdjustPositionForHeader(runningIndex + indexInGroup);
		}

		(int, int) GetGroupAndIndex(int absolutePosition)
		{
			absolutePosition = AdjustIndexForHeader(absolutePosition);

			var group = 0;
			var localIndex = 0;

			while (absolutePosition > 0)
			{
				localIndex += 1;

				if (localIndex == _groups[group].Count)
				{
					group += 1;
					localIndex = 0;
				}

				absolutePosition -= 1;
			}

			return (group, localIndex);
		}

		int AdjustIndexForHeader(int index)
		{
			return index - (HasHeader ? 1 : 0);
		}

		int AdjustPositionForHeader(int position)
		{
			return position + (HasHeader ? 1 : 0);
		}

		int CountItemsInGroups(int groupStartIndex, int groupCount)
		{
			var itemCount = 0;
			for (int n = 0; n < groupCount; n++)
			{
				itemCount += _groups[groupStartIndex + n].Count;
			}
			return itemCount;
		}
	}
}