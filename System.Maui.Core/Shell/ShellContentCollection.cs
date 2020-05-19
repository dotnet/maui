using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace System.Maui
{
	internal sealed class ShellContentCollection : IList<ShellContent>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler VisibleItemsChanged;
		public event NotifyCollectionChangedEventHandler VisibleItemsChangedInternal;
		public ReadOnlyCollection<ShellContent> VisibleItems { get; }

		ObservableCollection<ShellContent> _inner = new ObservableCollection<ShellContent>();
		ObservableCollection<ShellContent> _visibleContents = new ObservableCollection<ShellContent>();
		bool _pauseCollectionChanged;
		List<NotifyCollectionChangedEventArgs> _notifyCollectionChangedEventArgs;

		public ShellContentCollection()
		{
			_notifyCollectionChangedEventArgs = new List<NotifyCollectionChangedEventArgs>();
			_inner.CollectionChanged += InnerCollectionChanged;
			VisibleItems = new ReadOnlyCollection<ShellContent>(_visibleContents);
			_visibleContents.CollectionChanged += (_, args) =>
			{
				if(_pauseCollectionChanged)
				{
					_notifyCollectionChangedEventArgs.Add(args);
					return;
				}

				OnVisibleItemsChanged(args);
			};
		}

		void OnVisibleItemsChanged(NotifyCollectionChangedEventArgs args)
		{
			VisibleItemsChangedInternal?.Invoke(VisibleItems, args);
			VisibleItemsChanged?.Invoke(VisibleItems, args);
		}

		void PauseCollectionChanged() => _pauseCollectionChanged = true;

		void ResumeCollectionChanged()
		{
			_pauseCollectionChanged = false;

			var pendingEvents = _notifyCollectionChangedEventArgs.ToList();
			_notifyCollectionChangedEventArgs.Clear();

			foreach(var args in pendingEvents)
				OnVisibleItemsChanged(args);
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
				if (controller.Page == null || controller.Page.IsVisible)
				{
					if (_visibleContents.Contains(shellContent))
						return;

					int visibleIndex = 0;
					for (var i = 0; i < _inner.Count; i++)
					{
						var item = _inner[i];

						if (item == shellContent)
						{
							_visibleContents.Insert(visibleIndex, shellContent);
							break;
						}

						visibleIndex++;
					}
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
			try
			{
				PauseCollectionChanged();
				Removing(_inner);
			}
			finally
			{
				ResumeCollectionChanged();
			}

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