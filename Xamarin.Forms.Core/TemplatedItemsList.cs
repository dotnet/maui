using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Cadenza.Collections;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Internals
{

	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class TemplatedItemsList<TView, TItem> : BindableObject, ITemplatedItemsList<TItem>, IList, IDisposable
												where TView : BindableObject, IItemsView<TItem>
												where TItem : BindableObject
	{
		public static readonly BindableProperty NameProperty = BindableProperty.Create("Name", typeof(string), typeof(TemplatedItemsList<TView, TItem>), null);

		public static readonly BindableProperty ShortNameProperty = BindableProperty.Create("ShortName", typeof(string), typeof(TemplatedItemsList<TView, TItem>), null);

		static readonly BindablePropertyKey HeaderContentPropertyKey = BindableProperty.CreateReadOnly("HeaderContent", typeof(TItem), typeof(TemplatedItemsList<TView, TItem>), null);

		internal static readonly BindablePropertyKey ListProxyPropertyKey = BindableProperty.CreateReadOnly("ListProxy", typeof(ListProxy), typeof(TemplatedItemsList<TView, TItem>), null,
			propertyChanged: OnListProxyChanged);

		static readonly BindableProperty GroupProperty = BindableProperty.Create("Group", typeof(TemplatedItemsList<TView, TItem>), typeof(TItem), null);

		static readonly BindableProperty IndexProperty = BindableProperty.Create("Index", typeof(int), typeof(TItem), -1);

		static readonly BindablePropertyKey IsGroupHeaderPropertyKey = BindableProperty.CreateAttachedReadOnly("IsGroupHeader", typeof(bool), typeof(Cell), false);

		readonly BindableProperty _itemSourceProperty;
		readonly BindableProperty _itemTemplateProperty;

		readonly TView _itemsView;

		readonly List<TItem> _templatedObjects = new List<TItem>();

		bool _disposed;
		BindingBase _groupDisplayBinding;
		OrderedDictionary<object, TemplatedItemsList<TView, TItem>> _groupedItems;
		DataTemplate _groupHeaderTemplate;
		BindingBase _groupShortNameBinding;
		ShortNamesProxy _shortNames;

		internal TemplatedItemsList(TView itemsView, BindableProperty itemSourceProperty, BindableProperty itemTemplateProperty)
		{
			if (itemsView == null)
				throw new ArgumentNullException("itemsView");
			if (itemSourceProperty == null)
				throw new ArgumentNullException("itemSourceProperty");
			if (itemTemplateProperty == null)
				throw new ArgumentNullException("itemTemplateProperty");

			_itemsView = itemsView;
			_itemsView.PropertyChanged += BindableOnPropertyChanged;

			_itemSourceProperty = itemSourceProperty;
			_itemTemplateProperty = itemTemplateProperty;

			IEnumerable source = GetItemsViewSource();
			if (source != null)
				ListProxy = new ListProxy(source);
			else
				ListProxy = new ListProxy(new object[0]);
		}

		internal TemplatedItemsList(TemplatedItemsList<TView, TItem> parent, IEnumerable itemSource, TView itemsView, BindableProperty itemTemplateProperty, int windowSize = int.MaxValue)
		{
			if (itemsView == null)
				throw new ArgumentNullException("itemsView");
			if (itemTemplateProperty == null)
				throw new ArgumentNullException("itemTemplateProperty");

			Parent = parent;

			_itemsView = itemsView;
			_itemsView.PropertyChanged += BindableOnPropertyChanged;
			_itemTemplateProperty = itemTemplateProperty;

			if (itemSource != null)
			{
				ListProxy = new ListProxy(itemSource, windowSize);
				ListProxy.CollectionChanged += OnProxyCollectionChanged;
			}
			else
				ListProxy = new ListProxy(new object[0]);
		}

		event PropertyChangedEventHandler ITemplatedItemsList<TItem>.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		public BindingBase GroupDisplayBinding
		{
			get { return _groupDisplayBinding; }
			set
			{
				_groupDisplayBinding = value;
				OnHeaderTemplateChanged();
			}
		}

		public DataTemplate GroupHeaderTemplate
		{
			get
			{
				DataTemplate groupHeader = null;
				if (GroupHeaderTemplateProperty != null)
					groupHeader = (DataTemplate)_itemsView.GetValue(GroupHeaderTemplateProperty);

				return groupHeader ?? _groupHeaderTemplate;
			}

			set
			{
				if (_groupHeaderTemplate == value)
					return;

				_groupHeaderTemplate = value;
				OnHeaderTemplateChanged();
			}
		}

		public BindableProperty GroupHeaderTemplateProperty { get; set; }

		public BindingBase GroupShortNameBinding
		{
			get { return _groupShortNameBinding; }
			set
			{
				_groupShortNameBinding = value;
				OnShortNameBindingChanged();
			}
		}

		public TItem HeaderContent
		{
			get { return (TItem)GetValue(HeaderContentPropertyKey.BindableProperty); }
			private set { SetValue(HeaderContentPropertyKey, value); }
		}

		public bool IsGroupingEnabled
		{
			get { return (IsGroupingEnabledProperty != null) && (bool)_itemsView.GetValue(IsGroupingEnabledProperty); }
		}

		public BindableProperty IsGroupingEnabledProperty { get; set; }

		public IEnumerable ItemsSource
		{
			get { return ListProxy.ProxiedEnumerable; }
		}

		public string Name
		{
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		public TemplatedItemsList<TView, TItem> Parent { get; }

		public BindableProperty ProgressiveLoadingProperty { get; set; }

		public string ShortName
		{
			get { return (string)GetValue(ShortNameProperty); }
			set { SetValue(ShortNameProperty, value); }
		}

		public IReadOnlyList<string> ShortNames
		{
			get { return _shortNames; }
		}

		internal ListViewCachingStrategy CachingStrategy
		{
			get
			{
				var listView = _itemsView as ListView;
				if (listView == null)
					return ListViewCachingStrategy.RetainElement;

				return listView.CachingStrategy;
			}
		}

		internal IListProxy ListProxy
		{
			get { return (IListProxy)GetValue(ListProxyPropertyKey.BindableProperty); }
			private set { SetValue(ListProxyPropertyKey, value); }
		}

		IListProxy ITemplatedItemsList<TItem>.ListProxy
		{
			get { return ListProxy; }
		}

		DataTemplate ItemTemplate
		{
			get { return (DataTemplate)_itemsView.GetValue(_itemTemplateProperty); }
		}

		bool ProgressiveLoading
		{
			get { return (ProgressiveLoadingProperty != null) && (bool)_itemsView.GetValue(ProgressiveLoadingProperty); }
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_itemsView.PropertyChanged -= BindableOnPropertyChanged;

			TItem header = HeaderContent;
			if (header != null)
				UnhookItem(header);

			for (var i = 0; i < _templatedObjects.Count; i++)
			{
				TItem item = _templatedObjects[i];
				if (item != null)
					UnhookItem(item);
			}

			_disposed = true;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (IsGroupingEnabled)
				return _groupedItems.Values.GetEnumerator();

			return GetEnumerator();
		}

		public IEnumerator<TItem> GetEnumerator()
		{
			var i = 0;
			foreach (object item in ListProxy)
				yield return GetOrCreateContent(i++, item);
		}

		int IList.Add(object item)
		{
			throw new NotSupportedException();
		}

		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		bool IList.Contains(object item)
		{
			throw new NotImplementedException();
		}

		int IList.IndexOf(object item)
		{
			if (IsGroupingEnabled)
			{
				var til = item as TemplatedItemsList<TView, TItem>;
				if (til != null)
					return _groupedItems.Values.IndexOf(til);
			}

			return IndexOf((TItem)item);
		}

		void IList.Insert(int index, object item)
		{
			throw new NotSupportedException();
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		object IList.this[int index]
		{
			get
			{
				if (IsGroupingEnabled)
					return GetGroup(index);

				return this[index];
			}
			set { throw new NotSupportedException(); }
		}

		void IList.Remove(object item)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public int Count
		{
			get { return ListProxy.Count; }
		}

		public TItem this[int index]
		{
			get { return GetOrCreateContent(index, ListProxy[index]); }
		}

		public int GetDescendantCount()
		{
			if (!IsGroupingEnabled)
				return Count;

			if (_groupedItems == null)
				return 0;

			int count = Count;
			foreach (TemplatedItemsList<TView, TItem> group in _groupedItems.Values)
				count += group.GetDescendantCount();

			return count;
		}

		public int GetGlobalIndexForGroup(ITemplatedItemsList<TItem> group)
		{
			if (group == null)
				throw new ArgumentNullException("group");

			int groupIndex = _groupedItems.Values.IndexOf(group);

			var index = 0;
			for (var i = 0; i < groupIndex; i++)
				index += _groupedItems[i].GetDescendantCount() + 1;

			return index;
		}

		public int GetGlobalIndexOfGroup(object item)
		{
			var count = 0;
			if (IsGroupingEnabled && _groupedItems != null)
			{
				foreach (object group in _groupedItems.Keys)
				{
					if (group == item)
						return count;
					count++;
				}
			}

			return -1;
		}

		public int GetGlobalIndexOfItem(object item)
		{
			if (!IsGroupingEnabled)
				return ListProxy.IndexOf(item);

			var count = 0;
			if (_groupedItems != null)
			{
				foreach (TemplatedItemsList<TView, TItem> children in _groupedItems.Values)
				{
					count++;

					int index = children.GetGlobalIndexOfItem(item);
					if (index != -1)
						return count + index;

					count += children.GetDescendantCount();
				}
			}

			return -1;
		}

		public int GetGlobalIndexOfItem(object group, object item)
		{
			if (!IsGroupingEnabled)
				return ListProxy.IndexOf(item);

			var count = 0;
			if (_groupedItems != null)
			{
				foreach (KeyValuePair<object, TemplatedItemsList<TView, TItem>> kvp in _groupedItems)
				{
					count++;

					if (ReferenceEquals(group, kvp.Key))
					{
						int index = kvp.Value.GetGlobalIndexOfItem(item);
						if (index != -1)
							return count + index;
					}

					count += kvp.Value.GetDescendantCount();
				}
			}

			return -1;
		}

		public Tuple<int, int> GetGroupAndIndexOfItem(object item)
		{
			if (item == null)
				return new Tuple<int, int>(-1, -1);
			if (!IsGroupingEnabled)
				return new Tuple<int, int>(0, GetGlobalIndexOfItem(item));

			var group = 0;
			if (_groupedItems != null)
			{
				foreach (TemplatedItemsList<TView, TItem> children in _groupedItems.Values)
				{
					int index = children.GetGlobalIndexOfItem(item);
					if (index != -1)
						return new Tuple<int, int>(group, index);

					group++;
				}
			}

			return new Tuple<int, int>(-1, -1);
		}

		public Tuple<int, int> GetGroupAndIndexOfItem(object group, object item)
		{
			if (!IsGroupingEnabled)
				return new Tuple<int, int>(0, GetGlobalIndexOfItem(item));
			if (_groupedItems == null)
				return new Tuple<int, int>(-1, -1);

			var groupIndex = 0;
			foreach (TemplatedItemsList<TView, TItem> children in _groupedItems.Values)
			{
				if (ReferenceEquals(children.BindingContext, group) || group == null)
				{
					for (var i = 0; i < children.Count; i++)
					{
						if (ReferenceEquals(children[i].BindingContext, item))
							return new Tuple<int, int>(groupIndex, i);
					}

					if (group != null)
						return new Tuple<int, int>(groupIndex, -1);
				}

				groupIndex++;
			}

			return new Tuple<int, int>(-1, -1);
		}

		public int GetGroupIndexFromGlobal(int globalIndex, out int leftOver)
		{
			leftOver = 0;

			var index = 0;
			for (var i = 0; i < _groupedItems.Count; i++)
			{
				if (index == globalIndex)
					return i;

				TemplatedItemsList<TView, TItem> group = _groupedItems[i];
				int count = group.GetDescendantCount();

				if (index + count >= globalIndex)
				{
					leftOver = globalIndex - index;
					return i;
				}

				index += count + 1;
			}

			return -1;
		}

		public event NotifyCollectionChangedEventHandler GroupedCollectionChanged;
		event NotifyCollectionChangedEventHandler ITemplatedItemsList<TItem>.GroupedCollectionChanged
		{
			add { GroupedCollectionChanged += value; }
			remove { GroupedCollectionChanged -= value; }
		}

		public int IndexOf(TItem item)
		{
			TemplatedItemsList<TView, TItem> group = GetGroup(item);
			if (group != null && group != this)
				return -1;

			return GetIndex(item);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public DataTemplate SelectDataTemplate(object item)
		{
			return ItemTemplate.SelectDataTemplate(item, _itemsView);
		}

		public TItem CreateContent(int index, object item, bool insert = false)
		{
			TItem content = ItemTemplate != null ? (TItem)ItemTemplate.CreateContent(item, _itemsView) : _itemsView.CreateDefault(item);

			content = UpdateContent(content, index, item);

			if ((CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
				return content;

			for (int i = _templatedObjects.Count; i <= index; i++)
				_templatedObjects.Add(null);

			if (!insert)
				_templatedObjects[index] = content;
			else
				_templatedObjects.Insert(index, content);

			return content;
		}

		internal void ForceUpdate()
		{
			ListProxy.Clear();
		}

		internal TemplatedItemsList<TView, TItem> GetGroup(int index)
		{
			if (!IsGroupingEnabled)
				return this;

			return _groupedItems[index];
		}

		ITemplatedItemsList<TItem> ITemplatedItemsList<TItem>.GetGroup(int index)
		{
			return GetGroup(index);
		}

		internal static TemplatedItemsList<TView, TItem> GetGroup(TItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return (TemplatedItemsList<TView, TItem>)item.GetValue(GroupProperty);
		}

		internal static int GetIndex(TItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return (int)item.GetValue(IndexProperty);
		}

		internal static bool GetIsGroupHeader(BindableObject bindable)
		{
			return (bool)bindable.GetValue(IsGroupHeaderPropertyKey.BindableProperty);
		}

		internal TItem GetOrCreateContent(int index, object item)
		{
			TItem content;
			if (_templatedObjects.Count <= index || (content = _templatedObjects[index]) == null)
				content = CreateContent(index, item);

			return content;
		}

		internal static void SetIsGroupHeader(BindableObject bindable, bool value)
		{
			bindable.SetValue(IsGroupHeaderPropertyKey, value);
		}

		internal TItem UpdateContent(TItem content, int index, object item)
		{
			content.BindingContext = item;

			if (Parent != null)
				SetGroup(content, this);

			SetIndex(content, index);

			_itemsView.SetupContent(content, index);

			return content;
		}

		internal TItem UpdateContent(TItem content, int index)
		{
			object item = ListProxy[index];
			return UpdateContent(content, index, item);
		}
		TItem ITemplatedItemsList<TItem>.UpdateContent(TItem content, int index)
		{
			return UpdateContent(content, index);
		}

		internal TItem UpdateHeader(TItem content, int groupIndex)
		{
			if (Parent != null && Parent.GroupHeaderTemplate == null)
			{
				content.BindingContext = this;
			}
			else
			{
				content.BindingContext = ListProxy.ProxiedEnumerable;
			}

			SetIndex(content, groupIndex);

			_itemsView.SetupContent(content, groupIndex);

			return content;
		}
		TItem ITemplatedItemsList<TItem>.UpdateHeader(TItem content, int groupIndex)
		{
			return UpdateHeader(content, groupIndex);
		}

		void BindableOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_itemSourceProperty != null && e.PropertyName == _itemSourceProperty.PropertyName)
				OnItemsSourceChanged();
			else if (e.PropertyName == _itemTemplateProperty.PropertyName)
				OnItemTemplateChanged();
			else if (ProgressiveLoadingProperty != null && e.PropertyName == ProgressiveLoadingProperty.PropertyName)
				OnInfiniteScrollingChanged();
			else if (GroupHeaderTemplateProperty != null && e.PropertyName == GroupHeaderTemplateProperty.PropertyName)
				OnHeaderTemplateChanged();
			else if (IsGroupingEnabledProperty != null && e.PropertyName == IsGroupingEnabledProperty.PropertyName)
				OnGroupingEnabledChanged();
		}

		IList ConvertContent(int startingIndex, IList items, bool forceCreate = false, bool setIndex = false)
		{
			var contentItems = new List<TItem>(items.Count);
			for (var i = 0; i < items.Count; i++)
			{
				int index = i + startingIndex;
				TItem content = !forceCreate ? GetOrCreateContent(index, items[i]) : CreateContent(index, items[i]);
				if (setIndex)
					SetIndex(content, index);

				contentItems.Add(content);
			}

			return contentItems;
		}

		IEnumerable GetItemsViewSource()
		{
			return (IEnumerable)_itemsView.GetValue(_itemSourceProperty);
		}

		object ITemplatedItemsList<TItem>.BindingContext
		{
			get
			{
				return BindingContext;
			}
		}

		void GroupedReset()
		{
			if (_groupedItems != null)
			{
				foreach (KeyValuePair<object, TemplatedItemsList<TView, TItem>> group in _groupedItems)
				{
					group.Value.CollectionChanged -= OnInnerCollectionChanged;
					group.Value.Dispose();
				}
				_groupedItems.Clear();
			}

			_templatedObjects.Clear();

			var i = 0;
			foreach (object item in ListProxy)
				InsertGrouped(item, i++);

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		TemplatedItemsList<TView, TItem> InsertGrouped(object item, int index)
		{
			var children = item as IEnumerable;

			var groupProxy = new TemplatedItemsList<TView, TItem>(this, children, _itemsView, _itemTemplateProperty);
			if (GroupDisplayBinding != null)
				groupProxy.SetBinding(NameProperty, GroupDisplayBinding.Clone());
			else if (GroupHeaderTemplate == null && item != null)
				groupProxy.Name = item.ToString();

			if (GroupShortNameBinding != null)
				groupProxy.SetBinding(ShortNameProperty, GroupShortNameBinding.Clone());

			groupProxy.BindingContext = item;

			if (GroupHeaderTemplate != null)
			{
				groupProxy.HeaderContent = (TItem)GroupHeaderTemplate.CreateContent(groupProxy.ItemsSource, _itemsView);
				groupProxy.HeaderContent.BindingContext = groupProxy.ItemsSource;
				//groupProxy.HeaderContent.BindingContext = groupProxy;
				//groupProxy.HeaderContent.SetBinding (BindingContextProperty, "ItemsSource");
			}
			else
			{
				// HACK: TemplatedItemsList shouldn't assume what the default is, but it needs
				// to be able to setup bindings. Needs some internal-API tweaking there isn't
				// time for right now.
				groupProxy.HeaderContent = _itemsView.CreateDefault(ListProxy.ProxiedEnumerable);
				groupProxy.HeaderContent.BindingContext = groupProxy;
				groupProxy.HeaderContent.SetBinding(TextCell.TextProperty, "Name");
			}

			SetIndex(groupProxy.HeaderContent, index);
			SetIsGroupHeader(groupProxy.HeaderContent, true);

			_itemsView.SetupContent(groupProxy.HeaderContent, index);

			_templatedObjects.Insert(index, groupProxy.HeaderContent);
			_groupedItems.Insert(index, item, groupProxy);

			groupProxy.CollectionChanged += OnInnerCollectionChanged;

			return groupProxy;
		}

		void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler changed = CollectionChanged;
			if (changed != null)
				changed(this, e);
		}

		void OnCollectionChangedGrouped(NotifyCollectionChangedEventArgs e)
		{
			if (_groupedItems == null)
				_groupedItems = new OrderedDictionary<object, TemplatedItemsList<TView, TItem>>();

			List<TemplatedItemsList<TView, TItem>> newItems = null, oldItems = null;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					for (int i = e.NewStartingIndex; i < _templatedObjects.Count; i++)
						SetIndex(_templatedObjects[i], i + e.NewItems.Count);

					newItems = new List<TemplatedItemsList<TView, TItem>>(e.NewItems.Count);

					for (var i = 0; i < e.NewItems.Count; i++)
					{
						TemplatedItemsList<TView, TItem> converted = InsertGrouped(e.NewItems[i], e.NewStartingIndex + i);
						newItems.Add(converted);
					}

					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex));

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					int removeIndex = e.OldStartingIndex;
					for (int i = removeIndex + e.OldItems.Count; i < _templatedObjects.Count; i++)
						SetIndex(_templatedObjects[i], removeIndex++);

					oldItems = new List<TemplatedItemsList<TView, TItem>>(e.OldItems.Count);
					for (var i = 0; i < e.OldItems.Count; i++)
					{
						int index = e.OldStartingIndex + i;
						TemplatedItemsList<TView, TItem> til = _groupedItems[index];
						til.CollectionChanged -= OnInnerCollectionChanged;
						oldItems.Add(til);
						_groupedItems.RemoveAt(index);
						_templatedObjects.RemoveAt(index);
						til.Dispose();
					}

					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex));

					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					oldItems = new List<TemplatedItemsList<TView, TItem>>(e.OldItems.Count);
					newItems = new List<TemplatedItemsList<TView, TItem>>(e.NewItems.Count);

					for (var i = 0; i < e.OldItems.Count; i++)
					{
						int index = e.OldStartingIndex + i;

						TemplatedItemsList<TView, TItem> til = _groupedItems[index];
						til.CollectionChanged -= OnInnerCollectionChanged;
						oldItems.Add(til);

						_groupedItems.RemoveAt(index);
						_templatedObjects.RemoveAt(index);

						newItems.Add(InsertGrouped(e.NewItems[i], index));
						til.Dispose();
					}

					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, e.OldStartingIndex));

					break;

				case NotifyCollectionChangedAction.Move:
					if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					bool movingForward = e.OldStartingIndex < e.NewStartingIndex;

					if (movingForward)
					{
						int moveIndex = e.OldStartingIndex;
						for (int i = moveIndex + e.OldItems.Count; i <= e.NewStartingIndex; i++)
							SetIndex(_templatedObjects[i], moveIndex++);
					}
					else
					{
						for (var i = 0; i < e.OldStartingIndex - e.NewStartingIndex; i++)
						{
							TItem item = _templatedObjects[i + e.NewStartingIndex];
							SetIndex(item, GetIndex(item) + e.OldItems.Count);
						}
					}

					oldItems = new List<TemplatedItemsList<TView, TItem>>(e.OldItems.Count);

					for (var i = 0; i < e.OldItems.Count; i++)
					{
						oldItems.Add(_groupedItems[e.OldStartingIndex]);

						_templatedObjects.RemoveAt(e.OldStartingIndex);
						_groupedItems.RemoveAt(e.OldStartingIndex);
					}

					int insertIndex = e.NewStartingIndex;
					if (e.OldStartingIndex < e.NewStartingIndex)
						insertIndex -= e.OldItems.Count - 1;

					for (var i = 0; i < oldItems.Count; i++)
					{
						TemplatedItemsList<TView, TItem> til = oldItems[i];
						_templatedObjects.Insert(insertIndex + i, til.HeaderContent);
						_groupedItems.Insert(insertIndex + i, til.BindingContext, til);
						SetIndex(til.HeaderContent, insertIndex + i);
					}

					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, oldItems, e.OldStartingIndex, e.NewStartingIndex));

					break;

				case NotifyCollectionChangedAction.Reset:
					GroupedReset();
					break;
			}
		}

		void OnGroupingEnabledChanged()
		{
			if ((CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
				_templatedObjects.Clear();

			OnItemsSourceChanged(true);

			if (!IsGroupingEnabled && _shortNames != null)
			{
				_shortNames.Dispose();
				_shortNames = null;
			}
			else
				OnShortNameBindingChanged();
		}

		void OnHeaderTemplateChanged()
		{
			OnItemTemplateChanged();
		}

		void OnInfiniteScrollingChanged()
		{
			OnItemsSourceChanged();
		}

		void OnInnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = GroupedCollectionChanged;
			if (handler != null)
				handler(sender, e);
		}

		void OnItemsSourceChanged(bool fromGrouping = false)
		{
			ListProxy.CollectionChanged -= OnProxyCollectionChanged;

			IEnumerable itemSource = GetItemsViewSource();
			if (itemSource == null)
				ListProxy = new ListProxy(new object[0]);
			else
				ListProxy = new ListProxy(itemSource);

			ListProxy.CollectionChanged += OnProxyCollectionChanged;
			OnProxyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		void OnItemTemplateChanged()
		{
			if (ListProxy.Count == 0)
				return;

			OnProxyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		static void OnListProxyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var til = (TemplatedItemsList<TView, TItem>)bindable;
			til.OnPropertyChanged("ItemsSource");
		}

		void OnProxyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnProxyCollectionChanged(sender, e, true);
		}

		void OnProxyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, bool fixWindows = true)
		{
			if (IsGroupingEnabled)
			{
				OnCollectionChangedGrouped(e);
				return;
			}

			if ((CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
			{
				OnCollectionChanged(e);
				return;
			}

			/* HACKAHACKHACK: LongListSelector on WP SL has a bug in that it completely fails to deal with
			 * INCC notifications that include more than 1 item. */
			if (fixWindows && Device.RuntimePlatform == Device.WinPhone)
			{
				SplitCollectionChangedItems(e);
				return;
			}

			int count = Count;
			var ex = e as NotifyCollectionChangedEventArgsEx;
			if (ex != null)
				count = ex.Count;

			var maxindex = 0;
			if (e.NewStartingIndex >= 0 && e.NewItems != null)
				maxindex = Math.Max(maxindex, e.NewStartingIndex + e.NewItems.Count);
			if (e.OldStartingIndex >= 0 && e.OldItems != null)
				maxindex = Math.Max(maxindex, e.OldStartingIndex + e.OldItems.Count);
			if (maxindex > _templatedObjects.Count)
				_templatedObjects.InsertRange(_templatedObjects.Count, Enumerable.Repeat<TItem>(null, maxindex - _templatedObjects.Count));

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex >= 0)
					{
						for (int i = e.NewStartingIndex; i < _templatedObjects.Count; i++)
							SetIndex(_templatedObjects[i], i + e.NewItems.Count);

						_templatedObjects.InsertRange(e.NewStartingIndex, Enumerable.Repeat<TItem>(null, e.NewItems.Count));

						IList items = ConvertContent(e.NewStartingIndex, e.NewItems, true, true);
						e = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Add, items, e.NewStartingIndex);
					}
					else
					{
						goto case NotifyCollectionChangedAction.Reset;
					}

					break;

				case NotifyCollectionChangedAction.Move:
					if (e.NewStartingIndex < 0 || e.OldStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					bool movingForward = e.OldStartingIndex < e.NewStartingIndex;

					if (movingForward)
					{
						int moveIndex = e.OldStartingIndex;
						for (int i = moveIndex + e.OldItems.Count; i <= e.NewStartingIndex; i++)
							SetIndex(_templatedObjects[i], moveIndex++);
					}
					else
					{
						for (var i = 0; i < e.OldStartingIndex - e.NewStartingIndex; i++)
						{
							TItem item = _templatedObjects[i + e.NewStartingIndex];
							if (item != null)
								SetIndex(item, GetIndex(item) + e.OldItems.Count);
						}
					}

					TItem[] itemsToMove = _templatedObjects.Skip(e.OldStartingIndex).Take(e.OldItems.Count).ToArray();

					_templatedObjects.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
					_templatedObjects.InsertRange(e.NewStartingIndex, itemsToMove);
					for (var i = 0; i < itemsToMove.Length; i++)
						SetIndex(itemsToMove[i], e.NewStartingIndex + i);

					e = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Move, itemsToMove, e.NewStartingIndex, e.OldStartingIndex);
					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex >= 0)
					{
						int removeIndex = e.OldStartingIndex;
						for (int i = removeIndex + e.OldItems.Count; i < _templatedObjects.Count; i++)
							SetIndex(_templatedObjects[i], removeIndex++);

						var items = new TItem[e.OldItems.Count];
						for (var i = 0; i < items.Length; i++)
						{
							TItem item = _templatedObjects[e.OldStartingIndex + i];
							if (item == null)
								continue;

							UnhookItem(item);
							items[i] = item;
						}

						_templatedObjects.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
						e = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Remove, items, e.OldStartingIndex);
					}
					else
					{
						goto case NotifyCollectionChangedAction.Reset;
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.NewStartingIndex >= 0)
					{
						IList oldItems = ConvertContent(e.NewStartingIndex, e.OldItems);
						IList newItems = ConvertContent(e.NewStartingIndex, e.NewItems, true, true);

						for (var i = 0; i < oldItems.Count; i++)
						{
							UnhookItem((TItem)oldItems[i]);
						}

						e = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Replace, newItems, oldItems, e.NewStartingIndex);
					}
					else
					{
						goto case NotifyCollectionChangedAction.Reset;
					}

					break;

				case NotifyCollectionChangedAction.Reset:
					e = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Reset);
					UnhookAndClear();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			OnCollectionChanged(e);
		}

		void OnShortNameBindingChanged()
		{
			if (!IsGroupingEnabled)
				return;

			if (GroupShortNameBinding != null && _shortNames == null)
				_shortNames = new ShortNamesProxy(this);
			else if (GroupShortNameBinding == null && _shortNames != null)
			{
				_shortNames.Dispose();
				_shortNames = null;
			}

			if (_groupedItems != null)
			{
				if (GroupShortNameBinding == null)
				{
					foreach (TemplatedItemsList<TView, TItem> list in _groupedItems.Values)
						list.SetValue(ShortNameProperty, null);

					return;
				}

				foreach (TemplatedItemsList<TView, TItem> list in _groupedItems.Values)
					list.SetBinding(ShortNameProperty, GroupShortNameBinding.Clone());
			}

			if (_shortNames != null)
				_shortNames.Reset();
		}

		static void SetGroup(TItem item, TemplatedItemsList<TView, TItem> group)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			item.SetValue(GroupProperty, group);
		}

		static void SetIndex(TItem item, int index)
		{
			if (item == null)
				return;

			item.SetValue(IndexProperty, index);
		}

		void SplitCollectionChangedItems(NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex < 0)
						goto default;

					for (var i = 0; i < e.NewItems.Count; i++)
						OnProxyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems[i], e.NewStartingIndex + i), false);

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex < 0)
						goto default;

					for (var i = 0; i < e.OldItems.Count; i++)
						OnProxyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems[i], e.OldStartingIndex + i), false);

					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex < 0)
						goto default;

					for (var i = 0; i < e.OldItems.Count; i++)
						OnProxyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems[i], e.OldItems[i], e.OldStartingIndex + i), false);

					break;

				default:
					OnProxyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), false);
					break;
			}
		}

		void UnhookAndClear()
		{
			for (var i = 0; i < _templatedObjects.Count; i++)
			{
				TItem item = _templatedObjects[i];
				if (item == null)
					continue;

				UnhookItem(item);
			}

			_templatedObjects.Clear();
		}

		async void UnhookItem(TItem item)
		{
			SetIndex(item, -1);
			_itemsView.UnhookContent(item);

			//Hack: the cell could still be visible on iOS because the cells are reloaded after this unhook 
			//this causes some visual updates caused by a null datacontext and default values like IsVisible
			if (Device.RuntimePlatform == Device.iOS && CachingStrategy == ListViewCachingStrategy.RetainElement)
				await Task.Delay(100);
			item.BindingContext = null;
		}

		class ShortNamesProxy : IReadOnlyList<string>, INotifyCollectionChanged, IDisposable
		{
			readonly HashSet<TemplatedItemsList<TView, TItem>> _attachedItems = new HashSet<TemplatedItemsList<TView, TItem>>();
			readonly TemplatedItemsList<TView, TItem> _itemsList;

			readonly Dictionary<TemplatedItemsList<TView, TItem>, string> _oldNames = new Dictionary<TemplatedItemsList<TView, TItem>, string>();

			bool _disposed;

			internal ShortNamesProxy(TemplatedItemsList<TView, TItem> itemsList)
			{
				_itemsList = itemsList;
				_itemsList.CollectionChanged += OnItemsListCollectionChanged;
			}

			public void Dispose()
			{
				if (_disposed)
					return;
				_disposed = true;

				_itemsList.CollectionChanged -= OnItemsListCollectionChanged;

				ResetCore(false);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<string> GetEnumerator()
			{
				if (_itemsList._groupedItems == null)
					yield break;

				foreach (TemplatedItemsList<TView, TItem> item in _itemsList._groupedItems.Values)
				{
					AttachList(item);
					yield return item.ShortName;
				}
			}

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			public int Count
			{
				get { return _itemsList._groupedItems.Count; }
			}

			public string this[int index]
			{
				get
				{
					TemplatedItemsList<TView, TItem> list = _itemsList._groupedItems[index];
					AttachList(list);

					return list.ShortName;
				}
			}

			public void Reset()
			{
				ResetCore(true);
			}

			void AttachList(TemplatedItemsList<TView, TItem> list)
			{
				if (_attachedItems.Contains(list))
					return;

				list.PropertyChanging += OnChildListPropertyChanging;
				list.PropertyChanged += OnChildListPropertyChanged;
				_attachedItems.Add(list);
			}

			List<string> ConvertItems(IList list)
			{
				var newList = new List<string>(list.Count);
				newList.AddRange(list.Cast<TemplatedItemsList<TView, TItem>>().Select(tl => tl.ShortName));
				return newList;
			}

			void OnChildListPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName != ShortNameProperty.PropertyName)
					return;

				var list = (TemplatedItemsList<TView, TItem>)sender;
				string old = _oldNames[list];
				_oldNames.Remove(list);

				int index = _itemsList._groupedItems.Values.IndexOf(list);

				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, list.ShortName, old, index));
			}

			void OnChildListPropertyChanging(object sender, PropertyChangingEventArgs e)
			{
				if (e.PropertyName != ShortNameProperty.PropertyName)
					return;

				var list = (TemplatedItemsList<TView, TItem>)sender;
				_oldNames[list] = list.ShortName;
			}

			void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
			{
				NotifyCollectionChangedEventHandler changed = CollectionChanged;
				if (changed != null)
					changed(this, e);
			}

			void OnItemsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ConvertItems(e.NewItems), e.NewStartingIndex);
						break;

					case NotifyCollectionChangedAction.Move:
						e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, ConvertItems(e.OldItems), e.NewStartingIndex, e.OldStartingIndex);
						break;

					case NotifyCollectionChangedAction.Remove:
						e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ConvertItems(e.OldItems), e.OldStartingIndex);
						break;

					case NotifyCollectionChangedAction.Replace:
						e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ConvertItems(e.NewItems), ConvertItems(e.OldItems), e.OldStartingIndex);
						break;
				}

				OnCollectionChanged(e);
			}

			void ResetCore(bool raiseReset)
			{
				foreach (TemplatedItemsList<TView, TItem> list in _attachedItems)
				{
					list.PropertyChanged -= OnChildListPropertyChanged;
					list.PropertyChanging -= OnChildListPropertyChanging;
				}

				_attachedItems.Clear();

				if (raiseReset)
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}
	}
}