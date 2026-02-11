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
		readonly DataTemplate? _groupHeaderTemplate;
		readonly DataTemplate? _groupFooterTemplate;
		readonly BindableObject _container;
		readonly IMauiContext? _mauiContext;
		readonly Dictionary<object, NotifyCollectionChangedEventHandler> _groupSubscriptions = new();
		bool _suppressNotifications;

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

			RebuildFlatList();
			SubscribeToGroups(_itemsSource);

			if (_itemsSource is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += GroupsChanged;
			}
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!_suppressNotifications)
			{
				base.OnCollectionChanged(e);
			}
		}

		ItemTemplateContext2 CreateItemContext(object item) =>
			new(_itemTemplate, item, _container, mauiContext: _mauiContext);

		ItemTemplateContext2 CreateHeaderContext(object group) =>
			new(_groupHeaderTemplate!, group, _container, null, null, null, isHeader: true, isFooter: false, mauiContext: _mauiContext);

		ItemTemplateContext2 CreateFooterContext(object group) =>
			new(_groupFooterTemplate!, group, _container, null, null, null, isHeader: false, isFooter: true, mauiContext: _mauiContext);

		void SubscribeToGroups(IEnumerable? groups)
		{
			if (groups is null)
				return;

			foreach (var group in groups)
			{
				SubscribeToGroup(group);
			}
		}

		void SubscribeToGroup(object group)
		{
			if (group is INotifyCollectionChanged incc && !_groupSubscriptions.ContainsKey(group))
			{
				var handler = CreateGroupItemsChangedHandler(group);
				incc.CollectionChanged += handler;
				_groupSubscriptions[group] = handler;
			}
		}

		NotifyCollectionChangedEventHandler CreateGroupItemsChangedHandler(object group) =>
			(s, e) => _container.Dispatcher.DispatchIfRequired(() => GroupItemsChanged(group, e));

		void UnsubscribeFromGroups(IEnumerable? groups)
		{
			if (groups is null)
				return;

			foreach (var group in groups)
			{
				UnsubscribeFromGroup(group);
			}
		}

		void UnsubscribeFromGroup(object group)
		{
			if (_groupSubscriptions.TryGetValue(group, out var handler))
			{
				if (group is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= handler;
				}
				_groupSubscriptions.Remove(group);
			}
		}

		void UnsubscribeFromAllGroups()
		{
			foreach (var kvp in _groupSubscriptions)
			{
				if (kvp.Key is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= kvp.Value;
				}
			}
			_groupSubscriptions.Clear();
		}

		void RebuildFlatList()
		{
			Items.Clear();

			foreach (var group in _itemsSource)
			{
				if (group is not IList itemsList)
					continue;

				if (_groupHeaderTemplate is not null)
				{
					Items.Add(CreateHeaderContext(group));
				}

				foreach (var item in itemsList)
				{
					Items.Add(CreateItemContext(item));
				}

				if (_groupFooterTemplate is not null)
				{
					Items.Add(CreateFooterContext(group));
				}
			}
		}

		void GroupsChanged(object? sender, NotifyCollectionChangedEventArgs args) =>
			_container.Dispatcher.DispatchIfRequired(() => OnGroupsCollectionChanged(args));

		void OnGroupsCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					SubscribeToGroups(args.NewItems);
					ResetWithoutResubscribe();
					break;
				case NotifyCollectionChangedAction.Move:
					ResetWithoutResubscribe();
					break;
				case NotifyCollectionChangedAction.Remove:
					if (args.OldItems is not null)
					{
						UnsubscribeFromGroups(args.OldItems);
						ResetWithoutResubscribe();
					}
					break;

				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
			}
		}

		/// <summary>
		/// Gets the flat index for the first item in a group by iterating through the source collection.
		/// Returns the index after the group header (if present), pointing to where group items start.
		/// </summary>
		int GetFlatIndexForGroupItems(object targetGroup)
		{
			int flatIndex = 0;

			foreach (var group in _itemsSource)
			{
				if (ReferenceEquals(group, targetGroup))
				{
					// Found the group - return index after header (if present)
					return _groupHeaderTemplate is not null ? flatIndex + 1 : flatIndex;
				}

				if (group is IList itemsList)
				{
					// Count header + items + footer for this group
					if (_groupHeaderTemplate is not null)
						flatIndex++;

					flatIndex += itemsList.Count;

					if (_groupFooterTemplate is not null)
						flatIndex++;
				}
			}

			return -1;
		}

		void GroupItemsChanged(object group, NotifyCollectionChangedEventArgs e)
		{
			int flatIndex = GetFlatIndexForGroupItems(group);
			if (flatIndex == -1)
				return;

			if (group is not IList groupList)
				return;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					HandleGroupItemsAdd(e, flatIndex, groupList);
					break;

				case NotifyCollectionChangedAction.Remove:
					HandleGroupItemsRemove(e, flatIndex);
					break;

				case NotifyCollectionChangedAction.Replace:
					HandleGroupItemsReplace(e, flatIndex);
					break;

				case NotifyCollectionChangedAction.Move:
					HandleGroupItemsMove(e, flatIndex);
					break;

				case NotifyCollectionChangedAction.Reset:
					ResetWithoutResubscribe();
					break;
			}
		}

		void HandleGroupItemsAdd(NotifyCollectionChangedEventArgs e, int flatIndex, IList groupList)
		{
			if (e.NewItems is null)
				return;

			int insertIndex = flatIndex + (e.NewStartingIndex >= 0 ? e.NewStartingIndex : groupList.Count - e.NewItems.Count);
			var newItems = new List<ItemTemplateContext2>(e.NewItems.Count);

			_suppressNotifications = true;
			foreach (var item in e.NewItems)
			{
				var newItem = CreateItemContext(item);
				newItems.Add(newItem);
				Items.Insert(insertIndex++, newItem);
			}
			_suppressNotifications = false;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add, newItems, insertIndex - newItems.Count));
		}

		void HandleGroupItemsRemove(NotifyCollectionChangedEventArgs e, int flatIndex)
		{
			if (e.OldItems is null)
				return;

			int removeIndex = flatIndex + (e.OldStartingIndex >= 0 ? e.OldStartingIndex : 0);
			var removedItems = new List<ItemTemplateContext2>(e.OldItems.Count);

			_suppressNotifications = true;
			for (int i = 0; i < e.OldItems.Count; i++)
			{
				removedItems.Add(Items[removeIndex]);
				Items.RemoveAt(removeIndex);
			}
			_suppressNotifications = false;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Remove, removedItems, removeIndex));
		}

		void HandleGroupItemsReplace(NotifyCollectionChangedEventArgs e, int flatIndex)
		{
			if (e.NewItems is null || e.OldItems is null)
				return;

			int replaceIndex = flatIndex + (e.NewStartingIndex >= 0 ? e.NewStartingIndex : 0);
			var oldItems = new List<ItemTemplateContext2>(e.NewItems.Count);
			var newItems = new List<ItemTemplateContext2>(e.NewItems.Count);

			_suppressNotifications = true;
			for (int i = 0; i < e.NewItems.Count; i++)
			{
				oldItems.Add(Items[replaceIndex + i]);
				var newItem = CreateItemContext(e.NewItems[i]!);
				newItems.Add(newItem);
				Items[replaceIndex + i] = newItem;
			}
			_suppressNotifications = false;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Replace, newItems, oldItems, replaceIndex));
		}

		void HandleGroupItemsMove(NotifyCollectionChangedEventArgs e, int flatIndex)
		{
			if (e.OldItems is null)
				return;

			int oldIndex = flatIndex + (e.OldStartingIndex >= 0 ? e.OldStartingIndex : 0);
			int newIndex = flatIndex + (e.NewStartingIndex >= 0 ? e.NewStartingIndex : 0);
			var movedItems = new List<ItemTemplateContext2>(e.OldItems.Count);

			_suppressNotifications = true;
			for (int i = 0; i < e.OldItems.Count; i++)
			{
				var item = Items[oldIndex];
				Items.RemoveAt(oldIndex);
				Items.Insert(newIndex + i, item);
				movedItems.Add(item);
			}
			_suppressNotifications = false;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Move, movedItems, newIndex, oldIndex));
		}

		void ResetWithoutResubscribe()
		{
			_suppressNotifications = true;
			RebuildFlatList();
			_suppressNotifications = false;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Full reset that also resubscribes to all groups.
		/// Used for Replace and Reset actions where group references may have changed.
		/// </summary>
		public void Reset()
		{
			UnsubscribeFromAllGroups();

			_suppressNotifications = true;
			RebuildFlatList();
			_suppressNotifications = false;

			SubscribeToGroups(_itemsSource);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Unsubscribes from all group and top-level collection changed events.
		/// Must be called when the collection is being replaced or the handler disconnects.
		/// </summary>
		public void CleanUp()
		{
			UnsubscribeFromAllGroups();

			if (_itemsSource is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged -= GroupsChanged;
			}
		}
	}
}
