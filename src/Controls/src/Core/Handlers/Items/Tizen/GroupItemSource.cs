using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class GroupItemSource : IList, INotifyCollectionChanged, IDisposable
	{
		IList<IList> _groupSource = new List<IList>();
		IList<object> _originalGroupSource = new List<object>();
		INotifyCollectionChanged? _groupColletionChanged;
		int _groupHeaderFooterCount;
		bool _disposedValue;

		public GroupItemSource(GroupableItemsView itemsView)
		{
			ItemsView = itemsView;
			HasGroupHeader = ItemsView.GroupHeaderTemplate != null;
			HasGroupFooter = ItemsView.GroupFooterTemplate != null;
			_groupHeaderFooterCount = (HasGroupHeader ? 1 : 0) + (HasGroupFooter ? 1 : 0);
			BuildGroupSource(ItemsView.ItemsSource);
		}

		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		public object? this[int index]
		{
			get => GetItem(index);
			set => throw new NotImplementedException();
		}

		public int Count => GetCount();

		bool HasGroupHeader { get; set; }

		bool HasGroupFooter { get; set; }

		GroupableItemsView ItemsView { get; }

		#region NotImplemented IList interface
		public bool IsFixedSize => false;
		public bool IsReadOnly => false;
		public bool IsSynchronized => false;
		public object SyncRoot => this;
		public int Add(object? value) => throw new NotImplementedException();
		public void Clear() => throw new NotImplementedException();
		public void Insert(int index, object? value) => throw new NotImplementedException();
		public void Remove(object? value) => throw new NotImplementedException();
		public void RemoveAt(int index) => throw new NotImplementedException();
		public void CopyTo(Array array, int index) => throw new NotImplementedException();
		public IEnumerator GetEnumerator() => throw new NotImplementedException();
		#endregion

		public bool Contains(object? value) => IndexOf(value) != -1;

		public int IndexOf(object? value)
		{
			for (int groupIndex = 0; groupIndex < _groupSource.Count; groupIndex++)
			{
				if (_groupSource[groupIndex] == value)
				{
					return GetAbsoluteIndex(groupIndex, -1);
				}

				var idx = _groupSource[groupIndex].IndexOf(value);
				if (idx > -1)
				{
					return GetAbsoluteIndex(groupIndex, idx);
				}
			}
			return -1;
		}

		/// <summary>
		/// Get group and index
		/// </summary>
		/// <param name="index">a global index</param>
		/// <returns>index of group , index in group</returns>
		public (int, int) GetGroupAndIndex(int index)
		{
			int groupIndex = 0;
			int count = 0;
			foreach (var src in _groupSource)
			{
				count += src.Count + _groupHeaderFooterCount;
				if (count > index)
				{
					var groupStartIndex = count -= src.Count + _groupHeaderFooterCount;
					int inGroupIndex = index - groupStartIndex;
					return (groupIndex, AdjustIndex(inGroupIndex, src.Count));
				}
				groupIndex++;
			}
			return (-1, -1);
		}

		/// <summary>
		/// Get absolute index in groups
		/// </summary>
		/// <param name="group">Index of group</param>
		/// <param name="ingroup">Index of item in group</param>
		/// <returns>Index that converted to absolute position</returns>
		public int GetAbsoluteIndex(int group, int ingroup)
		{
			int absIdx = 0;
			for (int i = 0; i < group; i++)
			{
				absIdx += _groupSource[i].Count + _groupHeaderFooterCount;
			}
			absIdx += ingroup + (HasGroupHeader ? 1 : 0);
			return absIdx;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					if (_groupColletionChanged != null)
						_groupColletionChanged.CollectionChanged -= OnGroupCollectionChanged;
				}
				_disposedValue = true;
			}
		}

		IList<IList> BuildGroupSource(IEnumerable itemSource)
		{
			foreach (var src in itemSource)
			{
				AddGroupItem(src);
			}

			if (itemSource is INotifyCollectionChanged groupCollectionChanged)
			{
				groupCollectionChanged.CollectionChanged += OnGroupCollectionChanged;
				_groupColletionChanged = groupCollectionChanged;
			}

			return _groupSource;
		}

		object? GetItem(int index)
		{
			var (group, inGroup) = GetGroupAndIndex(index);

			if (group == -1)
				return null;

			if (inGroup < 0)
				return _groupSource[group];
			else
				return _groupSource[group][inGroup];
		}

		int GetCount()
		{
			int count = 0;
			foreach (var inGroup in _groupSource)
			{
				count += inGroup.Count + _groupHeaderFooterCount;
			}
			return count;
		}

		int AdjustIndex(int index, int count)
		{
			if (HasGroupHeader)
				index--;
			if (index == count && HasGroupFooter)
				index = -2;

			return index;
		}

		void OnGroupCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
			{
				int idx = e.NewStartingIndex != -1 ? e.NewStartingIndex : _groupSource.Count;
				foreach (var groupItem in e.NewItems)
				{
					HandleGroupCollectionAdded(idx++, groupItem);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
			{
				foreach (var groupItem in e.OldItems)
				{
					HandleGroupCollectionRemoved(groupItem);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
			{
				for (int i = 0; i < e.OldItems.Count; i++)
				{
					int replaceIdx = _originalGroupSource.IndexOf(e.OldItems[i]!);
					HandleGroupCollectionRemoved(e.OldItems[i]!);
					HandleGroupCollectionAdded(replaceIdx, e.NewItems[i]!);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var item in _originalGroupSource)
				{
					RemoveGroupItem(item);
				}
				CollectionChanged?.Invoke(this, e);
			}
		}

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			var groupIdx = _groupSource.IndexOf(sender);
			if (groupIdx == -1)
				return;

			if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
			{
				var startIndexInGroup = e.NewStartingIndex != -1 ? e.NewStartingIndex : _groupSource[groupIdx].Count;
				var startIndexInGlobal = GetAbsoluteIndex(groupIdx, startIndexInGroup);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, startIndexInGlobal));
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
			{
				var startIndexInGroup = e.OldStartingIndex != -1 ? e.OldStartingIndex : _groupSource[groupIdx].Count + e.OldItems.Count - 1;
				var startIndexInGlobal = GetAbsoluteIndex(groupIdx, startIndexInGroup);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, startIndexInGlobal));
			}
			else if (e.Action == NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
			{
				var startIndexInGlobal = e.NewStartingIndex == -1 ? -1 : GetAbsoluteIndex(groupIdx, e.NewStartingIndex);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, startIndexInGlobal));
			}
			else if (e.Action == NotifyCollectionChangedAction.Move && e.NewItems != null)
			{
				var oldIndex = GetAbsoluteIndex(groupIdx, e.OldStartingIndex);
				var newIndex = GetAbsoluteIndex(groupIdx, e.NewStartingIndex);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, newIndex, oldIndex));
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				CollectionChanged?.Invoke(this, e);
			}
		}

		void HandleGroupCollectionAdded(int startIndex, object groupItem)
		{
			InsertGroupItem(startIndex, groupItem);

			var startIdx = GetAbsoluteIndex(startIndex, 0) + (HasGroupHeader ? -1 : 0);
			var newitems = _groupSource[startIndex].Cast<object>().ToList();

			if (HasGroupHeader)
				newitems.Insert(0, _groupSource[startIndex]);
			if (HasGroupFooter)
				newitems.Add(_groupSource[startIndex]);

			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newitems, startIdx));
		}

		void HandleGroupCollectionRemoved(object groupItem)
		{
			var groupIdx = _originalGroupSource.IndexOf(groupItem);

			if (groupIdx == -1)
				return;

			var olditems = _groupSource[groupIdx].Cast<object>().ToList();
			if (HasGroupHeader)
				olditems.Insert(0, _groupSource[groupIdx]);
			if (HasGroupFooter)
				olditems.Add(_groupSource[groupIdx]);

			var startIdx = GetAbsoluteIndex(groupIdx, 0) + (HasGroupHeader ? -1 : 0);

			RemoveGroupItem(groupItem);
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, olditems, startIdx));
		}

		void AddGroupItem(object groupItem)
		{
			if (groupItem is IEnumerable enumerable)
			{
				_groupSource.Add(CreateSource(enumerable));
				_originalGroupSource.Add(groupItem);
				if (groupItem is INotifyCollectionChanged collectionChanged)
				{
					collectionChanged.CollectionChanged += OnCollectionChanged;
				}
			}
		}

		void InsertGroupItem(int index, object groupItem)
		{
			if (groupItem is IEnumerable enumerable)
			{
				_groupSource.Insert(index, CreateSource(enumerable));
				_originalGroupSource.Insert(index, groupItem);
				if (groupItem is INotifyCollectionChanged collectionChanged)
				{
					collectionChanged.CollectionChanged += OnCollectionChanged;
				}
			}
		}

		void RemoveGroupItem(object groupItem)
		{
			var idx = _originalGroupSource.IndexOf(groupItem);
			_groupSource.RemoveAt(idx);
			_originalGroupSource.RemoveAt(idx);
			if (groupItem is INotifyCollectionChanged collectionChanged)
			{
				collectionChanged.CollectionChanged -= OnCollectionChanged;
			}
		}

		static IList CreateSource(IEnumerable source)
		{
			if (source is IList list)
				return list;

			var listSource = new List<object>();

			foreach (object item in source)
			{
				listSource.Add(item);
			}
			return listSource;
		}
	}
}
