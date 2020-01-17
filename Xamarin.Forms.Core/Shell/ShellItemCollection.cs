using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	internal sealed class ShellItemCollection : IList<ShellItem>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler VisibleItemsChanged;

		IList<ShellItem> _inner;
		ObservableCollection<ShellItem> _visibleContents = new ObservableCollection<ShellItem>();
		public ReadOnlyCollection<ShellItem> VisibleItems { get; }

		public ShellItemCollection()
		{
			VisibleItems = new ReadOnlyCollection<ShellItem>(_visibleContents);
			_visibleContents.CollectionChanged += (_, args) =>
			{
				VisibleItemsChanged?.Invoke(VisibleItems, args);
			};
		}

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)Inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)Inner).CollectionChanged -= value; }
		}

		public int Count => Inner.Count;
		public bool IsReadOnly => ((IList<ShellItem>)Inner).IsReadOnly;
		internal IList<ShellItem> Inner
		{
			get
			{
				return _inner;
			}
			set
			{			
				_inner = value;
				((INotifyCollectionChanged)_inner).CollectionChanged += InnerCollectionChanged;
			}
		}

		void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (ShellItem element in e.NewItems)
				{
					if (element is IShellItemController controller)
						controller.ItemsCollectionChanged += OnShellItemControllerItemsCollectionChanged;

					CheckVisibility(element);
				}
			}

			if (e.OldItems != null)
			{
				foreach (ShellItem element in e.OldItems)
				{
					if (_visibleContents.Contains(element))
						_visibleContents.Remove(element);

					if (element is IShellItemController controller)
						controller.ItemsCollectionChanged -= OnShellItemControllerItemsCollectionChanged;
				}
			}
		}
		void OnShellItemControllerItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (ShellSection section in (e.NewItems ?? e.OldItems ?? (IList)_inner))
			{
				if (section.Parent == null)
					section.ParentSet += OnParentSet;
				else
					CheckVisibility(section.Parent as ShellItem);
			}

			void OnParentSet(object s, System.EventArgs __)
			{
				var shellSection = (ShellSection)s;
				shellSection.ParentSet -= OnParentSet;
				CheckVisibility(shellSection.Parent as ShellItem);
			}
		}

		void CheckVisibility(ShellItem shellItem)
		{
			if (IsShellItemVisible(shellItem))
			{
				if (_visibleContents.Contains(shellItem))
					return;

				int visibleIndex = 0;
				for (var i = 0; i < _inner.Count; i++)
				{
					var item = _inner[i];

					if (!IsShellItemVisible(item))
						continue;

					if (item == shellItem)
					{
						_visibleContents.Insert(visibleIndex, shellItem);
						break;
					}

					visibleIndex++;
				}
			}
			else if (_visibleContents.Contains(shellItem))
			{
				_visibleContents.Remove(shellItem);
			}

			bool IsShellItemVisible(ShellItem item)
			{
				return (item is IShellItemController itemController && itemController.GetItems().Count > 0) ||
					item is IMenuItemController;
			}
		}

		

		public ShellItem this[int index]
		{
			get => Inner[index];
			set => Inner[index] = value;
		}

		public void Add(ShellItem item)
		{
			/*
			 * This is purely for the case where a user is only specifying Tabs at the highest level
			 * <shell>
			 * <tab></tab>
			 * <tab></tab>
			 * </shell>
			 * */
			if (Routing.IsImplicit(item) &&
				item is TabBar
				)
			{
				int i = Count - 1;
				if (i >= 0 && this[i] is TabBar && Routing.IsImplicit(this[i]))
				{
					this[i].Items.Add(item.Items[0]);
					return;
				}
			}

			Inner.Add(item);
		}

		public void Clear() => Inner.Clear();

		public bool Contains(ShellItem item) => Inner.Contains(item);

		public void CopyTo(ShellItem[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellItem> GetEnumerator() => Inner.GetEnumerator();

		public int IndexOf(ShellItem item) => Inner.IndexOf(item);

		public void Insert(int index, ShellItem item) => Inner.Insert(index, item);

		public bool Remove(ShellItem item) => Inner.Remove(item);

		public void RemoveAt(int index) => Inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();
	}
}
