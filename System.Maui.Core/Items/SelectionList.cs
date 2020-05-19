using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	// Used by the SelectableItemsView to keep track of (and respond to changes in) the SelectedItems property
	internal class SelectionList : IList<object>
	{
		static readonly IList<object> s_empty = new List<object>(0);
		readonly SelectableItemsView _selectableItemsView;
		readonly IList<object> _internal;
		IList<object> _shadow;
		bool _externalChange;

		public SelectionList(SelectableItemsView selectableItemsView, IList<object> items = null)
		{
			_selectableItemsView = selectableItemsView ?? throw new ArgumentNullException(nameof(selectableItemsView));
			_internal = items ?? new List<object>();
			_shadow = Copy();

			if (items is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += OnCollectionChanged;
			}
		}

		public object this[int index] { get => _internal[index]; set => _internal[index] = value; }

		public int Count => _internal.Count;

		public bool IsReadOnly => false;

		public void Add(object item)
		{
			_externalChange = true;
			_internal.Add(item);
			_externalChange = false;

			_selectableItemsView.SelectedItemsPropertyChanged(_shadow, _internal);
			_shadow.Add(item);
		}

		public void Clear()
		{
			_externalChange = true;
			_internal.Clear();
			_externalChange = false;

			_selectableItemsView.SelectedItemsPropertyChanged(_shadow, s_empty);
			_shadow.Clear();
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
			_externalChange = true;
			_internal.Insert(index, item);
			_externalChange = false;

			_selectableItemsView.SelectedItemsPropertyChanged(_shadow, _internal);
			_shadow.Insert(index, item);
		}

		public bool Remove(object item)
		{
			_externalChange = true;
			var removed = _internal.Remove(item);
			_externalChange = false;

			if (removed)
			{
				_selectableItemsView.SelectedItemsPropertyChanged(_shadow, _internal);
				_shadow.Remove(item);
			}

			return removed;
		}

		public void RemoveAt(int index)
		{
			_externalChange = true;
			_internal.RemoveAt(index);
			_externalChange = false;

			_selectableItemsView.SelectedItemsPropertyChanged(_shadow, _internal);
			_shadow.RemoveAt(index);
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

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (_externalChange)
			{
				// If this change was initiated by a renderer or direct manipulation of ColllectionView.SelectedItems,
				// we don't need to send a selection change notification
				return;
			}

			// This change is coming from a bound viewmodel property
			// Emit a selection change notification, then bring the shadow copy up-to-date
			_selectableItemsView.SelectedItemsPropertyChanged(_shadow, _internal);
			_shadow = Copy();
		}
	}
}
