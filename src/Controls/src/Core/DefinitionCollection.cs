#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public class DefinitionCollection<T> : IList<T>, ICollection<T> where T : IDefinition
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
		readonly List<T> _internalList;

		internal DefinitionCollection() => _internalList = new List<T>();

		internal DefinitionCollection(params T[] items) => _internalList = new List<T>(items);

		public void Add(T item)
		{
			_internalList.Add(item);
			item.SizeChanged += OnItemSizeChanged;
			OnItemSizeChanged(this, EventArgs.Empty);
		}

		public void Clear()
		{
			foreach (T item in _internalList)
				item.SizeChanged -= OnItemSizeChanged;
			_internalList.Clear();
			OnItemSizeChanged(this, EventArgs.Empty);
		}

		public bool Contains(T item)
		{
			return _internalList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_internalList.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _internalList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(T item)
		{
			item.SizeChanged -= OnItemSizeChanged;
			bool success = _internalList.Remove(item);
			if (success)
				OnItemSizeChanged(this, EventArgs.Empty);
			return success;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return _internalList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_internalList.Insert(index, item);
			item.SizeChanged += OnItemSizeChanged;
			OnItemSizeChanged(this, EventArgs.Empty);
		}

		public T this[int index]
		{
			get { return _internalList[index]; }
			set
			{
				if (index < _internalList.Count && index >= 0 && _internalList[index] != null)
					_internalList[index].SizeChanged -= OnItemSizeChanged;

				_internalList[index] = value;
				value.SizeChanged += OnItemSizeChanged;
				OnItemSizeChanged(this, EventArgs.Empty);
			}
		}

		public void RemoveAt(int index)
		{
			T item = _internalList[index];
			_internalList.RemoveAt(index);
			item.SizeChanged -= OnItemSizeChanged;
			OnItemSizeChanged(this, EventArgs.Empty);
		}

		public event EventHandler ItemSizeChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		void OnItemSizeChanged(object sender, EventArgs e)
		{
			_weakEventManager.HandleEvent(this, e, nameof(ItemSizeChanged));
		}
	}
}
