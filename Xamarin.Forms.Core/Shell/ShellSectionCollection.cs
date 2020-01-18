using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	internal sealed class ShellSectionCollection : IList<ShellSection>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler VisibleItemsChanged;
		IList<ShellSection> _inner;
		ObservableCollection<ShellSection> _visibleContents = new ObservableCollection<ShellSection>();

		public ShellSectionCollection()
		{
			VisibleItems = new ReadOnlyCollection<ShellSection>(_visibleContents);
			_visibleContents.CollectionChanged += (_, args) =>
			{
				VisibleItemsChanged?.Invoke(VisibleItems, args);
			};
		}

		public ReadOnlyCollection<ShellSection> VisibleItems { get; }

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)Inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)Inner).CollectionChanged -= value; }
		}

		public int Count => Inner.Count;
		public bool IsReadOnly => Inner.IsReadOnly;
		internal IList<ShellSection> Inner
		{
			get => _inner;
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
				foreach (ShellSection element in e.NewItems)
				{
					if (element is IShellSectionController controller)
						controller.ItemsCollectionChanged += OnShellSectionControllerItemsCollectionChanged;

					CheckVisibility(element);
				}
			}

			if (e.OldItems != null)
			{
				foreach (ShellSection element in e.OldItems)
				{
					if (_visibleContents.Contains(element))
						_visibleContents.Remove(element);

					if (element is IShellSectionController controller)
						controller.ItemsCollectionChanged -= OnShellSectionControllerItemsCollectionChanged;
				}
			}
		}

		void OnShellSectionControllerItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (ShellContent content in (e.NewItems ?? e.OldItems ?? (IList)_inner))
			{
				if(content.Parent == null)
					content.ParentSet += OnParentSet;
				else	
					CheckVisibility(content.Parent as ShellSection);
			}

			void OnParentSet(object s, System.EventArgs __)
			{
				var shellContent = (ShellContent)s;
				shellContent.ParentSet -= OnParentSet;
				CheckVisibility(shellContent.Parent as ShellSection);
			}
		}

		void CheckVisibility(ShellSection section)
		{
			if (section is IShellSectionController controller && controller.GetItems().Count > 0)
			{
				if (_visibleContents.Contains(section))
					return;

				int visibleIndex = 0;
				for (var i = 0; i < _inner.Count; i++)
				{
					var item = _inner[i];

					if (item == section)
					{
						_visibleContents.Insert(visibleIndex, section);
						break;
					}

					visibleIndex++;
				}
			}
			else if (_visibleContents.Contains(section))
			{
				_visibleContents.Remove(section);
			}
		}

		public ShellSection this[int index]
		{
			get => Inner[index];
			set => Inner[index] = value;
		}

		public void Add(ShellSection item) => Inner.Add(item);

		public void Clear() => Inner.Clear();

		public bool Contains(ShellSection item) => Inner.Contains(item);

		public void CopyTo(ShellSection[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellSection> GetEnumerator() => Inner.GetEnumerator();

		public int IndexOf(ShellSection item) => Inner.IndexOf(item);

		public void Insert(int index, ShellSection item) => Inner.Insert(index, item);

		public bool Remove(ShellSection item) => Inner.Remove(item);

		public void RemoveAt(int index) => Inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();
	}
}