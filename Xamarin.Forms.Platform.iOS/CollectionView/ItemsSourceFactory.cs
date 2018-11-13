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
			switch (itemsSource)
			{
				case IList _ when itemsSource is INotifyCollectionChanged:
					return new ObservableItemsSource(itemsSource, collectionView);
				case IEnumerable<object> generic:
					return new ListSource(generic);
			}

			return new ListSource(itemsSource);
		}
	}
}