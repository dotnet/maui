#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;


namespace Microsoft.Maui.Controls.Handlers.Items2;

internal static class ItemsSourceFactory2
{
    public static ILoopItemsViewSource CreateForCarouselView(IEnumerable itemsSource, UICollectionViewController collectionViewController, bool loop)
    {
        if (itemsSource == null)
        {
            return new EmptySource();
        }

        switch (itemsSource)
        {
            case IList _ when itemsSource is INotifyCollectionChanged:
                return new LoopObservableItemsSource2(itemsSource as IList, collectionViewController, loop);
            case IEnumerable _ when itemsSource is INotifyCollectionChanged:
                return new LoopObservableItemsSource2(itemsSource as IEnumerable, collectionViewController, loop);
            case IEnumerable<object> generic:
                return new LoopListSource2(generic, loop);
        }

        return new LoopListSource(itemsSource, loop);
    }

}
