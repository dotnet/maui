using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xamarin.Forms.Platform.WPF.Helpers;
using WList = System.Windows.Controls.ListView;
using WGrid = System.Windows.Controls.Grid;
using WpfScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility;

namespace Xamarin.Forms.Platform.WPF
{
	public class ListViewRenderer : ViewRenderer<ListView, WGrid>
	{
		class ScrollViewerBehavior
		{
			public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double),
				typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

			static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
			{
				var scrollViewer = target as ScrollViewer;
				scrollViewer?.ScrollToVerticalOffset((double)e.NewValue);
			}
		}
		
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;
		WpfScrollBarVisibility? _defaultHorizontalScrollVisibility;
		WpfScrollBarVisibility? _defaultVerticalScrollVisibility;
		ScrollViewer _scrollViewer;

		// Header and Footer
        readonly WGrid _grid = new WGrid(); 
		WList _listview = null;
		IVisualElementRenderer _headerRenderer;
		IVisualElementRenderer _footerRenderer;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(40, 40);
			return result;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				e.OldElement.ItemSelected -= OnElementItemSelected;
				e.OldElement.ScrollToRequested -= OnElementScrollToRequested;

				var templatedItems = ((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems;
				templatedItems.CollectionChanged -= OnCollectionChanged;
				templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				if (_listview != null)
				{
					_listview.MouseUp -= OnNativeMouseUp;
					_listview.KeyUp -= OnNativeKeyUp;
					_listview.TouchUp -= OnNativeTouchUp;
					_listview.StylusUp -= OnNativeStylusUp;
					_listview.Loaded -= ControlOnLoaded;
				}
				
				if (_scrollViewer != null)
				{
					_scrollViewer.ScrollChanged -= SendScrolled;
				}

				if(Control is object)
				{
					Control.SizeChanged -= Grid_SizeChanged;
				}
			}

			if (e.NewElement != null)
			{
				e.NewElement.ItemSelected += OnElementItemSelected;
				e.NewElement.ScrollToRequested += OnElementScrollToRequested;

				if (_listview == null) // Construct and SetNativeControl and suscribe control event
				{
					_listview = new WList
					{
						DataContext = Element,
						ItemTemplate = (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["CellTemplate"],
						Style = (System.Windows.Style)System.Windows.Application.Current.Resources["ListViewTemplate"]
					};

					VirtualizingPanel.SetVirtualizationMode(_listview, VirtualizationMode.Recycling);
					VirtualizingPanel.SetScrollUnit(_listview, ScrollUnit.Pixel);
					
					SetNativeControl(_grid);

					// Setup grid for header/listview/footer
					Control.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
					Control.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
					Control.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

					WGrid.SetRow(_listview, 1);
					
					Control.Children.Add(_listview);

					_listview.MouseUp += OnNativeMouseUp;
					_listview.KeyUp += OnNativeKeyUp;
					_listview.TouchUp += OnNativeTouchUp;
					_listview.StylusUp += OnNativeStylusUp;
					_listview.Loaded += ControlOnLoaded;
				}

				// Suscribe element events
				var templatedItems = TemplatedItemsView.TemplatedItems;
				templatedItems.CollectionChanged += OnCollectionChanged;
				templatedItems.GroupedCollectionChanged += OnGroupedCollectionChanged;

				Control.SizeChanged += Grid_SizeChanged;
								
				// Update control properties
				UpdateHeader();
				UpdateFooter();
				UpdateItemSource();
				UpdateHorizontalScrollBarVisibility();
				UpdateVerticalScrollBarVisibility();

				if (Element.SelectedItem != null)
					OnElementItemSelected(null, new SelectedItemChangedEventArgs(Element.SelectedItem, -1));
			}

			base.OnElementChanged(e);
		}

		// If the control size changes, then re-layout the header and footer.
		private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_headerRenderer is object)
			{
				_headerRenderer.GetNativeElement().Width = e.NewSize.Width;
				_headerRenderer.GetNativeElement().UpdateLayout();

				var header = Element.HeaderElement;
				var headerView = (VisualElement)header;
				SizeRequest request = headerView.Measure(4096, 4096);

				Layout.LayoutChildIntoBoundingRegion(headerView, new Rectangle(0, 0, e.NewSize.Width, request.Request.Height));
			}

