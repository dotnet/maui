using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms
{

	internal abstract class ShellElementCollection :
		IList<BaseShellItem>,
		INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler VisibleItemsChangedInternal;
		readonly List<NotifyCollectionChangedEventArgs> _notifyCollectionChangedEventArgs;
		bool _pauseCollectionChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public event NotifyCollectionChangedEventHandler VisibleItemsChanged;
		public int Count => Inner.Count;
		public bool IsReadOnly => Inner.IsReadOnly;
		IList _inner;
		IList _visibleItems;

		protected ShellElementCollection()
		{
			_notifyCollectionChangedEventArgs = new List<NotifyCollectionChangedEventArgs>();
		}

		internal IList Inner
		{
			get => _inner;
			private protected set
			{
				if (_inner != null)
					throw new ArgumentException("Inner can only be set once");

				_inner = value;
				((INotifyCollectionChanged)_inner).CollectionChanged += InnerCollectionChanged;
			}
		}

		protected void OnVisibleItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args?.NewItems?.Count > 0 && _pauseCollectionChanged)
			{
				_notifyCollectionChangedEventArgs.Add(args);
				ResumeCollectionChanged();
				return;
			}

			if (_pauseCollectionChanged)
			{
				_notifyCollectionChangedEventArgs.Add(args);
				return;
			}

			VisibleItemsChangedInternal?.Invoke(VisibleItemsReadOnly, args);
			VisibleItemsChanged?.Invoke(VisibleItemsReadOnly, args);
		}

		protected IList VisibleItems
		{
			get => _visibleItems;
			private protected set
			{
				_visibleItems = value;
				((INotifyCollectionChanged)_visibleItems).CollectionChanged += OnVisibleItemsChanged;
			}
		}

		public IReadOnlyCollection<BaseShellItem> VisibleItemsReadOnly
		{
			get;
			private protected set;
		}

		// Pause Collection Changed events when the list has zero items
		// we don't want to propagate out a visible collection changed event until the next visible item
		// is realized
		void PauseCollectionChanged() => _pauseCollectionChanged = true;

		void ResumeCollectionChanged()
		{
			_pauseCollectionChanged = false;

			// process the added items first and then remove
			var pendingEvents = _notifyCollectionChangedEventArgs.OrderBy(x => x.NewItems != null ? 0 : 1).ToList();
			_notifyCollectionChangedEventArgs.Clear();

			foreach (var args in pendingEvents)
				VisibleItemsChangedInternal?.Invoke(VisibleItemsReadOnly, args);

			foreach (var args in pendingEvents)
				VisibleItemsChanged?.Invoke(VisibleItemsReadOnly, args);
		}

		#region IList

		public BaseShellItem this[int index]
		{
			get => (BaseShellItem)Inner[index];
			set => Inner[index] = value;
		}

		public void Clear()
		{
			var list = Inner.Cast<BaseShellItem>().ToList();
			try
			{
				PauseCollectionChanged();
				Removing(Inner);
			}
			finally
			{
				ResumeCollectionChanged();
			}

			Inner.Clear();
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list));
		}

		public virtual void Add(BaseShellItem item) => Inner.Add(item);

		public virtual bool Contains(BaseShellItem item) => Inner.Contains(item);

		public virtual void CopyTo(BaseShellItem[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public abstract IEnumerator<BaseShellItem> GetEnumerator();

		public virtual int IndexOf(BaseShellItem item) => Inner.IndexOf(item);

		public virtual void Insert(int index, BaseShellItem item) => Inner.Insert(index, item);

		public abstract bool Remove(BaseShellItem item);

		public virtual void RemoveAt(int index) => Inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();

		#endregion

		void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (BaseShellItem element in e.NewItems)
				{
					if (element is IElementController controller)
						OnElementControllerInserting(controller);

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
			foreach (BaseShellItem element in items)
			{
				if (VisibleItems.Contains(element))
					VisibleItems.Remove(element);

				if (element is IElementController controller)
					OnElementControllerRemoving(controller);
			}
		}

		protected void CheckVisibility(BaseShellItem element)
		{
			if (IsBaseShellItemVisible(element))
			{
				if (VisibleItems.Contains(element))
					return;

				int visibleIndex = 0;
				for (var i = 0; i < Inner.Count; i++)
				{
					var item = Inner[i];

					if (!IsBaseShellItemVisible(element))
						continue;

					if (item == element)
					{
						VisibleItems.Insert(visibleIndex, element);
						break;
					}

					if (VisibleItems.Contains(item))
						visibleIndex++;
				}
			}
			else if (VisibleItems.Contains(element))
			{
				VisibleItems.Remove(element);
			}

			bool IsBaseShellItemVisible(BaseShellItem item)
			{
				if (!item.IsVisible)
					return false;

				if (item is ShellGroupItem sgi)
				{
					return (sgi.ShellElementCollection.VisibleItemsReadOnly.Count > 0) ||
						item is IMenuItemController;
				}

				return IsShellElementVisible(item);
			}
		}

		protected virtual bool IsShellElementVisible(BaseShellItem item)
		{
			return false;
		}


		protected virtual void OnElementControllerInserting(IElementController controller)
		{
			if (controller is ShellGroupItem sgi)
			{
				sgi.ShellElementCollection.VisibleItemsChangedInternal += OnShellElementControllerItemsCollectionChanged;
			}

			if (controller is BaseShellItem bsi)
				bsi.PropertyChanged += BaseShellItemPropertyChanged;
		}

		protected virtual void OnElementControllerRemoving(IElementController controller)
		{
			if (controller is ShellGroupItem sgi)
			{
				sgi.ShellElementCollection.VisibleItemsChangedInternal -= OnShellElementControllerItemsCollectionChanged;
			}

			if (controller is BaseShellItem bsi)
				bsi.PropertyChanged -= BaseShellItemPropertyChanged;
		}

		void BaseShellItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(BaseShellItem.IsVisible))
				CheckVisibility((BaseShellItem)sender);
		}

		void OnShellElementControllerItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (BaseShellItem bsi in (e.NewItems ?? e.OldItems ?? Inner))
			{
				if (bsi.Parent == null)
					bsi.ParentSet += OnParentSet;
				else
					CheckVisibility(bsi.Parent as BaseShellItem);
			}

			void OnParentSet(object s, System.EventArgs __)
			{
				var baseShellItem = (BaseShellItem)s;
				baseShellItem.ParentSet -= OnParentSet;
				CheckVisibility(baseShellItem.Parent as BaseShellItem);
			}
		}

	}

	internal abstract class ShellElementCollection<TBaseShellItem> :
		ShellElementCollection,
		IList<TBaseShellItem>
		where TBaseShellItem : BaseShellItem
	{

		public ShellElementCollection()
		{
			var items = new ObservableCollection<TBaseShellItem>();
			VisibleItems = items;
			VisibleItemsReadOnly = new ReadOnlyCollection<TBaseShellItem>(items);
		}

		public new ReadOnlyCollection<TBaseShellItem> VisibleItemsReadOnly
		{
			get => (ReadOnlyCollection<TBaseShellItem>)base.VisibleItemsReadOnly;
			private protected set => base.VisibleItemsReadOnly = value;
		}

		internal new IList<TBaseShellItem> Inner
		{
			get => (IList<TBaseShellItem>)base.Inner;
			set => base.Inner = (IList)value;
		}


		TBaseShellItem IList<TBaseShellItem>.this[int index]
		{
			get => (TBaseShellItem)Inner[index];
			set => Inner[index] = value;
		}

		public virtual void Add(TBaseShellItem item) => Inner.Add(item);

		public virtual bool Contains(TBaseShellItem item) => Inner.Contains(item);

		public virtual void CopyTo(TBaseShellItem[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public virtual int IndexOf(TBaseShellItem item) => Inner.IndexOf(item);

		public virtual void Insert(int index, TBaseShellItem item) => Inner.Insert(index, item);

		public virtual bool Remove(TBaseShellItem item) => Inner.Remove(item);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();

		IEnumerator<TBaseShellItem> IEnumerable<TBaseShellItem>.GetEnumerator()
		{
			return Inner.GetEnumerator();
		}

		public override IEnumerator<BaseShellItem> GetEnumerator()
		{
			return Inner.Cast<BaseShellItem>().GetEnumerator();
		}

		public override bool Remove(BaseShellItem item)
		{
			return Remove((TBaseShellItem)item);
		}
	}
}