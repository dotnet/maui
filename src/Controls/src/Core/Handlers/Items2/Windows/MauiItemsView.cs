using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WApp = Microsoft.UI.Xaml.Application;
using WControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
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
	WVisibility _emptyViewVisibility = WVisibility.Collapsed;

	ContentControl? _headerContentControl;
	FrameworkElement? _header;

	ContentControl? _footerContentControl;
	FrameworkElement? _footer;

	WStackPanel? _containerPanel;
	FrameworkElement? _itemsRepeater;
	bool _isHorizontalLayout;
	ScrollViewer? _scrollViewer;

	public MauiItemsView()
	{
		Template = (WControlTemplate)WApp.Current.Resources["MauiItemsViewTemplate"];

		// Suppress the native WinUI ItemContainer visual states (PointerOver,
		// Pressed, Selected, and their combinations) so they don't overlay
		// on top of MAUI's own VisualStateManager states. Setting these on
		// the parent ItemsView cascades to all child ItemContainer instances.
		// See: https://github.com/microsoft/microsoft-ui-xaml/blob/main/src/controls/dev/ItemContainer/ItemContainer_themeresources.xaml
		// Fixes: https://github.com/dotnet/maui/issues/13197
		var transparent = new WSolidColorBrush(Microsoft.UI.Colors.Transparent);

		// Background fills (PART_CommonVisual.Fill) — suppress the gray overlay
		// that WinUI shows on PointerOver/Pressed so it doesn't interfere with
		// MAUI's own VisualStateManager states. Fixes: #13197
		Resources["ItemContainerBackground"] = transparent;
		Resources["ItemContainerPointerOverBackground"] = transparent;
		Resources["ItemContainerPressedBackground"] = transparent;

		// Border strokes (PART_CommonVisual.Stroke)
		Resources["ItemContainerBorderBrush"] = transparent;
		Resources["ItemContainerPointerOverBorderBrush"] = transparent;
		Resources["ItemContainerPressedBorderBrush"] = transparent;
	}

	/// <summary>Gets or sets the visibility of the empty view overlay.</summary>
	public WVisibility EmptyViewVisibility
	{
		get => _emptyViewVisibility;
		set
		{
			_emptyViewVisibility = value;
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
		_scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

		if (_emptyViewContentControl is not null)
		{
			if (_emptyView is not null)
			{
				_emptyViewContentControl.Content = _emptyView;
			}
			_emptyViewContentControl.Visibility = _emptyViewVisibility;
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

			if (_scrollViewer is not null)
			{
				_scrollViewer.HorizontalScrollMode = UI.Xaml.Controls.ScrollMode.Enabled;
				_scrollViewer.VerticalScrollMode = UI.Xaml.Controls.ScrollMode.Disabled;
				_scrollViewer.HorizontalScrollBarVisibility = UI.Xaml.Controls.ScrollBarVisibility.Auto;
				_scrollViewer.VerticalScrollBarVisibility = UI.Xaml.Controls.ScrollBarVisibility.Disabled;
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

			if (_scrollViewer is not null)
			{
				_scrollViewer.HorizontalScrollMode = UI.Xaml.Controls.ScrollMode.Disabled;
				_scrollViewer.VerticalScrollMode = UI.Xaml.Controls.ScrollMode.Enabled;
				_scrollViewer.HorizontalScrollBarVisibility = UI.Xaml.Controls.ScrollBarVisibility.Disabled;
				_scrollViewer.VerticalScrollBarVisibility = UI.Xaml.Controls.ScrollBarVisibility.Auto;
			}
		}
	}

	protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
	{
		_mauiEmptyView?.Measure(availableSize.Width, availableSize.Height);

		// For horizontal layouts, the ScrollViewer provides infinite width to its content,
		// which prevents UniformGridLayout's ItemsStretch=Fill from stretching items to fill
		// the viewport when there are few items. Setting MinWidth on the ItemsRepeater
		// ensures items stretch to at least the viewport width.
		// Using MinWidth (not Width) preserves horizontal scrolling when content exceeds
		// the viewport — DesiredSize = max(contentWidth, MinWidth), so the ScrollViewer
		// still sees the full content extent for many-item scenarios.
		if (_isHorizontalLayout && _itemsRepeater is not null)
		{
			if (!double.IsInfinity(availableSize.Width) && availableSize.Width > 0)
			{
				_itemsRepeater.MinWidth = availableSize.Width;
			}
		}
		else if (_itemsRepeater is not null)
		{
			// Clear MinWidth for vertical layouts - width is naturally constrained
			// by the ScrollViewer when horizontal scroll is disabled.
			_itemsRepeater.ClearValue(MinWidthProperty);
		}

		return base.MeasureOverride(availableSize);
	}

	protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
	{
		_mauiEmptyView?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

		return base.ArrangeOverride(finalSize);
	}

}
