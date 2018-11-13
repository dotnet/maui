using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ItemsSourceFactory
	{
		public static IItemsViewSource Create(IEnumerable itemsSource, RecyclerView.Adapter adapter)
		{
			switch (itemsSource)
			{
				case IList _ when itemsSource is INotifyCollectionChanged:
					return new ObservableItemsSource(itemsSource, adapter);
				case IEnumerable<object> generic:
					return new ListSource(generic);
			}

			return new ListSource(itemsSource);
		}
	}
}