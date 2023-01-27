using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WASDKApp = Microsoft.UI.Xaml.Application;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSetter = Microsoft.UI.Xaml.Setter;
using WStyle = Microsoft.UI.Xaml.Style;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		View _currentHeader;
		View _currentFooter;

		protected override IItemsLayout Layout { get => ItemsView?.ItemsLayout; }

		public static void MapHeaderTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateHeader();
		}

		public static void MapFooterTemplate(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateFooter();
		}

		public static void MapItemsLayout(StructuredItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateItemsLayout();
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

		protected virtual void UpdateHeader()
		{
			if (ListViewBase == null)
			{
				return;
			}

			if (_currentHeader != null)
			{
				Element.RemoveLogicalChild(_currentHeader);
				_currentHeader = null;
			}

			var header = ItemsView.Header;

			switch (header)
			{
				case null:
					ListViewBase.Header = null;
					break;

				case string text:
					ListViewBase.HeaderTemplate = null;
					ListViewBase.Header = new TextBlock { Text = text };
					break;

				case View view:
					ListViewBase.HeaderTemplate = ViewTemplate;
					_currentHeader = view;
					Element.AddLogicalChild(_currentHeader);
					ListViewBase.Header = view;
					break;

				default:
					var headerTemplate = ItemsView.HeaderTemplate;
					if (headerTemplate != null)
					{
						ListViewBase.HeaderTemplate = ItemsViewTemplate;
						ListViewBase.Header = new ItemTemplateContext(headerTemplate, header, Element);
					}
					else
					{
						ListViewBase.HeaderTemplate = null;
						ListViewBase.Header = null;
					}
					break;
			}
		}

		protected virtual void UpdateFooter()
		{
			if (ListViewBase == null)
			{
				return;
			}

			if (_currentFooter != null)
			{
				Element.RemoveLogicalChild(_currentFooter);
				_currentFooter = null;
			}

			var footer = ItemsView.Footer;

			switch (footer)
			{
				case null:
					ListViewBase.Footer = null;
					break;

				case string text:
					ListViewBase.FooterTemplate = null;
					ListViewBase.Footer = new TextBlock { Text = text };
					break;

				case View view:
					ListViewBase.FooterTemplate = ViewTemplate;
					_currentFooter = view;
					Element.AddLogicalChild(_currentFooter);
					ListViewBase.Footer = view;
					break;

				default:
					var footerTemplate = ItemsView.FooterTemplate;
					if (footerTemplate != null)
					{
						ListViewBase.FooterTemplate = ItemsViewTemplate;
						ListViewBase.Footer = new ItemTemplateContext(footerTemplate, footer, Element);
					}
					else
					{
						ListViewBase.FooterTemplate = null;
						ListViewBase.Footer = null;
					}
					break;
			}
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
			style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

			return style;
		}

		static WStyle GetVerticalItemContainerStyle(LinearItemsLayout layout)
		{
			var v = layout?.ItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(0, v, 0, v);

			var style = new WStyle(typeof(ListViewItem));

			style.Setters.Add(new WSetter(ListViewItem.MarginProperty, margin));
			style.Setters.Add(new WSetter(GridViewItem.PaddingProperty, WinUIHelpers.CreateThickness(0)));
			style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

			return style;
		}

		static WStyle GetHorizontalItemContainerStyle(LinearItemsLayout layout)
		{
			var h = layout?.ItemSpacing ?? 0;
			var padding = WinUIHelpers.CreateThickness(h, 0, h, 0);

			var style = new WStyle(typeof(ListViewItem));

			style.Setters.Add(new WSetter(ListViewItem.PaddingProperty, padding));
			style.Setters.Add(new WSetter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));

			return style;
		}
	}
}
