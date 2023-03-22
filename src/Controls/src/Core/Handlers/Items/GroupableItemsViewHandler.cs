#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class GroupableItemsViewHandler<TItemsView> where TItemsView : GroupableItemsView
	{
		public GroupableItemsViewHandler() : base(GroupableItemsViewMapper)
		{

		}
		public GroupableItemsViewHandler(PropertyMapper mapper = null) : base(mapper ?? GroupableItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, GroupableItemsViewHandler<TItemsView>> GroupableItemsViewMapper = new PropertyMapper<TItemsView, GroupableItemsViewHandler<TItemsView>>(SelectableItemsViewHandler<SelectableItemsView>.SelectableItemsViewMapper)
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
#if WINDOWS || __ANDROID__ || TIZEN
			[GroupableItemsView.GroupFooterTemplateProperty.PropertyName] = MapIsGrouped,
			[GroupableItemsView.GroupHeaderTemplateProperty.PropertyName] = MapIsGrouped,
#endif
		};
	}
}
