using System;
using System.ComponentModel;

using Microsoft.UI.Xaml.Controls;

using UWPApp = Microsoft.UI.Xaml.Application;
using WListView = Microsoft.UI.Xaml.Controls.ListView;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSetter = Microsoft.UI.Xaml.Setter;
using WStyle = Microsoft.UI.Xaml.Style;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class StructuredItemsViewRenderer<TItemsView> : ItemsViewRenderer<TItemsView>
		where TItemsView : StructuredItemsView
	{
		View _currentHeader;
		View _currentFooter;

		protected override IItemsLayout Layout { get => ItemsView?.ItemsLayout; }

		protected override void SetUpNewElement(ItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			if (newElement == null)
			{
				return;
			}

			UpdateHeader();
			UpdateFooter();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.IsOneOf(StructuredItemsView.HeaderProperty, StructuredItemsView.HeaderTemplateProperty))
			{
				UpdateHeader();
			}
			else if (changedProperty.IsOneOf(StructuredItemsView.FooterProperty, StructuredItemsView.FooterTemplateProperty))
			{
				UpdateFooter();
			}
			else if (changedProperty.Is(StructuredItemsView.ItemsLayoutProperty))
			{
				UpdateItemsLayout();
			}
		}

		protected override ListViewBase SelectListViewBase()
		{
			switch (Layout)
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

		protected override void HandleLayoutPropertyChanged(PropertyChangedEventArgs property)
		{
			if (property.Is(GridItemsLayout.SpanProperty))
			{
				if (ListViewBase is FormsGridView formsGridView)
				{
					formsGridView.Span = ((GridItemsLayout)Layout).Span;
				}
			}
			else if (property.Is(GridItemsLayout.HorizontalItemSpacingProperty) || property.Is(GridItemsLayout.VerticalItemSpacingProperty))
			{
				if (ListViewBase is FormsGridView formsGridView)
				{
					formsGridView.ItemContainerStyle = GetItemContainerStyle((GridItemsLayout)Layout);
				}
			}
			else if (property.Is(LinearItemsLayout.ItemSpacingProperty))
			{
				switch (ListViewBase)
				{
					case FormsListView formsListView:
						formsListView.ItemContainerStyle = GetVerticalItemContainerStyle((LinearItemsLayout)Layout);
						break;
					case WListView listView:
						listView.ItemContainerStyle = GetHorizontalItemContainerStyle((LinearItemsLayout)Layout);
						break;
				}
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
				ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["HorizontalListItemsPanel"],
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