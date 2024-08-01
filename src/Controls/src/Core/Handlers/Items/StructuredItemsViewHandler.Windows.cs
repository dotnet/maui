#nullable disable
using System;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class StructuredItemsViewHandler<TItemsView> : ItemsViewHandler<TItemsView> where TItemsView : StructuredItemsView
	{
		View _currentHeader;
		View _currentFooter;
		WeakNotifyPropertyChangedProxy _layoutPropertyChangedProxy;
		PropertyChangedEventHandler _layoutPropertyChanged;

		~StructuredItemsViewHandler() => _layoutPropertyChangedProxy?.Unsubscribe();

		protected override IItemsLayout Layout { get => ItemsView?.ItemsLayout; }

		protected override void ConnectHandler(WItemsView platformView)
		{
			base.ConnectHandler(platformView);

			if (Layout is not null)
			{
				_layoutPropertyChanged ??= LayoutPropertyChanged;
				_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(Layout, _layoutPropertyChanged);
			}
			else if (_layoutPropertyChangedProxy is not null)
			{
				_layoutPropertyChangedProxy.Unsubscribe();
				_layoutPropertyChangedProxy = null;
			}
		}

		protected override void DisconnectHandler(WItemsView platformView)
		{
			base.DisconnectHandler(platformView);

			if (_layoutPropertyChangedProxy is not null)
			{
				_layoutPropertyChangedProxy.Unsubscribe();
				_layoutPropertyChangedProxy = null;
			}
		}

		void LayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == GridItemsLayout.SpanProperty.PropertyName)
				UpdateItemsLayoutSpan();
			else if (e.PropertyName == GridItemsLayout.HorizontalItemSpacingProperty.PropertyName || e.PropertyName == GridItemsLayout.VerticalItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
			else if (e.PropertyName == LinearItemsLayout.ItemSpacingProperty.PropertyName)
				UpdateItemsLayoutItemSpacing();
		}

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

		protected override WItemsView SelectListViewBase()
		{
			switch (VirtualView.ItemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridView(gridItemsLayout);
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical:
					return new WItemsView()
					{
						Layout = new Microsoft.UI.Xaml.Controls.StackLayout
						{
							Orientation = Orientation.Vertical,
							Spacing = listItemsLayout.ItemSpacing
						}
					};
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					var itemsView = new WItemsView()
					{
						Layout = new Microsoft.UI.Xaml.Controls.StackLayout
						{
							Orientation = Orientation.Horizontal,
							Spacing = listItemsLayout.ItemSpacing
						}
					};
					ScrollViewer.SetHorizontalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Enabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(itemsView, UI.Xaml.Controls.ScrollBarVisibility.Visible);

					ScrollViewer.SetVerticalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(itemsView, UI.Xaml.Controls.ScrollBarVisibility.Disabled);
					return itemsView;
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

			//var header = ItemsView.Header;
			//
			//switch (header)
			//{
			//	case null:
			//		ListViewBase.Header = null;
			//		break;
			//
			//	case string text:
			//		ListViewBase.HeaderTemplate = null;
			//		ListViewBase.Header = new TextBlock { Text = text };
			//		break;
			//
			//	case View view:
			//		ListViewBase.HeaderTemplate = ViewTemplate;
			//		_currentHeader = view;
			//		Element.AddLogicalChild(_currentHeader);
			//		ListViewBase.Header = view;
			//		break;
			//
			//	default:
			//		var headerTemplate = ItemsView.HeaderTemplate;
			//		if (headerTemplate != null)
			//		{
			//			ListViewBase.HeaderTemplate = ItemsViewTemplate;
			//			ListViewBase.Header = new ItemTemplateContext(headerTemplate, header, Element);
			//		}
			//		else
			//		{
			//			ListViewBase.HeaderTemplate = null;
			//			ListViewBase.Header = null;
			//		}
			//		break;
			//}
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

			//var footer = ItemsView.Footer;
			//
			//switch (footer)
			//{
			//	case null:
			//		ListViewBase.Footer = null;
			//		break;
			//
			//	case string text:
			//		ListViewBase.FooterTemplate = null;
			//		ListViewBase.Footer = new TextBlock { Text = text };
			//		break;
			//
			//	case View view:
			//		ListViewBase.FooterTemplate = ViewTemplate;
			//		_currentFooter = view;
			//		Element.AddLogicalChild(_currentFooter);
			//		ListViewBase.Footer = view;
			//		break;
			//
			//	default:
			//		var footerTemplate = ItemsView.FooterTemplate;
			//		if (footerTemplate != null)
			//		{
			//			ListViewBase.FooterTemplate = ItemsViewTemplate;
			//			ListViewBase.Footer = new ItemTemplateContext(footerTemplate, footer, Element);
			//		}
			//		else
			//		{
			//			ListViewBase.FooterTemplate = null;
			//			ListViewBase.Footer = null;
			//		}
			//		break;
			//}
		}

		static WItemsView CreateGridView(GridItemsLayout gridItemsLayout)
		{
			return new WItemsView()
			{
				Layout = new UniformGridLayout()
				{
					Orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
						? Orientation.Vertical : Orientation.Horizontal,
					MaximumRowsOrColumns = gridItemsLayout.Span,
					MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing,
					MinRowSpacing = gridItemsLayout.VerticalItemSpacing,
					ItemsStretch = UniformGridLayoutItemsStretch.Fill,
					ItemsJustification = UniformGridLayoutItemsJustification.Start,
				}
			};
		}

		void UpdateItemsLayoutSpan()
		{
			if (ListViewBase.Layout is UniformGridLayout listViewLayout &&
				Layout is GridItemsLayout gridItemsLayout)
			{
				listViewLayout.MaximumRowsOrColumns = gridItemsLayout.Span;
			}
		}

		void UpdateItemsLayoutItemSpacing()
		{
			if (ListViewBase.Layout is UniformGridLayout listViewLayout &&
				Layout is GridItemsLayout gridItemsLayout)
			{
				listViewLayout.MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing;
				listViewLayout.MinRowSpacing = gridItemsLayout.VerticalItemSpacing;
			}
			else if (ListViewBase.Layout is Microsoft.UI.Xaml.Controls.StackLayout stackLayout &&
				Layout is LinearItemsLayout linearItemsLayout)
			{
				stackLayout.Spacing = linearItemsLayout.ItemSpacing;
			}
		}
	}
}