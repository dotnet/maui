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
	internal class FormsGridView : GridView, IEmptyView
	{
		int _span;
		ItemsWrapGrid _wrapGrid;
		ContentControl _emptyViewContentControl;
		FrameworkElement _emptyView;
		View _formsEmptyView;
		Orientation _orientation;

		public FormsGridView()
		{
			// Using the full style for this control, because for some reason on 16299 we can't set the ControlTemplate
			// (it just fails silently saying it can't find the resource key)
			DefaultStyleKey = typeof(FormsGridView);

			RegisterPropertyChangedCallback(ItemsPanelProperty, ItemsPanelChanged);

			ChoosingItemContainer += OnChoosingItemContainer;
		}

		public int Span
		{
			get => _span;
			set
			{
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
			_wrapGrid = this.GetFirstDescendant<ItemsWrapGrid>();

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

		void UpdateItemSize()
		{
			// Avoid the ItemWrapGrid grow beyond what this grid view is configured to
			_wrapGrid.MaximumRowsOrColumns = Span;

			if (_orientation == Orientation.Horizontal)
			{
				_wrapGrid.ItemHeight = Math.Floor(_wrapGrid.ActualHeight / Span);
			}
			else
			{
				_wrapGrid.ItemWidth = Math.Floor(_wrapGrid.ActualWidth / Span);
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

			if (_emptyView != null && _emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			_formsEmptyView?.Layout(new Rect(0, 0, finalSize.Width, finalSize.Height));

			return base.ArrangeOverride(finalSize);
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			GroupFooterItemTemplateContext.EnsureSelectionDisabled(element, item);
			base.PrepareContainerForItemOverride(element, item);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl == null)
			{
				return;
			}

			_emptyViewContentControl.Visibility = visibility;
		}
	}
}
