using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal static class ItemsSourceFactory
	{
		public static IItemsViewSource Create(IEnumerable itemsSource, UICollectionView collectionView)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			switch (itemsSource)
			{
				case IList _ when itemsSource is INotifyCollectionChanged:
					return new ObservableItemsSource(itemsSource as IList, collectionView);
				case IEnumerable<object> generic:
					return new ListSource(generic);
			}

			return new ListSource(itemsSource);
		}

		public static IItemsViewSource CreateGrouped(IEnumerable itemsSource, UICollectionView collectionView)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			return new ObservableGroupedSource(itemsSource, collectionView);
		}
	}
}