using System.Collections;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class TemplatedItemSourceFactory
	{
		internal static object Create(IEnumerable itemsSource, DataTemplate itemTemplate, BindableObject container, double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null)
		{
			switch (itemsSource)
			{
				case IList list when itemsSource is INotifyCollectionChanged:
					return new ObservableItemTemplateCollection(list, itemTemplate, container, itemHeight, itemWidth, itemSpacing);
			}

			return new ItemTemplateEnumerator(itemsSource, itemTemplate, container, itemHeight, itemWidth, itemSpacing);
		}
	}
}