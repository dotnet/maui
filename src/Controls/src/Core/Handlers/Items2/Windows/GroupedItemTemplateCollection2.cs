#nullable disable
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
								count += itemsList.Count;
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
					if (_groupHeaderTemplate is not null)
					{
						Items.Add(new ItemTemplateContext2(_groupHeaderTemplate, group, _container, null,
							null, null, isHeader: true, isFooter: false, mauiContext: _mauiContext));
					}

					foreach (var item in itemsList)
					{
						Items.Add(new ItemTemplateContext2(_itemTemplate, item, _container, mauiContext: _mauiContext));
					}

					if (_groupFooterTemplate is not null)
					{
						Items.Add(new ItemTemplateContext2(_groupFooterTemplate, group, _container, null,
							null, null, isHeader: false, isFooter: true, mauiContext: _mauiContext));
					}

				}
			}

			var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnCollectionChanged(reset);
		}
	}
}
