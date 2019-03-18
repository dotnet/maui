using System;
using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	// Used by the SelectableItemsView to keep track of (and respond to changes in) the SelectedItems property
	internal class SelectionList : IList<object>
	{
		readonly SelectableItemsView _selectableItemsView;
		List<object> _internal;
		static readonly IList<object> s_empty = new List<object>(0);

		public SelectionList(SelectableItemsView selectableItemsView)
		{
			_selectableItemsView = selectableItemsView ?? throw new ArgumentNullException(nameof(selectableItemsView));
			_internal = new List<object>();
		}

		public object this[int index] { get => _internal[index]; set => _internal[index] = value; }

		public int Count => _internal.Count;
		public bool IsReadOnly => false;

		public void Add(object item)
		{
			var oldItems = Copy();

			_internal.Add(item);

			_selectableItemsView.SelectedItemsPropertyChanged(oldItems, Copy());
		}

		public void Clear()
		{
			var oldItems = Copy();
			_internal.Clear();

			_selectableItemsView.SelectedItemsPropertyChanged(oldItems, s_empty);
		}

		public bool Contains(object item)
		{
			return _internal.Contains(item);
		}

		public void CopyTo(object[] array, int arrayIndex)
		{
			_internal.CopyTo(array, arrayIndex);
		}

		public IEnumerator<object> GetEnumerator()
		{
			return _internal.GetEnumerator();
		}

		public int IndexOf(object item)
		{
			return _internal.IndexOf(item);
		}

		public void Insert(int index, object item)
		{
			var oldItems = Copy();

			_internal.Insert(index, item);

			_selectableItemsView.SelectedItemsPropertyChanged(oldItems, Copy());
		}

		public bool Remove(object item)
		{
			var oldItems = Copy();

			var removed = _internal.Remove(item);

			if (removed)
			{
				_selectableItemsView.SelectedItemsPropertyChanged(oldItems, Copy());
			}

			return removed;
		}

		public void RemoveAt(int index)
		{
			var oldItems = Copy();

			_internal.RemoveAt(index);

			_selectableItemsView.SelectedItemsPropertyChanged(oldItems, Copy());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _internal.GetEnumerator();
		}

		List<object> Copy()
		{
			var items = new List<object>();
			for (int n = 0; n < _internal.Count; n++)
			{
				items.Add(_internal[n]);
			}

			return items;
		}

		public void ClearQuietly()
		{
			_internal.Clear();
		}

		public void AddQuietly(object item)
		{
			_internal.Add(item);
		}
	}
}
