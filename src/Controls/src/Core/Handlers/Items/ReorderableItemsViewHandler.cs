#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class ReorderableItemsViewHandler<TItemsView> where TItemsView : ReorderableItemsView
	{
		public ReorderableItemsViewHandler() : base(ReorderableItemsViewMapper)
		{

		}
		public ReorderableItemsViewHandler(PropertyMapper mapper = null) : base(mapper ?? ReorderableItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, ReorderableItemsViewHandler<TItemsView>> ReorderableItemsViewMapper = new PropertyMapper<TItemsView, ReorderableItemsViewHandler<TItemsView>>(GroupableItemsViewHandler<GroupableItemsView>.GroupableItemsViewMapper)
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
#if WINDOWS || __ANDROID__
			[GroupableItemsView.GroupFooterTemplateProperty.PropertyName] = MapIsGrouped,
			[GroupableItemsView.GroupHeaderTemplateProperty.PropertyName] = MapIsGrouped,
#endif
			[ReorderableItemsView.CanReorderItemsProperty.PropertyName] = MapCanReorderItems,
		};
	}
}
