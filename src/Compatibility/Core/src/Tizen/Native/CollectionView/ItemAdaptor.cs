using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;
using ESize = ElmSharp.Size;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	public interface IEmptyAdaptor { }

	public abstract class ItemAdaptor : INotifyCollectionChanged
	{
		IList _itemsSource;

		public CollectionView CollectionView { get; set; }

		protected ItemAdaptor(IEnumerable items)
		{
			SetItemsSource(items);
		}

		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public virtual void SendItemSelected(int index)
		{
			ItemSelected?.Invoke(this, new SelectedItemChangedEventArgs(this[index], index));
		}

		public virtual void UpdateViewState(EvasObject view, ViewHolderState state)
		{
		}

		public void RequestItemSelected(object item)
		{
			if (CollectionView != null)
			{
				CollectionView.SelectedItemIndex = _itemsSource.IndexOf(item);
			}
		}

		protected void SetItemsSource(IEnumerable items)
		{
			switch (items)
			{
				case IList list:
					_itemsSource = list;
					_observableCollection = list as INotifyCollectionChanged;
					break;
				case IEnumerable<object> generic:
					_itemsSource = new List<object>(generic);
					break;
				case IEnumerable _:
					_itemsSource = new List<object>();
					foreach (var item in items)
					{
						_itemsSource.Add(item);
					}
					break;
			}
		}

		public object this[int index]
		{
			get
			{
				return _itemsSource[index];
			}
		}

		public int Count => _itemsSource.Count;

		INotifyCollectionChanged _observableCollection;
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add
			{
				if (_observableCollection != null)
				{
					_observableCollection.CollectionChanged += value;
				}
			}
			remove
			{
				if (_observableCollection != null)
				{
					_observableCollection.CollectionChanged -= value;
				}
			}
		}

		public int GetItemIndex(object item)
		{
			return _itemsSource.IndexOf(item);
		}

		public virtual object GetViewCategory(int index)
		{
			return this;
		}

		public abstract EvasObject CreateNativeView(EvasObject parent);

		public abstract EvasObject CreateNativeView(int index, EvasObject parent);

		public abstract EvasObject GetHeaderView(EvasObject parent);

		public abstract EvasObject GetFooterView(EvasObject parent);

		public abstract void RemoveNativeView(EvasObject native);

		public abstract void SetBinding(EvasObject view, int index);
		public abstract void UnBinding(EvasObject view);

		public abstract ESize MeasureItem(int widthConstraint, int heightConstraint);

		public abstract ESize MeasureItem(int index, int widthConstraint, int heightConstraint);

		public abstract ESize MeasureHeader(int widthConstraint, int heightConstraint);

		public abstract ESize MeasureFooter(int widthConstraint, int heightConstraint);
	}
}
