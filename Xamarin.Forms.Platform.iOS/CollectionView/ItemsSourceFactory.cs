using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal static class ItemsSourceFactory
	{
		public static IItemsViewSource Create(IEnumerable itemsSource, UICollectionViewController collectionViewController)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			switch (itemsSource)
			{
				case INotifyCollectionChanged _:
					return new ObservableItemsSource(itemsSource as IList, collectionViewController);
				case IEnumerable _:
				default:
					return new ListSource(itemsSource);
			}
		}

		public static IItemsViewSource CreateGrouped(IEnumerable itemsSource, UICollectionViewController collectionViewController)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			return new ObservableGroupedSource(itemsSource, collectionViewController);
		}
	}
}