			if (_footerRenderer is object)
			{
				_footerRenderer.GetNativeElement().Width = e.NewSize.Width;
				_footerRenderer.GetNativeElement().UpdateLayout();

				var footer = Element.FooterElement;
				var footerView = (VisualElement)footer;
				SizeRequest request = footerView.Measure(4096, 4096);

				Layout.LayoutChildIntoBoundingRegion(footerView, new Rectangle(0, 0, e.NewSize.Width, request.Request.Height));
			}
		}

		void UpdateHeader()
		{
			var header = Element.HeaderElement;
			var headerView = (VisualElement)header;

			if (_headerRenderer is object)
			{
				Control.Children.Remove(_headerRenderer.GetNativeElement());
				_headerRenderer.Dispose();
			}

			if (headerView is null)
				return;

			_headerRenderer = Platform.CreateRenderer(headerView);
			Platform.SetRenderer(headerView, _headerRenderer);
		
			WGrid.SetRow(_headerRenderer.GetNativeElement(), 0);
			Control.Children.Add(_headerRenderer.GetNativeElement());

		}
		
		void UpdateFooter() 
		{
			var footer = Element.FooterElement;
			var footerView = (VisualElement)footer;

			if (_footerRenderer is object)
			{
				Control.Children.Remove(_footerRenderer.GetNativeElement());
				_footerRenderer.Dispose();
			}

			if (footerView is null)
				return;

			_footerRenderer = Platform.CreateRenderer(footerView);
			Platform.SetRenderer(footerView, _footerRenderer);

			WGrid.SetRow(_footerRenderer.GetNativeElement(), 2);
			Control.Children.Add(_footerRenderer.GetNativeElement());
		}

		void ControlOnLoaded(object sender, RoutedEventArgs e)
		{
			_scrollViewer = _listview.FindVisualChild<ScrollViewer>();
			if (_scrollViewer != null)
			{
				_scrollViewer.ScrollChanged += SendScrolled;
			}
		}

		void SendScrolled(object sender, ScrollChangedEventArgs e)
		{
			var args = new ScrolledEventArgs(0, _scrollViewer.VerticalOffset);
			Element?.SendScrolled(args);
		}

		void OnElementScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;
			ScrollTo(scrollArgs.Group, scrollArgs.Item, e.Position, e.ShouldAnimate);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
			{
				UpdateHorizontalScrollBarVisibility();
			}
			else if (e.PropertyName == ListView.HeaderProperty.PropertyName || e.PropertyName == "HeaderElement")
			{
				UpdateHeader();
			}
			else if (e.PropertyName == ListView.FooterProperty.PropertyName || e.PropertyName == "FooterElement")
			{
				UpdateFooter();
			}
		}

		void UpdateItemSource()
		{
			List<object> items = new List<object>();

			if (Element.IsGroupingEnabled)
			{
				int index = 0;
				foreach (var groupItem in TemplatedItemsView.TemplatedItems)
				{
					var group = TemplatedItemsView.TemplatedItems.GetGroup(index);

					if (group.Count != 0)
					{
						items.Add(group.HeaderContent);

						items.AddRange(group);
					}

					index++;
				}

				_listview.ItemsSource = items;
			}
			else
			{
				foreach (var item in TemplatedItemsView.TemplatedItems)
				{
					items.Add(item);
				}

				_listview.ItemsSource = items;
			}
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == null)
				_defaultVerticalScrollVisibility = ScrollViewer.GetVerticalScrollBarVisibility(_listview);

			switch (Element.VerticalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetVerticalScrollBarVisibility(_listview, WpfScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetVerticalScrollBarVisibility(_listview, WpfScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetVerticalScrollBarVisibility(_listview, (WpfScrollBarVisibility)_defaultVerticalScrollVisibility);
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
				_defaultHorizontalScrollVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(_listview);

			switch (Element.HorizontalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetHorizontalScrollBarVisibility(_listview, WpfScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetHorizontalScrollBarVisibility(_listview, WpfScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetHorizontalScrollBarVisibility(_listview, (WpfScrollBarVisibility)_defaultHorizontalScrollVisibility);
					break;
			}
		}

		void OnNativeKeyUp(object sender, KeyEventArgs e)
			=> Element.NotifyRowTapped(_listview.SelectedIndex, cell: null);

		void OnNativeMouseUp(object sender, MouseButtonEventArgs e)
			=> Element.NotifyRowTapped(_listview.SelectedIndex, cell: null);

		void OnNativeTouchUp(object sender, TouchEventArgs e)
			=> Element.NotifyRowTapped(_listview.SelectedIndex, cell: null);

		void OnNativeStylusUp(object sender, StylusEventArgs e)
			=> Element.NotifyRowTapped(_listview.SelectedIndex, cell: null);

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (_listview != null)
				{
					_listview.MouseUp -= OnNativeMouseUp;
					_listview.KeyUp -= OnNativeKeyUp;
					_listview.TouchUp -= OnNativeTouchUp;
					_listview.StylusUp -= OnNativeStylusUp;
					_listview.Loaded -= ControlOnLoaded;
				}
				if (_scrollViewer != null)
				{
					_scrollViewer.ScrollChanged -= SendScrolled;
				}

				if (Element != null)
				{
					TemplatedItemsView.TemplatedItems.CollectionChanged -= OnCollectionChanged;
					TemplatedItemsView.TemplatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				}

				if (Control is object)
				{
					Control.SizeChanged -= Grid_SizeChanged;
				}

				_footerRenderer?.Dispose();
				_headerRenderer?.Dispose();
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		void OnElementItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (Element == null)
				return;

			if (e.SelectedItem == null)
			{
				_listview.SelectedIndex = -1;
				return;
			}

			var templatedItems = TemplatedItemsView.TemplatedItems;
			var index = 0;

			if (Element.IsGroupingEnabled)
			{
				int selectedItemIndex = templatedItems.GetGlobalIndexOfItem(e.SelectedItem);
				var leftOver = 0;
				int groupIndex = templatedItems.GetGroupIndexFromGlobal(selectedItemIndex, out leftOver);

				index = selectedItemIndex - (groupIndex + 1);
			}
			else
			{
				index = templatedItems.GetGlobalIndexOfItem(e.SelectedItem);
			}

			_listview.SelectedIndex = index;
		}

		void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateItemSource();
		}

		void OnGroupedCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateItemSource();
		}
		void ScrollTo(object group, object item, ScrollToPosition toPosition, bool shouldAnimate)
		{
			var viewer = _listview.FindVisualChild<ScrollViewer>();
			if (viewer == null)
			{
				RoutedEventHandler loadedHandler = null;
				loadedHandler = (o, e) =>
				{
					_listview.Loaded -= loadedHandler;
					Device.BeginInvokeOnMainThread(() => { ScrollTo(group, item, toPosition, shouldAnimate); });
				};
				_listview.Loaded += loadedHandler;
				return;
			}
			var templatedItems = TemplatedItemsView.TemplatedItems;
			Tuple<int, int> location = templatedItems.GetGroupAndIndexOfItem(group, item);
			if (location.Item1 == -1 || location.Item2 == -1)
				return;

			var t = templatedItems.GetGroup(location.Item1).ToArray();
			var c = t[location.Item2];

			Device.BeginInvokeOnMainThread(() =>
			{
				ScrollToPositionInView(_listview, viewer, c, toPosition, shouldAnimate);
			});
		}
		static void ScrollToPositionInView(WList control, ScrollViewer sv, object item, ScrollToPosition position, bool animated)
		{
			// Scroll immediately if possible
			if (!TryScrollToPositionInView(control, sv, item, position, animated))
			{
				control.ScrollIntoView(item);
				control.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
				{
					TryScrollToPositionInView(control, sv, item, position, animated);
				}));
			}
		}

		static bool TryScrollToPositionInView(ItemsControl itemsControl, ScrollViewer sv, object item, ScrollToPosition position, bool animated)
		{
			var cell = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
			if (cell == null)
				return false;

			var cellHeight = cell.RenderSize.Height;
			var offsetInViewport = cell.TransformToAncestor(sv).Transform(new System.Windows.Point(0, 0)).Y;

			double newOffsetInViewport;
			switch (position)
			{
				case ScrollToPosition.Start:
					newOffsetInViewport = 0;
					break;
				case ScrollToPosition.Center:
					newOffsetInViewport = (sv.ViewportHeight - cellHeight) / 2;
					break;
				case ScrollToPosition.End:
					newOffsetInViewport = sv.ViewportHeight - cellHeight;
					break;
				case ScrollToPosition.MakeVisible:
					{
						var startOffset = 0;
						var endOffset = sv.ViewportHeight - cellHeight;
						var startDistance = Math.Abs(offsetInViewport - startOffset);
						var endDistance = Math.Abs(offsetInViewport - endOffset);
						// already in view, no action
						if (endOffset >= offsetInViewport && startOffset <= offsetInViewport)
							newOffsetInViewport = offsetInViewport;
						else if (startDistance < endDistance)
							newOffsetInViewport = startOffset;
						else
							newOffsetInViewport = endOffset;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(position), position, null);
			}

			if (newOffsetInViewport != offsetInViewport)
			{
				var offset = sv.VerticalOffset + offsetInViewport - newOffsetInViewport;
				ScrollToOffset(sv, offset, animated);
			}
			return true;
		}

		static void ScrollToOffset(ScrollViewer sv, double offset, bool animated)
		{
			if (sv.CanContentScroll)
			{
				var maxPossibleValue = sv.ExtentHeight - sv.ViewportHeight;
				offset = Math.Min(maxPossibleValue, Math.Max(0, offset));
				if (animated)
				{
					var animation = new DoubleAnimation
					{
						From = sv.VerticalOffset,
						To = offset,
						DecelerationRatio = 0.2,
						Duration = new Duration(TimeSpan.FromMilliseconds(200))
					};
					var storyboard = new Storyboard();
					storyboard.Children.Add(animation);
					Storyboard.SetTarget(animation, sv);
					Storyboard.SetTargetProperty(animation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
					storyboard.Begin();
				}
				else
				{
					sv.ScrollToVerticalOffset(offset);
				}
			}
		}
	}
}
