#nullable disable
using System.Collections;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class TemplatedItemSourceFactory
	{
		internal static object Create(IEnumerable itemsSource, DataTemplate itemTemplate, BindableObject container,
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext mauiContext = null)
		{
			switch (itemsSource)
			{
				case IList observable when itemsSource is INotifyCollectionChanged:
					return new ObservableItemTemplateCollection(observable, itemTemplate, container, itemHeight, itemWidth, itemSpacing, mauiContext);
				case IList list:
					return new ItemTemplateContextList(list, itemTemplate, container, itemHeight, itemWidth, itemSpacing, mauiContext);
			}

			return new ItemTemplateContextEnumerable(itemsSource, itemTemplate, container, itemHeight, itemWidth, itemSpacing, mauiContext);
		}

		internal static object CreateGrouped(IEnumerable itemsSource, DataTemplate itemTemplate,
			DataTemplate groupHeaderTemplate, DataTemplate groupFooterTemplate, BindableObject container, IMauiContext mauiContext = null)
		{
			return new GroupedItemTemplateCollection(itemsSource, itemTemplate, groupHeaderTemplate, groupFooterTemplate, container, mauiContext);
		}
	}
}


