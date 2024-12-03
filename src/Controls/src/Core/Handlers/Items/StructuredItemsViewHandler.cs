#nullable disable
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

		public static PropertyMapper<TItemsView, StructuredItemsViewHandler<TItemsView>> StructuredItemsViewMapper = new(ItemsViewMapper)
		{
#if TIZEN
			[StructuredItemsView.HeaderProperty.PropertyName] = MapHeader,
			[StructuredItemsView.FooterProperty.PropertyName] = MapFooter,
			[StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeader,
			[StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooter,
#endif
			[StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
			[StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooterTemplate,
			[StructuredItemsView.HeaderProperty.PropertyName] = MapHeaderTemplate,
			[StructuredItemsView.FooterProperty.PropertyName] = MapFooterTemplate,
			[StructuredItemsView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
			[StructuredItemsView.ItemSizingStrategyProperty.PropertyName] = MapItemSizingStrategy
		};

	}
}
