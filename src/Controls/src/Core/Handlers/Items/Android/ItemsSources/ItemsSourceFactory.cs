#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class ItemsSourceFactory
	{
		public static IItemsViewSource Create(IEnumerable itemsSource, BindableObject container, ICollectionChangedNotifier notifier)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			switch (itemsSource)
			{
				case IList list when itemsSource is INotifyCollectionChanged:
					return new ObservableItemsSource(new MarshalingObservableCollection(list), container, notifier);
				case IEnumerable _ when itemsSource is INotifyCollectionChanged:
					return new ObservableItemsSource(itemsSource, container, notifier);
				case IList list:
					return new ListSource(list);
				case IEnumerable<object> generic:
					return new ListSource(generic);
			}

			return new ListSource(itemsSource);
		}

		public static IItemsViewSource Create(IEnumerable itemsSource, BindableObject container, RecyclerView.Adapter adapter)
		{
			return Create(itemsSource, container, new AdapterNotifier(adapter));
		}

		public static IItemsViewSource Create(ItemsView itemsView, RecyclerView.Adapter adapter)
		{
			return Create(itemsView.ItemsSource, itemsView, adapter);
		}

		public static IGroupableItemsViewSource Create(GroupableItemsView itemsView, RecyclerView.Adapter adapter)
		{
			var source = itemsView.ItemsSource;

			if (itemsView.IsGrouped && source != null)
			{
				return new ObservableGroupedSource(itemsView, new AdapterNotifier(adapter));
			}

			return new UngroupedItemsSource(Create(itemsView.ItemsSource, itemsView, adapter));
		}
	}
}
