using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xamarin.Forms.Pages
{
	internal class DataSourceList : IList<IDataItem>, IReadOnlyList<IDataItem>, INotifyCollectionChanged
	{
		readonly List<int> _maskedIndexes = new List<int>(); // Indices  
		readonly HashSet<string> _maskedKeys = new HashSet<string>();
		IList<IDataItem> _mainList;

		public IList<IDataItem> MainList
		{
			get { return _mainList; }
			set
			{
				var observable = _mainList as INotifyCollectionChanged;
				if (observable != null)
					observable.CollectionChanged -= OnMainCollectionChanged;
				_mainList = value;
				observable = _mainList as INotifyCollectionChanged;
				if (observable != null)
					observable.CollectionChanged += OnMainCollectionChanged;
				_maskedIndexes.Clear();
				for (var i = 0; i < _mainList.Count; i++)
				{
					IDataItem data = _mainList[i];
					if (_maskedKeys.Contains(data.Name))
						_maskedIndexes.Add(i);
				}
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public IEnumerable<string> MaskedKeys => _maskedKeys;

		public void Add(IDataItem item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(IDataItem item)
		{
			return MainList != null && !_maskedKeys.Contains(item.Name) && MainList.Contains(item);
		}

		public void CopyTo(IDataItem[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		public int Count
		{
			get
			{
				if (MainList == null)
					return 0;
				var result = 0;
				result += MainList.Count;
				result -= _maskedIndexes.Count;
				return result;
			}
		}

		public bool IsReadOnly => true;

		public bool Remove(IDataItem item)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<IDataItem> GetEnumerator()
		{
			var index = 0;
			if (MainList == null)
				yield break;
			foreach (IDataItem item in MainList)
			{
				if (!_maskedIndexes.Contains(index))
					yield return item;
				index++;
			}
		}

		public int IndexOf(IDataItem item)
		{
			if (_maskedKeys.Contains(item.Name))
				return -1;

			if (MainList != null)
			{
				int result = MainList.IndexOf(item);
				if (result >= 0)
					return PublicIndexFromMainIndex(result);
			}
			return -1;
		}

		public void Insert(int index, IDataItem item)
		{
			throw new NotSupportedException();
		}

		public IDataItem this[int index]
		{
			get
			{
				foreach (int i in _maskedIndexes)
				{
					if (i <= index)
						index++;
				}
				if (_mainList == null)
					throw new IndexOutOfRangeException();
				return _mainList[index];
			}
			set { throw new NotSupportedException(); }
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void MaskKey(string key)
		{
			if (_maskedKeys.Contains(key) || _mainList == null)
				return;
			_maskedKeys.Add(key);
			var index = 0;
			foreach (IDataItem item in _mainList)
			{
				if (item.Name == key)
				{
					// We need to keep our indexes list sorted, so we insert everything pre-sorted
					var added = false;
					for (var i = 0; i < _maskedIndexes.Count; i++)
					{
						if (_maskedIndexes[i] > index)
						{
							_maskedIndexes.Insert(i, index);
							added = true;
							break;
						}
					}
					if (!added)
						_maskedIndexes.Add(index);
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, PublicIndexFromMainIndex(index)));
					break;
				}
				index++;
			}
		}

		public void UnmaskKey(string key)
		{
			_maskedKeys.Remove(key);
			if (_mainList == null)
				return;
			var index = 0;
			foreach (IDataItem item in _mainList)
			{
				if (item.Name == key)
				{
					bool removed = _maskedIndexes.Remove(index);
					if (removed)
					{
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, PublicIndexFromMainIndex(index)));
					}
					break;
				}
				index++;
			}
		}

		protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			CollectionChanged?.Invoke(this, args);
		}

		void OnMainCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			// much complexity to be had here
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewItems, PublicIndexFromMainIndex(args.NewStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Move:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, args.OldItems, PublicIndexFromMainIndex(args.NewStartingIndex),
						PublicIndexFromMainIndex(args.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Remove:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItems, PublicIndexFromMainIndex(args.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Replace:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.NewItems, args.OldItems, PublicIndexFromMainIndex(args.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Reset:
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		int PublicIndexFromMainIndex(int index)
		{
			var count = 0;
			for (var x = 0; x < _maskedIndexes.Count; x++)
			{
				int i = _maskedIndexes[x];
				if (i < index)
					count++;
			}
			return index - count;
		}
	}
}