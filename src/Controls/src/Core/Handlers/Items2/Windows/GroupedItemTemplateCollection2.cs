using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class GroupedItemTemplateCollection2 : ObservableCollection<ItemTemplateContext2>
	{
		readonly IEnumerable _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly DataTemplate _groupHeaderTemplate;
		readonly DataTemplate _groupFooterTemplate;
		readonly BindableObject _container;
		readonly IMauiContext? _mauiContext;
		readonly Dictionary<object, INotifyCollectionChanged> _groupSubscriptions = new();

		public GroupedItemTemplateCollection2(IEnumerable itemsSource,
			DataTemplate itemTemplate, DataTemplate groupHeaderTemplate, DataTemplate groupFooterTemplate,
			BindableObject container, IMauiContext? mauiContext = null)
		{
			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_groupHeaderTemplate = groupHeaderTemplate;
			_groupFooterTemplate = groupFooterTemplate;
			_container = container;
			_mauiContext = mauiContext;

			AddItems(_itemsSource);
			SubscribeToGroups(_itemsSource);
			if (_itemsSource is IList groupList && _itemsSource is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += GroupsChanged;
			}
		}

		void SubscribeToGroups(IEnumerable? groups)
		{
			if (groups is null) return;
			foreach (var group in groups)
			{
				if (group is INotifyCollectionChanged incc && !_groupSubscriptions.ContainsKey(group))
				{
					incc.CollectionChanged += (s, e) => GroupItemsChanged(group, e);
					_groupSubscriptions[group] = incc;
				}
			}
		}

		void UnsubscribeFromGroups(IEnumerable? groups)
		{
			if (groups is null) return;
			foreach (var group in groups)
			{
				if (_groupSubscriptions.TryGetValue(group, out var incc))
				{
					incc.CollectionChanged -= (s, e) => GroupItemsChanged(group, e);
					_groupSubscriptions.Remove(group);
				}
			}
		}

		void AddItems(IEnumerable? items)
		{
			if (items is null)
			{
				return;
			}

			var newItems = new List<ItemTemplateContext2>();
			int index = Items.Count;
			foreach (var group in items)
			{
				if (group is IList itemsList)
				{
					if (_groupHeaderTemplate is not null)
					{
						var newItem = new ItemTemplateContext2(_groupHeaderTemplate, group, _container, null,
							null, null, true, false, mauiContext: _mauiContext);
						newItems.Add(newItem);
						Items.Add(newItem);
					}

					foreach (var item in itemsList)
					{
						var newItem = new ItemTemplateContext2(_itemTemplate, item, _container, mauiContext: _mauiContext);
						newItems.Add(newItem);
						Items.Add(newItem);
					}

					if (_groupFooterTemplate is not null)
					{
						var newItem = new ItemTemplateContext2(_groupFooterTemplate, group, _container, null,
							null, null, isHeader: false, isFooter: true, mauiContext: _mauiContext);
						newItems.Add(newItem);
						Items.Add(newItem);
					}
				}
			}

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add, newItems, index));
		}

		void GroupsChanged(object? sender, NotifyCollectionChangedEventArgs args)
		{
			_container.Dispatcher.DispatchIfRequired(() => OnNotifyCollectionChanged(args));
		}

		void OnNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddItems(args.NewItems);
					SubscribeToGroups(args.NewItems);
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					if (args.OldItems is not null)
					{
						UnsubscribeFromGroups(args.OldItems);
						var startIndex = args.OldStartingIndex;
						if (startIndex < 0)
						{
							Reset();
							return;
						}

						int count = 0;
						foreach (var item in args.OldItems)
						{
							count++;
							if (item is IList itemsList)
							{
								count += itemsList.Count;
							}
						}

						for (int index = startIndex + count - 1; index >= startIndex; index--)
						{
							RemoveAt(index);
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
			}
		}

		void GroupItemsChanged(object group, NotifyCollectionChangedEventArgs e)
		{
			// Find the start index of this group's items in the flat list
			int groupIndex = -1;
			int current = 0;
			foreach (var g in _itemsSource)
			{
				if (g == group)
				{
					groupIndex = current;
					break;
				}
				current++;
			}
			if (groupIndex == -1) return;

			// Calculate the flat list index for the first item of this group
			int flatIndex = 0;
			current = 0;
			foreach (var g in _itemsSource)
			{
				if (g is IList itemsList)
				{
					if (_groupHeaderTemplate is not null) flatIndex++;
					if (current == groupIndex) break;
					flatIndex += itemsList.Count;
					if (_groupFooterTemplate is not null) flatIndex++;
				}
				current++;
			}

			// Do NOT increment flatIndex for header here; it is already handled above

			if (group is IList groupList)
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						if (e.NewItems is not null)
						{
							int insertIndex = flatIndex + (e.NewStartingIndex >= 0 ? e.NewStartingIndex : groupList.Count - e.NewItems.Count);
							var newItems = new List<ItemTemplateContext2>();
							foreach (var item in e.NewItems)
							{
								var newItem = new ItemTemplateContext2(_itemTemplate, item, _container, mauiContext: _mauiContext);
								newItems.Add(newItem);
								Items.Insert(insertIndex++, newItem);
							}
							OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, insertIndex - newItems.Count));
						}
						break;
					case NotifyCollectionChangedAction.Remove:
						if (e.OldItems is not null)
						{
							int removeIndex = flatIndex + (e.OldStartingIndex >= 0 ? e.OldStartingIndex : 0);
							for (int i = 0; i < e.OldItems.Count; i++)
							{
								Items.RemoveAt(removeIndex);
							}
							OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, removeIndex));
						}
						break;
					case NotifyCollectionChangedAction.Replace:
						if (e.NewItems is not null && e.OldItems is not null)
						{
							int replaceIndex = flatIndex + (e.NewStartingIndex >= 0 ? e.NewStartingIndex : 0);
							for (int i = 0; i < e.NewItems.Count; i++)
							{
								var newItem = new ItemTemplateContext2(_itemTemplate, e.NewItems[i]!, _container, mauiContext: _mauiContext);
								Items[replaceIndex + i] = newItem;
							}
							OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, replaceIndex));
						}
						break;
					case NotifyCollectionChangedAction.Move:
						if (e.OldItems is not null && e.NewItems is not null)
						{
							int oldIndex = flatIndex + (e.OldStartingIndex >= 0 ? e.OldStartingIndex : 0);
							int newIndex = flatIndex + (e.NewStartingIndex >= 0 ? e.NewStartingIndex : 0);
							var moved = new List<ItemTemplateContext2>();
							for (int i = 0; i < e.OldItems.Count; i++)
							{
								var item = Items[oldIndex];
								Items.RemoveAt(oldIndex);
								Items.Insert(newIndex + i, item);
								moved.Add(item);
							}
							OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, moved, newIndex, oldIndex));
						}
						break;
					case NotifyCollectionChangedAction.Reset:
						Reset();
						break;
					}
				}
		}

		public void Reset()
		{
			UnsubscribeFromGroups(_itemsSource);
			Items.Clear();
			AddItems(_itemsSource);
			SubscribeToGroups(_itemsSource);
			var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnCollectionChanged(reset);
		}
	}
}
