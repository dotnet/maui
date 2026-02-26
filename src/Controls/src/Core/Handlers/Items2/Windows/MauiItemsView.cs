using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WApp = Microsoft.UI.Xaml.Application;
using WControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using WScrollView = Microsoft.UI.Xaml.Controls.ScrollView;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Handlers.Items2;
/// <summary>
/// Custom <see cref="UI.Xaml.Controls.ItemsView"/> subclass that adds support for
/// empty views, headers, footers, and layout orientation for the CollectionView Handler 2.
/// </summary>
internal partial class MauiItemsView : UI.Xaml.Controls.ItemsView, IEmptyView
{
	ContentControl? _emptyViewContentControl;
	FrameworkElement? _emptyView;
	View? _mauiEmptyView;

	ContentControl? _headerContentControl;
	FrameworkElement? _header;

	ContentControl? _footerContentControl;
	FrameworkElement? _footer;

	WStackPanel? _containerPanel;
	FrameworkElement? _itemsRepeater;
	bool _isHorizontalLayout;
	WScrollView? _scrollView;

	public MauiItemsView()
	{
		Template = (WControlTemplate)WApp.Current.Resources["MauiItemsViewTemplate"];
	}

	/// <summary>Gets or sets the visibility of the empty view overlay.</summary>
	public WVisibility EmptyViewVisibility
	{
		get => _emptyViewContentControl?.Visibility ?? WVisibility.Collapsed;
		set
		{
			if (_emptyViewContentControl is not null)
			{
				_emptyViewContentControl.Visibility = value;
			}
		}
	}

	/// <summary>Gets or sets the visibility of the header element.</summary>
	internal WVisibility HeaderVisibility
	{
		get => _headerContentControl?.Visibility ?? WVisibility.Collapsed;
		set
		{
			if (_headerContentControl is not null)
			{
				_headerContentControl.Visibility = value;
			}
		}
	}

	/// <summary>Gets or sets the visibility of the footer element.</summary>
	internal WVisibility FooterVisibility
	{
		get => _footerContentControl?.Visibility ?? WVisibility.Collapsed;
		set
		{
			if (_footerContentControl is not null)
			{
				_footerContentControl.Visibility = value;
			}
		}
	}

	/// <summary>Sets the empty view content and its MAUI view counterpart.</summary>
	public void SetEmptyView(FrameworkElement emptyView, View mauiEmptyView)
	{
		_emptyView = emptyView;
		_mauiEmptyView = mauiEmptyView;

		if (_emptyViewContentControl is not null)
		{
			_emptyViewContentControl.Content = emptyView;
		}
	}

	/// <summary>Sets the header content and its MAUI view counterpart.</summary>
	internal void SetHeader(FrameworkElement header, View? mauiHeader)
	{
		_header = header;

		if (_headerContentControl is not null)
		{
			_headerContentControl.Content = header;
		}
	}

	/// <summary>Sets the footer content and its MAUI view counterpart.</summary>
	internal void SetFooter(FrameworkElement footer, View? mauiFooter)
	{
		_footer = footer;

		if (_footerContentControl is not null)
		{
			_footerContentControl.Content = footer;
		}
	}

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;
		_headerContentControl = GetTemplateChild("HeaderContentControl") as ContentControl;
		_footerContentControl = GetTemplateChild("FooterContentControl") as ContentControl;
		_containerPanel = GetTemplateChild("PART_ContainerStack") as WStackPanel;
		_itemsRepeater = GetTemplateChild("PART_ItemsRepeater") as FrameworkElement;
		_scrollView = GetTemplateChild("PART_ScrollView") as WScrollView;

		if (_emptyView is not null && _emptyViewContentControl is not null)
		{
			_emptyViewContentControl.Content = _emptyView;
		}

		if (_header is not null && _headerContentControl is not null)
		{
			_headerContentControl.Content = _header;
		}

		if (_footer is not null && _footerContentControl is not null)
		{
			_footerContentControl.Content = _footer;
		}

		// Apply orientation if it was set before template was applied
		ApplyLayoutOrientation();
	}

	/// <summary>Sets the layout orientation and updates the visual tree accordingly.</summary>
	internal void SetLayoutOrientation(bool isHorizontal)
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
			// For horizontal layout, the container panel should stretch vertically
			_containerPanel.VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch;
			_containerPanel.HorizontalAlignment = UI.Xaml.HorizontalAlignment.Left;

			// Items should stretch vertically (cross-axis)
			_itemsRepeater.VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch;
			_itemsRepeater.HorizontalAlignment = UI.Xaml.HorizontalAlignment.Left;
			_headerContentControl.VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch;
			_headerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Stretch;
			_headerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Left;
			_footerContentControl.VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch;
			_footerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Stretch;
			_footerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Left;

			if (_scrollView is not null)
			{
				_scrollView.ContentOrientation = UI.Xaml.Controls.ScrollingContentOrientation.Horizontal;
			}
		}
		else
		{
			_containerPanel.Orientation = Orientation.Vertical;
			// For vertical layout, the container panel should stretch horizontally
			_containerPanel.VerticalAlignment = UI.Xaml.VerticalAlignment.Top;
			_containerPanel.HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch;

			// Items should stretch horizontally (cross-axis)
			_itemsRepeater.VerticalAlignment = UI.Xaml.VerticalAlignment.Top;
			_itemsRepeater.HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch;
			_headerContentControl.VerticalAlignment = UI.Xaml.VerticalAlignment.Top;
			_headerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Top;
			_headerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Stretch;
			_footerContentControl.VerticalAlignment = UI.Xaml.VerticalAlignment.Top;
			_footerContentControl.VerticalContentAlignment = UI.Xaml.VerticalAlignment.Top;
			_footerContentControl.HorizontalContentAlignment = UI.Xaml.HorizontalAlignment.Stretch;

			if (_scrollView is not null)
			{
				_scrollView.ContentOrientation = UI.Xaml.Controls.ScrollingContentOrientation.Vertical;
			}
		}
	}

	// NOTE: MeasureOverride and ArrangeOverride are currently commented out.
	// The EmptyView is hosted in _emptyViewContentControl (a ContentControl), which automatically
	// handles measure/arrange as part of the WinUI layout system. Manual measure/arrange calls here
	// can cause layout conflicts, especially when header/footer visibility changes dynamically:
	// - When footer is removed: EmptyView may extend beyond bounds (measuring with full size)
	// - When footer is added back: EmptyView may overlap footer (arranging without accounting for footer space)
	// The ContentControl's built-in layout correctly respects the StackPanel's space allocation for
	// header, ItemsRepeater, footer, and EmptyView overlay without needing manual intervention.
	
	//protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
	//{
	//	_mauiEmptyView?.Measure(availableSize.Width, availableSize.Height);
	//	return base.MeasureOverride(availableSize);
	//}

	//protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
	//{
	//	_mauiEmptyView?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
	//	return base.ArrangeOverride(finalSize);
	//}

}
