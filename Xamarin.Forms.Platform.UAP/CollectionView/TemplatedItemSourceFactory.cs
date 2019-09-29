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
				case IList observable when itemsSource is INotifyCollectionChanged:
					return new ObservableItemTemplateCollection(observable, itemTemplate, container, itemHeight, itemWidth, itemSpacing);
				case IList list:
					return new ItemTemplateContextList(list, itemTemplate, container, itemHeight, itemWidth, itemSpacing);
			}

			return new ItemTemplateContextEnumerable(itemsSource, itemTemplate, container, itemHeight, itemWidth, itemSpacing);
		}
	}
}