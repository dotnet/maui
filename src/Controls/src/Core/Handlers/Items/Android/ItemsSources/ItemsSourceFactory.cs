#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class ItemsSourceFactory
	{
		public static IItemsViewSource Create(IEnumerable itemsSource, BindableObject container, ICollectionChangedNotifier notifier)
			=> Create(itemsSource, container, notifier, null);

		internal static IItemsViewSource Create(IEnumerable itemsSource, BindableObject container, ICollectionChangedNotifier notifier, IDispatcher dispatcher)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			switch (itemsSource)
			{
				case IEnumerable observable when itemsSource is INotifyCollectionChanged:
					// During RecyclerView layout, defer the snapshot mutation together with its adapter notification.
					return new ObservableItemsSource(new MarshalingObservableCollection(observable, dispatcher ?? container.Dispatcher), container, notifier, disposeItemsSource: true);
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
			=> Create(itemsView, adapter, null);

		internal static IGroupableItemsViewSource Create(GroupableItemsView itemsView, RecyclerView.Adapter adapter, IDispatcher dispatcher)
		{
			var source = itemsView.ItemsSource;

			if (itemsView.IsGrouped && source != null)
			{
				return new ObservableGroupedSource(itemsView, new AdapterNotifier(adapter), dispatcher);
			}

			return new UngroupedItemsSource(Create(itemsView.ItemsSource, itemsView, new AdapterNotifier(adapter), dispatcher));
		}
	}
}
