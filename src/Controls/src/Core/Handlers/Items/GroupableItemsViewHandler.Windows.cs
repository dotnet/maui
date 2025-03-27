#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> : SelectableItemsViewHandler<TItemsView> where TItemsView : GroupableItemsView
	{
		public static void MapIsGrouped(GroupableItemsViewHandler<TItemsView> handler, GroupableItemsView itemsView)
		{
			handler.UpdateItemsSource();
		}

		protected override CollectionViewSource CreateCollectionViewSource()
		{
			if (ItemsView != null && ItemsView.IsGrouped)
			{
				var itemTemplate = Element.ItemTemplate;
				var itemsSource = Element.ItemsSource;

				return (itemTemplate is not null && itemsSource is not null)
					? CreateGroupedCollectionViewSource(itemsSource, itemTemplate)
					: CreateDefaultGroupedCollectionViewSource(itemsSource);
			}
			else
			{
				return base.CreateCollectionViewSource();
			}
		}

		private CollectionViewSource CreateGroupedCollectionViewSource(IEnumerable itemsSource, DataTemplate itemTemplate)
		{
			return new CollectionViewSource
			{
				Source = TemplatedItemSourceFactory.CreateGrouped(itemsSource, itemTemplate,
				ItemsView.GroupHeaderTemplate, ItemsView.GroupFooterTemplate, Element, mauiContext: MauiContext),
				IsSourceGrouped = true,
				ItemsPath = new Microsoft.UI.Xaml.PropertyPath(nameof(GroupTemplateContext.Items))
			};
		}

		// Creates and returns a grouped CollectionViewSource using itemsSource as the data source when an itemTemplate is not defined.
		private CollectionViewSource CreateDefaultGroupedCollectionViewSource(IEnumerable itemsSource)
		{
			return new CollectionViewSource
			{
				Source = itemsSource,
				IsSourceGrouped = true,
			};
		}

		protected override void UpdateItemTemplate()
		{
			base.UpdateItemTemplate();

			ListViewBase.GroupStyleSelector = new GroupHeaderStyleSelector();
		}
	}
}
