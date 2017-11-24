using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	internal sealed class ListProxy : IReadOnlyList<object>, IListProxy, INotifyCollectionChanged
	{
		readonly ICollection _collection;
		readonly IList _list;
		readonly int _windowSize;
		readonly ConditionalWeakTable<ListProxy, WeakNotifyProxy> _sourceToWeakHandlers;

		IEnumerator _enumerator;
		int _enumeratorIndex;

		bool _finished;
		HashSet<int> _indexesCounted;

		Dictionary<int, object> _items;
		int _version;

		int _windowIndex;

		internal ListProxy(IEnumerable enumerable, int windowSize = int.MaxValue)
		{
			_windowSize = windowSize;

			ProxiedEnumerable = enumerable;
			_collection = enumerable as ICollection;
			_sourceToWeakHandlers = new ConditionalWeakTable<ListProxy, WeakNotifyProxy>();

			if (_collection == null && enumerable is IReadOnlyCollection<object>)
				_collection = new ReadOnlyListAdapter((IReadOnlyCollection<object>)enumerable);

			_list = enumerable as IList;
			if (_list == null && enumerable is IReadOnlyList<object>)
				_list = new ReadOnlyListAdapter((IReadOnlyList<object>)enumerable);

			var changed = enumerable as INotifyCollectionChanged;
			if (changed != null)
				_sourceToWeakHandlers.Add(this, new WeakNotifyProxy(this, changed));
		}

		public IEnumerable ProxiedEnumerable { get; }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<object> GetEnumerator()
		{
			return new ProxyEnumerator(this);
		}

		/// <summary>
		///     Gets whether or not the current window contains the <paramref name="item" />.
		/// </summary>
		/// <param name="item">The item to search for.</param>
		/// <returns><c>true</c> if the item was found in a list or the current window, <c>false</c> otherwise.</returns>
		public bool Contains(object item)
		{
			if (_list != null)
				return _list.Contains(item);

			EnsureWindowCreated();

			if (_items != null)
				return _items.Values.Contains(item);

			return false;
		}

		/// <summary>
		///     Gets the index for the <paramref name="item" /> if in a list or the current window.
		/// </summary>
		/// <param name="item">The item to search for.</param>
		/// <returns>The index of the item if in a list or the current window, -1 otherwise.</returns>
		public int IndexOf(object item)
		{
			if (_list != null)
				return _list.IndexOf(item);

			EnsureWindowCreated();

			if (_items != null)
			{
				foreach (KeyValuePair<int, object> kvp in _items)
				{
					if (Equals(kvp.Value, item))
						return kvp.Key;
				}
			}

			return -1;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public int Count
		{
			get
			{
				if (_collection != null)
					return _collection.Count;

				EnsureWindowCreated();

				if (_indexesCounted != null)
					return _indexesCounted.Count;

				return 0;
			}
		}

		public object this[int index]
		{
			get
			{
				object value;
				if (!TryGetValue(index, out value))
					throw new ArgumentOutOfRangeException("index");

				return value;
			}
		}

		public void Clear()
		{
			_version++;
			_finished = false;
			_windowIndex = 0;
			_enumeratorIndex = 0;

			if (_enumerator != null)
			{
				var dispose = _enumerator as IDisposable;
				if (dispose != null)
					dispose.Dispose();

				_enumerator = null;
			}

			if (_items != null)
				_items.Clear();
			if (_indexesCounted != null)
				_indexesCounted.Clear();

			OnCountChanged();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public event EventHandler CountChanged;

		void ClearRange(int index, int clearCount)
		{
			if (_items == null)
				return;

			for (int i = index; i < index + clearCount; i++)
				_items.Remove(i);
		}

		bool CountIndex(int index)
		{
			if (_collection != null)
				return false;

			// A collection is used in case TryGetValue is called out of order.
			if (_indexesCounted == null)
				_indexesCounted = new HashSet<int>();

			if (_indexesCounted.Contains(index))
				return false;

			_indexesCounted.Add(index);
			return true;
		}

		void EnsureWindowCreated()
		{
			if (_items != null && _items.Count > 0)
				return;

			object value;
			TryGetValue(0, out value);
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Action action;
			if (_list == null)
			{
				action = Clear;
			}
			else
			{
				action = () =>
				{
					_version++;
					OnCollectionChanged(e);
				};
			}

			CollectionSynchronizationContext sync;
			if (BindingBase.TryGetSynchronizedCollection(ProxiedEnumerable, out sync))
			{
				sync.Callback(ProxiedEnumerable, sync.Context, () =>
				{
					e = e.WithCount(Count);
					Device.BeginInvokeOnMainThread(action);
				}, false);
			}
			else
			{
				e = e.WithCount(Count);
				if (Device.IsInvokeRequired)
					Device.BeginInvokeOnMainThread(action);
				else
					action();
			}
		}

		void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler changed = CollectionChanged;
			if (changed != null)
				changed(this, e);
		}

		void OnCountChanged()
		{
			EventHandler changed = CountChanged;
			if (changed != null)
				changed(this, EventArgs.Empty);
		}

		bool TryGetValue(int index, out object value)
		{
			value = null;

			CollectionSynchronizationContext syncContext;
			BindingBase.TryGetSynchronizedCollection(ProxiedEnumerable, out syncContext);

			if (_list != null)
			{
				object indexedValue = null;
				var inRange = false;
				Action getFromList = () =>
				{
					if (index >= _list.Count)
						return;

					indexedValue = _list[index];
					inRange = true;
				};

				if (syncContext != null)
					syncContext.Callback(ProxiedEnumerable, syncContext.Context, getFromList, false);
				else
					getFromList();

				value = indexedValue;
				return inRange;
			}

			if (_collection != null && index >= _collection.Count)
				return false;
			if (_items != null)
			{
				bool found = _items.TryGetValue(index, out value);
				if (found || _finished)
					return found;
			}

			if (index >= _windowIndex + _windowSize)
			{
				int newIndex = index - _windowSize / 2;
				ClearRange(_windowIndex, newIndex - _windowIndex);
				_windowIndex = newIndex;
			}
			else if (index < _windowIndex)
			{
				int clearIndex = _windowIndex;
				int clearSize = _windowSize;
				if (clearIndex <= index + clearSize)
				{
					int diff = index + clearSize - clearIndex;
					clearIndex += diff + 1;
					clearSize -= diff;
				}

				ClearRange(clearIndex, clearSize);
				_windowIndex = 0;

				var dispose = _enumerator as IDisposable;
				if (dispose != null)
					dispose.Dispose();

				_enumerator = null;
				_enumeratorIndex = 0;
			}

			if (_enumerator == null)
				_enumerator = ProxiedEnumerable.GetEnumerator();
			if (_items == null)
				_items = new Dictionary<int, object>();

			var countChanged = false;
			int end = _windowIndex + _windowSize;

			for (; _enumeratorIndex < end; _enumeratorIndex++)
			{
				var moved = false;
				Action move = () =>
				{
					try
					{
						moved = _enumerator.MoveNext();
					}
					catch (InvalidOperationException ioex)
					{
						throw new InvalidOperationException("You must call UpdateNonNotifyingList() after updating a list that does not implement INotifyCollectionChanged", ioex);
					}

					if (!moved)
					{
						var dispose = _enumerator as IDisposable;
						if (dispose != null)
							dispose.Dispose();

						_enumerator = null;
						_enumeratorIndex = 0;
						_finished = true;
					}
				};

				if (syncContext == null)
					move();
				else
					syncContext.Callback(ProxiedEnumerable, syncContext.Context, move, false);

				if (!moved)
					break;

				if (CountIndex(_enumeratorIndex))
					countChanged = true;

				if (_enumeratorIndex >= _windowIndex)
					_items.Add(_enumeratorIndex, _enumerator.Current);
			}

			if (countChanged)
				OnCountChanged();

			return _items.TryGetValue(index, out value);
		}

		class WeakNotifyProxy
		{
			readonly WeakReference<INotifyCollectionChanged> _weakCollection;
			readonly WeakReference<ListProxy> _weakProxy;
			readonly ConditionalWeakTable<ListProxy, NotifyCollectionChangedEventHandler> _sourceToWeakHandlers;

			public WeakNotifyProxy(ListProxy proxy, INotifyCollectionChanged incc)
			{
				_sourceToWeakHandlers = new ConditionalWeakTable<ListProxy, NotifyCollectionChangedEventHandler>();
				NotifyCollectionChangedEventHandler handler = new NotifyCollectionChangedEventHandler(OnCollectionChanged);

				_sourceToWeakHandlers.Add(proxy, handler);
				incc.CollectionChanged += handler;

				_weakProxy = new WeakReference<ListProxy>(proxy);
				_weakCollection = new WeakReference<INotifyCollectionChanged>(incc);
			}

			void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				ListProxy proxy;
				if (!_weakProxy.TryGetTarget(out proxy))
				{
					INotifyCollectionChanged collection;
					if (_weakCollection.TryGetTarget(out collection))
						collection.CollectionChanged -= OnCollectionChanged;

					return;
				}

				proxy.OnCollectionChanged(sender, e);
			}
		}

		class ProxyEnumerator : IEnumerator<object>
		{
			readonly ListProxy _proxy;
			readonly int _version;

			int _index;

			public ProxyEnumerator(ListProxy proxy)
			{
				_proxy = proxy;
				_version = proxy._version;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (_proxy._version != _version)
					throw new InvalidOperationException();

				object value;
				bool next = _proxy.TryGetValue(_index++, out value);
				if (next)
					Current = value;

				return next;
			}

			public void Reset()
			{
				_index = 0;
				Current = null;
			}

			public object Current { get; private set; }
		}

		#region IList

		bool IListProxy.TryGetValue(int index, out object value)
			=> TryGetValue(index, out value);

		object IList.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { throw new NotSupportedException(); }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		int IList.Add(object item)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object item)
		{
			throw new NotSupportedException();
		}

		void IList.Insert(int index, object item)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}