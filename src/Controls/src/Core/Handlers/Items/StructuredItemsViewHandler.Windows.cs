using System;
using Microsoft.UI.Xaml.Controls;
using WSetter = Microsoft.UI.Xaml.Setter;
using WStyle = Microsoft.UI.Xaml.Style;
using Microsoft.Maui.Controls.Platform;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WASDKApp = Microsoft.UI.Xaml.Application;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
		
		}

		public static void MapItemSizingStrategy(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
		
		}



		protected override ListViewBase SelectListViewBase()
		{
			switch (VirtualView.ItemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridView(gridItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical:
					return CreateVerticalListView(listItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					return CreateHorizontalListView(listItemsLayout);
			}

			throw new NotImplementedException("The layout is not implemented");
		}

		static ListViewBase CreateGridView(GridItemsLayout gridItemsLayout)
		{
			return new FormsGridView
			{
				Orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? Orientation.Horizontal
					: Orientation.Vertical,

				Span = gridItemsLayout.Span,
				ItemContainerStyle = GetItemContainerStyle(gridItemsLayout)
			};
		}

		static ListViewBase CreateVerticalListView(LinearItemsLayout listItemsLayout)
		{
			return new FormsListView()
			{
				ItemContainerStyle = GetVerticalItemContainerStyle(listItemsLayout)
			};
		}

		static ListViewBase CreateHorizontalListView(LinearItemsLayout listItemsLayout)
		{
			var horizontalListView = new FormsListView()
			{
				ItemsPanel = (ItemsPanelTemplate)WASDKApp.Current.Resources["HorizontalListItemsPanel"],
				ItemContainerStyle = GetHorizontalItemContainerStyle(listItemsLayout)
			};
			ScrollViewer.SetVerticalScrollBarVisibility(horizontalListView, Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden);
			ScrollViewer.SetVerticalScrollMode(horizontalListView, WScrollMode.Disabled);
			ScrollViewer.SetHorizontalScrollMode(horizontalListView, WScrollMode.Auto);
			ScrollViewer.SetHorizontalScrollBarVisibility(horizontalListView, Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto);

			return horizontalListView;
		}

		static WStyle GetItemContainerStyle(GridItemsLayout layout)
		{
			var h = layout?.HorizontalItemSpacing ?? 0;
			var v = layout?.VerticalItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(h, v, h, v);

			var style = new WStyle(typeof(GridViewItem));

			style.Setters.Add(new WSetter(GridViewItem.MarginProperty, margin));
			style.Setters.Add(new WSetter(GridViewItem.PaddingProperty, WinUIHelpers.CreateThickness(0)));

			return style;
		}

		static WStyle GetVerticalItemContainerStyle(LinearItemsLayout layout)
		{
			var v = layout?.ItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(0, v, 0, v);

			var style = new WStyle(typeof(ListViewItem));

			style.Setters.Add(new WSetter(ListViewItem.MarginProperty, margin));
			style.Setters.Add(new WSetter(GridViewItem.PaddingProperty, WinUIHelpers.CreateThickness(0)));

			return style;
		}

		static WStyle GetHorizontalItemContainerStyle(LinearItemsLayout layout)
		{
			var h = layout?.ItemSpacing ?? 0;
			var padding = WinUIHelpers.CreateThickness(h, 0, h, 0);

			var style = new WStyle(typeof(ListViewItem));

			style.Setters.Add(new WSetter(ListViewItem.PaddingProperty, padding));

			return style;
		}
	}
}
