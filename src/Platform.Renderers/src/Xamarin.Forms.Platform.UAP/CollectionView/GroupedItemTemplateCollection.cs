using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace Xamarin.Forms.Platform.UWP
{
	internal class GroupedItemTemplateCollection : ObservableCollection<GroupTemplateContext>
	{
		readonly IEnumerable _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly DataTemplate _groupHeaderTemplate;
		readonly DataTemplate _groupFooterTemplate;
		readonly BindableObject _container;
		readonly IList _groupList;

		public GroupedItemTemplateCollection(IEnumerable itemsSource, DataTemplate itemTemplate, 
			DataTemplate groupHeaderTemplate, DataTemplate groupFooterTemplate, BindableObject container)
		{
			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_groupHeaderTemplate = groupHeaderTemplate;
			_groupFooterTemplate = groupFooterTemplate;
			_container = container;

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
					? new ItemTemplateContext(_groupHeaderTemplate, group, _container) : null;

			var groupFooterTemplateContext = _groupFooterTemplate != null
				? new GroupFooterItemTemplateContext(_groupFooterTemplate, group, _container) : null;

			// This is where we'll eventually look at GroupItemPropertyName
			var groupItemsList = TemplatedItemSourceFactory.Create(group as IEnumerable, _itemTemplate, _container);

			return new GroupTemplateContext(groupHeaderTemplateContext, groupFooterTemplateContext, groupItemsList);
		}

		void GroupsChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (_container.Dispatcher.IsInvokeRequired)
			{
				_container.Dispatcher.BeginInvokeOnMainThread(() => GroupsChanged(args));
			}
			else
			{
				GroupsChanged(args);
			}
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