using System.Collections;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal static class TemplatedItemSourceFactory2
	{
		internal static object Create(IEnumerable itemsSource, DataTemplate itemTemplate, BindableObject container,
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext? mauiContext = null)
		{
			switch (itemsSource)
			{
				case IList observable when itemsSource is INotifyCollectionChanged:
					return new ObservableItemTemplateCollection2(observable, itemTemplate, container, itemHeight, itemWidth, itemSpacing, mauiContext);
				case IList list:
					return new ItemTemplateContextList2(list, itemTemplate, container, itemHeight, itemWidth, itemSpacing, mauiContext);
			}

			return new ItemTemplateContextEnumerable2(itemsSource, itemTemplate, container, itemHeight, itemWidth, itemSpacing, mauiContext);
		}

		internal static object CreateGrouped(IEnumerable itemsSource, DataTemplate itemTemplate,
			DataTemplate groupHeaderTemplate, DataTemplate groupFooterTemplate, BindableObject container, IMauiContext? mauiContext = null)
		{
			return new GroupedItemTemplateCollection2(itemsSource, itemTemplate, groupHeaderTemplate, groupFooterTemplate, container, mauiContext);
		}
	}
}
