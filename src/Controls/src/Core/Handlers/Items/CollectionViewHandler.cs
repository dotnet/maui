#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CollectionViewHandler
	{
		public CollectionViewHandler() : base(Mapper)
		{

		}
		public CollectionViewHandler(PropertyMapper mapper = null) : base(mapper ?? Mapper)
		{

		}

		public static PropertyMapper<CollectionView, CollectionViewHandler> Mapper = new PropertyMapper<CollectionView, CollectionViewHandler>(ViewMapper)
		{
			[Controls.ItemsView.ItemsSourceProperty.PropertyName] = MapItemsSource,
			[Controls.ItemsView.HorizontalScrollBarVisibilityProperty.PropertyName] = MapHorizontalScrollBarVisibility,
			[Controls.ItemsView.VerticalScrollBarVisibilityProperty.PropertyName] = MapVerticalScrollBarVisibility,
			[Controls.ItemsView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
			[Controls.ItemsView.EmptyViewProperty.PropertyName] = MapEmptyView,
			[Controls.ItemsView.EmptyViewTemplateProperty.PropertyName] = MapEmptyViewTemplate,
			[Controls.ItemsView.FlowDirectionProperty.PropertyName] = MapFlowDirection,
			[Controls.ItemsView.IsVisibleProperty.PropertyName] = MapIsVisible,
			[Controls.ItemsView.ItemsUpdatingScrollModeProperty.PropertyName] = MapItemsUpdatingScrollMode,
			[StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
			[StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooterTemplate,
			[StructuredItemsView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
			[StructuredItemsView.ItemSizingStrategyProperty.PropertyName] = MapItemSizingStrategy,
			[SelectableItemsView.SelectedItemProperty.PropertyName] = MapSelectedItem,
			[SelectableItemsView.SelectedItemsProperty.PropertyName] = MapSelectedItems,
			[SelectableItemsView.SelectionModeProperty.PropertyName] = MapSelectionMode,
			[GroupableItemsView.IsGroupedProperty.PropertyName] = MapIsGrouped,
#if TIZEN
			[StructuredItemsView.HeaderProperty.PropertyName] = MapHeader,
			[StructuredItemsView.FooterProperty.PropertyName] = MapFooter,
			[GroupableItemsView.GroupFooterTemplateProperty.PropertyName] = MapIsGrouped,
			[GroupableItemsView.GroupHeaderTemplateProperty.PropertyName] = MapIsGrouped,
#endif
			[ReorderableItemsView.CanReorderItemsProperty.PropertyName] = MapCanReorderItems

		};
	}
}
