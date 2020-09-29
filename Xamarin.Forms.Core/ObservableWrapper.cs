using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms
{
	internal class ObservableWrapper<TTrack, TRestrict> : IList<TRestrict>, IList, INotifyCollectionChanged where TTrack : Element where TRestrict : TTrack
	{
		readonly ObservableCollection<TTrack> _list;

		public ObservableWrapper(ObservableCollection<TTrack> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			_list = list;

			list.CollectionChanged += ListOnCollectionChanged;
		}

		public void Add(TRestrict item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (IsReadOnly)
				throw new NotSupportedException("The collection is read-only.");

			if (_list.Contains(item))
				return;

			item.Owned = true;
			_list.Add(item);
		}

		public void Clear()
		{
			if (IsReadOnly)
				throw new NotSupportedException("The collection is read-only.");

			foreach (TRestrict item in _list.OfType<TRestrict>().ToArray())
			{
				_list.Remove(item);
				item.Owned = false;
			}
		}

		public bool Contains(TRestrict item)
		{
			return item.Owned && _list.Contains(item);
		}

		public void CopyTo(TRestrict[] array, int destIndex)
		{
			if (array.Length - destIndex < Count)
				throw new ArgumentException("Destination array was not long enough. Check destIndex and length, and the array's lower bounds.");
			foreach (TRestrict item in this)
			{
				array[destIndex] = item;
				destIndex++;
			}
		}

		public int Count
		{
			//get { return _list.Where(i => i.Owned).OfType<TRestrict>().Count(); }
			get
			{
				int result = 0;
				for (int i = 0; i < _list.Count; i++)
				{
					var item = _list[i];
					if (item.Owned && item is TRestrict)
						result++;
				}
				return result;
			}
		}

		public bool IsReadOnly { get; internal set; }

		public bool Remove(TRestrict item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (IsReadOnly)
				throw new NotSupportedException("The collection is read-only.");

			if (!item.Owned)
				return false;

			if (_list.Remove(item))
			{
				item.Owned = false;
				return true;
			}
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<TRestrict> GetEnumerator()
		{
			return _list.Where(i => i.Owned).OfType<TRestrict>().GetEnumerator();
		}

		public int IndexOf(TRestrict value)
		{
			int innerIndex = _list.IndexOf(value);
			if (innerIndex == -1)
				return -1;
			return ToOuterIndex(innerIndex);
		}

		public void Insert(int index, TRestrict item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (IsReadOnly)
				throw new NotSupportedException("The collection is read-only.");

			item.Owned = true;
			_list.Insert(ToInnerIndex(index), item);
		}

		public TRestrict this[int index]
		{
			get { return (TRestrict)_list[ToInnerIndex(index)]; }
			set
			{
				int innerIndex = ToInnerIndex(index);
				if (value != null)
					value.Owned = true;
				TTrack old = _list[innerIndex];
				_list[innerIndex] = value;

				if (old != null)
					old.Owned = false;
			}
		}

		public void RemoveAt(int index)
		{
			if (IsReadOnly)
				throw new NotSupportedException("The collection is read-only");
			int innerIndex = ToInnerIndex(index);
			TTrack item = _list[innerIndex];
			if (item.Owned)
			{
				_list.RemoveAt(innerIndex);
				item.Owned = false;
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		void ListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			if (handler == null)
				return;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex == -1 || e.NewItems.Count > 1)
						goto case NotifyCollectionChangedAction.Reset;

					var newItem = e.NewItems[0] as TRestrict;
					if (newItem == null || !newItem.Owned)
						break;

					int outerIndex = ToOuterIndex(e.NewStartingIndex);
					handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, outerIndex));
					break;
				case NotifyCollectionChangedAction.Move:
					if (e.NewStartingIndex == -1 || e.OldStartingIndex == -1 || e.NewItems.Count > 1)
						goto case NotifyCollectionChangedAction.Reset;

					var movedItem = e.NewItems[0] as TRestrict;
					if (movedItem == null || !movedItem.Owned)
						break;

					int outerOldIndex = ToOuterIndex(e.OldStartingIndex);
					int outerNewIndex = ToOuterIndex(e.NewStartingIndex);
					handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, outerNewIndex, outerOldIndex));
					break;
				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1 || e.OldItems.Count > 1)
						goto case NotifyCollectionChangedAction.Reset;

					var removedItem = e.OldItems[0] as TRestrict;
					if (removedItem == null || !removedItem.Owned)
						break;

					int outerRemovedIndex = ToOuterIndex(e.OldStartingIndex);
					var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, outerRemovedIndex);
					handler(this, args);
					break;
				case NotifyCollectionChangedAction.Replace:
					if (e.NewStartingIndex == -1 || e.OldStartingIndex == -1 || e.NewItems.Count > 1)
						goto case NotifyCollectionChangedAction.Reset;

					var newReplaceItem = e.NewItems[0] as TRestrict;
					var oldReplaceItem = e.OldItems[0] as TRestrict;

					if ((newReplaceItem == null || !newReplaceItem.Owned) && (oldReplaceItem == null || !oldReplaceItem.Owned))
					{
						break;
					}
					if (newReplaceItem == null || !newReplaceItem.Owned || oldReplaceItem == null || !oldReplaceItem.Owned)
					{
						goto case NotifyCollectionChangedAction.Reset;
					}

					int index = ToOuterIndex(e.NewStartingIndex);

					var replaceArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newReplaceItem, oldReplaceItem, index);
					handler(this, replaceArgs);
					break;
				case NotifyCollectionChangedAction.Reset:
					handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		int ToInnerIndex(int outterIndex)
		{
			var outerIndex = 0;
			int innerIndex;
			for (innerIndex = 0; innerIndex < _list.Count; innerIndex++)
			{
				TTrack item = _list[innerIndex];
				if (item is TRestrict && item.Owned)
				{
					if (outerIndex == outterIndex)
						return innerIndex;
					outerIndex++;
				}
			}

			return innerIndex;
		}

		int ToOuterIndex(int innerIndex)
		{
			var outerIndex = 0;
			for (var index = 0; index < innerIndex; index++)
			{
				TTrack item = _list[index];
				if (item is TRestrict && item.Owned)
				{
					outerIndex++;
				}
			}

			return outerIndex;
		}

		#region IList
		public int Add(object value)
		{
			Add((TRestrict)value);
			return IndexOf(value);
		}

		public bool Contains(object value)
		{
			return Contains((TRestrict)value);
		}

		public int IndexOf(object value)
		{
			return IndexOf((TRestrict)value);
		}

		public void Insert(int index, object value)
		{
			Insert(index, (TRestrict)value);
		}

		public void Remove(object value)
		{
			Remove((TRestrict)value);
		}

		public void CopyTo(Array array, int index)
		{
			CopyTo(array.Cast<TRestrict>().ToArray(), index);
		}

		public bool IsFixedSize => ((IList)_list).IsFixedSize;

		public bool IsSynchronized => ((IList)_list).IsSynchronized;

		public object SyncRoot => ((IList)_list).SyncRoot;

		object IList.this[int index]
		{
			get => (this as IList<TRestrict>)[index];
			set => (this as IList<TRestrict>)[index] = (TRestrict)value;
		}

		#endregion

	}
}