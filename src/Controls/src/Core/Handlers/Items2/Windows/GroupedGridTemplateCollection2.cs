using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// An observable collection of <see cref="GroupGridContext"/> items, one per group.
	/// <para>
	/// Used for grouped grid mode where each group is a single item in the outer ItemsView.
	/// The outer ItemsView uses StackLayout to virtualize groups, and each group's UI is built
	/// by the <see cref="ItemFactory"/> using the GroupGridContext.
	/// </para>
	/// </summary>
	internal class GroupedGridTemplateCollection2 : ObservableCollection<GroupGridContext>
	{
		readonly IEnumerable _itemsSource;
		readonly DataTemplate? _itemTemplate;
		readonly DataTemplate? _groupHeaderTemplate;
		readonly DataTemplate? _groupFooterTemplate;
		readonly BindableObject _container;
		readonly IMauiContext? _mauiContext;
		readonly Dictionary<object, NotifyCollectionChangedEventHandler> _groupSubscriptions = new();
		bool _suppressNotifications;

		public GroupedGridTemplateCollection2(
			IEnumerable itemsSource,
			DataTemplate? itemTemplate,
			DataTemplate? groupHeaderTemplate,
			DataTemplate? groupFooterTemplate,
			BindableObject container,
			IMauiContext? mauiContext = null)
		{
			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_groupHeaderTemplate = groupHeaderTemplate;
			_groupFooterTemplate = groupFooterTemplate;
			_container = container;
			_mauiContext = mauiContext;

			RebuildGroupList();
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

		GroupGridContext CreateGroupContext(object group) =>
			new(group, _itemTemplate, _groupHeaderTemplate, _groupFooterTemplate, _container, _mauiContext);

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

		void RebuildGroupList()
		{
			Items.Clear();

			foreach (var group in _itemsSource)
			{
				Items.Add(CreateGroupContext(group));
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
		/// Called when items within a group change. Fires a Replace notification for the group
		/// so the ItemsRepeater re-creates the group's UI with the updated items.
		/// </summary>
		void GroupItemsChanged(object group, NotifyCollectionChangedEventArgs e)
		{
			int groupIndex = FindGroupIndex(group);
			if (groupIndex == -1)
				return;

			var oldContext = Items[groupIndex];
			var newContext = CreateGroupContext(group);

			_suppressNotifications = true;
			Items[groupIndex] = newContext;
			_suppressNotifications = false;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Replace, newContext, oldContext, groupIndex));
		}

		int FindGroupIndex(object targetGroup)
		{
			int index = 0;
			foreach (var group in _itemsSource)
			{
				if (ReferenceEquals(group, targetGroup))
					return index;
				index++;
			}
			return -1;
		}

		void ResetWithoutResubscribe()
		{
			_suppressNotifications = true;
			RebuildGroupList();
			_suppressNotifications = false;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Full reset that also resubscribes to all groups.
		/// </summary>
		public void Reset()
		{
			UnsubscribeFromAllGroups();

			_suppressNotifications = true;
			RebuildGroupList();
			_suppressNotifications = false;

			SubscribeToGroups(_itemsSource);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Unsubscribes from all group and top-level collection changed events.
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
