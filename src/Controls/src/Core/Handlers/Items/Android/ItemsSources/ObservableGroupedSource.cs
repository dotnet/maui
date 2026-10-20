#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class ObservableGroupedSource : IGroupableItemsViewSource, ICollectionChangedNotifier, IObservableItemsViewSource
	{
		readonly GroupableItemsView _groupableItemsView;
		readonly ICollectionChangedNotifier _notifier;
		readonly IList _groupSource;
		readonly IDispatcher _dispatcher;
		readonly bool _disposeGroupSource;
		List<IItemsViewSource> _groups = new List<IItemsViewSource>();
		readonly bool _hasGroupHeaders;
		readonly bool _hasGroupFooters;
		bool _disposed;
		bool _hasNonGroupItems;

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
		public bool ObserveChanges { get; set; } = true;

		public ObservableGroupedSource(GroupableItemsView groupableItemsView, ICollectionChangedNotifier notifier)
			: this(groupableItemsView, notifier, null)
		{
		}

		internal ObservableGroupedSource(GroupableItemsView groupableItemsView, ICollectionChangedNotifier notifier, IDispatcher dispatcher)
		{
			_groupableItemsView = groupableItemsView;
			_notifier = notifier;
			var groupSource = groupableItemsView.ItemsSource;
			_dispatcher = dispatcher;
			if (dispatcher != null && groupSource is INotifyCollectionChanged)
			{
				_groupSource = new MarshalingObservableCollection(groupSource, dispatcher);
				_disposeGroupSource = true;
			}
			else
			{
				_groupSource = groupSource as IList ?? new ListSource(groupSource);
			}

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
				if (_groupSource[groupIndex].Equals(item))
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

		public object GetGroup(int groupIndex)
		{
			return _groupSource[groupIndex];
		}

		public IItemsViewSource GetGroupItemsViewSource(int groupIndex)
		{
			// The uint cast is being used as an optimization to handle both negative numbers and out-of-bounds indices in a single comparison
			if ((uint)groupIndex >= (uint)_groups.Count)
			{
				System.Diagnostics.Debug.WriteLine($"Invalid Group index: {groupIndex}, Group count: {_groups.Count}");
				return null;
			}

			return _groups[groupIndex];
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

		public void NotifyItemRangeChanged(IItemsViewSource group, int localStartIndex, int count)
		{
			localStartIndex = GetAbsolutePosition(group, localStartIndex);
			_notifier.NotifyItemRangeChanged(this, localStartIndex, count);
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
				if (_disposeGroupSource)
					((MarshalingObservableCollection)_groupSource).Dispose();
			}
		}

		void UpdateGroupTracking()
		{
			ClearGroupTracking();

			for (int n = 0; n < _groupSource.Count; n++)
			{
				var group = _groupSource[n];

				if (group is IEnumerable list && group is not string) // Exclude string: it implements IEnumerable<char> but is a scalar value, not a group
				{
					_groups.Add(CreateGroupSource(list));
				}
			}
			_hasNonGroupItems = _groupSource.Count != _groups.Count;
		}

		IItemsViewSource CreateGroupSource(IEnumerable group)
		{
			var source = ItemsSourceFactory.Create(group, _groupableItemsView, this, _dispatcher);
			source.HasFooter = _hasGroupFooters;
			source.HasHeader = _hasGroupHeaders;
			return source;
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
			if (!ObserveChanges)
			{
				return;
			}

			if (_groupSource is MarshalingObservableCollection)
				CollectionChanged(args);
			else
				DispatchCollectionChanged(args);
		}

		void DispatchCollectionChanged(NotifyCollectionChangedEventArgs args) =>
			_groupableItemsView.Dispatcher.DispatchIfRequired(() => CollectionChanged(args));

		void CollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			if (_hasNonGroupItems || ContainsNonGroupItems(args.NewItems) || ContainsNonGroupItems(args.OldItems))
			{
				Reload();
				return;
			}

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

		static bool ContainsNonGroupItems(IList items)
		{
			if (items == null)
				return false;

			for (int n = 0; n < items.Count; n++)
			{
				if (items[n] is not IEnumerable || items[n] is string)
					return true;
			}
			return false;
		}

		void Reload()
		{
			UpdateGroupTracking();
			_notifier.NotifyDataSetChanged();
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var groupIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);
			var absolutePosition = AdjustPositionForHeader(CountItemsInGroups(0, groupIndex));
			var itemCount = InsertGroups(groupIndex, args.NewItems);

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
			RemoveGroups(groupIndex, groupCount);

			if (itemCount == 1)
			{
				_notifier.NotifyItemRemoved(this, absolutePosition);

				return;
			}

			_notifier.NotifyItemRangeRemoved(this, absolutePosition, itemCount);
		}

		void RemoveGroups(int index, int count)
		{
			for (int n = 0; n < count; n++)
				_groups[index + n].Dispose();
			_groups.RemoveRange(index, count);
		}

		int InsertGroups(int index, IList groups)
		{
			if (groups.Count == 1)
			{
				var source = CreateGroupSource((IEnumerable)groups[0]);
				_groups.Insert(index, source);
				return source.Count;
			}

			var sources = new IItemsViewSource[groups.Count];
			var itemCount = 0;
			for (int n = 0; n < sources.Length; n++)
			{
				var source = CreateGroupSource((IEnumerable)groups[n]);
				sources[n] = source;
				itemCount += source.Count;
			}
			_groups.InsertRange(index, sources);
			return itemCount;
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

			var oldItemCount = CountItemsInGroups(oldStartIndex, groupCount);
			var firstGroupIndex = Math.Min(newStartIndex, oldStartIndex);
			var absolutePosition = GetAbsolutePosition(_groups[firstGroupIndex], 0);
			RemoveGroups(oldStartIndex, groupCount);
			var newItemCount = InsertGroups(newStartIndex, args.NewItems);
			if (newItemCount != oldItemCount)
			{
				_notifier.NotifyDataSetChanged();
			}
			else if (newItemCount == 1)
			{
				_notifier.NotifyItemChanged(this, absolutePosition);
			}
			else
			{
				_notifier.NotifyItemRangeChanged(this, absolutePosition, newItemCount);
			}
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + args.NewItems.Count;

			var itemCount = CountItemsInGroups(start, end - start);
			var absolutePosition = GetAbsolutePosition(_groups[start], 0);

			var movedGroups = _groups.GetRange(args.OldStartingIndex, args.NewItems.Count);
			_groups.RemoveRange(args.OldStartingIndex, args.NewItems.Count);
			_groups.InsertRange(args.NewStartingIndex, movedGroups);
			_notifier.NotifyItemRangeChanged(this, absolutePosition, itemCount);
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

		public (int group, int index) GetGroupAndIndex(int absolutePosition)
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
