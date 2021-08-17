using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		public StructuredItemsViewHandler() : base(StructuredItemsViewMapper)
		{

		}
		public StructuredItemsViewHandler(PropertyMapper mapper = null) : base(mapper ?? StructuredItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, StructuredItemsViewHandler<TItemsView>> StructuredItemsViewMapper = new PropertyMapper<TItemsView, StructuredItemsViewHandler<TItemsView>>(ItemsViewHandler<ItemsView>.ItemsViewMapper)
		{ 
					[StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
					[StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooterTemplate,
					[StructuredItemsView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
					[StructuredItemsView.ItemSizingStrategyProperty.PropertyName] = MapItemSizingStrategy
		};
	
	}
}
