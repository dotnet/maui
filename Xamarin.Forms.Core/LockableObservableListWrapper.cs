using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class LockableObservableListWrapper : IList<string>, ICollection<string>, INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyList<string>, IReadOnlyCollection<string>, IEnumerable<string>, IEnumerable
	{
		public readonly ObservableCollection<string> _list = new ObservableCollection<string>();

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)_list).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)_list).CollectionChanged -= value; }
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { ((INotifyPropertyChanged)_list).PropertyChanged += value; }
			remove { ((INotifyPropertyChanged)_list).PropertyChanged -= value; }
		}

		public bool IsLocked { get; set; }

		void ThrowOnLocked()
		{
			if (IsLocked)
				throw new InvalidOperationException("The Items list can not be manipulated if the ItemsSource property is set");
		}

		public string this[int index]
		{
			get { return _list[index]; }
			set
			{
				ThrowOnLocked();
				_list[index] = value;
			}
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public bool IsReadOnly
		{
			get { return ((IList<string>)_list).IsReadOnly; }
		}

		public void InternalAdd(string item)
		{
			_list.Add(item);
		}

		public void Add(string item)
		{
			ThrowOnLocked();
			InternalAdd(item);
		}

		public void InternalClear()
		{
			_list.Clear();
		}

		public void Clear()
		{
			ThrowOnLocked();
			InternalClear();
		}

		public bool Contains(string item)
		{
			return _list.Contains(item);
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public int IndexOf(string item)
		{
			return _list.IndexOf(item);
		}

		public void InternalInsert(int index, string item)
		{
			_list.Insert(index, item);
		}

		public void Insert(int index, string item)
		{
			ThrowOnLocked();
			InternalInsert(index, item);
		}

		public bool InternalRemove(string item)
		{
			return _list.Remove(item);
		}

		public bool Remove(string item)
		{
			ThrowOnLocked();
			return InternalRemove(item);
		}

		public void InternalRemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public void RemoveAt(int index)
		{
			ThrowOnLocked();
			InternalRemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_list).GetEnumerator();
		}
	}
}