#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Maui.Controls.Platform
{
	internal class GroupedItemTemplateCollection2 : ObservableCollection<ItemTemplateContext>
	{
		readonly IEnumerable _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly DataTemplate _groupHeaderTemplate;
		readonly DataTemplate _groupFooterTemplate;
		readonly BindableObject _container;
		readonly IMauiContext _mauiContext;

		public GroupedItemTemplateCollection2(IEnumerable itemsSource, 
			DataTemplate itemTemplate, DataTemplate groupHeaderTemplate, DataTemplate groupFooterTemplate, 
			BindableObject container, IMauiContext mauiContext = null)
		{
			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_groupHeaderTemplate = groupHeaderTemplate;
			_groupFooterTemplate = groupFooterTemplate;
			_container = container;
			_mauiContext = mauiContext;

			AddItems(_itemsSource);
			if (_itemsSource is IList groupList && _itemsSource is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += GroupsChanged;
			}
		}

		private void AddItems(IEnumerable items)
		{
			var newItems = new List<ItemTemplateContext>();
			int index = Items.Count;
			foreach (var group in items)
			{
				if (group is IList itemsList)
				{
					var newItem = new ItemTemplateContext(_groupHeaderTemplate, group, _container, mauiContext: _mauiContext);
					newItems.Add(newItem);
					Items.Add(newItem);

					foreach (var item in itemsList)
					{
						newItem = new ItemTemplateContext(_itemTemplate, item, _container, mauiContext: _mauiContext);
						newItems.Add(newItem);
						Items.Add(newItem);
					}
				}
			}
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add, newItems, index));
		}

		void GroupsChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			_container.Dispatcher.DispatchIfRequired(() => Incc_CollectionChanged(args));
		}

		private void Incc_CollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(args.NewItems);
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					{
						var startIndex = args.OldStartingIndex;
						if (startIndex < 0)
						{
							// INCC implementation isn't giving us enough information to know where the removed items were in the
							// collection. So the best we can do is a full Reset.
							Reset();
							return;
						}

						int count = 0;
						foreach (var item in args.OldItems)
						{
							count++;

							if (item is IList itemsList)
							{
								foreach (var childItem in itemsList)
								{
									count++;
								}
							}
						}

						for (int n = startIndex + count - 1; n >= startIndex; n--)
						{
							RemoveAt(n);
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
			}
		}

		public void Reset()
		{
			Items.Clear();

			foreach (var group in _itemsSource)
			{
				if (group is IList itemsList)
				{
					Items.Add(new ItemTemplateContext(_groupHeaderTemplate, group, _container, mauiContext: _mauiContext));
					foreach (var item in itemsList)
					{
						Items.Add(new ItemTemplateContext(_itemTemplate, item, _container, mauiContext: _mauiContext));
					}
				}
			}

			var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnCollectionChanged(reset);
		}
	}

	internal class GroupedItemTemplateCollection : ObservableCollection<GroupTemplateContext>
	{
		readonly IEnumerable _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly DataTemplate _groupHeaderTemplate;
		readonly DataTemplate _groupFooterTemplate;
		readonly BindableObject _container;
		readonly IMauiContext _mauiContext;
		readonly IList _groupList;

		public GroupedItemTemplateCollection(IEnumerable itemsSource, DataTemplate itemTemplate,
			DataTemplate groupHeaderTemplate, DataTemplate groupFooterTemplate, BindableObject container, IMauiContext mauiContext = null)
		{
			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_groupHeaderTemplate = groupHeaderTemplate;
			_groupFooterTemplate = groupFooterTemplate;
			_container = container;
			_mauiContext = mauiContext;

			foreach (var group in _itemsSource)
			{
				var groupTemplateContext = CreateGroupTemplateContext(group);
				Add(groupTemplateContext);
			}

			if (_itemsSource is IList groupList && _itemsSource is INotifyCollectionChanged incc)
			{
				_groupList = groupList;
				incc.CollectionChanged += GroupsChanged;
			}
		}

		GroupTemplateContext CreateGroupTemplateContext(object group)
		{
			var groupHeaderTemplateContext = _groupHeaderTemplate != null
					? new ItemTemplateContext(_groupHeaderTemplate, group, _container, mauiContext: _mauiContext) : null;

			var groupFooterTemplateContext = _groupFooterTemplate != null
				? new GroupFooterItemTemplateContext(_groupFooterTemplate, group, _container, mauiContext: _mauiContext) : null;

			// This is where we'll eventually look at GroupItemPropertyName
			var groupItemsList = TemplatedItemSourceFactory.Create(group as IEnumerable, _itemTemplate, _container, mauiContext: _mauiContext);

			return new GroupTemplateContext(groupHeaderTemplateContext, groupFooterTemplateContext, groupItemsList);
		}

		void GroupsChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			_container.Dispatcher.DispatchIfRequired(() => GroupsChanged(args));
		}

		void GroupsChanged(NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
			}
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupList.IndexOf(args.NewItems[0]);

			var count = args.NewItems.Count;

			for (int n = 0; n < count; n++)
			{
				Insert(startIndex, CreateGroupTemplateContext(args.NewItems[n]));
			}
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			if (args.OldStartingIndex > args.NewStartingIndex)
			{
				for (int n = 0; n < count; n++)
				{
					Move(args.OldStartingIndex + n, args.NewStartingIndex + n);
				}

				return;
			}

			for (int n = count - 1; n >= 0; n--)
			{
				Move(args.OldStartingIndex + n, args.NewStartingIndex + n);
			}
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a full Reset.
				Reset();
				return;
			}

			var count = args.OldItems.Count;

			for (int n = startIndex + count - 1; n >= startIndex; n--)
			{
				RemoveAt(n);
			}
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newItemCount = args.NewItems.Count;

			if (newItemCount == args.OldItems.Count)
			{
				for (int n = 0; n < newItemCount; n++)
				{
					var index = args.OldStartingIndex + n;
					var oldItem = this[index];
					var newItem = CreateGroupTemplateContext(args.NewItems[0]);
					Items[index] = newItem;
					var update = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index);
					OnCollectionChanged(update);
				}
			}
			else
			{
				// If we're replacing one set with an equal size set, we can do a soft reset; if not, we have to completely
				// rebuild the collection
				Reset();
			}
		}

		void Reset()
		{
			Items.Clear();

			foreach (var group in _itemsSource)
			{
				var groupTemplateContext = CreateGroupTemplateContext(group);
				Items.Add(groupTemplateContext);
			}

			var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnCollectionChanged(reset);
		}
	}
}