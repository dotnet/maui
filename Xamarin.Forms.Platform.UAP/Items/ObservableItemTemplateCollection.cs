using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.UWP
{
	internal class ObservableItemTemplateCollection : ObservableCollection<ItemTemplatePair>
	{
		readonly IList _innerSource;
		readonly DataTemplate _itemTemplate;

		public ObservableItemTemplateCollection(IList itemsSource, DataTemplate itemTemplate)
		{
			if (!(itemsSource is INotifyCollectionChanged notifyCollectionChanged))
			{
				throw new ArgumentException($"{nameof(itemsSource)} must implement {nameof(INotifyCollectionChanged)}");
			}

			_innerSource = itemsSource;
			_itemTemplate = itemTemplate;

			for (int n = 0; n < itemsSource.Count; n++)
			{
				Add(new ItemTemplatePair (itemTemplate, itemsSource[n]));
			}

			notifyCollectionChanged.CollectionChanged += InnerCollectionChanged;
		}

		void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			// TODO hartez 2018/07/31 16:02:50 Handle the rest of these cases 
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					break;
				case NotifyCollectionChangedAction.Reset:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			// TODO hartez 2018-07-31 01:47 PM Figure out what scenarios might cause a NewStartingIndex of -1
			// I'm worried that the IndexOf lookup could cause problems when the list has duplicate items
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _innerSource.IndexOf(args.NewItems[0]);

			for (int n = args.NewItems.Count - 1; n >= 0; n--)
			{
				Insert(startIndex, new ItemTemplatePair(_itemTemplate, args.NewItems[n]));
			}
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex > -1 ? args.OldStartingIndex : _innerSource.IndexOf(args.OldItems[0]);

			for (int n = 0; n < args.OldItems.Count; n++)
			{
				RemoveAt(startIndex);
			}
		}
	}
}