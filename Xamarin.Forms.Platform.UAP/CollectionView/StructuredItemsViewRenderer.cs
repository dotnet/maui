using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UAP;

namespace Xamarin.Forms.Platform.UWP
{
	public class StructuredItemsViewRenderer : ItemsViewRenderer
	{
		StructuredItemsView _structuredItemsView;
		View _currentHeader;
		View _currentFooter;

		protected override IItemsLayout Layout { get => _structuredItemsView.ItemsLayout; }

		protected override void SetUpNewElement(ItemsView newElement)
		{
			_structuredItemsView = newElement as StructuredItemsView;

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
		}

		protected override ListViewBase SelectListViewBase()
		{
			switch (Layout)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridView(gridItemsLayout);
				case LinearItemsLayout listItemsLayout
					when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					return CreateHorizontalListView();
			}

			// Default to a plain old vertical ListView
			return new FormsListView();
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

			var header = _structuredItemsView.Header;

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
					var headerTemplate = _structuredItemsView.HeaderTemplate;
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

			var footer = _structuredItemsView.Footer;

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
					var footerTemplate = _structuredItemsView.FooterTemplate;
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

		protected override void HandleLayoutPropertyChange(PropertyChangedEventArgs property)
		{
			if (property.Is(GridItemsLayout.SpanProperty))
			{
				if (ListViewBase is FormsGridView formsGridView)
				{
					formsGridView.MaximumRowsOrColumns = ((GridItemsLayout)Layout).Span;
				}
			}
		}

		static ListViewBase CreateGridView(GridItemsLayout gridItemsLayout)
		{
			var gridView = new FormsGridView();

			if (gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				gridView.UseHorizontalItemsPanel();

				// TODO hartez 2018/06/06 12:13:38 Should this logic just be built into FormsGridView?	
				ScrollViewer.SetHorizontalScrollMode(gridView, ScrollMode.Auto);
				ScrollViewer.SetHorizontalScrollBarVisibility(gridView,
					Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto);
			}
			else
			{
				gridView.UseVerticalItemsPanel();
			}

			gridView.MaximumRowsOrColumns = gridItemsLayout.Span;

			return gridView;
		}

		static ListViewBase CreateHorizontalListView()
		{
			// TODO hartez 2018/06/05 16:18:57 Is there any performance benefit to caching the ItemsPanelTemplate lookup?	
			// TODO hartez 2018/05/29 15:38:04 Make sure the ItemsViewStyles.xaml xbf gets into the nuspec	
			var horizontalListView = new Windows.UI.Xaml.Controls.ListView()
			{
				ItemsPanel =
					(ItemsPanelTemplate)Windows.UI.Xaml.Application.Current.Resources["HorizontalListItemsPanel"]
			};

			ScrollViewer.SetHorizontalScrollMode(horizontalListView, ScrollMode.Auto);
			ScrollViewer.SetHorizontalScrollBarVisibility(horizontalListView,
				Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto);

			return horizontalListView;
		}
	}
}
