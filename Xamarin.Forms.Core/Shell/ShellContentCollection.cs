using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms
{
	internal sealed class ShellContentCollection : IList<ShellContent>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler VisibleItemsChanged;
		ObservableCollection<ShellContent> _inner = new ObservableCollection<ShellContent>();
		ObservableCollection<ShellContent> _visibleContents = new ObservableCollection<ShellContent>();

		public ReadOnlyCollection<ShellContent> VisibleItems { get; }

		public ShellContentCollection()
		{
			_inner.CollectionChanged += InnerCollectionChanged;
			VisibleItems = new ReadOnlyCollection<ShellContent>(_visibleContents);
			_visibleContents.CollectionChanged += (_, args) =>
			{
				VisibleItemsChanged?.Invoke(VisibleItems, args);
			};
		}

		void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (ShellContent element in e.NewItems)
				{
					if (element is IShellContentController controller)
						controller.IsPageVisibleChanged += OnIsPageVisibleChanged;
					CheckVisibility(element);
				}
			}

			if (e.OldItems != null)
			{
				Removing(e.OldItems);
			}
			
			CollectionChanged?.Invoke(this, e);
		}

		void Removing(IEnumerable items)
		{
			foreach (ShellContent element in items)
			{
				if (_visibleContents.Contains(element))
					_visibleContents.Remove(element);
				
				if (element is IShellContentController controller)
					controller.IsPageVisibleChanged -= OnIsPageVisibleChanged;
			}
		}

		void OnIsPageVisibleChanged(object sender, EventArgs e)
		{
			CheckVisibility((ShellContent)sender);
		}

		void CheckVisibility(ShellContent shellContent)
		{
			if (shellContent is IShellContentController controller)
			{
				// Assume incoming page will be visible
				if (controller.Page == null)
				{
					if (!_visibleContents.Contains(shellContent))
						_visibleContents.Add(shellContent);
				}
				else if(controller.Page.IsVisible)
				{
					if (!_visibleContents.Contains(shellContent))
						_visibleContents.Add(shellContent);
				}
				else
				{
					_visibleContents.Remove(shellContent);
				}
			}
			else if (_visibleContents.Contains(shellContent))
			{
				_visibleContents.Remove(shellContent);
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public int Count => _inner.Count;

		public bool IsReadOnly => ((IList<ShellContent>)_inner).IsReadOnly;

		public ShellContent this[int index]
		{
			get => _inner[index];
			set => _inner[index] = value;
		}

		public void Add(ShellContent item) => _inner.Add(item);

		public void Clear()
		{
			var list = _inner.ToList();
			Removing(_inner);
			_inner.Clear();
			
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list));
		}

		public bool Contains(ShellContent item) => _inner.Contains(item);

		public void CopyTo(ShellContent[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellContent> GetEnumerator() => _inner.GetEnumerator();

		public int IndexOf(ShellContent item) => _inner.IndexOf(item);

		public void Insert(int index, ShellContent item) => _inner.Insert(index, item);

		public bool Remove(ShellContent item) => _inner.Remove(item);

		public void RemoveAt(int index) => _inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
	}
}