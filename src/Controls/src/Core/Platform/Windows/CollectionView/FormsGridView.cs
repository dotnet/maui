#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPControls = Microsoft.UI.Xaml.Controls;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class FormsGridView : GridView, IEmptyView
	{
		int _span;
		ItemsWrapGrid _wrapGrid;
		ContentControl _emptyViewContentControl;
		ScrollViewer _scrollViewer;
		FrameworkElement _emptyView;
		View _formsEmptyView;
		Orientation _orientation;
		ItemSizingStrategy _itemSizingStrategy;
		bool _updatingItemSize;
		double _measuredItemWidth;

		public FormsGridView()
		{
			// Using the full style for this control, because for some reason on 16299 we can't set the ControlTemplate
			// (it just fails silently saying it can't find the resource key)
			DefaultStyleKey = typeof(FormsGridView);

			RegisterPropertyChangedCallback(ItemsPanelProperty, ItemsPanelChanged);
			RegisterPropertyChangedCallback(ItemsSourceProperty, ResetMeasuredItemWidth);
			RegisterPropertyChangedCallback(ItemTemplateProperty, ResetMeasuredItemWidth);
			RegisterPropertyChangedCallback(ItemContainerStyleProperty, ResetMeasuredItemWidth);

			ChoosingItemContainer += OnChoosingItemContainer;
			ContainerContentChanging += OnContainerContentChanging;
		}

		public ItemSizingStrategy ItemSizingStrategy
		{
			get => _itemSizingStrategy;
			set
			{
				if (_itemSizingStrategy == value)
					return;

				_itemSizingStrategy = value;
				_measuredItemWidth = 0;
				if (_wrapGrid is not null)
					UpdateItemSize();
			}
		}

		public int Span
		{
			get => _span;
			set
			{
				if (_span != value)
					_measuredItemWidth = 0;
				_span = value;
				if (_wrapGrid != null)
				{
					UpdateItemSize();
				}
			}
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility),
				typeof(FormsGridView), new PropertyMetadata(WVisibility.Collapsed, EmptyViewVisibilityChanged));

		static void EmptyViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FormsGridView gridView)
			{
				// Update this manually; normally we'd just bind this, but TemplateBinding doesn't seem to work
				// for WASDK right now.
				gridView.UpdateEmptyViewVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility EmptyViewVisibility
		{
			get { return (WVisibility)GetValue(EmptyViewVisibilityProperty); }
			set { SetValue(EmptyViewVisibilityProperty, value); }
		}

		public Orientation Orientation
		{
			get => _orientation;
			set
			{
				if (_orientation != value)
					_measuredItemWidth = 0;
				_orientation = value;
				if (_orientation == Orientation.Horizontal)
				{
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["HorizontalGridItemsPanel"];
					ScrollViewer.SetHorizontalScrollMode(this, WScrollMode.Auto);
					ScrollViewer.SetHorizontalScrollBarVisibility(this, UWPControls.ScrollBarVisibility.Auto);
				}
				else
				{
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["VerticalGridItemsPanel"];
				}
			}
		}

		void FindItemsWrapGrid()
		{
			var wrapGrid = this.GetFirstDescendant<ItemsWrapGrid>();
			if (!ReferenceEquals(_wrapGrid, wrapGrid))
			{
				if (_wrapGrid is not null)
				{
					_wrapGrid.SizeChanged -= WrapGridSizeChanged;
				}

				_wrapGrid = wrapGrid;
				_measuredItemWidth = 0;
			}

			if (_wrapGrid == null)
			{
				return;
			}

			_wrapGrid.SizeChanged -= WrapGridSizeChanged;
			_wrapGrid.SizeChanged += WrapGridSizeChanged;

			UpdateItemSize();
		}

		void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
		{
			FindItemsWrapGrid();
		}

		void WrapGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateItemSize();
		}

		void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.InRecycleQueue || _orientation != Orientation.Horizontal ||
				ItemSizingStrategy != ItemSizingStrategy.MeasureAllItems)
			{
				return;
			}

			if (args.Phase == 0)
				args.RegisterUpdateCallback(1, OnContainerContentChanging);
			else
				FindItemsWrapGrid();
		}

		void ResetMeasuredItemWidth(DependencyObject sender, DependencyProperty property)
		{
			_measuredItemWidth = 0;
		}

		void UpdateItemSize()
		{
			if (_updatingItemSize)
				return;

			_updatingItemSize = true;
			try
			{
				// Avoid the ItemWrapGrid grow beyond what this grid view is configured to
				_wrapGrid.MaximumRowsOrColumns = Span;

				if (_orientation == Orientation.Horizontal)
				{
					_wrapGrid.ItemHeight = Math.Floor(_wrapGrid.ActualHeight / Span);
					if (ItemSizingStrategy == ItemSizingStrategy.MeasureAllItems)
					{
						// WinUI's automatic width uses only the first cell.
						var width = _measuredItemWidth;
						var horizontalMargin = 0d;
						var hasMeasuredContent = false;
						var childrenCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(_wrapGrid);
						for (var i = 0; i < childrenCount; i++)
						{
							if (Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(_wrapGrid, i) is GridViewItem container &&
								container.ContentTemplateRoot is UIElement content)
							{
								// Unrealized item controls report a provisional width of 88 for virtualization.
								if (content is ItemContentControl { Content: null })
									continue;

								var margin = container.Margin;
								var padding = container.Padding;
								var height = Math.Max(0, _wrapGrid.ItemHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom);
								content.Measure(new global::Windows.Foundation.Size(double.PositiveInfinity, height));
								// Cache content, not the container's transient theme minimum or previous spacing.
								width = Math.Max(width, content.DesiredSize.Width + padding.Left + padding.Right);
								horizontalMargin = Math.Max(horizontalMargin, margin.Left + margin.Right);
								hasMeasuredContent = true;
							}
						}

						var itemWidth = width + horizontalMargin;
						if (hasMeasuredContent && width > 0 && _wrapGrid.ItemWidth != itemWidth)
							_wrapGrid.ItemWidth = itemWidth;
						_measuredItemWidth = width;
					}
					else
					{
						_wrapGrid.ClearValue(ItemsWrapGrid.ItemWidthProperty);
					}
				}
				else
				{
					if (Span > 1)
					{
						_wrapGrid.ItemWidth = Math.Floor(_wrapGrid.ActualWidth / Span);
					}
					else
					{
						_wrapGrid.ClearValue(ItemsWrapGrid.ItemWidthProperty);
					}
				}
			}
			finally
			{
				_updatingItemSize = false;
			}
		}

		void ItemsPanelChanged(DependencyObject sender, DependencyProperty dp)
		{
			FindItemsWrapGrid();
		}

		public void SetEmptyView(FrameworkElement emptyView, View formsEmptyView)
		{
			_emptyView = emptyView;
			_formsEmptyView = formsEmptyView;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;

			_scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;

			if (_emptyView != null && _emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			_formsEmptyView?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

			return base.ArrangeOverride(finalSize);
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			GroupFooterItemTemplateContext.EnsureSelectionDisabled(element, item);
			base.PrepareContainerForItemOverride(element, item);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl is null)
			{
				return;
			}

			// Adjust the ScrollViewer's hit test visibility if it exists
			if (_scrollViewer is not null)
			{
				// When the empty view is visible, disable hit testing for the ScrollViewer.
				// This ensures that interactions are directed to the empty view instead of the ScrollViewer.
				// In the template, the empty view is placed below the ScrollViewer in the visual tree.
				_scrollViewer.IsHitTestVisible = visibility != WVisibility.Visible;
			}

			_emptyViewContentControl.Visibility = visibility;
		}
	}
}
