#nullable disable
using System.Collections;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPControls = Microsoft.UI.Xaml.Controls;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using System.Collections.Generic;

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
		ItemTemplateContext _currentFocusedItem;

		public FormsGridView()
		{
			// Using the full style for this control, because for some reason on 16299 we can't set the ControlTemplate
			// (it just fails silently saying it can't find the resource key)
			DefaultStyleKey = typeof(FormsGridView);

			RegisterPropertyChangedCallback(ItemsPanelProperty, ItemsPanelChanged);
			Loaded += OnLoaded;
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

		void WrapGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateItemSize();
		}

		void UpdateItemSize()
		{
			if (_orientation == Orientation.Horizontal)
			{
				_wrapGrid.ItemHeight = _wrapGrid.ActualHeight / Span;
			}
			else
			{
				_wrapGrid.ItemWidth = _wrapGrid.ActualWidth / Span;
			}
		}

		void ItemsPanelChanged(DependencyObject sender, DependencyProperty dp)
		{
			FindItemsWrapGrid();
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			FindItemsWrapGrid();

			// Watch for keyboard events on the GridView
			((GridView)sender).AddHandler(KeyDownEvent, new KeyEventHandler(CheckForTapActivation), true);

			// Keep track of the focused item as the user clicks/taps/arrows around the GridView
			GotFocus += TrackFocusedItem;
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
			if (_formsEmptyView != null)
			{
				_formsEmptyView.Layout(new Rect(0, 0, finalSize.Width, finalSize.Height));
			}

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

		// Find the index of a data item in a CollectionView's items source
		static int FindIndex(object item, IEnumerable itemsSource)
		{
			if (itemsSource is IList<object> list)
			{
				// The easy way
				return list.IndexOf(item);
			}

			// The hard way, if it comes to that
			var index = 0;
			foreach (var o in itemsSource)
			{
				if (o == item)
				{
					return index;
				}

				index += 1;
			}

			// Not Found
			return -1;
		}

		void TrackFocusedItem(object sender, RoutedEventArgs args)
		{
			if (args.OriginalSource is GridViewItem { Content: ItemTemplateContext itc })
			{
				_currentFocusedItem = itc;
			}
			else
			{
				_currentFocusedItem = null;
			}
		}

		void CheckForTapActivation(object sender, KeyRoutedEventArgs args)
		{
			if (args.Key != VirtualKey.Enter && args.Key != VirtualKey.Space)
			{
				return;
			}

			if (_currentFocusedItem == null)
			{
				return;
			}

			// Get the bound data item
			var item = _currentFocusedItem.Item;

			// And the CollectionView this represents
			var collectionView = (CollectionView)_currentFocusedItem.Container;

			// From there we can retrieve the data source
			var itemsSource = collectionView.ItemsSource;

			var index = FindIndex(item, itemsSource);

			var element = collectionView.LogicalChildrenInternal[index]; // TODO This index might need to be adjusted to account for the Header

			if (element is not View view)
			{
				return;
			}

			foreach (var gestureRecognizer in view.GestureRecognizers)
			{
				// TODO Do we need to add a check for semantic info here? Or is this sufficient?
				if (gestureRecognizer is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
					tgr.SendTapped(view);
				}
			}
		}
	}
}
