using System.Collections;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class TemplatedItemSourceFactory
	{
		internal static object Create(IEnumerable itemsSource, DataTemplate itemTemplate)
		{
			switch (itemsSource)
			{
				case IList list when itemsSource is INotifyCollectionChanged:
					return new ObservableItemTemplateCollection(list, itemTemplate);
			}

			return new ItemTemplateEnumerator(itemsSource, itemTemplate);
		}
	}
}