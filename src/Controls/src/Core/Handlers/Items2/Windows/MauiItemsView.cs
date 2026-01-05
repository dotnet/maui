using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WApp = Microsoft.UI.Xaml.Application;
using WControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal partial class MauiItemsView : UI.Xaml.Controls.ItemsView, IEmptyView
	{
		ContentControl? _emptyViewContentControl;
		FrameworkElement? _emptyView;
		View? _mauiEmptyView;

		ContentControl? _headerContentControl;
		FrameworkElement? _header;
		View? _mauiHeader;
		
		ContentControl? _footerContentControl;
		FrameworkElement? _footer;
		View? _mauiFooter;

		WStackPanel? _containerPanel;
		FrameworkElement? _itemsRepeater;
		bool _isHorizontalLayout;

		public MauiItemsView()
		{
			Template = (WControlTemplate)WApp.Current.Resources["MauiItemsViewTemplate"];
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility),
				typeof(MauiItemsView), new PropertyMetadata(WVisibility.Collapsed, EmptyViewVisibilityChanged));

		static void EmptyViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MauiItemsView itemsView)
			{
				// Update this manually; normally we'd just bind this, but TemplateBinding doesn't seem to work
				// for WASDK right now.
				itemsView.UpdateEmptyViewVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility EmptyViewVisibility
		{
			get
			{
				return (WVisibility)GetValue(EmptyViewVisibilityProperty);
			}
			set
			{
				SetValue(EmptyViewVisibilityProperty, value);
			}
		}
		
		public static readonly DependencyProperty HeaderVisibilityProperty =
			DependencyProperty.Register(nameof(HeaderVisibility), typeof(Visibility),
				typeof(MauiItemsView), new PropertyMetadata(WVisibility.Collapsed, HeaderVisibilityChanged));

		static void HeaderVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MauiItemsView itemsView)
			{
				itemsView.UpdateHeaderVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility HeaderVisibility
		{
			get => (WVisibility)GetValue(HeaderVisibilityProperty);
			set => SetValue(HeaderVisibilityProperty, value);
		}
		
		public static readonly DependencyProperty FooterVisibilityProperty =
			DependencyProperty.Register(nameof(FooterVisibility), typeof(Visibility),
				typeof(MauiItemsView), new PropertyMetadata(WVisibility.Collapsed, FooterVisibilityChanged));

		static void FooterVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MauiItemsView itemsView)
			{
				itemsView.UpdateFooterVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility FooterVisibility
		{
			get => (WVisibility)GetValue(FooterVisibilityProperty);
			set => SetValue(FooterVisibilityProperty, value);
		}

		public void SetEmptyView(FrameworkElement emptyView, View mauiEmptyView)
		{
			_emptyView = emptyView;
			_mauiEmptyView = mauiEmptyView;

			if (_emptyViewContentControl is not null)
			{
				_emptyViewContentControl.Content = emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}
		
		public void SetHeader(FrameworkElement header, View? mauiHeader)
		{
			_header = header;
			_mauiHeader = mauiHeader;

			if (_headerContentControl is not null)
			{
				_headerContentControl.Content = header;
				UpdateHeaderVisibility(HeaderVisibility);
			}
		}
		
		public void SetFooter(FrameworkElement footer, View? mauiFooter)
		{
			_footer = footer;
			_mauiFooter = mauiFooter;

			if (_footerContentControl is not null)
			{
				_footerContentControl.Content = footer;
				UpdateFooterVisibility(FooterVisibility);
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;
			_headerContentControl = GetTemplateChild("HeaderContentControl") as ContentControl;
			_footerContentControl = GetTemplateChild("FooterContentControl") as ContentControl;
			_containerPanel = GetTemplateChild("PART_ContainerGrid") as WStackPanel;
			_itemsRepeater = GetTemplateChild("PART_ItemsRepeater") as FrameworkElement;

			if (_emptyView is not null && _emptyViewContentControl is not null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
			
			if (_header is not null && _headerContentControl is not null)
			{
				_headerContentControl.Content = _header;
				UpdateHeaderVisibility(HeaderVisibility);
			}
			
			if (_footer is not null && _footerContentControl is not null)
			{
				_footerContentControl.Content = _footer;
				UpdateFooterVisibility(FooterVisibility);
			}
			
			// Apply orientation if it was set before template was applied
			ApplyLayoutOrientation();
		}
		
		public void SetLayoutOrientation(bool isHorizontal)
		{
			_isHorizontalLayout = isHorizontal;
			ApplyLayoutOrientation();
		}

		void ApplyLayoutOrientation()
		{
			if (_containerPanel is null || _headerContentControl is null || _footerContentControl is null || _itemsRepeater is null)
			{
				return;
			}

			if (_isHorizontalLayout)
			{
				_containerPanel.Orientation = Orientation.Horizontal;
				_itemsRepeater.VerticalAlignment = UI.Xaml.VerticalAlignment.Top;
				_itemsRepeater.HorizontalAlignment = UI.Xaml.HorizontalAlignment.Left;
				_headerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Top;
				_headerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Left;
				_footerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Top;
				_footerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Left;
			}
			else
			{
				_containerPanel.Orientation = Orientation.Vertical;
				_itemsRepeater.VerticalAlignment = UI.Xaml.VerticalAlignment.Top;
				_itemsRepeater.HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch;
				_headerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Top;
				_headerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Stretch;
				_footerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Top;
				_footerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Stretch;
			}
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			_mauiEmptyView?.Measure(availableSize.Width, availableSize.Height);

			return base.MeasureOverride(availableSize);
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			_mauiEmptyView?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

			return base.ArrangeOverride(finalSize);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl is null)
			{
				return;
			}

			_emptyViewContentControl.Visibility = visibility;
		}
		
		void UpdateHeaderVisibility(WVisibility visibility)
		{
			if (_headerContentControl is null)
			{
				return;
			}

			_headerContentControl.Visibility = visibility;
		}
		
		void UpdateFooterVisibility(WVisibility visibility)
		{
			if (_footerContentControl is null)
			{
				return;
			}

			_footerContentControl.Visibility = visibility;
		}
	}
}
