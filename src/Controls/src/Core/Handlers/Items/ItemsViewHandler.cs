#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> where TItemsView : ItemsView
	{
		public ItemsViewHandler() : this(ItemsViewMapper, ItemsViewCommandMapper)
		{

		}

		public ItemsViewHandler(PropertyMapper mapper = null) : this(mapper ?? ItemsViewMapper, ItemsViewCommandMapper)
		{

		}

		// Make public for NET9
#if !ANDROID
		private
#endif
		protected ItemsViewHandler(PropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper ?? ItemsViewCommandMapper)
		{
		}

		// Make public for NET9
		internal static CommandMapper<TItemsView, ItemsViewHandler<TItemsView>> ItemsViewCommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		public static PropertyMapper<TItemsView, ItemsViewHandler<TItemsView>> ItemsViewMapper = new(ViewMapper)
		{
			[Controls.ItemsView.ItemsSourceProperty.PropertyName] = MapItemsSource,
			[Controls.ItemsView.HorizontalScrollBarVisibilityProperty.PropertyName] = MapHorizontalScrollBarVisibility,
			[Controls.ItemsView.VerticalScrollBarVisibilityProperty.PropertyName] = MapVerticalScrollBarVisibility,
			[Controls.ItemsView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
			[Controls.ItemsView.EmptyViewProperty.PropertyName] = MapEmptyView,
			[Controls.ItemsView.EmptyViewTemplateProperty.PropertyName] = MapEmptyViewTemplate,
			[Controls.ItemsView.FlowDirectionProperty.PropertyName] = MapFlowDirection,
			[Controls.ItemsView.IsVisibleProperty.PropertyName] = MapIsVisible,
			[Controls.ItemsView.ItemsUpdatingScrollModeProperty.PropertyName] = MapItemsUpdatingScrollMode
		};
	}
}
