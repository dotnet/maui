using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class FilteredCollectionView<T, TFilterArg> : IList<T>, IList, INotifyCollectionChanged, IDisposable
	{
		readonly ObservableCollection<T> dataSource;
		readonly Func<T, TFilterArg, bool> filter;
		readonly SortedList<T> filteredList;

		TFilterArg filterArgument;

		public FilteredCollectionView(ObservableCollection<T> dataSource, Func<T, TFilterArg, bool> filter, TFilterArg filterArgument, IComparer<T> sort)
		{
			if (sort == null)
				throw new ArgumentNullException(nameof(sort));

			this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
			this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
			this.filterArgument = filterArgument;
			filteredList = new SortedList<T>(sort);

			this.dataSource.CollectionChanged += DataSource_CollectionChanged;

			foreach (var item in this.dataSource)
			{
				OnAdded(item);
			}
		}

		public TFilterArg FilterArgument
		{
			get { return filterArgument; }
			set
			{
				if (filterArgument.Equals(value))
				{
					return;
				}

				filterArgument = value;
				RefreshFilter();
			}
		}

		public void Dispose()
		{
			dataSource.CollectionChanged -= DataSource_CollectionChanged;

			foreach (var item in dataSource.OfType<INotifyPropertyChanged>())
			{
				item.PropertyChanged -= DataSource_ItemChanged;
			}

			filteredList.Clear();
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException();
		}

		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		bool IList.Contains(object value)
		{
			return Contains((T)value);
		}

		int IList.IndexOf(object value)
		{
			return IndexOf((T)value);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		bool IList.IsFixedSize => false;

		void IList.Remove(object value)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			filteredList.CopyTo((T[])array, index);
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		public void Add(T item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(T item)
		{
			return filteredList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			filteredList.CopyTo(array, arrayIndex);
		}

		public int Count => filteredList.Count;

		public bool IsReadOnly => true;

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return filteredList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return filteredList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public T this[int index]
		{
			get { return filteredList[index]; }
			set { throw new NotSupportedException(); }
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		///     Raised when one of the items selected by the filter is changed.
		/// </summary>
		/// <remarks>
		///     The sender is reported to be the item changed.
		/// </remarks>
		public event EventHandler<PropertyChangedEventArgs> ItemChanged;

		protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			var collectionChanged = CollectionChanged;
			collectionChanged?.Invoke(this, args);
		}

		protected virtual void OnItemChanged(T sender, PropertyChangedEventArgs args)
		{
			var itemChanged = ItemChanged;
			itemChanged?.Invoke(sender, args);
		}

		void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (T item in e.NewItems)
					{
						OnAdded(item);
					}

					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (T item in e.OldItems)
					{
						OnRemoved(item);
					}

					break;
				case NotifyCollectionChangedAction.Replace:
					foreach (T item in e.OldItems)
					{
						OnRemoved(item);
					}

					foreach (T item in e.NewItems)
					{
						OnAdded(item);
					}

					break;
				case NotifyCollectionChangedAction.Reset:
					throw new NotSupportedException();
			}
		}

		void DataSource_ItemChanged(object sender, PropertyChangedEventArgs e)
		{
			var item = (T)sender;
			var index = filteredList.IndexOf(item);
			if (filter(item, FilterArgument))
			{
				if (index < 0)
				{
					filteredList.Insert(~index, item);
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, ~index));
				}
			}
			else if (index >= 0)
			{
				filteredList.RemoveAt(index);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
			}

			OnItemChanged(item, e);
		}

		void OnAdded(T item)
		{
			if (filter(item, filterArgument))
			{
				var index = filteredList.IndexOf(item);
				if (index < 0)
				{
					filteredList.Insert(~index, item);
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, ~index));
				}
			}

			if (item is INotifyPropertyChanged observable)
			{
				observable.PropertyChanged += DataSource_ItemChanged;
			}
		}

		void OnRemoved(T item)
		{
			if (item is INotifyPropertyChanged observable)
			{
				observable.PropertyChanged -= DataSource_ItemChanged;
			}

			var index = filteredList.IndexOf(item);
			if (index >= 0)
			{
				filteredList.RemoveAt(index);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
			}
		}

		void RefreshFilter()
		{
			filteredList.Clear();

			foreach (var item in dataSource)
			{
				if (filter(item, filterArgument))
				{
					filteredList.Add(item);
				}
			}

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}