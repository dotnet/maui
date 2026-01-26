#nullable disable
using System;
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
			var itemsSource = Element.ItemsSource;
			if (ItemsView is not null && ItemsView.IsGrouped && itemsSource is not null)
			{
				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.CreateGrouped(itemsSource, Element.ItemTemplate,
					ItemsView.GroupHeaderTemplate, ItemsView.GroupFooterTemplate, Element, mauiContext: MauiContext),
					IsSourceGrouped = true,
					ItemsPath = new Microsoft.UI.Xaml.PropertyPath(nameof(GroupTemplateContext.Items))
				};
			}
			else
			{
				return base.CreateCollectionViewSource();
			}
		}

		protected override void UpdateItemTemplate()
		{
			base.UpdateItemTemplate();

			ListViewBase.GroupStyleSelector = new GroupHeaderStyleSelector();
		}
	}
}
