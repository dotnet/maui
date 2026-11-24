#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		public SelectableItemsViewHandler() : base(SelectableItemsViewMapper)
		{

		}

		public SelectableItemsViewHandler(PropertyMapper mapper = null) : base(mapper ?? SelectableItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, SelectableItemsViewHandler<TItemsView>> SelectableItemsViewMapper = new(StructuredItemsViewMapper)
		{
			[SelectableItemsView.SelectedItemProperty.PropertyName] = MapSelectedItem,
			[SelectableItemsView.SelectedItemsProperty.PropertyName] = MapSelectedItems,
			[SelectableItemsView.SelectionModeProperty.PropertyName] = MapSelectionMode,
			[SelectableItemsView.IsItemTapEnabledMapperKey] = MapIsItemTapEnabled,
		};

		internal static void MapIsItemTapEnabled(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
			=> handler.UpdateIsItemTapEnabled(itemsView);

		partial void UpdateIsItemTapEnabled(SelectableItemsView itemsView);

	}
}
