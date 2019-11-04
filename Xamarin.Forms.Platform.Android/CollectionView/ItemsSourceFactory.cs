using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ItemsSourceFactory
	{
		public static IItemsViewSource Create(IEnumerable itemsSource, ICollectionChangedNotifier notifier)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			switch (itemsSource)
			{
				case IList _ when itemsSource is INotifyCollectionChanged:
					return new ObservableItemsSource(itemsSource as IList, notifier);
				case IEnumerable<object> generic:
					return new ListSource(generic);
			}

			return new ListSource(itemsSource);
		}

		public static IItemsViewSource Create(IEnumerable itemsSource, RecyclerView.Adapter adapter)
		{
			return Create(itemsSource, new AdapterNotifier(adapter));
		}

		public static IItemsViewSource Create(ItemsView itemsView, RecyclerView.Adapter adapter)
		{
			return Create(itemsView.ItemsSource, adapter);
		}

		public static IGroupableItemsViewSource Create(GroupableItemsView itemsView, RecyclerView.Adapter adapter)
		{
			var source = itemsView.ItemsSource;

			if (itemsView.IsGrouped && source != null)
			{
				return new ObservableGroupedSource(itemsView, new AdapterNotifier(adapter));
			}

			return new UngroupedItemsSource(Create(itemsView.ItemsSource, adapter));
		}
	}
}